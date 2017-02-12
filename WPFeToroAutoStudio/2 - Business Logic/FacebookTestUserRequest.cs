using System;
using System.Collections.Generic;

namespace Applenium
{
    /// <summary>
    /// FacebookTestUser Result for JSON
    /// </summary>
    public class FacebookTestUser
    {
        /// <summary>
        /// AppId
        /// </summary>
        private string AppId { get; set; }
        /// <summary>
        /// password after login
        /// </summary>
        private string AppSecret{ get; set; }
         /// <summary>
         /// LoginUrl
         /// </summary>
         private bool Installed { get; set; }


         private AppleniumLogger logger = new AppleniumLogger();
        /// <summary>
        /// Facebook API Request
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="appSecret"></param>
        /// <param name="installed"></param>
        /// <returns></returns>
        ///
        internal FacebookTestUserResultModel FacebookTestUserRequest(string appId, string appSecret,bool installed)
        {
            FacebookTestUserResultModel result=new FacebookTestUserResultModel();
            try
            {
            
                string url = string.Format("https://graph.facebook.com/{0}/accounts/test-users", appId);
                var data = new FacebookTestUser
                {
                    AppId = appId,
                    AppSecret = appSecret,
                    Installed=installed
                };

                string postData = string.Format("installed={0}&access_token={1}|{2}", data.Installed, data.AppId,data.AppSecret);

                result = HttpRequestExtensions.TryPostJson<FacebookTestUserResultModel>(url, postData,300000);
                if (result==null)
                      result = HttpRequestExtensions.TryPostJson<FacebookTestUserResultModel>(url, postData,300000);

                
            }
            catch (Exception exception)
            {
                LogObject logException = new LogObject();
                logException.Description = exception.Message + "Failed to create Facebook user";
                logException.StatusTag = Constants.ERROR;
                logException.Exception = exception;
                logger.Print(logException);


                result = null;

            }
            return result;
        }

        internal FacebookTestUserListResultModel FacebookTestUserListRequest(string appId, string appSecret, int count)
        {
            FacebookTestUserListResultModel result = new FacebookTestUserListResultModel();
            try
            {

                string url = string.Format("https://graph.facebook.com/{0}/accounts", appId);

                string postData = string.Format("type=test-users&access_token={0}|{1}&limit={2}", appId, appSecret, count);

                result = HttpRequestExtensions.TryGetJson<FacebookTestUserListResultModel>(url + "?" + postData, 300000);
                if (result == null)
                    result = HttpRequestExtensions.TryGetJson<FacebookTestUserListResultModel>(url + "?" + postData, 300000);


            }
            catch (Exception exception)
            {

                LogObject logException = new LogObject();
                logException.Description = exception.Message + "Failed to get  Facebook users list ";
                logException.StatusTag = Constants.ERROR;
                logException.Exception = exception;
                logger.Print(logException);


                result = null;

            }
            return result;
        }

         
        internal bool FacebookTestUserFriendRequest(string testUser1Id, string testUser2Id, string testUser1AccessToken, string testUser2AccessToken)
        {
            bool result;

            try
            {

                string url = string.Format("https://graph.facebook.com/{0}/friends/{1}", testUser1Id, testUser2Id);
                string postData = string.Format("access_token={0}", testUser1AccessToken);

                result = HttpRequestExtensions.TryPostJson<bool>(url, postData, 300000);

                if (result)
                {
                    url = string.Format("https://graph.facebook.com/{1}/friends/{0}", testUser1Id, testUser2Id);
                    postData = string.Format("access_token={0}", testUser2AccessToken);

                    result = HttpRequestExtensions.TryPostJson<bool>(url, postData);
                    if (!result)
                    {
                        //retry
                        result = HttpRequestExtensions.TryPostJson<bool>(url, postData);
                    }

                }
                else
                {
                    //retry
                    url = string.Format("https://graph.facebook.com/{1}/friends/{0}", testUser1Id, testUser2Id);
                    postData = string.Format("access_token={0}", testUser2AccessToken);

                    result = HttpRequestExtensions.TryPostJson<bool>(url, postData);
                    if (!result)
                    {
                        //retry
                        result = HttpRequestExtensions.TryPostJson<bool>(url, postData);
                    }

                }
            }
            catch (Exception exception)
            {     

                LogObject logException = new LogObject();
                logException.Description = exception.Message + "Failed to create Facebook friends connection ";
                logException.StatusTag = Constants.ERROR;
                logException.Exception = exception;

                logger.Print(logException);

                result = false;
            }
            return result;
        }
    }
}