using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
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
using System.Windows.Forms;
using Microsoft.VisualBasic;
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

        private string guiMapCommandName = string.Empty;
        private static readonly AppleniumLogger logger = new AppleniumLogger();

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
        private static ResultModel SetLastCreatedValue(string name, string value)
        {
            if (_lastCreatedValue.ContainsKey(name))
            {

                _lastCreatedValue[name] = value;

            }
            else
            {
                _lastCreatedValue.Add(name, value);

            }

            return new ResultModel(true, string.Format("NewValueCreated:{0}={1}", name, value));
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
                    LogObject exceptionLogger = new LogObject();
                    exceptionLogger.Description = "Error taking snapshot - " + e.Message;
                    exceptionLogger.CommandName = guiMapCommandName;
                    exceptionLogger.StatusTag = Constants.FAILED;
                    exceptionLogger.Exception = e;
                    logger.Print(exceptionLogger);

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

        private static void LocalLogFailure(string stepName, string logmessage, Exception exception, int failedOrError)
        {
            _lastFailureMessage = logmessage; //("Can't run this command: " + guiMapTagTypeValue.Trim());
            if (failedOrError == Constants.FAILED)
            { //failed


                LogObject exceptionLogger2 = new LogObject();
                exceptionLogger2.StatusTag = Constants.FAILED;
                exceptionLogger2.Description = logmessage;
                exceptionLogger2.StepName = stepName;
                exceptionLogger2.Exception = exception;
                logger.Print(exceptionLogger2);
            }

            else if (failedOrError == Constants.ERROR)
            {
                LogObject exceptionLogger2 = new LogObject();
                exceptionLogger2.StatusTag = Constants.ERROR;
                exceptionLogger2.Description = logmessage;
                exceptionLogger2.StepName = stepName;
                exceptionLogger2.Exception = exception;
                logger.Print(exceptionLogger2);
            }

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
                    LocalLogFailure(guiMapCommandName, "Couldn't run the command = " + guiMapCommandName + " on the element = " + guiMapTagTypeValue, null, Constants.ERROR);
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
                LocalLogFailure(guiMapCommandName, exception.Message, exception, Constants.ERROR);
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


            LogObject exceptionLogger2 = new LogObject();
            exceptionLogger2.StatusTag = Constants.PASSED;
            exceptionLogger2.Description = "Waiting to find " + webelement;
            logger.Print(exceptionLogger2);

            if (iresult != null)
                return true;
            LocalLogFailure(guiMapCommandName, "Couldn't find element - " + webelement + ". after: " + waitElementDisplayedTime + " seconds.",
                            null, Constants.FAILED);
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
                        //Logger.LogResult("Stale"+e.Message);
                    }
                }
            }

            else
                LocalLogFailure(guiMapCommandName,
                    "Couldn't find element - " + webelement + ". after: " + waitElementDisplayedTime + " seconds.",
                    null, Constants.FAILED);


            LogObject exceptionLogger2 = new LogObject();
            exceptionLogger2.StatusTag = Constants.PASSED;
            exceptionLogger2.Description = "Waited to display - " + webelement;
            exceptionLogger2.StepName = guiMapCommandName;
            logger.Print(exceptionLogger2);

            return result;
        }


        /// <summary>
        ///     Execute one step generaly called from execution manger (gets vlaue from data table )
        /// </summary>
        public ResultModel ExecuteOneStep(DataRow dr, string inputDataColumn, string inputTableValue, RemoteWebDriver driver)
        {
            int guiMapCommandId = -1;
            string guiMapTagTypeValue = string.Empty;
            var jp = new JsonParser();
            By webelement = By.XPath("null");

            var guimapadapter = new GuiMapTableAdapter();
            Sql sql = new Sql();

            bool result = true;
            ResultModel res = new ResultModel(false, "");
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
                    if (guiMapCommandId != 0)
                    {
                        var adapterCommand = new TestCommandTableAdapter();
                        guiMapCommandName = adapterCommand.GetTestCommandName(guiMapCommandId).Trim();
                    }
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
                                        logIt(guiMapCommandName, "Clicked sucessfully", Constants.DONE);
                                        res.Returnresult = true;
                                        break;

                                    }

                                    catch (Exception ex)
                                    {
                                        //Logger.LogResult("Couldn't click " + webelement, Constants.Error, ex);
                                        res.Returnresult = false;
                                        res.Message = ex.Message;
                                    }
                                    attempts++;
                                }
                            }
                            break;

                        case 2: //"sendkey":
                            try
                            {
                                driver.FindElement(webelement).Clear();
                                driver.FindElement(webelement).SendKeys(inputTableValue);
                                logIt(guiMapCommandName, "Sent keys (" + inputTableValue + ") to element - " + webelement, Constants.DONE);
                                res.Returnresult = true;

                            }
                            catch (Exception ex)
                            {
                                res.Returnresult = false;
                                res.Message = ex.Message;
                            }
                            break;
                        case 3: //navigate
                            try
                            {
                                driver.Navigate().GoToUrl(inputTableValue);
                                isPageLoaded(driver);
                                logIt(guiMapCommandName, "Navigated to " + inputTableValue, Constants.DONE);
                                res.Returnresult = true;
                            }
                            catch (Exception ex)
                            {
                                res.Returnresult = false;
                                res.Message = ex.Message;
                            }

                            break;
                        case 4: //findelementt
                            try
                            {
                                driver.FindElement(webelement);
                                logIt(guiMapCommandName, "Found -" + webelement, Constants.DONE);
                                res.Returnresult = true;
                            }

                            catch (Exception ex)
                            {
                                res.Returnresult = false;
                                res.Message = ex.Message;
                            }

                            break;
                        case 6: //"check":
                            try
                            {
                                driver.FindElement(webelement).Click();
                                logIt(guiMapCommandName, "Checked -" + webelement, Constants.DONE);
                                res.Returnresult = true;
                            }
                            catch (Exception ex)
                            {
                                res.Returnresult = false;
                                res.Message = ex.Message;
                            }
                            break;
                        case 7: //"mouseover":
                            try
                            {
                                var builder = new Actions(driver);
                                builder.MoveToElement(driver.FindElement(webelement)).Build().Perform();
                                Thread.Sleep(1000);
                                logIt(guiMapCommandName, "Mouse Over -" + webelement, Constants.DONE);
                                res.Returnresult = true;
                            }
                            catch (Exception ex)
                            {
                                res.Returnresult = false;
                                res.Message = ex.Message;
                            }
                            break;

                        case 8: //"waituntill-findelement":
                            try
                            {
                                result = Waituntilfindelement(webelement, waitElementExists, driver);
                                logIt(guiMapCommandName, "Waited to find - " + webelement.ToString().Trim(), Constants.DONE);
                                res.Returnresult = true;
                            }
                            catch (Exception ex)
                            {
                                res.Returnresult = false;
                                res.Message = ex.Message;
                            }
                            break;
                        case 9: //"compare text
                            try
                            {
                                if (String.CompareOrdinal(driver.FindElement(webelement).Text, inputTableValue) != 0)
                                {
                                    result = false;
                                    LocalLogFailure(guiMapCommandName,
                                        "The Expected text is wrong actual: " + driver.FindElement(webelement).Text +
                                        " vs expected: " + inputTableValue, null, Constants.FAILED);
                                }

                                logIt(guiMapCommandName, "Compared the text - " + webelement + " to " + inputTableValue, Constants.DONE);
                                res.Returnresult = true;
                            }
                            catch (Exception ex)
                            {
                                res.Returnresult = false;
                                res.Message = ex.Message;
                            }
                            break;
                        case 10: //select drop down by Index
                            // select the drop down list////System.Windows.MessageBox

                            IWebElement dropdown = driver.FindElement(webelement);
                            //create select element object 
                            var selectElement = new SelectElement(dropdown);
                            //select by index
                            selectElement.SelectByIndex(Convert.ToInt32(inputTableValue));
                            try
                            {
                                ExecuteJavaScript(
                                    "$('" + guiMapTagTypeValue + "').val('" + inputTableValue + "').change()", driver);

                                logIt(guiMapCommandName, "Dropdown element - " + webelement + ", with index   " + inputTableValue, Constants.DONE);
                                res.Returnresult = true;
                            }

                            catch (Exception ex)
                            {
                                res.Returnresult = false;
                                res.Message = ex.Message;
                            }

                            break;
                        case 11: //wait untill  displayed 
                            try
                            {
                                result = WaituntilelementDisplayed(webelement, waitElementDisplayed, driver, false);
                                logIt(guiMapCommandName, "Waited to display - " + webelement, Constants.DONE);

                                res.Returnresult = true;
                            }
                            catch (Exception ex)
                            {
                                res.Returnresult = false;
                                res.Message = "Waited to display element - " + webelement + ". " + ex.Message;
                            }

                            break;
                        case 12: //verify is exists 

                            IWait<IWebDriver> wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitElementExists));
                            //IWebElement a = ExpectedConditions.ElementExists(webelement);
                            try
                            {
                                wait.Until(ExpectedConditions.ElementExists(webelement));
                                logIt(guiMapCommandName, "Element exists - " + webelement, Constants.DONE);

                                res.Returnresult = true;
                            }
                            catch (Exception ex)
                            {
                                res.Returnresult = false;
                                res.Message = ex.Message;
                            }
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
                            try
                            {
                                ExecuteJavaScript(
                                    "$('" + guiMapTagTypeValue + " option:selected').val('" + inputTableValue +
                                    "').change()", driver);

                                logIt(guiMapCommandName, "Dropdown element - " + webelement + ", with index   " + inputTableValue, Constants.DONE);
                                res.Returnresult = true;
                            }
                            catch (Exception ex)
                            {
                                res.Returnresult = false;
                                res.Message = ex.Message;
                            }
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
                            try
                            {
                                ExecuteJavaScript(
                                    "$('" + guiMapTagTypeValue + "').val('" + inputTableValue + "').change()", driver);

                                logIt(guiMapCommandName, "Dropdown element - " + webelement + ", with index   " + inputTableValue, Constants.DONE);
                                res.Returnresult = true;
                            }
                            catch (Exception ex)
                            {
                                res.Returnresult = false;
                                res.Message = ex.Message;
                            }

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
                            logIt(guiMapCommandName, "Java script executed with   " + inputTableValue, Constants.DONE);

                            res.Returnresult = true;
                            break;
                        case 16: //wait untill  find element long time expanded 
                            try
                            {
                                WaituntilelementDisplayed(webelement, expandedWaitFindElement, driver, false);
                                res.Returnresult = true;
                                logIt(guiMapCommandName, "Expanded wait for element success - " + inputTableValue, Constants.DONE);

                            }
                            catch (Exception ex)
                            {
                                res.Returnresult = false;
                                res.Message = "Waited to display (ExpandedWait) element - " + webelement + ". " + ex.Message;
                            }
                            break;
                        case 17: //Count number of elements and Compare
                            int elementcount = driver.FindElements(webelement).Count();
                            if (elementcount != Convert.ToInt32(inputTableValue))
                            {
                                result = false;
                                LocalLogFailure(guiMapCommandName,
                                    "The Expected count is wrong. EXPECTED: " + inputTableValue + " vs ACTUAL(Found): " + elementcount,
                                    null, Constants.FAILED);
                                res.Returnresult = false;
                            }

                            logIt(guiMapCommandName, "Number of elements - " + webelement + " is " + elementcount + " compared to:   " +
                                    inputTableValue, Constants.DONE);

                            res.Returnresult = true;
                            break;
                        case 18: //scrolldown
                            try
                            {
                                ExecuteJavaScript("window.scroll(0,document.body.scrollHeight)", driver);
                                logIt(guiMapCommandName, "Scrolled Down", Constants.DONE);
                                res.Returnresult = true;
                            }
                            catch (Exception ex)
                            {
                                res.Returnresult = false;
                                res.Message = ex.Message;
                            }
                            break;
                        case 19: //scrollup
                            try
                            {
                                ExecuteJavaScript("window.scroll(0,0)", driver);
                                logIt(guiMapCommandName, "Scrolled Up", Constants.DONE);
                                res.Returnresult = true;
                            }
                            catch (Exception ex)
                            {
                                res.Returnresult = false;
                                res.Message = ex.Message;
                            }
                            break;
                        case 20: //switchtowindow
                            string currentWindow = null;
                            try
                            {
                                currentWindow = driver.CurrentWindowHandle;


                            }
                            catch (Exception exception)
                            {
                                res.Returnresult = false;
                                res.Message = exception.Message;
                            }

                            var availableWindows = new List<string>(driver.WindowHandles);
                            if (inputTableValue.Contains("switch"))
                            {

                                foreach (string w in availableWindows)
                                {
                                    if (w != currentWindow)
                                    {
                                        driver.SwitchTo().Window(w);
                                        res.Returnresult = true;

                                        return res;
                                    }
                                    else if (currentWindow != null) driver.SwitchTo().Window(currentWindow);

                                }
                            }

                            else
                            {
                                res.Returnresult = SwitchTowindow(driver, inputTableValue);
                            }
                            break;
                        case 21: //validateOpenTrades

                            string usernameorig = inputTableValue;
                            var openbook = new OpenBookValidator(true);
                            // get positions from UI
                            List<OpenBookPositionData> openbookpositiondata = openbook.GetOpenBookPositionData(driver,
                                guiMapId);
                            // Compare positions from UI to DB
                            //string eranstring = ConfigurationManager.AppSettings["ConnectionString"];
                            OpenBookValidator.ValidationResponse validationResponseresult =
                                openbook.ValidateClientOpenBookOpenTrades(usernameorig, openbookpositiondata, 888,
                                    888, "OpenbookPositionData", 888,
                                    "OB_OpenTrades");
                            res.Returnresult = validationResponseresult.Successful;
                            LastFailureMessage = validationResponseresult.Information;
                            break;
                        case 22: //case close current window
                            driver.Close();
                            res.Returnresult = true;
                            break;
                        case 23: //close  window by name 
                            string currentwindowhandle = driver.CurrentWindowHandle;
                            SwitchTowindow(driver, inputTableValue);
                            driver.Close();
                            //back to current window 
                            driver.SwitchTo().Window(currentwindowhandle);
                            res.Returnresult = false;
                            break;
                        case 24: //clear cookeis

                            driver.Manage().Cookies.DeleteAllCookies();
                            // driver.Navigate().Refresh();
                            res.Returnresult = true;

                            break;
                        case 25: //wait untill  find element with refresh
                            try
                            {
                                WaituntilelementDisplayed(webelement, waitElementDisplayed, driver, true);
                                res.Returnresult = true;
                            }

                            catch (Exception exception)
                            {
                                res.Returnresult = false;
                                res.Message = exception.Message;

                            }
                            break;

                        case 26: //create new user 
                            try
                            {
                                res.Returnresult = CreateUser(guiMapId, inputTableValue);
                            }

                            catch (Exception exception)
                            {
                                res.Message = exception.Message;
                                res.Returnresult = false;
                            }
                            break;

                        case 28: //refresh page 
                            driver.Navigate().Refresh();
                            res.Returnresult = true;
                            break;
                        case 29: //runhedge
                            string automaticHedgerPath = jp.ReadJson("AutomaticHedgerPath");
                            Process newProcess = Process.Start(automaticHedgerPath);
                            Thread.Sleep(10000);
                            if (newProcess != null)
                                newProcess.Kill();
                            res.Returnresult = true;
                            break;
                        case 30: //sleep
                            sleep(guiMapId);
                            res.Returnresult = true;
                            break;


                        case 31: //ComapreDB to savedText or value in the input table
                            // connect string + query split

                            conStringQuery = guimapadapter.GetTagTypeValue(guiMapId);
                            conStringQuerySplit = conStringQuery.Split('!');


                            dbResult = sql.GetDbSingleValue(conStringQuerySplit[0], conStringQuerySplit[1]);
                            if (String.CompareOrdinal(dbResult, inputTableValue) != 0)
                            {
                                res.Returnresult = false;
                                LocalLogFailure(guiMapCommandName,
                                    "The Expected text is wrong. ACTUAL: " + dbResult +
                                    " vs EXPECTED: " + inputTableValue, null, Constants.FAILED);
                            }

                            logIt(guiMapCommandName, "Compared " + conStringQuerySplit[1] + " to " + inputTableValue, Constants.DONE);
                            res.Returnresult = true;
                            break;
                        case 32:
                            //SaveElementText to a dictionary. Value can be retrieved with GetLastCreatrdValue method.
                            SetLastCreatedValue(inputDataColumn, driver.FindElement(webelement).Text);
                            res.Returnresult = true;

                            break;
                        case 33: //Accept Alert
                            driver.SwitchTo().Alert().Accept();
                            res.Returnresult = true;

                            break;
                        case 34: //Accept Dissmiss
                            driver.SwitchTo().Alert().Dismiss();
                            res.Returnresult = true;
                            break;
                        case 35: //SaveElementText to memmory
                            SetLastCreatedValue("memmory", driver.FindElement(webelement).Text);
                            res.Returnresult = true;
                            break;
                        case 36: //ComapreDB to... savedText or value in memmory
                            // connect string + query split

                            conStringQuery = guimapadapter.GetTagTypeValue(guiMapId);
                            conStringQuery = GetDbElement(conStringQuery, inputTableValue);
                            conStringQuerySplit = conStringQuery.Split('!');


                            dbResult = sql.GetDbSingleValue(conStringQuerySplit[0], conStringQuerySplit[1]);
                            if (String.CompareOrdinal(dbResult, GetLastCreatrdValue("memmory")) != 0)
                            {
                                res.Returnresult = false;
                                LocalLogFailure(guiMapCommandName,
                                    "The text comparison is wrong. ACTUAL: " + dbResult +
                                    " vs EXPECTED: " + GetLastCreatrdValue("memmory"), null, Constants.FAILED);
                            }

                            logIt(guiMapCommandName, "Compared " + conStringQuerySplit[1] + " to " + inputTableValue, Constants.DONE);

                            res.Returnresult = true;
                            break;
                        case 1038: //Comapre text property of an element to formerly saved value in memmory
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
                                res.Returnresult = false;

                                LocalLogFailure(guiMapCommandName,
                                    "The text is wrong. ACTUAL: " + text +
                                    " vs EXPECTED: " + GetLastCreatrdValue("memmory"), null, Constants.FAILED);
                            }

                            logIt(guiMapCommandName, "Compared " + text + " to " + inputTableValue, Constants.DONE);

                            res.Returnresult = true;

                            break;

                        case 37: //"compare text not equal
                            if (String.CompareOrdinal(driver.FindElement(webelement).Text, inputTableValue) == 0)
                            {
                                res.Returnresult = false;

                                LocalLogFailure(guiMapCommandName,
                                    "The Expected text is wrong (Shouldn't be equal). actual is: " + driver.FindElement(webelement).Text +
                                    driver.FindElement(webelement).Text +
                                    " vs expected :" + inputTableValue, null, Constants.FAILED);

                            }

                            else
                            {
                                res.Returnresult = true;
                            }

                            break;
                        case 1041: //bring mobile application activity up
                            driver.ExecuteScript("mobile: reset");
                            res.Returnresult = true;
                            break;
                        case 1042: //back button
                            driver.Navigate().Back();
                            res.Returnresult = true;
                            break;
                        case 1039: //"compare value equals
                            if (
                                String.CompareOrdinal(driver.FindElement(webelement).GetAttribute("value"),
                                    inputTableValue) != 0)
                            {
                                res.Returnresult = false;
                                LocalLogFailure(guiMapCommandName,
                                    "The Expected value is wrong (Shouldn't be equal). actual is: " + driver.FindElement(webelement).GetAttribute("value") +
                                    driver.FindElement(webelement).GetAttribute("value") +
                                    " vs expected :" + inputTableValue, null, Constants.FAILED);

                            }
                            else res.Returnresult = true;
                            break;

                        case 1049:
                            // edit user's document status in BackOffice for KYC screen 10 (instead of uploading a real document, chane it's status to "new upload")

                            int rowsChangedCount = 0;
                            //long real_CID;
                            //string userName = GetLastCreatrdValue("memmory");
                            //string conString = ConfigurationManager.AppSettings["RealMirrorQAConnectionString"];

                            //string getUserCidByUserNameCommand =
                            //    @"Select CID FROM [RealMirrorQA].[Customer].[Customer]" +
                            //    @" WHERE userName  = " + userName;


                            //dbResult = sql.GetDbSingleValue(conString, getUserCidByUserNameCommand);

                            long real_CID = Int32.Parse(_lastCreatedValue["CID"]);

                            string updateVerificationLevelIdCommand =
                                @"UPDATE [RealMirrorQA].[BackOffice].[Customer]" +
                                @" SET [DocumentStatusID]  = 2" +
                                @" WHERE [CID]  = " + real_CID;
                            rowsChangedCount = sql.UpdateBackOfficeCustomerTable(updateVerificationLevelIdCommand);
                            if (rowsChangedCount != 1) //could not update
                            {

                                logIt(guiMapCommandName, "Could not update user document status to NEW UPLOAD. ", Constants.DONE);
                                res.Returnresult = true;

                            }
                            break;


                        default:
                            LocalLogFailure(guiMapCommandName, "Can't run this command: " + guiMapTagTypeValue.Trim(), null,
                                            Constants.FAILED);
                            res.Returnresult = false;
                            break;

                        case 1043: // compare if text (2 strings) start with the same substring
                            if (!driver.FindElement(webelement).Text.StartsWith(inputTableValue))
                            {
                                res.Returnresult = false;
                                LocalLogFailure(guiMapCommandName,
                                    "The Expected text is wrong actual: " + driver.FindElement(webelement).Text +
                                    " vs expected :" + inputTableValue, null, Constants.FAILED);
                            }

                            logIt(guiMapCommandName, webelement + " compared text to " + inputTableValue, Constants.DONE);

                            res.Returnresult = true;
                            break;
                        case 1044: //Get Alert Text and svae to memory 
                            SetLastCreatedValue("memmory", driver.SwitchTo().Alert().Text);
                            res.Returnresult = true;
                            break;

                        case 1045: // Positive test for streams API
                            try
                            {
                                StreamScenario ss = new StreamScenario();
                                res.Returnresult = ss.Scenario(inputTableValue, true);
                            }

                            catch (Exception exception)
                            {

                                logIt(guiMapCommandName, "Failed to complete Streams scenario", Constants.DONE, exception);
                                res.Message = exception.Message;
                                res.Returnresult = false;
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

                                    logIt(guiMapCommandName, "Navigated to " + inputTableValue, Constants.DONE);

                                    res.Returnresult = true;
                                }
                                else
                                {
                                    res.Returnresult = false;
                                    LocalLogFailure(guiMapCommandName, "Status code is " + code.ToString(), null, Constants.FAILED);
                                }
                            }

                            catch (Exception ex)
                            {

                                logIt(guiMapCommandName, "Failed to complete scenario", Constants.ERROR, ex);


                                res.Message = ex.Message;
                                res.Returnresult = false;
                            }
                            break;

                        case 1048: // Negative test for streams API Block
                            try
                            {
                                StreamScenario ss = new StreamScenario();
                                res.Returnresult = ss.Scenario(inputTableValue, false);
                            }

                            catch (Exception ex)
                            {
                                logIt(guiMapCommandName, "Failed to complete Streams scenario", Constants.ERROR, ex);
                                res.Returnresult = false;
                                res.Message = ex.Message;
                            }
                            break;


                        case 1050: // upload document 

                            System.EventArgs args = null;
                            object sender = null;

                            if (inputTableValue.Equals("internalUploadMethod"))
                            {
                                try
                                {
                                    UploadUserDocInternal(sender, args, driver, guiMapTagTypeValue);
                                    //uses html file element to inject the file, without actually use the windows file system (good for silent run automation).
                                }
                                catch
                                    (Exception exception)
                                {
                                    result = false;
                                    logIt(guiMapCommandName, "Failed to upload document internally", Constants.ERROR, exception);

                                }
                            }

                            if (inputTableValue.Equals("externalUploadMethod"))
                            //actually sends the path of the file to windows file system windows.
                            {
                                try
                                {
                                    UploadUserDocExternal(sender, args, driver);
                                }
                                catch
                                    (Exception exception)
                                {
                                    res.Returnresult = false;

                                    logIt(guiMapCommandName, "Failed to upload document externally", Constants.ERROR, exception);

                                }
                            }


                            break;
                        case 1051:
                            // calculate expected new balance for user (based on the former balance and a new change like deposit and withdrawal) and save it into the memory.

                            //char[] MyChar = { '$',',', '.'};
                            Double oldBalance = 0;
                            Double balanceChange = 0;
                            Double newBalance = 0;
                            try
                            {
                                if (inputTableValue.All(char.IsDigit))
                                //is input from the table a digit that can be added to the new balance
                                {

                                    oldBalance = Double.Parse((driver.FindElement(webelement).Text).Trim('$'));
                                    balanceChange = Int32.Parse(inputTableValue);
                                    newBalance = oldBalance + balanceChange;
                                    SetLastCreatedValue("memmory", String.Format("{0:C}", newBalance));
                                }
                                else
                                {
                                    res.Returnresult = false;

                                    logIt(guiMapCommandName, "Failed to calculate expected new balance for user because input from the table is  NOT a digit that can be added to the new balance", Constants.ERROR);



                                }
                            }
                            catch (Exception exception)
                            {

                                logIt(guiMapCommandName, "Failed to calculate expected new balance for user", Constants.ERROR, exception);

                                res.Returnresult = false;
                                res.Message = exception.Message;
                            }


                            break;

                        case 1052: //editFTDdate - this command updates the Ftd date of the user's deposit. Please notice that the responsibility for the correct sql date representation is on the user of the command.

                            string command =
                                @"UPDATE [RealMirrorQA].[Billing].[Deposit]" +
                                @" SET [PaymentDate] =  " + "'" + inputTableValue + "'" +
                                @" WHERE  IsFTD=1  and [CID]  = " + GetLastCreatrdValue("CID");
                            try
                            {
                                var ctu = new CreateTestUser();
                                if (ctu.UpdateFtdUser(command, ctu._dbConnectionStrGlobalRegistry) != 1)
                                {
                                    res.Returnresult = false;

                                    logIt(guiMapCommandName, "The Expected result of changing FTD date wrong actual: 0" +
                                    " vs expected : 1", Constants.FAILED);



                                }

                            }
                            catch (Exception exception)
                            {

                                logIt(guiMapCommandName, "Failed to edit user's FTD date", Constants.FAILED, exception);

                                //LocalLogFailure(exception.Message, exception, Constants.Error);
                                res.Returnresult = false;
                            }
                            break;

                        case 1053: //edit "is user signed the risk disclaimer" in KYC 

                            string insertSignedRiskAnswerCommand =
                                    @"INSERT INTO UserApiDB.KYC.CustomerAnswers (GCID, QuestionId, AnswerId, OccurredAt) " +
                                    @"VALUES (" + GetLastCreatrdValue("GCID") + ", 2, 2, GETDATE() )";

                            string insertSignedRiskAnswerCommand2 =
                                    @"INSERT INTO UserApiDB.KYC.CustomerAnswers (GCID, QuestionId, AnswerId, OccurredAt) " +
                                    @"VALUES (" + GetLastCreatrdValue("GCID") + ", 3, 7, GETDATE() )";

                            try
                            {
                                // string riskDiscQuest = guimapadapter.GetTagTypeValue(guiMapId).Trim();
                                var ctu = new CreateTestUser();

                                if (inputTableValue.Equals("noKnowledge") || inputTableValue.Equals("noExperience"))
                                {
                                    if (inputTableValue.Equals("noKnowledge"))
                                        ctu.UpdateFtdUser(insertSignedRiskAnswerCommand,
                                            ctu._dbConnectionStrGlobalRegistry);
                                    if (inputTableValue.Equals("noExperience"))
                                        ctu.UpdateFtdUser(insertSignedRiskAnswerCommand2,
                                            ctu._dbConnectionStrGlobalRegistry);
                                }
                                else
                                {

                                    logIt(guiMapCommandName, "Failed to edit user's signed risk disc status because riskDiscQuest was not specified correctly", Constants.FAILED);

                                    result = false;
                                }
                            }
                            catch (Exception exception)
                            {

                                logIt(guiMapCommandName, "Failed to edit user's signed risk disc status", Constants.ERROR, exception);


                                //Logger.Failed("Failed to edit user's signed risk disc status" + exception.Message);
                                // LocalLogFailure(exception.Message, exception, Constants.Error);
                                res.Returnresult = false;
                            }
                            break;

                        case 1054: //update docs status in BackOffice

                            string updateDocsStatusCommand =
                                    @"Update [RealMirrorQA].[BackOffice].[Customer] Set [DocumentStatusID] = " + inputTableValue +
                                    @" Where CID =  " + GetLastCreatrdValue("CID");



                            try
                            {
                                var ctu = new CreateTestUser();
                                if (ctu.UpdateFtdUser(updateDocsStatusCommand, ctu._dbConnectionStrGlobalRegistry) != 1)
                                {

                                    logIt(guiMapCommandName, " The Expected result of changing doc status is wrong actual: 0" +
                                        " vs expected : 1" + inputTableValue, Constants.FAILED);

                                    //Logger.Failed("Failed to edit user's signed risk disc status" + exception.Message);
                                    // LocalLogFailure(exception.Message, exception, Constants.Error);
                                    res.Returnresult = false;
                                }

                            }
                            catch (Exception exception)
                            {

                                logIt(guiMapCommandName, "Failed to edit user's signed risk disc status", Constants.ERROR, exception);

                                //Logger.Failed("Failed to edit user's signed risk disc status" + exception.Message);
                                // LocalLogFailure(exception.Message, exception, Constants.Error);
                                res.Returnresult = false;


                            }
                            break;

                        case 1055: //update player status in BackOffice (usually to deposit block)

                            string updatePlayerStatusCommand =
                                    @"Update [RealMirrorQA].[Customer].[Customer] Set [PlayerStatusID] = " + inputTableValue +
                                    @" Where CID =  " + GetLastCreatrdValue("CID");
                            try
                            {
                                var ctu = new CreateTestUser();
                                if (ctu.UpdateFtdUser(updatePlayerStatusCommand, ctu._dbConnectionStrGlobalRegistry) != 1)
                                {

                                    logIt(guiMapCommandName, "The Expected result of changing playerID status is wrong actual: 0" +
                                        " vs expected : 1" + inputTableValue, Constants.FAILED);
                                    res.Returnresult = false;

                                }
                            }
                            catch (Exception exception)
                            {

                                logIt(guiMapCommandName, "Failed to edit user's playerID status", Constants.ERROR, exception);
                                res.Returnresult = false;
                            }
                            break;
                        case 1056: //update Identity Check  in BackOffice (usually to 2 sources)

                            //string updateIdentityCheckCommand =
                            //        @"Update [RealMirrorQA].[BackOffice].[ElectronicIdentityCheck] Set [ElectronicIdentityCheckID ] = " + inputTableValue +
                            //       @" Where CID =  " + GetLastCreatrdValue("CID");


                            string updateIdentityCheckCommand =
                                @"INSERT INTO [RealMirrorQA].[BackOffice].[ElectronicIdentityCheck]  (CID, ElectronicIdentityCheckID, ElectronicIdentityProviderID, TransactionID, TransactionDate) " +
                                @"VALUES (" + GetLastCreatrdValue("CID") + ", 2, 1, 'dadadad' ,GETDATE() )";


                            try
                            {
                                var ctu = new CreateTestUser();
                                if (ctu.UpdateFtdUser(updateIdentityCheckCommand, ctu._dbConnectionStrGlobalRegistry) != 1)
                                {
                                    res.Returnresult = false;
                                    logIt(guiMapCommandName, "The Expected result of changing playerID status is wrong actual: 0" +
                                        " vs expected : 1" + inputTableValue, Constants.FAILED);


                                }
                            }
                            catch (Exception exception)
                            {
                                logIt(guiMapCommandName, "Failed to edit user's playerID status", Constants.ERROR, exception);
                                res.Returnresult = false;

                            }
                            break;
                        case 1057: //edit user's verification level

                            string updateVerificationLevelCommand =
                                   @"Update [RealMirrorQA].[BackOffice].[Customer] Set [VerificationLevelID] = " + inputTableValue +
                                   @" Where CID =  " + GetLastCreatrdValue("CID");
                            try
                            {
                                var ctu = new CreateTestUser();
                                if (ctu.UpdateFtdUser(updateVerificationLevelCommand, ctu._dbConnectionStrGlobalRegistry) != 1)
                                {
                                    logIt(guiMapCommandName, "The Expected result of changing playerID status is wrong actual: 0" +
                                        " vs expected : 1" + inputTableValue, Constants.FAILED);


                                    res.Returnresult = false;

                                }
                            }
                            catch (Exception exception)
                            {

                                logIt(guiMapCommandName, " Failed to edit user's verification level", Constants.ERROR, exception);
                                res.Returnresult = false;

                            }
                            break;

                        case 1058: // add/reduce amount to users account
                            int cid = Int32.Parse(GetLastCreatrdValue("CID"));

                            if (CreateTestUser.AddRemoveAmount(10000, cid) == null)
                            {

                                logIt(guiMapCommandName, " Failed to add/reduce amount", Constants.FAILED);
                                res.Returnresult = false;


                            }

                            break;

                        case 1059: // create new and original username and email

                            string userName = "Front" + Guid.NewGuid().ToString().Substring(0, 7);
                            string email = userName + "@aa.com";
                            SetLastCreatedValue("userNameFront", userName);
                            SetLastCreatedValue("emailFront", email);
                            break;

                        case 1060: // Is Element Enabled
                            result = driver.FindElement(webelement).Enabled;
                            break;

                        case 1061: // is disabled
                            result = !(driver.FindElement(webelement).Enabled);
                            break;
                    }
                }
                else
                {
                    res.Returnresult = false;
                    LocalLogFailure(guiMapCommandName, guiMapCommandId + ": web element was not found : " + guiMapTagTypeValue.Trim(), null,
                                    Constants.FAILED);
                }
            }


            catch (Exception exception)
            {
                LocalLogFailure(guiMapCommandName, "While working on element - (" + guiMapTagTypeValue + "), Exception has occured in: " + exception.TargetSite, null, Constants.ERROR);
                res.Returnresult = false;
            }


            //if (res.Returnresult)
            //    AppleniumLogger.LogResult("TestStep: " + guiMapCommandName, Constants.Passed, null);
            //else
            //    AppleniumLogger.LogResult("TestStep: " + guiMapCommandName, Constants.Failed, null);
            return res;
        }

        private void UploadUserDocExternal(object sender, System.EventArgs e, RemoteWebDriver driver) //actually sends the path of the file to windows file system windows.
        {
            const int SUSPEND_TIMEOUT = 1000;
            const string DIALOG_TITLE = "Open";
            const string NETWORK_LOCATION = "\\\\192.168.11.4\\Data\\QA\\KYC2.0";
            ;
            const string FILE_NAME = "id.jpg";
            const string ENTER_KEY = "{ENTER}";

            Interaction.AppActivate(DIALOG_TITLE);
            Thread.Sleep(SUSPEND_TIMEOUT);
            SendKeys.SendWait(NETWORK_LOCATION);
            Thread.Sleep(SUSPEND_TIMEOUT);
            SendKeys.SendWait((ENTER_KEY));
            Thread.Sleep(SUSPEND_TIMEOUT);
            SendKeys.SendWait(FILE_NAME);
            Thread.Sleep(SUSPEND_TIMEOUT);
            SendKeys.SendWait((ENTER_KEY));
        }

        private void UploadUserDocInternal(object sender, System.EventArgs e, RemoteWebDriver driver, string guiMapTagTypeValue) //uses html file element to inject the file, without actually use the windows file system.
        {
            // string cssSelectorStr = guiMapTagTypeValue;

            //IWebElement fileInput = driver.FindElement(By.CssSelector(".passport-container"));
            IWebElement fileInput = driver.FindElement(By.CssSelector(guiMapTagTypeValue));
            fileInput.SendKeys("\\\\192.168.11.4\\Data\\QA\\KYC2.0\\id.jpg");
        }


        private void sleep(int guiMapId)
        {
            var guimapadapter = new GuiMapTableAdapter();
            int sleeptime = Convert.ToInt32(guimapadapter.GetTagTypeValue(guiMapId));
            Thread.Sleep(sleeptime * 1000);
        }

        private bool CreateUser(int guiMapId, string veficationLevel)
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

                            int friendnumber = Convert.ToInt32(veficationLevel);
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
                            NewUser newUser = ctu.CreateUser(usertype, veficationLevel); //affWiz,KYC usage (input is actually a verification level of the user: for affWiz input = 3, for KYC input = 0,1,2,3)
                            if (newUser != null)
                            {
                                SetLastCreatedValue("UserName", newUser.UserName);
                                SetLastCreatedValue("Password", newUser.Password);
                                SetLastCreatedValue("CID", newUser.Real_CID.ToString());
                                SetLastCreatedValue("GCID", newUser.GCID.ToString());

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

                LogObject logobj5 = new LogObject();
                logobj5.CommandName = guiMapCommandName;
                logobj5.Description = exception.Message;
                logobj5.StatusTag = Constants.ERROR;
                logobj5.Exception = exception;

                logger.Print(logobj5);

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

                            //get configuration
                            string emulatorExe;
                            string appium = jp.ReadJson("Appium");
                            string emulatorName = jp.ReadJson("AndroidEmulatorDefaultName");
                            string newCommandTimeout = jp.ReadJson("AndroidNewCommandTimeout");
                            string appPackage = jp.ReadJson("app-package");
                            string appActivity = jp.ReadJson("app-activity");
                            string app = jp.ReadJson("app");
                            string AndroidEmulatorType = jp.ReadJson("AndroidEmulatorType");
                           
                           

                            
                            //Set new session capability
                            capability = new DesiredCapabilities();
                            capability.SetCapability("app-package", appPackage);

                            capability.SetCapability("browserName", "");
                            capability.SetCapability("device", "Android");
                            capability.SetCapability("app-activity", appActivity);                            
                            capability.SetCapability("device ID", "uniquedeviceid");
                            capability.SetCapability("newCommandTimeout", newCommandTimeout);
                            //capability.SetCapability("takesScreenshot", true);
                          
                            if (app != string.Empty)
                            {
                                app = app.Replace(@"\\", @"\");
                                capability.SetCapability("app", app);
                            }

                            //run emulator and waiting for up
                            Process newProcess;
                            if (emulatorName != string.Empty)
                            {
                                if (AndroidEmulatorType == "Google")
                                {
                                    emulatorExe = jp.ReadJson("AndroidEmulatorExe");
                                    newProcess = Process.Start(emulatorExe, "-avd " + emulatorName);
                                    Thread.Sleep(90000);
                                }
                                else
                                {
                                    emulatorExe = jp.ReadJson("AndroidEmulatorGenymotionExe");
                                    newProcess = Process.Start(emulatorExe, "--vm-name " + emulatorName);
                                    Thread.Sleep(30000);
                                }
                            }

                            //run cmd 
                           
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

                LogObject logobj5 = new LogObject();
                logobj5.CommandName = guiMapCommandName;
                logobj5.Description = exception.Message;
                logobj5.StatusTag = Constants.ERROR;
                logobj5.Exception = exception;

                logger.Print(logobj5);

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

        private void logIt(string guiMapCommandName, string description, int status)
        {
            LogObject logobj = new LogObject();
            logobj.CommandName = guiMapCommandName;
            logobj.Description = description;
            logobj.StatusTag = status;

            logger.Print(logobj);
        }

        private void logIt(string guiMapCommandName, string description, int status, Exception ex)
        {
            LogObject logobj = new LogObject();
            logobj.CommandName = guiMapCommandName;
            logobj.Description = description;
            logobj.StatusTag = status;
            logobj.Exception = ex;

            logger.Print(logobj);
        }
    }
}