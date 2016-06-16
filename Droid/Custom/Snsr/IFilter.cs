using System;
namespace HuePod.Droid.Custom.Snsr
{
	public interface IFilter
	{
		float Push(float value);

		void Reset(float value);

		float Get();
	}
}

