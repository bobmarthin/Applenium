using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Applenium._2___Business_Logic;
using NUnit.Framework;
using Applenium._4____Infrustructure;

namespace Applenium._6___Unit_Tests
{

    [TestFixture]
    internal class Loader
    {
        [Test]
        public void PutExecuteTest()
        {
            string appKeyName = "loaderio-auth";
            string appKey = "05bf62a8dffd7a28436e4d615991608a";
            string url = string.Format("https://api.loader.io/v2/tests/{0}/run", "702a39093c578167a4ba001b54714c2f");

            string postData = string.Empty;
            //need to get results id 
            var result = HttpRequestExtensions.TryPutJson<LoaderIoResultTestResultModel>(url, appKeyName, appKey, postData, 300000);

        }

    }
}
