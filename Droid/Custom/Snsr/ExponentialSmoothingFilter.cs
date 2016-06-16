using System;
using Java.Lang;

namespace HuePod.Droid.Custom.Snsr
{
	public class ExponentialSmoothingFilter : IFilter
	{
		float _lastValue;
		float _factor;

		public ExponentialSmoothingFilter(float smoothingFactor, float initialValue)
		{
			_factor = smoothingFactor;
			Reset(initialValue);
		}

		/// <summary>
		/// Sets the smoothing factor.
		/// </summary>
		/// <returns>The smoothing factor.</returns>
		/// <param name="factor">Factor.</param>
		public void SetSmoothingFactor(float factor)
		{
			_factor = factor;
		}

		public float Push(float value)
		{
			_lastValue = _lastValue + _factor * (value - _lastValue);
			return Get();
		}

		public void Reset(float value)
		{
			_lastValue = value;
		}

		public float Get()
		{
			return _lastValue;
		}
	}
}

