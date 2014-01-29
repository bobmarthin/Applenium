using System;
using Applenium.DataSetAutoTestTableAdapters;
using NUnit.Framework;
using Applenium;
using System.Configuration;
using ExecutionManager = Applenium.ExecutionManager;

namespace UnitTestExecutionManager
{
    [TestFixture]
    public class ObWebDriver
    {
       [Test]
        public void ObRegression()
        {
            var adapterTestresult = new TestResultsTableAdapter();
            int runExecutionId = Convert.ToInt32(adapterTestresult.LastRunExecutionID()) + 1;
            ExecutionManager em = new ExecutionManager(runExecutionId);
            bool result = em.ExecuteOneBatch(ConfigurationManager.AppSettings["BatchToExecute"]);
            Assert.AreEqual(true, result,"The Gui Automation failed , please check Applenium log and snapshots on build machine");
        }

       ////[Test]
       // public void ObTest()
       // {
       //     ExecutionManager em = new ExecutionManager();
       //     bool result = em.ExecuteOneBatch("5");
       //     Assert.AreEqual(true,result);
       // }
    }
}
