using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Applenium
{
    /// <summary>
    /// FaceBook Api Json result model
    /// </summary>
    public class FacebookTestUserResultModel
    {
        /// <summary>
        /// Id method 
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// access_token
        /// </summary>
        public string access_token { get; set; }
        /// <summary>
        /// login_url
        /// </summary>
        public string login_url { get; set; }
        /// <summary>
        /// email
        /// </summary>
        public string email { get; set; }
        /// <summary>
        /// password
        /// </summary>
        public string password { get; set; }
    }

    /// <summary>
    /// FacebookTestUserResultModelList
    /// </summary>
    public class FacebookTestUserResultModelShort
    {
        /// <summary>
        /// Id method 
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// access_token
        /// </summary>
        public string access_token { get; set; }
        /// <summary>
        /// login_url
        /// </summary>
        public string login_url { get; set; }  
    }
    /// <summary>
    /// cursors
    /// </summary>
    public class cursors
    {
        /// <summary>
        /// after
        /// </summary>
        public string after { get; set; }
        /// <summary>
        /// before
        /// </summary>
        public string before { get; set; }
    }
    /// <summary>
    /// PagingResultModel
    /// </summary>
    public class PagingResultModel
    {
        /// <summary>
        /// Id method 
        /// </summary>
        public cursors cursors { get; set; }
        /// <summary>
        /// access_token
        /// </summary>
        public string next { get; set; }
       
    }

    /// <summary>
    /// FacebookTestUserListResultModel
    /// </summary>
    public class FacebookTestUserListResultModel
    {
        /// <summary>
        /// data
        /// </summary>
        public List<FacebookTestUserResultModelShort> data { get; set; }
        public PagingResultModel paging { get; set; }
    }
}
