using System;
using NUnit.Framework;
using Applenium;
using ExecutionManager = Applenium.ExecutionManager;

namespace UnitTestExecutionManager
{
    [TestFixture]
    public class ObWebDriver
    {
       [Test]
        public void ObRegression()
        {
          
            ExecutionManager em=new ExecutionManager();
            bool result=em.ExecuteOneBatch("1");
            Assert.AreEqual(true, result);
        }

       // [Test]
        public void ObTest()
        {
            ExecutionManager em = new ExecutionManager();
            bool result = em.ExecuteOneBatch("5");

            Assert.AreEqual(true,result);

        }
    }
}
