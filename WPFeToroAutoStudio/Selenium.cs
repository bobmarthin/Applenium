﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using Applenium.DataSetAutoTestTableAdapters;
using AutomaticTest;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using TradeAPITypes.Data;
using WebAPI;

namespace Applenium
{

    /// <summary>
    ///     Class for dealing with Selenium functions
    /// </summary>
    public class Selenium
    {

        /// <summary>
        ///     this value to save value between tests
        /// </summary>
        public static Dictionary<string, string> _lastCreatedValue = new Dictionary<string, string>();


        private static string _lastFailureMessage;
        private static bool _ismobile = false;

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
            {

                _lastCreatedValue[name] = value;

            }
            else
            {
                _lastCreatedValue.Add(name, value);

            }
            Logger.Done(string.Format("NewValueCreated:{0}={1}", name, value));
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
        /// Get sourcepage
        /// </summary>
        /// <param name="driver"></param>
        /// <returns></returns>
        public string SourcePage(RemoteWebDriver driver)
        {
            return driver.PageSource;
        }
        /// <summary>
        ///     Take snapshot when error accured
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="folderName"></param>
        /// <returns></returns>
        public string ScreenShot(RemoteWebDriver driver, string folderName)
        {
            string pngfile;
            try
            {
                Screenshot ss;
                // Create Screenshot folder
                string createdFolderLocation = folderName;

                // Take the screenshot     
                try
                {
                    driver.Manage().Window.Maximize();
                }
                catch (Exception e)
                {
                    Logger.Failed("Error taking picture" + e.Message);
                }
                if (driver.GetType().Name.IndexOf("RemoteWebDriver", StringComparison.Ordinal) >= 0)
                {
                    ss = ((ScreenShotRemoteWebDriver)driver).GetScreenshot();
                }
                else
                {
                    ss = ((ITakesScreenshot)driver).GetScreenshot();
                }


                string time =
                    DateTime.Now.ToString(CultureInfo.InvariantCulture)
                            .Replace(":", "_")
                            .Replace("/", "_")
                            .Replace(" ", "_") +
                    DateTime.Now.Millisecond.ToString();
                // Save the screenshot
                ss.SaveAsFile((string.Format("{0}\\{1}", createdFolderLocation, time + ".png")), ImageFormat.Png);
                pngfile = string.Format("{0}\\{1}", createdFolderLocation, time + ".png");
            }
            catch (Exception)
            {
                pngfile = "can't take picture";
            }
            return pngfile;
        }

        /// <summary>
        ///     Take snapshot when error accured
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="folderName"></param>
        /// <returns></returns>
        public string HtmlShot(RemoteWebDriver driver, string folderName)
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

        private bool SwitchTowindow(RemoteWebDriver driver, string windowname)
        {
            bool result = false;
            Thread.Sleep(1000);


            IList<string> windows = driver.WindowHandles;

            foreach (String window in windows)
            {
                driver.SwitchTo().Window(window);
                if (driver.Title.Equals(windowname, StringComparison.Ordinal))
                {
                    result = true;
                    break;
                }

            }

            return result;
        }

        internal object GetWebElement(int guiMapTagTypeId, string guiMapTagTypeValue, string inputTableValue)
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

        private static string GetDbElement(string guiMapTagTypeValue, string inputTableValue)
        {

            if (guiMapTagTypeValue.IndexOf(Constants.RegularExpressionAnyValue, 0, StringComparison.Ordinal) >= 0)
            {
                guiMapTagTypeValue = guiMapTagTypeValue.Replace(Constants.RegularExpressionAnyValue, inputTableValue.ToString(CultureInfo.InvariantCulture));
            }

            return guiMapTagTypeValue;
        }

        /// <summary>
        ///     Drow borfer around web element (higlihgt it )
        /// </summary>
        public bool DrawBorder(int guiMapTagTypeId, string guiMapTagTypeValue, RemoteWebDriver driver)
        {
            bool result = true;
            try
            {
                // draws a border around WebElement
                var webelement = (By)GetWebElement(guiMapTagTypeId, guiMapTagTypeValue, null);
                IWebElement element = driver.FindElement(webelement);

                var js = (IJavaScriptExecutor)driver;
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
        private bool ExecuteJavaScript(string inputJava, RemoteWebDriver driver)
        {
            var js = (IJavaScriptExecutor)driver;
            object result = js.ExecuteScript(inputJava);
            return result != null;
        }


        private bool Waituntilfindelement(By webelement, int waitElementDisplayedTime, RemoteWebDriver driver)
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

        internal bool CheckIfElementFind(IWebElement parentElement, By webelement, RemoteWebDriver driver)
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

        private bool WaituntilelementDisplayed(By webelement, int waitElementDisplayedTime, RemoteWebDriver driver,
                                               bool isRefresh)
        {
            bool result = false;
            if (Waituntilfindelement(webelement, waitElementDisplayedTime, driver))
            {
                int i = 0;

                while (i < waitElementDisplayedTime)
                {
                    try
                    {
                        result = driver.FindElement(webelement).Displayed;
                        if (result)
                            break;
                        i++;
                        Thread.Sleep(1000);
                        if (isRefresh)
                            driver.Navigate().Refresh();
                    }
                    catch (StaleElementReferenceException)
                    {
                        //Logger.Error("Stale"+e.Message);
                    }
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
        public bool ExecuteOneStep(DataRow dr, string inputDataColumn, string inputTableValue, RemoteWebDriver driver)
        {
            int guiMapCommandId = -1;
            string guiMapTagTypeValue = string.Empty;
            var jp = new JsonParser();
            By webelement = By.XPath("null");

            var guimapadapter = new GuiMapTableAdapter();
            Sql sql = new Sql();

            bool result = true;
            int waitElementDisplayed = Convert.ToInt32(jp.ReadJson("WaitElementDisplayed"));
            int waitElementExists = Convert.ToInt32(jp.ReadJson("WaitElementExists"));
            int expandedWaitFindElement = Convert.ToInt32(jp.ReadJson("ExpandedWaitFindElement"));
            int delayBetweenCommands = Convert.ToInt32(jp.ReadJson("DelayBetweenCommands"));
            if (inputTableValue.IndexOf(Constants.RegularExpressionTestingEnvironment, 0, StringComparison.Ordinal) >= 0)
            {
                inputTableValue = inputTableValue.Replace(Constants.RegularExpressionTestingEnvironment, jp.ReadJson("TestingEnvironment"));
            }

            else if (inputTableValue.IndexOf(Constants.RegularExpressionTestingEnvironmentBackend, 0, StringComparison.Ordinal) >= 0)
            {
                inputTableValue = inputTableValue.Replace(Constants.RegularExpressionTestingEnvironmentBackend, jp.ReadJson("TestingEnvironmentBack"));

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
                        webelement = (By)GetWebElement(guiMapTagTypeId, guiMapTagTypeValue, inputTableValue);
                    }
                }
                if (webelement != null)
                {
                    Thread.Sleep(delayBetweenCommands);

                    string conStringQuery;
                    string dbResult;
                    string[] conStringQuerySplit;
                    switch (guiMapCommandId)
                    {
                        case 1: //"click":
                            bool loaded = isPageLoaded(driver);
                            if (loaded)
                            {

                                int attempts = 0;
                                while (attempts < 2)
                                {
                                    try
                                    {
                                        driver.FindElement(webelement).Click();
                                        Logger.Done(webelement + " clicked");
                                        break;
                                    }

                                    catch (StaleElementReferenceException)
                                    {

                                    }
                                    attempts++;
                                }
                            } break;

                        case 2: //"sendkey":
                            driver.FindElement(webelement).Clear();
                            driver.FindElement(webelement).SendKeys(inputTableValue);
                            Logger.Done(webelement + " Sentkey with " + inputTableValue);

                            break;
                        case 3: //navigate

                            driver.Navigate().GoToUrl(inputTableValue);
                            isPageLoaded(driver);
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

                            result = WaituntilelementDisplayed(webelement, waitElementDisplayed, driver, false);
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
                            if (!_ismobile)
                                ExecuteJavaScript(inputTableValue, driver);
                            else
                            {
                                string[] script = inputTableValue.Split(',');
                                Dictionary<string, string> paramdict = new Dictionary<string, string>();
                                for (int i = 1; i < script.Length; i++)
                                {
                                    string[] paramarr = script[i].Split(':');
                                    paramdict.Add(paramarr[0].Trim(), paramarr[1].Trim());
                                }
                                driver.ExecuteScript(script[0], paramdict);
                            }
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
                            string currentWindow = null;
                            try
                            {
                                currentWindow = driver.CurrentWindowHandle;
                            }
                            catch (Exception e)
                            {
                                Logger.Error(e.Message);
                                result = false;
                            }

                            var availableWindows = new List<string>(driver.WindowHandles);
                            if (inputTableValue.Contains("switch"))
                            {

                                foreach (string w in availableWindows)
                                {
                                    if (w != currentWindow)
                                    {
                                        driver.SwitchTo().Window(w);
                                        return true;
                                    }
                                    else if (currentWindow != null) driver.SwitchTo().Window(currentWindow);

                                }
                            }

                            else
                            {
                                result = SwitchTowindow(driver, inputTableValue);
                            }
                            break;
                        case 21: //validateOpenTrades

                            string usernameorig = inputTableValue;
                            var openbook = new OpenBookValidator(true);
                            // get positions from UI
                            List<OpenBookPositionData> openbookpositiondata = openbook.GetOpenBookPositionData(driver, guiMapId);
                            // Compare positions from UI to DB
                            //string eranstring = ConfigurationManager.AppSettings["ConnectionString"];
                            OpenBookValidator.ValidationResponse validationResponseresult = openbook.ValidateClientOpenBookOpenTrades(usernameorig, openbookpositiondata, 888,
                                                                               888, "OpenbookPositionData", 888,
                                                                                "OB_OpenTrades");
                            result = validationResponseresult.Successful;
                            LastFailureMessage = validationResponseresult.Information;
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
                            result = CreateUser(guiMapId, inputTableValue);

                            break;

                        case 28://refresh page 
                            driver.Navigate().Refresh();
                            break;
                        case 29://runhedge
                            string automaticHedgerPath = jp.ReadJson("AutomaticHedgerPath");
                            Process newProcess = Process.Start(automaticHedgerPath);
                            Thread.Sleep(10000);
                            if (newProcess != null)
                                newProcess.Kill();

                            break;
                        case 30://sleep
                            sleep(guiMapId);
                            break;


                        case 31://ComapreDB to savedText or value in the input table
                            // connect string + query split

                            conStringQuery = guimapadapter.GetTagTypeValue(guiMapId);
                            conStringQuerySplit = conStringQuery.Split('!');


                            dbResult = sql.GetDbSingleValue(conStringQuerySplit[0], conStringQuerySplit[1]);
                            if (String.CompareOrdinal(dbResult, inputTableValue) != 0)
                            {
                                result = false;
                                LocalLogFailure(
                                    "The Expected text is wrong actual: " + dbResult +
                                    " vs expected :" + inputTableValue, null, Constants.Failed);
                            }

                            Logger.Done(conStringQuerySplit[1] + " compared text to " + inputTableValue);
                            break;
                        case 32://SaveElementText to a dictionary. Value can be retrieved with GetLastCreatrdValue method.
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

                            conStringQuery = guimapadapter.GetTagTypeValue(guiMapId);
                            conStringQuery = GetDbElement(conStringQuery, inputTableValue);
                            conStringQuerySplit = conStringQuery.Split('!');


                            dbResult = sql.GetDbSingleValue(conStringQuerySplit[0], conStringQuerySplit[1]);
                            if (String.CompareOrdinal(dbResult, GetLastCreatrdValue("memmory")) != 0)
                            {
                                result = false;
                                LocalLogFailure(
                                    "The Expected text is wrong. Actual: " + dbResult +
                                    " vs expected: " + GetLastCreatrdValue("memmory"), null, Constants.Failed);
                            }

                            Logger.Done(conStringQuerySplit[1] + " compared text to " + inputTableValue);
                            break;
                        case 1038://Comapre text property of an element to formerly saved value in memmory
                            // connect string + query split
                            // var guimapadapter1038 = new GuiMapTableAdapter();
                            // string ConString_Query1038 = guimapadapter1038.GetTagTypeValue(guiMapId);
                            // ConString_Query1038 = GetDBElement(ConString_Query1038, inputTableValue);
                            //string[] ConString_QuerySplit1038 = ConString_Query1038.Split('!');

                            //var sql1038 = new Sql();
                            // string DBResult1038 = sql1038.GetDBSingleValue(ConString_QuerySplit1038[0], ConString_QuerySplit1038[1]);

                            //string text = driver.FindElement(webelement).Text;
                            string text = inputTableValue;
                            if (String.CompareOrdinal(text, GetLastCreatrdValue("memmory")) != 0)
                            {
                                result = false;
                                LocalLogFailure(
                                    "The Expected text is wrong. actual: " + text +
                                    " vs expected: " + GetLastCreatrdValue("memmory"), null, Constants.Failed);
                            }

                            Logger.Done(text + " was compared text to " + inputTableValue);
                            break;

                        case 37: //"compare text not equal
                            if (String.CompareOrdinal(driver.FindElement(webelement).Text, inputTableValue) == 0)
                            {
                                result = false;
                                LocalLogFailure(
                                    "The Expected text is wrong (Shouldn't be equal). actual is: " + driver.FindElement(webelement).Text +
                                    " vs expected :" + inputTableValue, null, Constants.Failed);

                            }

                            break;
                        case 1041://bring mobile application activity up
                            driver.ExecuteScript("mobile: reset");
                            break;
                        case 1042://back button
                            driver.Navigate().Back();
                            break;
                        case 1039: //"compare value equals
                            if (String.CompareOrdinal(driver.FindElement(webelement).GetAttribute("value"), inputTableValue) != 0)
                            {
                                result = false;
                                LocalLogFailure(
                                    "The Expected value is wrong (Shouldn't be equal). actual is: " + driver.FindElement(webelement).GetAttribute("value") +
                                    " vs expected :" + inputTableValue, null, Constants.Failed);

                            }
                            break;

                        default:
                            LocalLogFailure("Can't run this command:" + guiMapTagTypeValue.Trim(), null,
                                            Constants.Failed);
                            break;

                        case 1043: // compare if text (2 strings) start with the same substring
                            if (!driver.FindElement(webelement).Text.StartsWith(inputTableValue))
                            {
                                result = false;
                                LocalLogFailure(
                                    "The Expected text is wrong actual: " + driver.FindElement(webelement).Text +
                                    " vs expected :" + inputTableValue, null, Constants.Failed);
                            }

                            Logger.Done(webelement + " compared text to " + inputTableValue);
                            break;
                        case 1044://Get Alert Text and svae to memory 
                            SetLastCreatedValue("memmory", driver.SwitchTo().Alert().Text);

                            break;

                        case 1045: // Positive test for streams API
                            try
                            {
                                StreamScenario ss = new StreamScenario();
                                result = ss.Scenario(inputTableValue, true);
                            }

                            catch (Exception)
                            {
                                Logger.Failed("Failed to complete Streams scenario");
                            }
                            break;

                        case 1046: // Get Status code from url
                            try
                            {
                                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(inputTableValue);
                                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                                HttpStatusCode code = response.StatusCode;

                                if (code.ToString() == "OK")
                                {
                                    Logger.Done("Navigated to " + inputTableValue);
                                    result = true;
                                }
                                else
                                {
                                    result = false;
                                    LocalLogFailure("Status code is " + code.ToString(), null, Constants.Failed);
                                }
                            }

                            catch (Exception ex)
                            {
                                Logger.Failed("Failed to complete scenario" + ex.Message);
                                result = false;
                            }
                            break;

                        case 1048: // Negative test for streams API Block
                            try
                            {
                                StreamScenario ss = new StreamScenario();
                                result = ss.Scenario(inputTableValue, false);
                            }

                            catch (Exception)
                            {
                                Logger.Failed("Failed to complete Streams scenario");
                                result = false;
                            }
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
                Logger.Passed(guiMapCommandId + ": TestStep=" + guiMapTagTypeValue.Trim());
            else
                Logger.Failed(guiMapCommandId + ": TestStep=" + guiMapTagTypeValue.Trim());
            return result;
        }

        private void sleep(int guiMapId)
        {
            var guimapadapter = new GuiMapTableAdapter();
            int sleeptime = Convert.ToInt32(guimapadapter.GetTagTypeValue(guiMapId));
            Thread.Sleep(sleeptime * 1000);
        }

        private bool CreateUser(int guiMapId, string input)
        {
            try
            {
                string facebookAppSecret = ConfigurationManager.AppSettings["FacebookAppSecret"];
                string facebookAppId = ConfigurationManager.AppSettings["FacebookAppId"];
                bool result = true;
                var ctu = new CreateTestUser();
                var guimapadapter = new GuiMapTableAdapter();

                FacebookTestUser ftu = new FacebookTestUser();
                FacebookTestUserResultModel fturm;

                string usertype = guimapadapter.GetTagTypeValue(guiMapId).Trim();
                switch (usertype)
                {
                    case (Constants.FacebookTestUser):
                        {

                            fturm = ftu.FacebookTestUserRequest(facebookAppId, facebookAppSecret,
                                                                true);


                            //add user details to current test add value to inmemory db
                            //save value to dictionary.
                            if (fturm != null)
                            {
                                SetLastCreatedValue("FBID", fturm.id);
                                SetLastCreatedValue("FBEmail", fturm.email);
                                SetLastCreatedValue("FBPassword", fturm.password);

                            }
                            else
                            {
                                result = false;
                            }
                            break;
                        }
                    case (Constants.FacebookTestUserFriends):
                        {

                            int friendnumber = Convert.ToInt32(input);
                            var arrfacebookfriends =
                                new FacebookTestUserResultModel[friendnumber];

                            //get existing users get twice because some user are corrupted 

                            FacebookTestUserListResultModel listOfExistUsers = ftu.FacebookTestUserListRequest(facebookAppId, facebookAppSecret, friendnumber * 2);
                            int i;
                            int count = 0;
                            for (i = 0; (i <= listOfExistUsers.data.Count - 1) && (count <= friendnumber - 1); i++)
                            {
                                arrfacebookfriends[count] = new FacebookTestUserResultModel();
                                if ((listOfExistUsers.data[i].id != null) &&
                                    (listOfExistUsers.data[i].login_url != null) &&
                                    (listOfExistUsers.data[i].access_token != null))
                                {
                                    arrfacebookfriends[count].id = listOfExistUsers.data[i].id;
                                    arrfacebookfriends[count].login_url = listOfExistUsers.data[i].login_url;
                                    arrfacebookfriends[count].access_token = listOfExistUsers.data[i].access_token;
                                    count++;
                                }
                            }
                            if (count < friendnumber)
                            {

                                for (; count < friendnumber - 1; count++)
                                {
                                    arrfacebookfriends[count] = ftu.FacebookTestUserRequest(facebookAppId, facebookAppSecret, true);
                                }
                            }

                            //user
                            var testuser = ftu.FacebookTestUserRequest(facebookAppId,
                                                                       facebookAppSecret, true);

                            for (i = 0; i < friendnumber - 1; i++)
                            {

                                result = ftu.FacebookTestUserFriendRequest(testuser.id, arrfacebookfriends[i].id,
                                                                           testuser.access_token,
                                                                           arrfacebookfriends[i].access_token);
                                if (!result)
                                    break;
                            }

                            //connect all users to new user 

                            //add user details to current test add value to inmemory db
                            //save value to dictionary.
                            if (testuser != null)
                            {
                                SetLastCreatedValue("FBID", testuser.id);
                                SetLastCreatedValue("FBPassword", testuser.password);
                                SetLastCreatedValue("FBEmail", testuser.email);
                            }
                            else
                            {
                                result = false;
                            }
                            break;
                        }
                    default:
                        {
                            NewUser newUser = ctu.CreateUser(usertype);
                            if (newUser != null)
                            {
                                SetLastCreatedValue("UserName", newUser.UserName);
                                SetLastCreatedValue("Password", newUser.Password);
                                SetLastCreatedValue("CID", newUser.Real_CID.ToString());

                            }
                            else
                                result = false;

                            break;
                        }
                }

                return result;
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message);
                return false;
            }


        }


        internal string GetCleanRate(String rateString)
        {
            string trimmed = rateString.Trim(new char[] { '$' });
            return trimmed;
        }

        /// <summary>
        ///     set driver with browser
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="browserId"></param>
        /// <param name="islocal"></param>
        /// <returns></returns>
        public RemoteWebDriver SetWebDriverBrowser(RemoteWebDriver driver, string browserId, bool islocal)
        {
            try
            {

                _ismobile = false;
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
                            ChromeOptions croptions = new ChromeOptions();
                            driver = new ChromeDriver(ChromeDriverService.CreateDefaultService(), croptions, TimeSpan.FromSeconds(120));
                            break;
                        case "2":
                        case "firefox":
                            var profile = new FirefoxProfile { EnableNativeEvents = true };
                            driver = new FirefoxDriver(profile);
                            //driver = new FirefoxDriver();
                            break;
                        case "3":
                        case "IE":
                            driver = new InternetExplorerDriver();
                            break;
                        case "4":
                        case "android":
                            _ismobile = true;
                            //run emulator 
                            string emulatorExe = jp.ReadJson("AndroidEmulatorExe");
                            string emulatorName = jp.ReadJson("AndroidEmulatorDefaultName");
                            Process newProcess;
                            if (emulatorName != string.Empty)
                            {
                                newProcess = Process.Start(emulatorExe, "-avd " + emulatorName);
                                Thread.Sleep(90000);
                            }


                            capability = new DesiredCapabilities();
                            //capability.SetCapability("app-package", "com.etoro.mobileclient");
                            string appPackage = jp.ReadJson("app-package");
                            capability.SetCapability("app-package", appPackage);
                            capability.SetCapability("browserName", "");
                            capability.SetCapability("device", "Android");
                            string appActivity = jp.ReadJson("app-activity");
                            //capability.SetCapability("app-activity", "com.etoro.mobileclient.Views.Login");
                            capability.SetCapability("app-activity", appActivity);
                            //capability.SetCapability("takesScreenshot", true);
                            //caps.SetCapability("version", "4.3.0");
                            capability.SetCapability("device ID", "uniquedeviceid");
                            //caps.SetCapability("app", @"C:\Temp\version 1.0.70-Maxim.apk");
                            string app = jp.ReadJson("app");
                            if (app != string.Empty)
                            {

                                app = app.Replace(@"\\", @"\");
                                capability.SetCapability("app", app);
                            }
                            //run cmd 
                            string appium = jp.ReadJson("Appium");
                            newProcess = Process.Start(appium);
                            Thread.Sleep(10000);
                            driver = new ScreenShotRemoteWebDriver(new Uri("http://localhost:4723/wd/hub/"), capability, TimeSpan.FromSeconds(120));


                            break;
                        case "5":
                        case "ios":
                            _ismobile = true;
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
                            capability = DesiredCapabilities.Firefox();
                            break;
                        case "3":
                            capability = DesiredCapabilities.InternetExplorer();
                            break;
                        case "4":
                            _ismobile = true;
                            capability = new DesiredCapabilities();
                            //capability.SetCapability("app-package", "com.etoro.mobileclient");
                            string appPackage = jp.ReadJson("app-package");
                            capability.SetCapability("app-package", appPackage);
                            capability.SetCapability("browserName", "");
                            capability.SetCapability("device", "Android");
                            string appActivity = jp.ReadJson("app-activity");
                            //capability.SetCapability("app-activity", "com.etoro.mobileclient.Views.Login");
                            capability.SetCapability("app-activity", appActivity);
                            string appWaitActivity = jp.ReadJson("app-wait-activity");
                            //capability.SetCapability("app-activity", "com.etoro.mobileclient.Views.Login");
                            capability.SetCapability("app-activity", appWaitActivity);
                            //capability.SetCapability("takesScreenshot", true);
                            //caps.SetCapability("version", "4.3.0");
                            capability.SetCapability("device ID", "uniquedeviceid");
                            //caps.SetCapability("app", @"C:\Temp\version 1.0.70-Maxim.apk");
                            string app = jp.ReadJson("app");
                            if (app != string.Empty)
                            {

                                app = app.Replace(@"\\", @"\");
                                capability.SetCapability("app", app);
                            }

                            //driver = new RemoteWebDriver(new Uri("http://localhost:4723/wd/hub/"), capability);
                            break;
                    }
                    string hubAddress = jp.ReadJson("HubAddress");
                    var uri = new Uri("http://" + hubAddress + "/wd/hub");

                    capability.SetCapability(CapabilityType.TakesScreenshot, true);
                    capability.IsJavaScriptEnabled = true;
                    //capability.BrowserName= setBrowserName(browser);
                    try
                    {
                        driver = new ScreenShotRemoteWebDriver(uri, capability, TimeSpan.FromSeconds(120));
                        driver.Manage().Window.Maximize();

                    }
                    catch (Exception)
                    {
                        return driver;
                    }
                    //driver = new RemoteWebDriver(uri, capability);
                }
                if (!_ismobile)
                {
                    driver.Manage().Window.Maximize();
                    var js = (IJavaScriptExecutor)driver;
                    string defaulturl = jp.ReadJson("DefaultURL");
                    defaulturl = defaulturl.Replace(@"\", @"\\");
                    if ((driver != null) && (defaulturl != string.Empty))
                        driver.Navigate().GoToUrl(defaulturl);

                    string onboarding = jp.ReadJson("RunJavaScriptOnPageLoad");

                    if ((driver != null) && (!string.IsNullOrEmpty(onboarding)))
                    {
                        js.ExecuteScript(onboarding);
                    }
                }
                return driver;
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message);
                return driver;
            }
        }

        private bool isPageLoaded(RemoteWebDriver driver)
        {
            bool result;


            if (_ismobile == false)
            {
                JsonParser jp = new JsonParser();
                String extendedTime = jp.ReadJson("ExpandedWaitFindElement");

                IWait<IWebDriver> wait = new OpenQA.Selenium.Support.UI.WebDriverWait(driver,
                                                                                      TimeSpan.FromSeconds(
                                                                                          Convert.ToInt32(extendedTime)));
                result =
                    wait.Until(
                        driver1 =>
                        ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));

            }
            else
            {
                result = true;
            }
            return result;
        }

    }
}