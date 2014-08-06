using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Applenium._2___Business_Logic
{
    class LoaderIoResultModel
    {
        public string  name { get; set; }
        public string duration { get; set; }
        public string timeout { get; set; }
        public string notes { get; set; }
        public string initial { get; set; }
        public string total { get; set; }
        public string complete { get; set; }
        public string test_id { get; set; }
        public string test_type { get; set; }
        public string callback { get; set; }
        public string callback_email { get; set; }
        public string scheduled_at { get; set; }
        public string domain { get; set; }
     
    }

    internal class LoaderIoResultModelConcatenate : LoaderIoResultModel
    {
        public string ConcatenateName { get; set; }

        public LoaderIoResultModelConcatenate(LoaderIoResultModel loaderIoResultModel)
        {
            ConcatenateName = loaderIoResultModel.test_id + ":" + loaderIoResultModel.name;
            name = loaderIoResultModel.name;
            duration = loaderIoResultModel.duration;
            timeout = loaderIoResultModel.timeout;
            initial = loaderIoResultModel.initial;
            total = loaderIoResultModel.total;
            complete = loaderIoResultModel.complete;
            test_id = loaderIoResultModel.test_id;
            test_type = loaderIoResultModel.test_type;
            callback = loaderIoResultModel.callback;
            callback_email = loaderIoResultModel.callback_email;
            scheduled_at = loaderIoResultModel.scheduled_at;
            domain = loaderIoResultModel.domain;
        }
    }


    class LoaderIoResultTestResultModel
    {
        
        public string  message { get; set; }
        public string test_id { get; set; }
        public string status { get; set; }
        public string result_id { get; set; }
      
    }
}
