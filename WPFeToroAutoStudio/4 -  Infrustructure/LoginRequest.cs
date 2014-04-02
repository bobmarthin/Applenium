namespace Applenium
{
    /// <summary>
    /// After Login Result for JSON
    /// </summary>
    internal class LoginRequest
    {
        /// <summary>
        /// user name after login 
        /// </summary>
        private string Username { get; set; }
        /// <summary>
        /// password after login
        /// </summary>
        private string Password { get; set; }


        /// <summary>
        /// Login API request
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        

        internal LoginResultModel FirstUserLogin(string username, string password)
        {
            JsonParser jsonParser = new JsonParser();
            string URL = "http://" + jsonParser.ReadJson("TestingEnvironment") + "/login/";
            var data = new LoginRequest
            {
                Password = password,
                Username = username
            };

            string postData = string.Format("username={0}&password={1}", data.Username, data.Password);

            var result = HttpRequestExtensions.TryPostJson<LoginResultModel>(URL, postData);
            return result;
        }


    }
}