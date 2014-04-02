using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Applenium
{
    [TestFixture]
    class InfrustructureUnitTests
    {
        [Test]
        public void ReadFromJsonParser()
        {
            var jsonParser = new JsonParser();
            var testingEnvironmnet = jsonParser.ReadJson("TestingEnvironmentVersion");
            Assert.IsNotNullOrEmpty(testingEnvironmnet);
        }

        [Test]
        public void WriteToJsonParser()
        {
            var jsonParser = new JsonParser();
            jsonParser.WriteJson("WaitElementExists", "22");
            var waitElementExists = jsonParser.ReadJson("WaitElementExists");
            Assert.AreEqual("22", waitElementExists);
            jsonParser.WriteJson("WaitElementExists", "30");
            Assert.AreEqual("30", waitElementExists);
        } 
    }
}
