using System;
using System.Collections.Generic;
using Android.Content;
using Android.Hardware;
using Android.Runtime;
using Android.Views;

namespace HuePod.Droid.Custom.Snsr
{
	public class TiltSensor : Java.Lang.Object, ISensorEventListener
	{
		// 1 radian = 180 / PI = 57.2957795 degrees
		private const float DEGREES_PER_RADIAN = 57.2957795f;

		private readonly SensorManager sensorManager;

		private bool tracking;

		/** @see {@link Display#getRotation()}. */
		private readonly SurfaceOrientation screenRotation;

		private bool relativeTilt;

		public interface ITiltListener
		{
			/**
 * Euler angles defined as per {@link SensorManager#getOrientation(float[], float[])}.
 * <p>
 * All three are in <b>radians</b> and <b>positive</b> in the <b>counter-clockwise</b>
 * direction.
 * @param yaw rotation around -Z axis. -PI to PI.
 * @param pitch rotation around -X axis. -PI/2 to PI/2.
 * @param roll rotation around Y axis. -PI to PI.
 */
			void OnTiltUpdate(float yaw, float pitch, float roll);
		}

		private List<ITiltListener> listeners;

		private readonly float[] rotationMatrix = new float[9];
		private readonly float[] rotationMatrixTemp = new float[9];
		private readonly float[] rotationMatrixOrigin = new float[9];
		/** [w, x, y, z] */
		private readonly float[] latestQuaternion = new float[4];
		/** [w, x, y, z] */
		private readonly float[] invQuaternionOrigin = new float[4];
		/** [w, x, y, z] */
		private readonly float[] rotationQuaternion = new float[4];
		private readonly float[] latestAccelerations = new float[3];
		private readonly float[] latestMagFields = new float[3];
		private readonly float[] orientation = new float[3];
		private bool haveGravData = false;
		private bool haveAccelData = false;
		private bool haveMagData = false;
		private bool haveRotOrigin = false;
		private bool haveQuatOrigin = false;
		private bool haveRotVecData = false;

		private IFilter yawFilter;
		private IFilter pitchFilter;
		private IFilter rollFilter;

		/** See {@link ExponentialSmoothingFilter#setSmoothingFactor(float)}. */
		private const float SMOOTHING_FACTOR_HIGH_ACC = 0.8f;
		private const float SMOOTHING_FACTOR_LOW_ACC = 0.05f;

		public TiltSensor(Context context, bool trackRelativeOrientation)
		{
			listeners = new List<ITiltListener>();

			InitialiseDefaultFilters(SMOOTHING_FACTOR_LOW_ACC);

			sensorManager = (SensorManager)context.GetSystemService(Context.SensorService);
			tracking = false;


			screenRotation = (context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>())
				.DefaultDisplay.Rotation;

			relativeTilt = trackRelativeOrientation;
		}
		/**
   * Registers for motion sensor events.
   * Do this to begin receiving {@link TiltListener#onTiltUpdate(float, float, float)} callbacks.
   * <p>
   * <b>You must call {@link #stopTracking()} to unregister when tilt updates are no longer
   * needed.</b>
   * @param samplingPeriodUs see {@link SensorManager#registerListener(SensorEventListener, Sensor, int)}
   */
		public void StartTracking(SensorDelay samplingPeriodUs)
		{

			sensorManager.RegisterListener(this, sensorManager.GetDefaultSensor(SensorType.RotationVector), samplingPeriodUs);

			sensorManager.RegisterListener(this, sensorManager.GetDefaultSensor(SensorType.MagneticField), samplingPeriodUs);

			sensorManager.RegisterListener(this, sensorManager.GetDefaultSensor(SensorType.Gravity), samplingPeriodUs);

			sensorManager.RegisterListener(this, sensorManager.GetDefaultSensor(SensorType.Accelerometer), samplingPeriodUs);

			tracking = true;
		}

		public bool IsTracking => tracking;

		public void StopTracking()
		{
			sensorManager.UnregisterListener(this);
			if (null != yawFilter) yawFilter.Reset(0);
			if (null != pitchFilter) pitchFilter.Reset(0);
			if (null != rollFilter) rollFilter.Reset(0);
			tracking = false;
		}

		public void AddListener(ITiltListener listener)
		{
			listeners.Add(listener);
		}

		public void removeListener(ITiltListener listener)
		{
			listeners.Remove(listener);
		}

		public void SetTrackRelativeOrientation(bool trackRelative)
		{
			relativeTilt = trackRelative;
		}

		public SurfaceOrientation ScreenRotation => screenRotation;

		void InitialiseDefaultFilters(float factor)
		{
			yawFilter = new ExponentialSmoothingFilter(factor, null == yawFilter ? 0 : yawFilter.Get());
			pitchFilter = new ExponentialSmoothingFilter(factor, null == pitchFilter ? 0 : pitchFilter.Get());
			rollFilter = new ExponentialSmoothingFilter(factor, null == rollFilter ? 0 : rollFilter.Get());
		}


		public void OnSensorChanged(SensorEvent e)
		{
			switch (e.Sensor.Type)
			{
				case SensorType.RotationVector:
					var values = new float[e.Values.Count];
					e.Values.CopyTo(values, 0);
					SensorManager.GetQuaternionFromVector(latestQuaternion, values);
					if (!haveRotVecData)
					{
						InitialiseDefaultFilters(SMOOTHING_FACTOR_HIGH_ACC);
					}
					haveRotVecData = true;
					break;
				case SensorType.Gravity:
					if (haveRotVecData)
					{
						// rotation vector sensor data is better
						sensorManager.UnregisterListener(this,sensorManager.GetDefaultSensor(SensorType.Gravity));
						break;
					}
					e.Values.CopyTo(latestAccelerations, 0);
					haveGravData = true;
                break;
				case SensorType.Accelerometer:
					if (haveGravData || haveRotVecData)
					{
						// rotation vector / gravity sensor data is better!
						// let's not listen to the accelerometer anymore
						sensorManager.UnregisterListener(this, sensorManager.GetDefaultSensor(SensorType.Accelerometer));
						break;
					}
					e.Values.CopyTo(latestAccelerations, 0);
					haveAccelData = true;
                break;

				case SensorType.MagneticField:
					if (haveRotVecData)
					{
						// rotation vector sensor data is better
						sensorManager.UnregisterListener(this, sensorManager.GetDefaultSensor(SensorType.MagneticField));
						break;
					}
					e.Values.CopyTo(latestMagFields, 0);
					haveMagData = true;
                break;
			}

			if (HaveDataNecessaryToComputeOrientation())
			{
				ComputeOrientation();
			}
		}

		/**
 * After {@link #startTracking(int)} has been called and sensor data has been received,
 * this method returns the sensor type chosen for orientation calculations.
 * @return one of {@link Sensor#TYPE_ROTATION_VECTOR}, {@link Sensor#TYPE_GRAVITY},
 *         {@link Sensor#TYPE_ACCELEROMETER} or 0 if none of the previous are available or
 *         {@link #startTracking(int)} has not yet been called.
 */
		public SensorType GetChosenSensorType()
		{
			if (haveRotVecData) return SensorType.RotationVector;
			if (haveGravData) return SensorType.Gravity;
			if (haveAccelData) return SensorType.Accelerometer;
			return 0;
		}

		private bool HaveDataNecessaryToComputeOrientation()
		{
			return haveRotVecData || ((haveGravData || haveAccelData) && haveMagData);
		}

		/**
     * Computes the latest rotation, remaps it according to the current {@link #screenRotation},
     * and stores it in {@link #rotationMatrix}.
     * <p>
     * Should only be called if {@link #haveDataNecessaryToComputeOrientation()} returns true and
     * {@link #haveRotVecData} is false, else result may be undefined.
     * @return true if rotation was retrieved and recalculated, false otherwise.
     */
		private bool ComputeRotationMatrix()
		{
			if (SensorManager.GetRotationMatrix(rotationMatrixTemp, null, latestAccelerations, latestMagFields))
			{
				switch (screenRotation)
				{
					case SurfaceOrientation.Rotation0:
						SensorManager.RemapCoordinateSystem(rotationMatrixTemp,
															Android.Hardware.Axis.X,
															Android.Hardware.Axis.Y, rotationMatrix);
						break;
					case SurfaceOrientation.Rotation90:
						//noinspection SuspiciousNameCombination
						SensorManager.RemapCoordinateSystem(rotationMatrixTemp,
															Android.Hardware.Axis.Y,
						                                    Android.Hardware.Axis.MinusX, rotationMatrix);
						break;
					case SurfaceOrientation.Rotation180:
						SensorManager.RemapCoordinateSystem(rotationMatrixTemp,
															Android.Hardware.Axis.MinusX,
															Android.Hardware.Axis.MinusY, rotationMatrix);
						break;
					case SurfaceOrientation.Rotation270:
						//noinspection SuspiciousNameCombination
						SensorManager.RemapCoordinateSystem(rotationMatrixTemp,
															Android.Hardware.Axis.MinusY,
															Android.Hardware.Axis.X, rotationMatrix);
						break;
				}
				return true;
			}
			return false;
		}


		private void ComputeOrientation()
		{
			bool updated = false;
			float yaw = 0;
			float pitch = 0;
			float roll = 0;

			if (haveRotVecData)
			{
				RemapQuaternionToScreenRotation(latestQuaternion, screenRotation);
				if (relativeTilt)
				{
					if (!haveQuatOrigin)
					{
						latestQuaternion.CopyTo(invQuaternionOrigin, 0);
						InvertQuaternion(invQuaternionOrigin);
						haveQuatOrigin = true;
					}
					MultQuaternions(rotationQuaternion, invQuaternionOrigin, latestQuaternion);
				}
				else 
				{
					latestQuaternion.CopyTo(rotationQuaternion, 0);
				}

				// https://en.wikipedia.org/wiki/Conversion_between_quaternions_and_Euler_angles
				float q0 = rotationQuaternion[0]; // w
				float q1 = rotationQuaternion[1]; // x
				float q2 = rotationQuaternion[2]; // y
				float q3 = rotationQuaternion[3]; // z

				float rotXRad = (float)Math.Atan2(2 * (q0 * q1 + q2 * q3), 1 - 2 * (q1 * q1 + q2 * q2));
				float rotYRad = (float)Math.Asin(2 * (q0 * q2 - q3 * q1));
				float rotZRad = (float)Math.Atan2(2 * (q0 * q3 + q1 * q2), 1 - 2 * (q2 * q2 + q3 * q3));

				// constructed to match output of SensorManager#getOrientation
				yaw = -rotZRad * DEGREES_PER_RADIAN;
				pitch = -rotXRad * DEGREES_PER_RADIAN;
				roll = rotYRad * DEGREES_PER_RADIAN;
				updated = true;
			}
			else if (ComputeRotationMatrix())
			{
				if (relativeTilt)
				{
					if (!haveRotOrigin)
					{
						rotationMatrix.CopyTo(rotationMatrixOrigin,0);
						haveRotOrigin = true;
					}
					// get yaw / pitch / roll relative to original rotation
					SensorManager.GetAngleChange(orientation, rotationMatrix, rotationMatrixOrigin);
				}
				else {
					// get absolute yaw / pitch / roll
					SensorManager.GetOrientation(rotationMatrix, orientation);
				}
				/*
				 * [0] : yaw, rotation around -z axis
				 * [1] : pitch, rotation around -x axis
				 * [2] : roll, rotation around y axis
				 */
				yaw = orientation[0] * DEGREES_PER_RADIAN;
				pitch = orientation[1] * DEGREES_PER_RADIAN;
				roll = orientation[2] * DEGREES_PER_RADIAN;
				updated = true;
			}

			if (!updated) return;


			if (null != yawFilter) yaw = yawFilter.Push(yaw);
			if (null != pitchFilter) pitch = pitchFilter.Push(pitch);
			if (null != rollFilter) roll = rollFilter.Push(roll);

			for (int i = 0; i < listeners.Count; i++)
			{
				listeners[i].OnTiltUpdate(yaw, pitch, roll);
			}
		}

		/**
		 * @param immediate if true, any sensor data filters are reset to new origin immediately.
		 *                  If false, values transition smoothly to new origin.
		 */
		public void ResetOrigin(bool immediate)
		{
			haveRotOrigin = false;
			haveQuatOrigin = false;
			if (immediate)
			{
				if (null != yawFilter) yawFilter.Reset(0);
				if (null != pitchFilter) pitchFilter.Reset(0);
				if (null != rollFilter) rollFilter.Reset(0);
			}
		}


		public void OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
		{
			//throw new NotImplementedException();
		}
		/**
		 * Please drop me a PM if you know of a more elegant way to accomplish this - Justas
		 * @param q [w, x, y, z]
		 * @param screenRotation see {@link Display#getRotation()}
		 */
		void RemapQuaternionToScreenRotation(float[] q, SurfaceOrientation screenRotation)
		{
			float x = q[1];
			float y = q[2];
			switch (screenRotation)
			{
				case SurfaceOrientation.Rotation0:
					break;
				case SurfaceOrientation.Rotation90:
					q[1] = -y;
					q[2] = x;
					break;
				case SurfaceOrientation.Rotation180:
					q[1] = -x;
					q[2] = -y;
					break;
				case SurfaceOrientation.Rotation270:
					q[1] = y;
					q[2] = -x;
					break;
			}
		}

		/**
		 *
		 * @param qOut [w, x, y, z] result.
		 * @param q1 [w, x, y, z] left.
		 * @param q2 [w, x, y, z] right.
		 */
		void MultQuaternions(float[] qOut, float[] q1, float[] q2)
		{
			// multiply quaternions
			float a = q1[0];
			float b = q1[1];
			float c = q1[2];
			float d = q1[3];

			float e = q2[0];
			float f = q2[1];
			float g = q2[2];
			float h = q2[3];

			qOut[0] = a * e - b * f - c * g - d * h;
			qOut[1] = b * e + a * f + c * h - d * g;
			qOut[2] = a * g - b * h + c * e + d * f;
			qOut[3] = a * h + b * g - c * f + d * e;
		}



		void InvertQuaternion(float[] q)
		{
			for (int i = 1; i < 4; i++)
			{
				q[i] = -q[i]; // invert quaternion
			}
		}
	}
}

