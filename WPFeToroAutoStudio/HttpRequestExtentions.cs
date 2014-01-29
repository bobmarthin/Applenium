using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace Applenium
{
    /// <summary>
    /// Http request Class
    /// </summary>
    public static class HttpRequestExtensions
    {
        private static readonly JavaScriptSerializer Serializer = new JavaScriptSerializer();

        /// <summary>
        /// Get Http request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="timeOut"></param>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public static TResult TryGetJson<TResult>(string url, int timeOut = 5000)
        {
            TResult result = default(TResult);
            try
            {
                //Log4NetLogger.Log(eLogLevel.Debug, string.Format("GetJson from url: {0}", url));
                var request = (HttpWebRequest) WebRequest.Create(url);
                request.Timeout = timeOut;
                using (var response = (HttpWebResponse) request.GetResponse())
                {
                    using (Stream reader = response.GetResponseStream())
                    {
                        if (reader == null)
                            return result;
                        using (var sr = new StreamReader(reader, Encoding.UTF8))
                        {
                            string queryResult = sr.ReadToEnd();
                            //Log4NetLogger.Log(eLogLevel.Debug, string.Format("GetJson response: {0}", queryResult));
                            result = Serializer.Deserialize<TResult>(queryResult);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message);
            }
            return result;
        }

        /// <summary>
        /// Post Http request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <param name="timeOut"></param>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public static TResult TryPostJson<TResult>(string url, string postData, int timeOut = 60000)
        {
            TResult result = default(TResult);
            string responseJson = string.Empty;
            HttpWebResponse response = null;
            Stream requestStream = null;
            HttpWebRequest request = null;
            try
            {
                request = (HttpWebRequest) WebRequest.Create(url);
                request.Method = "POST";
                request.Timeout = timeOut;
                request.ContentType = "application/x-www-form-urlencoded";
                //request.Accept = "application/json, text/javascript, */*; q=0.01";
                byte[] data = Encoding.UTF8.GetBytes(postData);
                request.ContentLength = data.Length;
                requestStream = request.GetRequestStream();
                requestStream.Write(data, 0, data.Length);
                requestStream.Close();
                response = (HttpWebResponse) request.GetResponse();
                requestStream = response.GetResponseStream();
                if (requestStream != null)
                {
                    using (var reader = new StreamReader(requestStream))
                    {
                        responseJson = reader.ReadToEnd();
                    }
                }
                result = Serializer.Deserialize<TResult>(responseJson);
            }
            catch (Exception)
            {
                //Logger.Log(eLogType.Info, string.Format("Failed to PostJson from url: {0}, json: {1}", url, json), ex);
            }
            finally
            {
                if (request != null)
                    request.Abort();

                if (response != null)
                    response.Close();

                if (requestStream != null)
                {
                    requestStream.Close();
                    requestStream.Dispose();
                }
            }
            return result;
        }

        public static TResult TryPostJson<TResult>(string url, string headers,string postData, int timeOut = 60000)
        {
            TResult result = default(TResult);
            string responseJson = string.Empty;
            HttpWebResponse response = null;
            Stream requestStream = null;
            HttpWebRequest request = null;
            try
            {
                request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.Timeout = timeOut;
                request.ContentType = "application/x-www-form-urlencoded";
                
                request.Headers["Authorization"] = headers;
                  
                //request.Accept = "application/json, text/javascript, */*; q=0.01";
                byte[] data = Encoding.UTF8.GetBytes(postData);
                request.ContentLength = data.Length;
                requestStream = request.GetRequestStream();
                requestStream.Write(data, 0, data.Length);
                requestStream.Close();
                response = (HttpWebResponse)request.GetResponse();
                requestStream = response.GetResponseStream();
                if (requestStream != null)
                {
                    using (var reader = new StreamReader(requestStream))
                    {
                        responseJson = reader.ReadToEnd();
                    }
                }
                result = Serializer.Deserialize<TResult>(responseJson);
            }
            catch (Exception exception)
            {
                Logger.Error(string.Format("Failed to PostJson from url: {0}, exeption: {1}", url, exception));
            }
            finally
            {
                if (request != null)
                    request.Abort();

                if (response != null)
                    response.Close();

                if (requestStream != null)
                {
                    requestStream.Close();
                    requestStream.Dispose();
                }
            }
            return result;
        }

    }
}