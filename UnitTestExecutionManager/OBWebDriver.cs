using System;
using Applenium._3___DAL.DataSetAutoTestTableAdapters;
using NUnit.Framework;
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
            int runExecutionId = (Int32)(DateTime.Now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
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
