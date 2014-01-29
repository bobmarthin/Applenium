namespace Applenium
{
    /// <summary>
    /// This class for JSON Login result model
    /// </summary>
    internal class LoginResultModel
    {
        /// <summary>
        /// IsLogedIn method 
        /// </summary>
        public bool IsLoggedIn { get; set; }
        public string Token { get; set; }
    }
}