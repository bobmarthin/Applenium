using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Applenium
{
    class StreamsApiRequest
    {
        private AppleniumLogger logger = new AppleniumLogger();
        // post a new message
        internal StreamsApiResultModel.PostMessageResultModel PostMessageRequest(string actionName, string autorization, string ownerUser, string messageBody)
        {
            StreamsApiResultModel.PostMessageResultModel result = new StreamsApiResultModel.PostMessageResultModel();
            try
            {
                JsonParser jsonParser = new JsonParser();
                string url = string.Format("http://{0}/api/streams", Constants.MemoryConf["TestingEnvironment"]);
                autorization = HttpUtility.UrlEncode(autorization);
                string headers = string.Format("{0}", autorization);

                string postData = string.Format("actionName={0}&ownerUser={1}&messageBody={2}", actionName, ownerUser, messageBody);

                result = HttpRequestExtensions.TryPostJson<StreamsApiResultModel.PostMessageResultModel>(url, headers, postData, 1000);
            }
            catch (Exception exception)
            {
                LogObject logException = new LogObject();
                logException.Description = exception.Message + "Failed to post message on " + ownerUser;
                logException.StatusTag = Constants.ERROR;
                logException.Exception = exception;
                logger.Print(logException);

                result = null;

            }
            return result;
        }


        // like a message
        internal StreamsApiResultModel.LikeMessageRequestModel LikePost(string actionName, string authorization, string messageID)
        {
            StreamsApiResultModel.LikeMessageRequestModel result = new StreamsApiResultModel.LikeMessageRequestModel();

            try
            {
                JsonParser jp = new JsonParser();
                string url = string.Format("http://{0}/api/streams", Constants.MemoryConf["TestingEnvironment"]);
                authorization = HttpUtility.UrlEncode(authorization);
                string headers = string.Format("{0}", authorization);

                string postData = string.Format("actionName={0}", actionName + messageID);

                result = HttpRequestExtensions.TryPostJson<StreamsApiResultModel.LikeMessageRequestModel>(url, headers, postData, 1000);

                if (result == null)
                {

                    LogObject logException = new LogObject();
                    logException.Description = "Failed to like message on " + messageID;
                    logException.StatusTag = Constants.FAILED;             
                    logger.Print(logException);

                  

                }
            }

            catch (Exception ex)
            {
                    LogObject logException = new LogObject();
                logException.Description = ex.Message + "Failed to like message on " + messageID;
                logException.StatusTag = Constants.ERROR;
                logException.Exception = ex;
                logger.Print(logException);

                result = null;
            }

            return result;
        }

        // get feed from user wall

        internal List<StreamsApiResultModel.GetStreamRequestModel> GetStream(string username, string actionName, string messageID)
        {
            List<StreamsApiResultModel.GetStreamRequestModel> result = new List<StreamsApiResultModel.GetStreamRequestModel>();
            try
            {
                JsonParser jp = new JsonParser();
                string url = string.Format("http://{0}/api/streams", Constants.MemoryConf["TestingEnvironment"]);
                //authorization = HttpUtility.UrlEncode(authorization);
                //string headers = string.Format("{0}", authorization);

                url = url + string.Format("/?actionName={0}", actionName + username);

                result = HttpRequestExtensions.TryGetJson<List<StreamsApiResultModel.GetStreamRequestModel>>(url, 1000);

                if (result == null)
                {

                    LogObject loggerException = new LogObject();
                    loggerException.Description = "Failed to Get message on userwall";
                    loggerException.StatusTag = Constants.FAILED;
                    logger.Print(loggerException);

                }
            }

            catch (Exception ex)
            {

                LogObject loggerException = new LogObject();
                loggerException.Description = "Failed to get message on user wall";
                loggerException.StatusTag = Constants.ERROR;
                loggerException.Exception = ex;
                logger.Print(loggerException);
            }

            return result;

        }

        // Login to get token
        internal string login(string userName, string password)
        {
            string token = "";
            try
            {
                LoginRequest lr = new LoginRequest();
                LoginResultModel lrm = lr.FirstUserLogin(userName, password);
                token = lrm.Token;
            }

            catch (Exception ex)
            {

                LogObject loggerException = new LogObject();
                loggerException.Description = "Failed to login with user " + userName;
                loggerException.StatusTag = Constants.ERROR;
                loggerException.Exception = ex;
                logger.Print(loggerException);


            }
            return token;

        }
    }
}
