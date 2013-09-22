using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using Applenium.DataSetAutoTestTableAdapters;
using Applenium;

namespace Applenium
{
    /// <summary>
    /// Implement ScreenShotRemoteWebDriver
    /// </summary>
    public class ScreenShotRemoteWebDriver : RemoteWebDriver, ITakesScreenshot 
    {
        /// <summary>
        /// Override ScreenShotRemoteWebDriver
        /// </summary>
        /// <param name="remoteAddress"></param>
        /// <param name="desiredCapabilities"></param>
        public ScreenShotRemoteWebDriver(Uri remoteAddress, ICapabilities desiredCapabilities) : base(remoteAddress, desiredCapabilities)
        {

        }

        /// <summary> 
         /// Gets a <see cref="Screenshot"/> object representing the image of the page on the screen. 
         /// </summary> 
         /// <returns>A <see cref="Screenshot"/> object containing the image.</returns> 
         public Screenshot GetScreenshot() 
         { 
             // Get the screenshot as base64. 
             Response screenshotResponse = Execute(DriverCommand.Screenshot, null); 
             string base64 = screenshotResponse.Value.ToString(); 
 
            // ... and convert it. 
             return new Screenshot(base64); 
         } 
 

    } 
    /// <summary>
    ///     Class for dealing with Selenium functions
    /// </summary>
    public class Selenium
    {
        #region Instance Variables

        //private readonly IWebDriver _driver;

        #endregion

        #region Constructor

        #endregion

        /// <summary>
        ///     this value to save value between tests
        /// </summary>
        public static Dictionary<string, string> _lastCreatedValue = new Dictionary<string, string>();


        private static string _lastFailureMessage;

        /// <summary>
        ///     FailuerMessage to read in other class
        /// </summary>
        public string LastFailureMessage
        {
            get { return _lastFailureMessage; }
            set { _lastFailureMessage = value; }
        }

        /// <summary>
        ///     FailuerMessage to read in other class
        /// </summary>
        private static void SetLastCreatedValue(string name, string value)
        {
            if (_lastCreatedValue.ContainsKey(name))
                _lastCreatedValue[name] = value;
            else
                _lastCreatedValue.Add(name, value);
        }

        /// <summary>
        ///     Get Last Created global value
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetLastCreatrdValue(string name)
        {
            if (_lastCreatedValue.ContainsKey(name))
            {
                return _lastCreatedValue[name];
            }
            return string.Empty;
        }

        /// <summary>
        ///     Take snapshot when error accured
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="folderName"></param>
        /// <returns></returns>
        public string ScreenShot(IWebDriver driver, string folderName)
        {
            Screenshot ss;
            // Create Screenshot folder
            string createdFolderLocation = folderName;

            // Take the screenshot     
            driver.Manage().Window.Maximize();
     
            if (driver.GetType().Name.IndexOf("RemoteWebDriver", StringComparison.Ordinal)>=0)
            {
                ss = ((ScreenShotRemoteWebDriver)driver).GetScreenshot();  
            }
            else
            {
                ss = ((ITakesScreenshot)driver).GetScreenshot();
            }
                
           
            string time = DateTime.Now.ToString(CultureInfo.InvariantCulture).Replace(":", "_").Replace("/", "_").Replace(" ", "_") +
                          DateTime.Now.Millisecond.ToString();
            // Save the screenshot
            ss.SaveAsFile((string.Format("{0}\\{1}", createdFolderLocation, time + ".png")), ImageFormat.Png);
            return string.Format("{0}\\{1}", createdFolderLocation, time + ".png");
        }

        /// <summary>
        ///     Take snapshot when error accured
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="folderName"></param>
        /// <returns></returns>
        public string HTMLShot(IWebDriver driver, string folderName)
        {
            
            // Create Screenshot folder
            string createdFolderLocation = folderName;

            // Take the screenshot     
            
            Screenshot ss = ((ITakesScreenshot)driver).GetScreenshot();
            string screenshot = ss.AsBase64EncodedString;
            byte[] screenshotAsByteArray = ss.AsByteArray;
            string time = DateTime.Now.ToString().Replace(":", "_").Replace("/", "_").Replace(" ", "_") +
                          DateTime.Now.Millisecond.ToString();

            // Save the screenshot
            ss.SaveAsFile((string.Format("{0}\\{1}", createdFolderLocation, time + ".png")), ImageFormat.Png);
            return string.Format("{0}\\{1}", createdFolderLocation, time + ".png");
        }

        private static void LocalLogFailure(string logmessage, Exception exception, int failedOrError)
        {
            _lastFailureMessage = logmessage; //("Can't run this command:" + guiMapTagTypeValue.Trim());
            if (failedOrError == Constants.Failed) //failed
                Logger.Failed(logmessage);
            if (failedOrError == Constants.Error)
                Logger.Error(logmessage, exception);
        }

        private void SwitchTowindow(IWebDriver driver, string windowname)
        {
            Thread.Sleep(1000);

            IList<string> windows = driver.WindowHandles;

            foreach (String window in windows)
            {
                driver.SwitchTo().Window(window);
                if (driver.Title.IndexOf(windowname, StringComparison.Ordinal) >= 0)
                    break;
            }
        }

        private static object GetWebElement(int guiMapTagTypeId, string guiMapTagTypeValue, string inputTableValue)
        {
            var findelement = new object();


            if (guiMapTagTypeValue.IndexOf(Constants.RegularExpressionAnyValue, 0, StringComparison.Ordinal) >= 0)
            {
                guiMapTagTypeValue = guiMapTagTypeValue.Replace(Constants.RegularExpressionAnyValue,
                                                                inputTableValue.ToString(CultureInfo.InvariantCulture));
            }
            switch (guiMapTagTypeId)
            {
                case 1:
                    findelement = By.CssSelector(guiMapTagTypeValue);
                    break;
                case 2:
                    findelement = By.Name(guiMapTagTypeValue);
                    break;
                case 4:
                    findelement = By.ClassName(guiMapTagTypeValue);
                    break;
                case 5:
                    findelement = By.XPath(guiMapTagTypeValue);
                    break;
                case 6:
                    findelement = By.Id(guiMapTagTypeValue);
                    break;
                case 7:
                    findelement = By.LinkText(guiMapTagTypeValue);
                    break;
                case 8:
                    findelement = By.TagName(guiMapTagTypeValue);
                    break;
                default:
                    LocalLogFailure("Can't run this command:" + guiMapTagTypeValue, null, Constants.Error);
                        //3 for error
                    break;
            }

            return findelement;
        }

        private static string GetDBElement(string guiMapTagTypeValue, string inputTableValue)
        {
           


            if (guiMapTagTypeValue.IndexOf(Constants.RegularExpressionAnyValue, 0, StringComparison.Ordinal) >= 0)
            {
                guiMapTagTypeValue = guiMapTagTypeValue.Replace(Constants.RegularExpressionAnyValue,inputTableValue.ToString(CultureInfo.InvariantCulture));
            }

            return guiMapTagTypeValue;
        }

        /// <summary>
        ///     Drow borfer around web element (higlihgt it )
        /// </summary>
        public bool DrawBorder(int guiMapTagTypeId, string guiMapTagTypeValue, IWebDriver driver)
        {
            bool result = true;
            try
            {
                // draws a border around WebElement
                var webelement = (By) GetWebElement(guiMapTagTypeId, guiMapTagTypeValue, null);
                IWebElement element = driver.FindElement(webelement);

                var js = (IJavaScriptExecutor) driver;
                for (int i = 1; i <= 3; i++)
                {
                    js.ExecuteScript("arguments[0].style.border='3px solid red'", element);
                    Thread.Sleep(1000);
                    js.ExecuteScript("arguments[0].style.border='none'", element);
                }
            }
            catch (Exception exception)
            {
                LocalLogFailure(exception.Message, exception, Constants.Error);
                result = false;
            }

            return result;
        }

        /// <summary>
        ///     Execute any java script
        /// </summary>
        private bool ExecuteJavaScript(string inputJava, IWebDriver driver)
        {
            var js = (IJavaScriptExecutor) driver;
            object result = js.ExecuteScript(inputJava);
            return result != null;
        }


        private bool Waituntilfindelement(By webelement, int waitElementDisplayedTime, IWebDriver driver)
        {
            driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(waitElementDisplayedTime));
            IWebElement iresult = driver.FindElement(webelement);
            driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(0));
            Logger.Info(webelement + " waited to find  ");
            if (iresult != null)
                return true;
            LocalLogFailure("can't find element " + webelement + "after " + waitElementDisplayedTime + " seconds",
                            null, Constants.Failed);
            return false;
        }

        private bool CheckIfElementFind(IWebElement parentElement, By webelement, IWebDriver driver)
        {
            try
            {
                if (parentElement != null)
                    parentElement.FindElement(webelement);
                else
                {
                    driver.FindElement(webelement);
                }
            }
            catch (NoSuchElementException)
            {
                return false;
            }
            return true;
        }

        private bool WaituntilelementDisplayed(By webelement, int waitElementDisplayedTime, IWebDriver driver,
                                               bool isRefresh)
        {
            bool result = false;
            if (Waituntilfindelement(webelement, waitElementDisplayedTime, driver))
            {
                int i = 0;

                while (i < waitElementDisplayedTime)
                {
                    result = driver.FindElement(webelement).Displayed;
                    if (result)
                        break;
                    i++;
                    Thread.Sleep(1000);
                    if (isRefresh)
                        driver.Navigate().Refresh();
                }
            }

            else
                LocalLogFailure(
                    "can't find displayed element " + webelement + "after " + waitElementDisplayedTime + " seconds",
                    null, Constants.Failed);


            Logger.Info(webelement + " waited to display    ");
            return result;
        }


        /// <summary>
        ///     Execute one step generaly called from execution manger (gets vlaue from data table )
        /// </summary>
        public bool ExecuteOneStep(DataRow dr, string inputDataColumn, string inputTableValue, IWebDriver driver)
        {
            int guiMapCommandId = -1;
            string guiMapTagTypeValue = string.Empty;
            var jp = new JsonParser();
            By webelement = By.XPath("null");
           
            bool result = true;
            int waitElementDisplayed = Convert.ToInt32(jp.ReadJson("WaitElementDisplayed"));
            int waitElementExists = Convert.ToInt32(jp.ReadJson("WaitElementExists"));
            int expandedWaitFindElement = Convert.ToInt32(jp.ReadJson("ExpandedWaitFindElement"));
            int delayBetweenCommands = Convert.ToInt32(jp.ReadJson("DelayBetweenCommands"));
             if (inputTableValue.IndexOf(Constants.RegularExpressionTestingEnvironment, 0, StringComparison.Ordinal) >= 0)
            {
                inputTableValue = inputTableValue.Replace(Constants.RegularExpressionTestingEnvironment, jp.ReadJson("TestingEnvironment"));
            }
            
            try
            {
                int guiMapId;
                using (var adapterGuimap = new GuiMapTableAdapter())
                {
                    guiMapId = Convert.ToInt32(dr["GuiMapID"].ToString().Trim());
                    guiMapCommandId = Convert.ToInt32(dr["GuiMapCommandID"].ToString().Trim());
                    int guiMapTagTypeId = Convert.ToInt32(adapterGuimap.GetTagTypeID(guiMapId));
                    if (guiMapId != 0)
                    {
                        guiMapTagTypeValue = adapterGuimap.GetTagTypeValue(guiMapId).Trim();
                        webelement = (By) GetWebElement(guiMapTagTypeId, guiMapTagTypeValue, inputTableValue);
                    }
                }
                if (webelement != null)
                {
                    Thread.Sleep(delayBetweenCommands);

                    switch (guiMapCommandId)
                    {
                        case 1: //"click":
                            // clickTheLinkAndFocusOnANewWindow(_driver,webelement);
                            driver.FindElement(webelement).Click();
                            Logger.Done(webelement + " clicked");
                            break;

                        case 2: //"sendkey":
                            driver.FindElement(webelement).Clear();
                            driver.FindElement(webelement).SendKeys(inputTableValue);
                            Logger.Done(webelement + " Sentkey with " + inputTableValue);

                            break;
                        case 3: //navigate
                            driver.Navigate().GoToUrl(inputTableValue);
                            Logger.Done("navigated to " + inputTableValue);
                            break;
                        case 4: //findelementt
                            driver.FindElement(webelement);
                            Logger.Done(webelement + " Found");

                            break;
                        case 6: //"check":
                            driver.FindElement(webelement).Click();
                            break;
                        case 7: //"mouseover":
                            var builder = new Actions(driver);

                            builder.MoveToElement(driver.FindElement(webelement)).Build().Perform();

                            Thread.Sleep(1000);
                            Logger.Done(webelement + " Mouse Overed");

                            break;

                        case 8: //"waituntill-findelement":
                            result = Waituntilfindelement(webelement, waitElementExists, driver);
                            Logger.Done(webelement.ToString().Trim() + " waited");
                            break;
                        case 9: //"compare text
                            if (String.CompareOrdinal(driver.FindElement(webelement).Text, inputTableValue) != 0)
                            {
                                result = false;
                                LocalLogFailure(
                                    "The Expected text is wrong actual: " + driver.FindElement(webelement).Text +
                                    " vs expected :" + inputTableValue, null, Constants.Failed);
                            }

                            Logger.Done(webelement + " compared text to " + inputTableValue);
                            break;
                        case 10: //select drop down by Index
                            // select the drop down list////System.Windows.MessageBox
                            IWebElement dropdown = driver.FindElement(webelement);
                            //create select element object 
                            var selectElement = new SelectElement(dropdown);
                            //select by index
                            selectElement.SelectByIndex(Convert.ToInt32(inputTableValue));

                            ExecuteJavaScript(
                                "$('" + guiMapTagTypeValue + "').val('" + inputTableValue + "').change()", driver);

                            Logger.Done(webelement + " dropeddown with index   " + inputTableValue);
                            break;
                        case 11: //wait untill  displayed 

                            WaituntilelementDisplayed(webelement, waitElementDisplayed, driver, false);
                            break;
                        case 12: //verify is exists 

                            IWait<IWebDriver> wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitElementExists));
                            //IWebElement a = ExpectedConditions.ElementExists(webelement);
                            wait.Until(ExpectedConditions.ElementExists(webelement));

                            break;
                        case 13: //select drop down
                            // select the drop down list////System.Windows.MessageBox
                            dropdown = driver.FindElement(webelement);
                            if (dropdown.Displayed)
                            {
                                //create select element object 
                                selectElement = new SelectElement(dropdown);
                                selectElement.SelectByText(inputTableValue);
                                // select by text
                                //selectElement.SelecyByText("HighSchool");
                            }
                            ExecuteJavaScript(
                                "$('" + guiMapTagTypeValue + " option:selected').val('" + inputTableValue +
                                "').change()", driver);
                            Logger.Done(webelement + " dropeddown with text   " + inputTableValue);
                            break;
                        case 14: //select drop down
                            // select the drop down list////System.Windows.MessageBox
                            dropdown = driver.FindElement(webelement);
                            if (dropdown.Displayed)
                            {
                                //create select element object 
                                selectElement = new SelectElement(dropdown);
                                //select by Value
                                selectElement.SelectByValue(inputTableValue);
                            }
                            ExecuteJavaScript(
                                "$('" + guiMapTagTypeValue + "').val('" + inputTableValue + "').change()", driver);
                            Logger.Done(webelement + " dropeddown with value   " + inputTableValue);
                            break;
                        case 15: //SetlocalStorage Run JavaScript(any java script)
                            // select the drop down list////System.Windows.MessageBox

                            ExecuteJavaScript(inputTableValue, driver);
                            Logger.Done("Java script executed with   " + inputTableValue);
                            break;
                        case 16: //wait untill  find element long time expanded 
                            WaituntilelementDisplayed(webelement, expandedWaitFindElement, driver, false);
                            break;
                        case 17: //Count number of elements and Compare
                            int elementcount = driver.FindElements(webelement).Count();
                            if (elementcount != Convert.ToInt32(inputTableValue))
                            {
                                result = false;
                                LocalLogFailure(
                                    "The Expected count is wrong:" + inputTableValue + " vs actual: " + elementcount,
                                    null, Constants.Failed);
                            }

                            Logger.Done("number of elements " + webelement + "is " + elementcount + "compared to:   " +
                                        inputTableValue);
                            break;
                        case 18: //scrolldown
                            ExecuteJavaScript("window.scroll(0,document.body.scrollHeight)", driver);
                            Logger.Done("Scrolled Down");

                            break;
                        case 19: //scrollup
                            ExecuteJavaScript("window.scroll(0,0)", driver);
                            Logger.Done("Scrolled Up");
                            break;
                        case 20: //switchtowindow
                            SwitchTowindow(driver, inputTableValue);
                            break;
                        case 21: //validateOpenTrades
                           
                            break;
                        case 22: //case close current window
                            driver.Close();
                            break;
                        case 23: //close  window by name 
                            string currentwindowhandle = driver.CurrentWindowHandle;
                            SwitchTowindow(driver, inputTableValue);
                            driver.Close();
                            //back to current window 
                            driver.SwitchTo().Window(currentwindowhandle);
                            break;
                        case 24: //clear cookeis

                            driver.Manage().Cookies.DeleteAllCookies();
                            // driver.Navigate().Refresh();

                            break;
                        case 25: //wait untill  find element with refresh
                            WaituntilelementDisplayed(webelement, waitElementDisplayed, driver, true);
                            break;
                        case 26: //create new user 
                          
                            break;
                        case 27: //create commenter badge 

                            break;
                        case 28://refresh page 
                            driver.Navigate().Refresh();
                            break;
                        case 29://runexe
                            string  automaticHedgerPath= jp.ReadJson("ExePath");
                            Process newProcess=Process.Start(automaticHedgerPath);
                            Thread.Sleep(10000); 
                            if (newProcess != null)
                                newProcess.Kill();

                            break;
                        case 30://sleep
                            sleep(guiMapId);
                            break;

                        
                        case 31://ComapreDB to... savedText or value
                            // connect string + query split
                            var guimapadapter = new GuiMapTableAdapter();
                            string ConString_Query = guimapadapter.GetTagTypeValue(guiMapId);
                            string[] ConString_QuerySplit= ConString_Query.Split('!');

                            var sql = new Sql();
                            string DBResult = sql.GetDBSingleValue(ConString_QuerySplit[0], ConString_QuerySplit[1]);
                            if (String.CompareOrdinal(DBResult, inputTableValue) != 0)
                            {
                                result = false;
                                LocalLogFailure(
                                    "The Expected text is wrong actual: " + DBResult +
                                    " vs expected :" + inputTableValue, null, Constants.Failed);
                            }

                            Logger.Done(ConString_QuerySplit[1] + " compared text to " + inputTableValue);
                            break;
                        case 32://SaveElementText
                            SetLastCreatedValue(inputDataColumn, driver.FindElement(webelement).Text);
                            
                            break;
                        case 33://Accept Alert
                            driver.SwitchTo().Alert().Accept();
                            
                            
                            break;
                        case 34://Accept Dissmiss
                            driver.SwitchTo().Alert().Dismiss();
                            
                            break;
                        case 35://SaveElementText to memmory
                            SetLastCreatedValue("memmory", driver.FindElement(webelement).Text);

                            break;
                        case 36://ComapreDB to... savedText or value in memmory
                            // connect string + query split
                            var guimapadapter36 = new GuiMapTableAdapter();
                            string ConString_Query36 = guimapadapter36.GetTagTypeValue(guiMapId);
                            ConString_Query36=GetDBElement(ConString_Query36, inputTableValue);
                            string[] ConString_QuerySplit36 = ConString_Query36.Split('!');

                            var sql36 = new Sql();
                            string DBResult36 = sql36.GetDBSingleValue(ConString_QuerySplit36[0], ConString_QuerySplit36[1]);
                            if (String.CompareOrdinal(DBResult36, GetLastCreatrdValue("memmory")) != 0)
                            {
                                result = false;
                                LocalLogFailure(
                                    "The Expected text is wrong actual: " + DBResult36 +
                                    " vs expected :" + GetLastCreatrdValue("memmory"), null, Constants.Failed);
                            }

                            Logger.Done(ConString_QuerySplit36[1] + " compared text to " + inputTableValue);
                            break;

                        default:
                            LocalLogFailure("Can't run this command:" + guiMapTagTypeValue.Trim(), null,
                                            Constants.Failed);
                            break;
                    }
                }
                else
                {
                    result = false;
                    LocalLogFailure(guiMapCommandId + ": web element was not found : " + guiMapTagTypeValue.Trim(), null,
                                    Constants.Failed);
                }
            }


            catch (Exception exception)
            {
                LocalLogFailure(exception.Message, exception, Constants.Error);
                result = false;
            }


            if (result)
                Logger.Passed(guiMapCommandId + ": TestStep : " + guiMapTagTypeValue.Trim());
            else
                Logger.Failed(guiMapCommandId + ": TestStep : " + guiMapTagTypeValue.Trim());
            return result;
        }

        private void sleep(int guiMapId)
        {
            var guimapadapter = new GuiMapTableAdapter();
            int sleeptime = Convert.ToInt32(guimapadapter.GetTagTypeValue(guiMapId));
            Thread.Sleep(sleeptime*1000);
        }

        
        /// <summary>
        ///     set driver with browser
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="browserId"></param>
        /// <param name="islocal"></param>
        /// <returns></returns>
        public IWebDriver SetWebDriverBrowser(IWebDriver driver, string browserId, bool islocal)
        {
            try
            {

            
            if (driver != null)
                driver.Quit();
            var jp = new JsonParser();
            string remote = jp.ReadJson("RemoteNode");
            var capability = new DesiredCapabilities();
            if (remote == "no" || islocal)
            {
                switch (browserId)
                {
                    case "1":
                    case "chrome":
                        driver = new ChromeDriver();
                        break;
                    case "2":
                    case "firefox":
                        var profile = new FirefoxProfile {EnableNativeEvents = true};
                        driver = new FirefoxDriver(profile);
                        //driver = new FirefoxDriver();
                        break;
                    case "3":
                    case "IE":
                        driver = new InternetExplorerDriver();
                        break;
                }
            }
            else if (remote == "yes")
            {
                switch (browserId)
                {
                    case "1":
                        capability = DesiredCapabilities.Chrome();
                        break;
                    case "2":

                        var fp = new FirefoxProfile {EnableNativeEvents = true};

                        //capability.SetCapability(FirefoxDriver.ProfileCapabilityName, fp);
                        capability = DesiredCapabilities.Firefox();

                        //_driver = new FirefoxDriver(profile);
                        break;
                    case "3":
                        capability = DesiredCapabilities.InternetExplorer();

                        break;
                }
                string hubAddress = jp.ReadJson("HubAddress");
                var uri =new  Uri("http://" + hubAddress + ":4444/wd/hub");

                capability.SetCapability(CapabilityType.TakesScreenshot, true);
                capability.IsJavaScriptEnabled = true;
                //capability.BrowserName= setBrowserName(browser);
                driver = new ScreenShotRemoteWebDriver(uri, capability); 
               
                //driver = new RemoteWebDriver( uri, capability);
            }
            var js = (IJavaScriptExecutor) driver;

            string defaulturl = jp.ReadJson("DefaultURL");
            defaulturl = defaulturl.Replace(@"\", @"\\");
            if (driver != null) 
                driver.Navigate().GoToUrl(defaulturl);
            string onboarding = jp.ReadJson("LocalStorageOnboardingMode");

            if (onboarding != string.Empty)
            {
                if (js != null)
                    js.ExecuteScript("EToro.storage.localStorage.write('onboardingMode', '" + onboarding + "')");
            }
            return driver;
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message); 
                 return driver;
            }
        }
    }
}