namespace SuperAdmin.Service.Helpers
{
    public static class OtpGenerator
    {
        /// <summary>
        /// This method generates a random 4-digit number between 0 and 9999
        /// </summary>
        /// <returns></returns>
        public static string GenerateFourDigitCode()
        {
            Random random = new Random();
            return random.Next(10000).ToString("D4"); 
        }
    }
}
