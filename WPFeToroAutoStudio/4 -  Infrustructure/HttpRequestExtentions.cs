using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Net.Http.Headers;
//using System.Net.Http.HttpContent;


namespace Applenium
{
    /// <summary>
    /// Http request Class
    /// </summary>
    public  static class HttpRequestExtensions
    {
        private static readonly JavaScriptSerializer Serializer = new JavaScriptSerializer();
        private static AppleniumLogger logger = new AppleniumLogger();

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

                LogObject logException = new LogObject();
                logException.Description = exception.Message;
                logException.StatusTag = Constants.ERROR;
                logException.Exception = exception;
                logger.Print(logException);

            }
            return result;
        }


        public static TResult TryGetJson<TResult>(string url, string headersName, string headersValue, int timeOut = 5000)
        {
            TResult result = default(TResult);
            try
            {
                //Log4NetLogger.Log(eLogLevel.Debug, string.Format("GetJson from url: {0}", url));
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Timeout = timeOut;
                request.Headers[headersName] = headersValue;
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    using (Stream reader = response.GetResponseStream())
                    {
                        if (reader == null)
                            return result;
                        using (var sr = new StreamReader(reader, Encoding.UTF8))
                        {
                            string queryResult = sr.ReadToEnd();                         
                            result = Serializer.Deserialize<TResult>(queryResult);
                        }
                    }
                }
            }
            catch (Exception exception)
            {

                LogObject logException = new LogObject();
                logException.Description = exception.Message;
                logException.StatusTag = Constants.ERROR;
                logException.Exception = exception;
                logger.Print(logException);

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

        public static TResult TryPostJson<TResult>(string url, string headers, string postData, int timeOut = 60000)
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

                request.Headers["Authorization"] = headers;

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
            catch (Exception exception)
            {

                LogObject logException = new LogObject();
                logException.Description = string.Format("Failed to PostJson from url: {0}, exeption: {1}", url,exception);
                logException.StatusTag = Constants.ERROR;
                logException.Exception = exception;
                logger.Print(logException);
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

   

        public static string GetJsonValue(string url, string key, int timeOut = 5000)
        {

            try
            {
                string parameterValue = string.Empty;
                var request = (HttpWebRequest) WebRequest.Create(url);
                request.Timeout = timeOut;
                using (var response = (HttpWebResponse) request.GetResponse())
                {
                    using (Stream reader = response.GetResponseStream())
                    {
                        if (reader == null)
                            return null;
                        using (var sr = new StreamReader(reader, Encoding.UTF8))
                        {
                            string queryResult = sr.ReadToEnd();
                            JObject json = JObject.Parse(queryResult);
                            foreach (var variable in json)
                            {
                                if (variable.Key == key)
                                    return variable.Value.ToString();
                            }

                        }
                    }
                }
            }
            catch
                (Exception exception)
            {

                LogObject logException = new LogObject();
                logException.Description = exception.Message;
                logException.StatusTag = Constants.ERROR;
                logException.Exception = exception;
                logger.Print(logException);
                return null;
            }
            return null;

        }
        /// <summary>
        /// TryPutJson
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headerName"></param>
        /// <param name="headerValue"></param>
        /// <param name="postData"></param>
        /// <param name="timeOut"></param>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public static TResult TryPutJson<TResult>(string url,string headerName, string headerValue , string postData, int timeOut = 60000)
        {
            TResult result = default(TResult);
            string responseJson = string.Empty;
            HttpWebResponse response = null;
            Stream requestStream = null;
            HttpWebRequest request = null;
            try
            {
                request = (HttpWebRequest) WebRequest.Create(url);
                request.Method = "PUT";
                request.ContentType = "application/json; charset=utf-8";
                request.Timeout = timeOut;
              
                request.Headers[headerName] = headerValue;
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

                LogObject logException = new LogObject();
                logException.Description = exception.Message;
                logException.StatusTag = Constants.ERROR;
                logException.Exception = exception;
                logger.Print(logException);                
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

        /// <summary>
        /// SIMPLE HTML CLIENT
        /// </summary>
        /// <param name="url"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public static string GetHTTPrequest(string url, int timeOut = 5000)
        {
            string httpout = "";
            using (var client = new HttpClient())
            {

                var response = client.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    // by calling .Result you are performing a synchronous call
                    var responseContent = response.Content;

                    // by calling .Result you are synchronously reading the result
                    httpout = responseContent.ReadAsStringAsync().Result;
                }
            }
            return httpout;

        }


        /// <summary>
        /// ADV HTML CLIENT
        /// </summary>
        /// <param name="url"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public static string GetHTTPrequestPost(string line)
        {
            /*  
                    m=POST|GET|PUT... 
                    u=URL 
                    p=POST data 
                    h=headers header::value,,header::value
             */
            string httpout = "";
            string mode = "";
            string url = "";
            string postdata = "";
            string headers_list = "";
            string key = "";
            string val = "";


            HttpContent content = null;
            HttpResponseMessage response = null;

            //GET mode
            Match match = Regex.Match(line, @"m=\s*(\w+)(\s+u=|\s+p=|\s+h=|\s*$)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                mode = match.Groups[1].Value;

            }
            else
            {
                mode = "get";
            }

            //header
            match = Regex.Match(line, @"h=\s*(.*?)(\s+u=|\s+p=|\s+m=|\s*$)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                headers_list = match.Groups[1].Value;

            }

            //url
            match = Regex.Match(line, @"u=\s*(.*?)(\s+m=|\s+p=|\s+h=|\s*$)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                url = match.Groups[1].Value;

            }
            else
            {
                return "ERROR: HTTP URL not found";
            }

            //POST DATA
            match = Regex.Match(line, @"p=\s*(.*?)(\s+m=|\s+u=|\s+h=|\s*$)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                //TODO: add read from file
                postdata = match.Groups[1].Value;
                content = new StringContent(postdata);
            }

            try
            {
                var client = new HttpClient();

                // headers def
                if (headers_list != String.Empty)
                {
                    string[] headers = Regex.Split(headers_list, @"\s*,,\s*");

                    foreach (string l in headers)
                    {
                        match = Regex.Match(l, @"^\s*(.*?)\s*::\s*(.*?)\s*$", RegexOptions.IgnoreCase);
                        if (match.Success)
                        {
                            key = match.Groups[1].Value;
                            val = match.Groups[2].Value;

                            match = Regex.Match(key, @"Content-Type", RegexOptions.IgnoreCase);
                            if (match.Success)
                            {
                                if (content.Headers.Contains(key))
                                {
                                    content.Headers.Remove(key);
                                    content.Headers.Add(key, val);
                                }
                            }
                            else
                            {
                                if (client.DefaultRequestHeaders.Contains(key))
                                {
                                    client.DefaultRequestHeaders.Remove(key);
                                }
                                client.DefaultRequestHeaders.Add(key, val);
                            }

                        }
                    }
                }

                //start message
                match = Regex.Match(mode, @"post", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    response = client.PostAsync(url, content).Result;
                }
                match = Regex.Match(mode, @"get", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    response = client.GetAsync(url).Result;
                }
                match = Regex.Match(mode, @"Put", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    response = client.PutAsync(url, content).Result;
                }


                if (response.IsSuccessStatusCode)
                {

                    var responseContent = response.Content;
                    httpout = responseContent.ReadAsStringAsync().Result;
                }
            }
            catch (Exception ex)
            {
                string vlad = ex.Message;
                string ee = vlad + vlad;
            }

            return httpout;

        }
        public static TResult JsomDeserialize<TResult>(string Data)
        {
            TResult result = default(TResult);
 
            result = Serializer.Deserialize<TResult>(Data);
 
            return result;
        }

    }
}