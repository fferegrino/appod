using System;
using Android.Content;
using Android.Hardware;
using Android.Util;
using Android.Widget;
using HuePod.Droid.Custom.Snsr;

namespace HuePod.Droid.Custom
{


	public class WindowView : ImageView, TiltSensor.ITiltListener
	{
		private float latestPitch;
		private float latestRoll;

		protected TiltSensor sensor;

		private static readonly float DEFAULT_MAX_PITCH_DEGREES = 30;
		private static readonly float DEFAULT_MAX_ROLL_DEGREES = 30;
		private static readonly float DEFAULT_HORIZONTAL_ORIGIN_DEGREES = 0;
		private static readonly float DEFAULT_VERTICAL_ORIGIN_DEGREES = 0;
		private float maxPitchDeg;
		private float maxRollDeg;
		private float horizontalOriginDeg;
		private float verticalOriginDeg;

		private const SensorDelay DEFAULT_SENSOR_SAMPLING_PERIOD_US = SensorDelay.Game;
		private SensorDelay sensorSamplingPeriod;

		/** Determines the basis in which device orientation is measured. */
		public enum OrientationMode
		{
			/** Measures absolute yaw / pitch / roll (i.e. relative to the world). */
			Absolute,
			/**
			 * Measures yaw / pitch / roll relative to the starting orientation.
			 * The starting orientation is determined upon receiving the first sensor data,
			 * but can be manually reset at any time using {@link #resetOrientationOrigin(boolean)}.
			 */
			Relative
		}

		private static readonly OrientationMode DEFAULT_ORIENTATION_MODE = OrientationMode.Relative;
		private OrientationMode orientationMode;

		private static readonly float DEFAULT_MAX_CONSTANT_TRANSLATION_DP = 150;
		private float maxConstantTranslation;

		/** Determines the relationship between change in device tilt and change in image translation. */
		public enum TranslateMode
		{
			/**
			 * The image is translated by a constant amount per unit of device tilt.
			 * Generally preferable when viewing multiple adjacent WindowViews that have different
			 * contents but should move in tandem.
			 * <p>
			 * Same amount of tilt will result in the same translation for two images of differing size.
			 */
			Constant,
			/**
			 * The image is translated proportional to its off-view size. Generally preferable when
			 * viewing a single WindowView, this mode ensures that the full image can be 'explored'
			 * within a fixed tilt amount range.
			 * <p>
			 * Same amount of tilt will result in different translation for two images of differing size.
			 */
			Proportional
		}

		private const TranslateMode DEFAULT_TRANSLATE_MODE = TranslateMode.Proportional;
		private TranslateMode translateMode;

		// layout
		protected bool heightMatches;
		protected float widthDifference;
		protected float heightDifference;

		public WindowView(Context context) : base(context)
		{

		}

		// http://stackoverflow.com/questions/10593022/monodroid-error-when-calling-constructor-of-custom-view-twodscrollview/10603714#10603714

		public WindowView(IntPtr handle, Android.Runtime.JniHandleOwnership transfer)
			: base (handle, transfer)
		{
		}

		public WindowView(Context context, IAttributeSet attrs) : base(context, attrs)
		{
			Init(context, attrs);
		}

		public WindowView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
		{
			Init(context, attrs);
		}

		public WindowView(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
		{
			Init(context, attrs);
		}

		protected void Init(Context context, IAttributeSet attrs)
		{
			sensorSamplingPeriod = DEFAULT_SENSOR_SAMPLING_PERIOD_US;
			maxPitchDeg = DEFAULT_MAX_PITCH_DEGREES;
			maxRollDeg = DEFAULT_MAX_ROLL_DEGREES;
			verticalOriginDeg = DEFAULT_VERTICAL_ORIGIN_DEGREES;
			horizontalOriginDeg = DEFAULT_HORIZONTAL_ORIGIN_DEGREES;
			orientationMode = DEFAULT_ORIENTATION_MODE;
			translateMode = DEFAULT_TRANSLATE_MODE;
			maxConstantTranslation = DEFAULT_MAX_CONSTANT_TRANSLATION_DP * Resources.DisplayMetrics.Density;

			if (null != attrs)
			{
				Android.Content.Res.TypedArray a = context.ObtainStyledAttributes(attrs, Resource.Styleable.WindowView);
				// Buggy line: 
				sensorSamplingPeriod = DEFAULT_SENSOR_SAMPLING_PERIOD_US; //(SensorDelay)(a.GetInt(Resource.Styleable.WindowView_sensor_sampling_period, (int)sensorSamplingPeriod));
				maxPitchDeg = a.GetFloat(Resource.Styleable.WindowView_max_pitch, maxPitchDeg);
				maxRollDeg = a.GetFloat(Resource.Styleable.WindowView_max_roll, maxRollDeg);
				verticalOriginDeg = a.GetFloat(Resource.Styleable.WindowView_vertical_origin,
						verticalOriginDeg);
				horizontalOriginDeg = a.GetFloat(Resource.Styleable.WindowView_horizontal_origin,
						horizontalOriginDeg);

				int orientationModeIndex = a.GetInt(Resource.Styleable.WindowView_orientation_mode, -1);
				if (orientationModeIndex >= 0)
				{
					orientationMode = (OrientationMode)orientationModeIndex;
				}
				int translateModeIndex = a.GetInt(Resource.Styleable.WindowView_translate_mode, -1);
				if (translateModeIndex >= 0)
				{
					translateMode = (TranslateMode)translateModeIndex;
				}

				maxConstantTranslation = a.GetDimension(Resource.Styleable.WindowView_max_constant_translation,
						maxConstantTranslation);
				a.Recycle();
			}

			if (!IsInEditMode)
			{
				sensor = new TiltSensor(context, orientationMode == OrientationMode.Relative);
				sensor.AddListener(this);
			}

			SetScaleType(ScaleType.CenterCrop);
		}
		/*
	     * LIFE-CYCLE
	     * Registering for sensor events should be tied to Activity / Fragment lifecycle events.
	     * However, this would mean that WindowView cannot be independent. We tie into a few
	     * lifecycle-esque View events that allow us to make WindowView completely independent.
	     *
	     * Un-registering from sensor events is done aggressively to minimise battery drain and
	     * performance impact.
	     * ---------------------------------------------------------------------------------------------
	     */
		public override void OnWindowFocusChanged(bool hasWindowFocus)
		{
			base.OnWindowFocusChanged(hasWindowFocus);
			if (hasWindowFocus)
			{
				sensor.StartTracking(sensorSamplingPeriod);
			}
			else {
				sensor.StopTracking();
			}
		}

		protected override void OnAttachedToWindow()
		{
			base.OnAttachedToWindow();
			if (!IsInEditMode) sensor.StartTracking(sensorSamplingPeriod);
		}

		protected override void OnDetachedFromWindow()
		{
			base.OnDetachedFromWindow();
			sensor.StopTracking();
		}

		/*
	     * DRAWING & LAYOUT
	     * ---------------------------------------------------------------------------------------------
	     */
		protected override void OnDraw(Android.Graphics.Canvas canvas)
		{// -1 -> 1
			float xOffset = 0f;
			float yOffset = 0f;
			if (heightMatches)
			{
				// only let user tilt horizontally
				xOffset = (-horizontalOriginDeg +
						ClampAbsoluteFloating(horizontalOriginDeg, latestRoll, maxRollDeg)) / maxRollDeg;
			}
			else {
				// only let user tilt vertically
				yOffset = (verticalOriginDeg -
						ClampAbsoluteFloating(verticalOriginDeg, latestPitch, maxPitchDeg)) / maxPitchDeg;
			}
			canvas.Save();
			switch (translateMode)
			{
				case TranslateMode.Constant:
					canvas.Translate(
						ClampAbsoluteFloating(0, maxConstantTranslation * xOffset, widthDifference / 2),
							ClampAbsoluteFloating(0, maxConstantTranslation * yOffset, heightDifference / 2));
					break;
				case TranslateMode.Proportional:
					canvas.Translate((float)Math.Round((widthDifference / 2f) * xOffset),
									 (float)Math.Round((heightDifference / 2f) * yOffset));
					break;
			}
			base.OnDraw(canvas);
			canvas.Restore();
		}

		float ClampAbsoluteFloating(float origin, float value, float maxAbsolute)
		{
			return value < origin ?
				  Math.Max(value, origin - maxAbsolute) : Math.Min(value, origin + maxAbsolute);
		}


		/** See {@link TranslateMode}. */
		public void SetTranslateMode(TranslateMode translateMode)
		{
			this.translateMode = translateMode;
		}

		public TranslateMode GetTranslateMode()
		{
			return translateMode;
		}

		/** Maximum image translation from center when using {@link TranslateMode#CONSTANT}. */
		public void SetMaxConstantTranslation(float maxConstantTranslation)
		{
			this.maxConstantTranslation = maxConstantTranslation;
		}

		public float GetMaxConstantTranslation()
		{
			return maxConstantTranslation;
		}

		/** Maximum angle (in degrees) from origin for vertical tilts. */
		public void SetMaxPitch(float maxPitch)
		{
			maxPitchDeg = maxPitch;
		}

		public float GetMaxPitch()
		{
			return maxPitchDeg;
		}

		/** Maximum angle (in degrees) from origin for horizontal tilts. */
		public void SetMaxRoll(float maxRoll)
		{
			this.maxRollDeg = maxRoll;
		}

		public float GetMaxRoll()
		{
			return maxRollDeg;
		}

		/**
		 * Horizontal origin (in degrees). When {@link #latestRoll} equals this value, the image
		 * is centered horizontally.
		 */
		public void SetHorizontalOrigin(float horizontalOrigin)
		{
			this.horizontalOriginDeg = horizontalOrigin;
		}

		public float GetHorizontalOrigin()
		{
			return horizontalOriginDeg;
		}

		/**
	     * Vertical origin (in degrees). When {@link #latestPitch} equals this value, the image
	     * is centered vertically.
	     */
		public void SetVerticalOrigin(float verticalOrigin)
		{
			this.verticalOriginDeg = verticalOrigin;
		}

		public float GetVerticalOrigin()
		{
			return verticalOriginDeg;
		}

		public override void SetImageDrawable(Android.Graphics.Drawables.Drawable drawable)
		{
			base.SetImageDrawable(drawable);
			RecalculateImageDimensions();
		}

		protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
		{
			base.OnSizeChanged(w, h, oldw, oldh);
			RecalculateImageDimensions();
		}

		void RecalculateImageDimensions()
		{
			var drawable = Drawable;
			if (null == drawable) return;

			var scaleType = GetScaleType();
			float width = Width;
			float height = Height;
			float imageWidth = drawable.IntrinsicWidth;
			float imageHeight = drawable.IntrinsicHeight;

			heightMatches = !WidthRatioGreater(width, height, imageWidth, imageHeight);

			if (scaleType == ScaleType.CenterCrop)
			{
				if (heightMatches)
				{
					imageWidth *= height / imageHeight;
					imageHeight = height;
				}
				else {
					imageWidth = width;
					imageHeight *= width / imageWidth;
				}
				widthDifference = imageWidth - width;
				heightDifference = imageHeight - height;
			}
			else {

				widthDifference = 0;
				heightDifference = 0;
			}
		}

		bool WidthRatioGreater(float width, float height, float otherWidth, float otherHeight)
		{
			return height / otherHeight < width / otherWidth;
		}

		public override void SetScaleType(ImageView.ScaleType scaleType)
		{
			if (ScaleType.CenterCrop != scaleType)
				throw new ArgumentException ("Image scale type " + scaleType +
						" is not supported by WindowView. Use ScaleType.CenterCrop instead.");
			base.SetScaleType(scaleType);
		}


		/*
		 * SENSOR DATA
		 * ---------------------------------------------------------------------------------------------
		 */

		public void OnTiltUpdate(float yaw, float pitch, float roll)
		{
			this.latestPitch = pitch;
			this.latestRoll = roll;
			Invalidate();
		}

		public void AddTiltListener(TiltSensor.ITiltListener listener)
		{
			sensor.AddListener(listener);
		}

		public void RemoveTiltListener(TiltSensor.ITiltListener listener)
		{
			sensor.removeListener(listener);
		}

		/**
	     * Manually resets the orientation origin. Has no effect unless {@link #getOrientationMode()}
	     * is {@link OrientationMode#RELATIVE}.
	     *
	     * @param immediate if false, the sensor values smoothly interpolate to the new origin.
	     */
		public void ResetOrientationOrigin(bool immediate)
		{
			sensor.ResetOrigin(immediate);
		}

		/**
		 * Determines the mapping of orientation to image offset.
		 * See {@link OrientationMode}.
		 */
		public void SetOrientationMode(OrientationMode orientationMode)
		{
			this.orientationMode = orientationMode;
			sensor.SetTrackRelativeOrientation(orientationMode == OrientationMode.Relative);
			sensor.ResetOrigin(true);
		}

		public OrientationMode GetOrientationMode()
		{
			return orientationMode;
		}

		/**
	     * @param samplingPeriodUs see {@link SensorManager#registerListener(SensorEventListener, Sensor, int)}
	     */
		public void setSensorSamplingPeriod(SensorDelay samplingPeriodUs)
		{
			this.sensorSamplingPeriod = samplingPeriodUs;
			if (sensor.IsTracking)
			{
				sensor.StopTracking();
				sensor.StartTracking(this.sensorSamplingPeriod);
			}
		}

		/** @return sensor sampling period (in microseconds). */
		public SensorDelay GetSensorSamplingPeriod()
		{
			return sensorSamplingPeriod;
		}
	}
}

