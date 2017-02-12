using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace Applenium
{
    [TestFixture]
    class BussinesLogicUnitests
    {
        [Test]
        public void SetWebDriverBrowser()
        {
            var sel = new Selenium();
            var driver = sel.SetWebDriverBrowser(null,"1",true );
            Assert.AreEqual("chrome", driver.Capabilities.BrowserName);
            driver.Close();
            driver.Dispose();
        }

        [Test]
        public void SetWebDriverBrowserIE()
        {
            var sel = new Selenium();
            var driver = sel.SetWebDriverBrowser(null, "IE", true);
            Assert.AreEqual("InternetExplorer", driver.Capabilities.BrowserName);
            driver.Close();
            driver.Dispose();
        }

        [Test]
        public void GetWebElement()
        {
            var sel = new Selenium();
             var findelement = new object();
            var webelement = sel.GetWebElement(1, "1", "");
             findelement = By.CssSelector("1");

             Assert.AreEqual(findelement, webelement);
        }

    }
}
