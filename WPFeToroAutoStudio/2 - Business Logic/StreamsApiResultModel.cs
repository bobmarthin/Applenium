using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Applenium
{
    class StreamsApiResultModel
    {
        public class PostMessageResultModel
        {
            /// <summary>
            /// access_token
            /// </summary>
            public string postURL { get; set; }
            /// <summary>          
            /// Id method 
            /// </summary>
            public string messageBody { get; set; }
            /// <summary>
            /// login_url
            /// </summary>
            public string id { get; set; }
            /// <summary>
            /// email
            /// </summary>
            public string occurredAt { get; set; }
            /// <summary>
            /// password
            /// </summary>
            public user user { get; set; }

            /// <summary>
            /// password
            /// </summary>
            public bool isFlaggedAsSpam { get; set; }
        }

        public class user
        {
            public string username { get; set; }
           
        }

        public class LikeMessageRequestModel
        {
            public string id { get; set; }
            public string occurredAt { get; set; }
        }

        public class GetStreamRequestModel
        {
            public rootData rootData { get; set; }
            public string id { get; set; }
        }

        public class reason
        {
            public string sourceID { get; set; }
            public string type { get; set; }
        }

        public class rootData
        {
            public reason reason { get; set; }
        }

   
    }
}
