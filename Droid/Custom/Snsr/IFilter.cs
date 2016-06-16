namespace HuePod.Droid.Custom.Snsr
{
    public interface IFilter
    {
        /// <summary>
        /// Update filter with the latest value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        float Push(float value);

        /// <summary>
        /// Reset filter to the given value
        /// </summary>
        /// <param name="value"></param>
        void Reset(float value);

        /// <summary>
        /// Latest filtered value
        /// </summary>
        /// <returns></returns>
        float Get();
    }
}