namespace SuperAdmin.Service.Extensions
{
    public static class DateExtensions
    {
        /// <summary>
        /// This method converts date to epoch timestamp in milliseconds 
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static double ToEpochTimestampInMilliseconds(this DateTime date)
        {
            DateTime epochDateTime = new DateTime(1970, 1, 1);
            return (date - epochDateTime).TotalMilliseconds;
        }

        /// <summary>
        /// This method converts date to epoch timestamp in seconds 
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static double ToEpochTimestampInSeconds(this DateTime date)
        {
            DateTime epochDateTime = new DateTime(1970, 1, 1);
            return (date - epochDateTime).TotalSeconds;
        }
    }
}
