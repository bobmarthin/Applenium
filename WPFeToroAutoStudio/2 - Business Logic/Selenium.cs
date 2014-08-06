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
using Applenium._2___Business_Logic;
using Applenium._3___DAL.DataSetAutoTestTableAdapters;
using Applenium._4____Infrustructure;
using AutomaticTest;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Safari;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using ServiceStack;
using TradeAPITypes.Data;
using WebAPI;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using System.IO;
using ServiceStack.Common;
using System.ServiceProcess;
using System.Text.RegularExpressions;
using Utilities = Applenium._4____Infrustructure.Utilities;


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

            if (Constants.MemoryConf.ContainsKey(name))
            {

                Constants.MemoryConf[name] = value;

            }
            else
            {
                Constants.MemoryConf.Add(name, value);

            }

            return new ResultModel(true, string.Format("NewValueCreated:{0}={1}", name, value), null);
        }

        /// <summary>
        ///     Get Last Created global value
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetLastCreatedValue(string name)
        {
            if (Constants.MemoryConf.ContainsKey(name))
            {

                return Constants.MemoryConf[name];
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
                if (ConfigurationManager.AppSettings["SharedSnapshotFolder"] != String.Empty)
                {
                    folderName = "\\\\" + Environment.MachineName + "\\" +
                                 ConfigurationManager.AppSettings["SharedSnapshotFolder"];
                }
                string createdFolderLocation = folderName;


                // Take the screenshot     
                try
                {
                    //if ((testType != "loadtest") && (browser != "android"))
                    if (!_ismobile)
                    {
                        driver.Manage().Window.Maximize();
                    }
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
                    ss = ((ScreenShotRemoteWebDriver) driver).GetScreenshot();
                }
                else
                {
                    ss = ((ITakesScreenshot) driver).GetScreenshot();
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

            Screenshot ss = ((ITakesScreenshot) driver).GetScreenshot();
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
            {
                //failed


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

        private ResultModel SwitchTowindow(RemoteWebDriver driver, string windowname)
        {
            ResultModel result = new ResultModel(false, "", null);
            Thread.Sleep(1000);


            IList<string> windows = driver.WindowHandles;

            foreach (String window in windows)
            {
                driver.SwitchTo().Window(window);
                if (driver.Title.Equals(windowname, StringComparison.Ordinal))
                {
                    result.Returnresult = true;
                    result.Message = "Switched to " + windowname;
                    break;
                }
                else
                {
                    result.Returnresult = false;
                    result.Message = "Couldn't switch window";
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
                    LocalLogFailure(guiMapCommandName,
                        "Couldn't run the command = " + guiMapCommandName + " on the element = " + guiMapTagTypeValue,
                        null, Constants.ERROR);
                    break;
            }

            return findelement;
        }

        private static string GetDbElement(string guiMapTagTypeValue, string inputTableValue)
        {

            if (guiMapTagTypeValue.IndexOf(Constants.RegularExpressionAnyValue, 0, StringComparison.Ordinal) >= 0)
            {
                guiMapTagTypeValue = guiMapTagTypeValue.Replace(Constants.RegularExpressionAnyValue,
                    inputTableValue.ToString(CultureInfo.InvariantCulture));
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
                LocalLogFailure(guiMapCommandName, exception.Message, exception, Constants.ERROR);
                result = false;
            }

            return result;
        }

        /// <summary>
        ///     Execute any java script
        /// </summary>
        private int ExecuteJavaScript(string inputJava, RemoteWebDriver driver)
        {
            var js = (IJavaScriptExecutor) driver;
            object result = js.ExecuteScript(inputJava);
            int intResult = 0;

            try
            {

                intResult = Convert.ToInt32(result);
                SetLastCreatedValue(Constants.Memory, intResult.ToString());
            }
            catch (InvalidCastException)
            {

            }




            return intResult;
        }


        private bool Waituntilfindelement(By webelement, int waitElementDisplayedTime, RemoteWebDriver driver)
        {
            IWebElement iresult = null;
            try
            {
                driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(waitElementDisplayedTime));
                iresult = driver.FindElement(webelement);
                driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(0));
            }

            catch (Exception ex)
            {
                LocalLogFailure(guiMapCommandName,
                    "Couldn't find element - " + webelement + ". after: " + waitElementDisplayedTime + " seconds.",
                    null, Constants.FAILED);
                return false;
            }
            if (iresult != null)
                return true;
            else
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

        private ResultModel WaituntilelementDisplayed(By webelement, int waitElementDisplayedTime,
            RemoteWebDriver driver,
            bool isRefresh)
        {
            ResultModel result = new ResultModel(false, "", null);
            if (Waituntilfindelement(webelement, waitElementDisplayedTime, driver))
            {
                int i = 0;

                while (i < waitElementDisplayedTime)
                {
                    try
                    {
                        result.Returnresult = driver.FindElement(webelement).Displayed;
                        if (result.Returnresult)
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
                return result;

            }

            else


                return result;
        }


        /// <summary>
        ///     Execute one step generaly called from execution manger (gets vlaue from data table )
        /// </summary>
        public ResultModel ExecuteOneStep(DataRow dr, string inputDataColumn, string inputTableValue,
            RemoteWebDriver driver)
        {
            int guiMapCommandId = -1;
            string guiMapTagTypeValue = string.Empty;
            var jp = new JsonParser();
            By webelement = By.XPath("null");


            var guimapadapter = new GuiMapTableAdapter();
            Sql sql = new Sql();

            bool result = true;
            ResultModel res = new ResultModel(false, "", null);
            int waitElementDisplayed = Convert.ToInt32(Constants.MemoryConf["WaitElementDisplayed"]);
            int waitElementExists = Convert.ToInt32(Constants.MemoryConf["WaitElementExists"]);
            int expandedWaitFindElement = Convert.ToInt32(Constants.MemoryConf["ExpandedWaitFindElement"]);
            int delayBetweenCommands = Convert.ToInt32(Constants.MemoryConf["DelayBetweenCommands"]);
            if (inputTableValue.IndexOf(Constants.RegularExpressionTestingEnvironment, 0, StringComparison.Ordinal) >= 0)
            {
                inputTableValue = inputTableValue.Replace(Constants.RegularExpressionTestingEnvironment,
                    Constants.MemoryConf["TestingEnvironment"]);
            }

            else if (inputTableValue.IndexOf(Constants.RegularExpressionTestingEnvironmentBackend, 0, StringComparison.Ordinal) >= 0)
            {
                inputTableValue = inputTableValue.Replace(Constants.RegularExpressionTestingEnvironmentBackend, Constants.MemoryConf["TestingEnvironmentBack"]);

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
                        if (inputTableValue != string.Empty)
                        {
                            jp.VariableFunctions(@"$$inputTableValue=" + inputTableValue);
                        }
                        Match match = Regex.Match(guiMapCommandName, @"^\s*(ssh|validation|var|http|db|include)", RegexOptions.IgnoreCase);
                        if (! match.Success)
                        {
                            if (guiMapCommandId == 2)
                            {
                                if (inputTableValue == string.Empty)
                                {
                                    Regex rgx = new Regex(@"^\s*(?<key>.*)\s*==>\s*(?<value>.*)\s*$");
                                    Match matching = rgx.Match(guiMapTagTypeValue);
                                    if (matching.Success)
                                    {
                                        guiMapTagTypeValue = matching.Result("${key}");
                                        inputTableValue = matching.Result("${value}");
                                        inputTableValue = jp.replaceVariable(inputTableValue);
                                    }
                                }
                            }

                            guiMapTagTypeValue = jp.replaceVariable(guiMapTagTypeValue);
                            webelement = (By)GetWebElement(guiMapTagTypeId, guiMapTagTypeValue, inputTableValue);
                        }
                        else
                        {
                            webelement =  By.XPath("NOO");
                        }
                        //VSH: Add latest performed command info to memory
                        match = Regex.Match(guiMapCommandName, @"^\s*(validation|var|include)", RegexOptions.IgnoreCase);
                        if (!match.Success)
                        {
                            string cmd_info = "guiMapCommandName=" + guiMapCommandName + ", inputTableValue=" + inputTableValue + ", guiMapTagTypeValue=" + guiMapTagTypeValue;
                            jp.AddKeyToMemory("LastCmdInfo", cmd_info);
                        }
                    }
                }
                if (webelement != null)
                {
                    Thread.Sleep(delayBetweenCommands);

                    string conStringQuery;
                    string dbapiResult;
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

                                    driver.FindElement(webelement).Click();

                                    res.Message = "Clicked sucessfully";
                                    res.Returnresult = true;
                                    attempts++;
                                    break;

                                }
                            }
                            break;

                        case 2: //"sendkey":
                            try
                            {
                                driver.FindElement(webelement).Clear();
                                string sendkey = "";
                                if (inputTableValue != string.Empty)
                                {
                                    driver.FindElement(webelement).SendKeys(inputTableValue);
                                }
                                else
                                {
                                    string url = guiMapTagTypeValue.Trim();
                                }
                                res.Message = "Sent keys (" + inputTableValue + ") to element - " + webelement;
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

                                

                                res.Message = "Navigated to " + inputTableValue;
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
                                res.Message = "Found -" + webelement;
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

                                res.Message = "Checked -" + webelement;
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
                                res.Message = "Mouse Over -" + webelement;
                                res.Returnresult = true;

                            }
                            catch (Exception ex)
                            {
                                res.Returnresult = false;
                                res.Message = ex.Message;
                            }
                            break;

                        case 8: //"waituntill-findelement":                          
                            res.Returnresult = Waituntilfindelement(webelement, waitElementExists, driver);
                            res.Message = "Waited to find - " + webelement.ToString().Trim();
                            
                            break;

                        case 9: //"compare text                        
                            res.Returnresult = true;
                            if (String.CompareOrdinal(driver.FindElement(webelement).Text, inputTableValue) != 0)
                            {
                                LocalLogFailure(guiMapCommandName,
                                    "The Expected text is wrong actual: " + driver.FindElement(webelement).Text +
                                    " vs expected: " + inputTableValue, null, Constants.FAILED);
                                res.Returnresult = false;
                            }

                            res.Message = "Compared the text - " + webelement + " to " + inputTableValue;
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

                            res.Message = "Dropdown element - " + webelement + ", with index   " + inputTableValue;
                            res.Returnresult = true;
                            break;

                        case 11: //wait untill  displayed 
                            res = WaituntilelementDisplayed(webelement, waitElementDisplayed, driver, false);
                            res.Message = "Waited to display - " + webelement;
                            
                            break;

                        case 12: //verify is exists 
                            IWait<IWebDriver> wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitElementExists));
                            //IWebElement a = ExpectedConditions.ElementExists(webelement);
                            wait.Until(ExpectedConditions.ElementExists(webelement));
                            res.Message = "Element exists - " + webelement;
                            res.Returnresult = true;
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

                            res.Message = "Dropdown element - " + webelement + ", with index   " + inputTableValue;
                            res.Returnresult = true;
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
                            res.Message = "Dropdown element - " + webelement + ", with index   " + inputTableValue;
                            res.Returnresult = true;
                            break;

                        case 15: //SetlocalStorage Run JavaScript(any java script)
                            // select the drop down list////System.Windows.MessageBox
                            if (!_ismobile)
                            {
                                int value = ExecuteJavaScript(inputTableValue, driver);
                                if (value != 0)
                                {
                                    SetLastCreatedValue(Constants.Memory, value.ToString());
                                }
                            }
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
                            res.Message = "Java script executed with   " + inputTableValue;
                            res.Returnresult = true;
                            break;

                        case 16: //wait untill  find element long time expanded 

                            WaituntilelementDisplayed(webelement, expandedWaitFindElement, driver, false);
                            res.Returnresult = true;
                            res.Message = "Expanded wait for element success - " + webelement;
                            //logIt(guiMapCommandName, "Expanded wait for element success - " + webelement, Constants.DONE);
                            break;

                        case 17: //Count number of elements and Compare
                            int elementcount = driver.FindElements(webelement).Count();
                            if (elementcount != Convert.ToInt32(inputTableValue))
                            {

                                res.Returnresult = false;
                                res.Message = "The Expected count is wrong. EXPECTED: " + inputTableValue +
                                              " vs ACTUAL(Found): " + elementcount;
                            }

                            else
                            {

                                res.Message = "Number of elements - " + webelement + " is " + elementcount +
                                              " compared to:   " +
                                              inputTableValue;
                                res.Returnresult = true;
                            }
                            break;

                        case 18: //scrolldown
                            ExecuteJavaScript("window.scroll(0,document.body.scrollHeight)", driver);
                            res.Message = "Scrolled Down";
                            res.Returnresult = true;
                            break;

                        case 19: //scrollup
                            ExecuteJavaScript("window.scroll(0,0)", driver);
                            res.Message = "Scrolled Up";
                            res.Returnresult = true;
                            break;

                        case 20: //switchtowindow
                            string currentWindow = null;
                            currentWindow = driver.CurrentWindowHandle;
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
                                res = SwitchTowindow(driver, inputTableValue);
                            }
                            break;

                        case 21: //validateOpenTrades

                            string usernameorig = inputTableValue;
                            var openbook = new OpenBookValidator(true);
                            // get positions from UI
                            List<IWebElement> openbookpositiondata = openbook.GetOpenBookPositionData(driver,
                                guiMapId);



                            // Compare positions from UI to DB
                            //string eranstring = ConfigurationManager.AppSettings["ConnectionString"];
                            //OpenBookValidator.ValidationResponse validationResponseresult =
                            //    openbook.ValidateClientOpenBookOpenTrades(usernameorig, openbookpositiondata, 888,
                            //        888, "OpenbookPositionData", 888,
                            //        "OB_OpenTrades");

                            res.Returnresult = true;
                            res.Message = "Found elements in UI.";

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
                            res.Returnresult = true;
                            break;
                        case 24: //clear cookeis

                            driver.Manage().Cookies.DeleteAllCookies();
                            // driver.Navigate().Refresh();
                            res.Returnresult = true;

                            break;
                        case 25: //wait untill  find element with refresh

                            WaituntilelementDisplayed(webelement, waitElementDisplayed, driver, true);
                            res.Returnresult = true;
                            break;

                        case 26: //create new user 
                            res.Returnresult = CreateUser(guiMapId, inputTableValue);
                            break;

                        case 28: //refresh page 
                            driver.Navigate().Refresh();
                            res.Returnresult = true;
                            break;
                        case 29: //runhedge
                            string automaticHedgerPath = Constants.MemoryConf["AutomaticHedgerPath"];
                            Process newProcess = Process.Start(automaticHedgerPath);
                            Thread.Sleep(10000);
                            if (newProcess != null)
                                newProcess.Kill();
                            res.Returnresult = true;
                            break;
                        case 30: //sleep
                            conStringQuery = guimapadapter.GetTagTypeValue(guiMapId);
                            conStringQuery = GetDbElement(conStringQuery, inputTableValue);
                            sleep(Convert.ToInt32(conStringQuery));
                            res.Message = "Sleeped for a while";
                            res.Returnresult = true;
                            break;


                        case 31: //ComapreDB to savedText or value in the input table
                            // connect string + query split

                            conStringQuery = guimapadapter.GetTagTypeValue(guiMapId);
                            conStringQuerySplit = conStringQuery.Split('!');


                            dbapiResult = sql.GetDbSingleValue(conStringQuerySplit[0], conStringQuerySplit[1]);
                            if (String.CompareOrdinal(dbapiResult, inputTableValue) != 0)
                            {
                                res.Returnresult = false;
                                res.Message = "The Expected text is wrong. ACTUAL: " + dbapiResult +
                                              " vs EXPECTED: " + inputTableValue;

                            }

                            res.Message = "Compared " + conStringQuerySplit[1] + " to " + inputTableValue;
                            res.Returnresult = true;
                            break;
                        case 32:
                            //SaveElementText to a dictionary. Value can be retrieved with GetLastCreatrdValue method.
                            SetLastCreatedValue(inputDataColumn, driver.FindElement(webelement).Text);

                            jp.AddKeyToMemory(Constants.Memory, driver.FindElement(webelement).Text);
                            
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
                        case 35: //SaveElementText to memory
                            SetLastCreatedValue(Constants.Memory, driver.FindElement(webelement).Text);
                            res.Returnresult = true;
                            break;
                        case 36: //ComapreDB to... savedText or value in memory
                            // connect string + query split

                            conStringQuery = guimapadapter.GetTagTypeValue(guiMapId);
                            conStringQuery = GetDbElement(conStringQuery, inputTableValue);
                            conStringQuerySplit = conStringQuery.Split('!');


                            dbapiResult = sql.GetDbSingleValue(conStringQuerySplit[0], conStringQuerySplit[1]);
                            if (String.CompareOrdinal(dbapiResult, GetLastCreatedValue(Constants.Memory)) != 0)
                            {
                                res.Returnresult = false;
                                res.Message = "The text comparison is wrong. ACTUAL: " + dbapiResult +
                                              " vs EXPECTED: " + GetLastCreatedValue(Constants.Memory);


                            }


                            res.Message = "Compared " + conStringQuerySplit[1] + " to " + inputTableValue;
                            res.Returnresult = true;
                            break;
                        case 1038: //Comapre text property of an element to formerly saved value in memory
                            // connect string + query split
                            // var guimapadapter1038 = new GuiMapTableAdapter();
                            // string ConString_Query1038 = guimapadapter1038.GetTagTypeValue(guiMapId);
                            // ConString_Query1038 = GetDBElement(ConString_Query1038, inputTableValue);
                            //string[] ConString_QuerySplit1038 = ConString_Query1038.Split('!');

                            //var sql1038 = new Sql();
                            // string DBResult1038 = sql1038.GetDBSingleValue(ConString_QuerySplit1038[0], ConString_QuerySplit1038[1]);

                            //string text = driver.FindElement(webelement).Text;
                            string text = inputTableValue;
                            if (String.CompareOrdinal(text, GetLastCreatedValue(Constants.Memory)) != 0)
                            {
                                res.Returnresult = false;
                                res.Message = "The text is wrong. ACTUAL: " + text +
                                              " vs EXPECTED: " + GetLastCreatedValue(Constants.Memory);



                            }

                            res.Message = "Compared " + text + " to " + inputTableValue;
                            res.Returnresult = true;

                            break;

                        case 37: //"compare text not equal
                            if (String.CompareOrdinal(driver.FindElement(webelement).Text, inputTableValue) == 0)
                            {
                                res.Returnresult = false;
                                res.Message = "The Expected text is wrong (Shouldn't be equal). actual is: " +
                                              driver.FindElement(webelement).Text +
                                              driver.FindElement(webelement).Text +
                                              " vs expected :" + inputTableValue;


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
                                res.Message = "The Expected value is wrong (Shouldn't be equal). actual is: " +
                                              driver.FindElement(webelement).GetAttribute("value") +
                                              driver.FindElement(webelement).GetAttribute("value") +
                                              " vs expected :" + inputTableValue;

                            }
                            else res.Returnresult = true;
                            break;

                        case 1049:
                            // edit user's document status in BackOffice for KYC screen 10 (instead of uploading a real document, chane it's status to "new upload")

                            int rowsChangedCount = 0;
                            //long real_CID;
                            //string userName = GetLastCreatrdValue(Constants.Memory);
                            //string conString = ConfigurationManager.AppSettings["RealMirrorQAConnectionString"];

                            //string getUserCidByUserNameCommand =
                            //    @"Select CID FROM [RealMirrorQA].[Customer].[Customer]" +
                            //    @" WHERE userName  = " + userName;


                            //dbResult = sql.GetDbSingleValue(conString, getUserCidByUserNameCommand);

                            long real_CID = Int32.Parse(Constants.MemoryConf["CID"]);

                            string updateVerificationLevelIdCommand =
                                @"UPDATE [RealMirrorQA].[BackOffice].[Customer]" +
                                @" SET [DocumentStatusID]  = 2" +
                                @" WHERE [CID]  = " + real_CID;
                            rowsChangedCount = sql.UpdateBackOfficeCustomerTable(updateVerificationLevelIdCommand);
                            if (rowsChangedCount != 1) //could not update
                            {

                                res.Message = "Could not update user document status to NEW UPLOAD. ";
                                res.Returnresult = true;

                            }
                            break;




                        case 1043: // compare if text (2 strings) start with the same substring
                            if (!driver.FindElement(webelement).Text.StartsWith(inputTableValue))
                            {
                                res.Returnresult = false;
                                res.Message = "The Expected text is wrong actual: " +
                                              driver.FindElement(webelement).Text +
                                              " vs expected :" + inputTableValue;

                            }


                            res.Message = webelement + " compared text to " + inputTableValue;
                            res.Returnresult = true;
                            break;
                        case 1044: //Get Alert Text and svae to memory 
                            SetLastCreatedValue(Constants.Memory, driver.SwitchTo().Alert().Text);
                            res.Returnresult = true;
                            break;

                        case 1045: // Positive test for streams API
                            StreamScenario ss = new StreamScenario();
                            res.Returnresult = ss.Scenario(inputTableValue, true);
                            break;

                        case 1046: // Get Status code from url

                            HttpWebRequest request = (HttpWebRequest) HttpWebRequest.Create(inputTableValue);
                            HttpWebResponse response = (HttpWebResponse) request.GetResponse();
                            HttpStatusCode code = response.StatusCode;

                            if (code.ToString() == "OK")
                            {

                                res.Message = "Navigated to " + inputTableValue;
                                res.Returnresult = true;
                            }
                            else
                            {
                                res.Returnresult = false;
                                res.Message = "Failed to call service - " + inputTableValue;
                            }

                            break;

                        case 1048: // Negative test for streams API Block

                            StreamScenario ssn = new StreamScenario();
                            res.Returnresult = ssn.Scenario(inputTableValue, false);
                            break;


                        case 1050: // upload document 

                            System.EventArgs args = null;
                            object sender = null;

                            if (inputTableValue.Equals("internalUploadMethod"))
                            {

                                UploadUserDocInternal(sender, args, driver, guiMapTagTypeValue);
                                //uses html file element to inject the file, without actually use the windows file system (good for silent run automation).
                                res.Returnresult = true;

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
                                    res.Message = "Failed to upload document externally. " + exception.Message;
                                    res.exc = exception;

                                }
                                res.Returnresult = true;
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
                                    SetLastCreatedValue(Constants.Memory, String.Format("{0:C}", newBalance));
                                    res.Returnresult = true;

                                }
                                else
                                {
                                    res.Returnresult = false;
                                    res.Message =
                                        "Failed to calculate expected new balance for user because input from the table is  NOT a digit that can be added to the new balance";




                                }
                            }
                            catch (Exception exception)
                            {


                                res.Returnresult = false;
                                res.Message = "Failed to calculate expected new balance for user. " + exception.Message;
                                res.exc = exception;

                            }


                            break;

                        case 1052:
                            //editFTDdate - this command updates the Ftd date of the user's deposit. Please notice that the responsibility for the correct sql date representation is on the user of the command.

                            string command =
                                @"UPDATE [RealMirrorQA].[Billing].[Deposit]" +
                                @" SET [PaymentDate] =  " + "'" + inputTableValue + "'" +
                                @" WHERE  IsFTD=1  and [CID]  = " + GetLastCreatedValue("CID");
                            try
                            {
                                var ctu = new CreateTestUser();
                                if (ctu.UpdateFtdUser(command, ctu.DbConnectionStrGlobalRegistry) != 1)
                                {
                                    res.Returnresult = false;
                                    res.Message = "The Expected result of changing FTD date wrong actual: 0" +
                                                  " vs expected : 1";
                                }
                                else
                                {
                                    res.Returnresult = true;
                                }

                            }
                            catch (Exception exception)
                            {


                                //LocalLogFailure(exception.Message, exception, Constants.Error);
                                res.Message = "Failed to edit user's FTD date. " + exception.Message;
                                res.Returnresult = false;
                                res.exc = exception;

                            }
                            break;

                        case 1053: //edit "is user signed the risk disclaimer" in KYC 

                            string insertSignedRiskAnswerCommand =
                                @"INSERT INTO UserApiDB.KYC.CustomerAnswers (GCID, QuestionId, AnswerId, OccurredAt) " +
                                @"VALUES (" + GetLastCreatedValue("GCID") + ", 2, 2, GETDATE() )";

                            string insertSignedRiskAnswerCommand2 =
                                @"INSERT INTO UserApiDB.KYC.CustomerAnswers (GCID, QuestionId, AnswerId, OccurredAt) " +
                                @"VALUES (" + GetLastCreatedValue("GCID") + ", 3, 7, GETDATE() )";

                            try
                            {
                                // string riskDiscQuest = guimapadapter.GetTagTypeValue(guiMapId).Trim();
                                var ctu = new CreateTestUser();

                                if (inputTableValue.Equals("noKnowledge") || inputTableValue.Equals("noExperience"))
                                {
                                    if (inputTableValue.Equals("noKnowledge"))
                                        ctu.UpdateFtdUser(insertSignedRiskAnswerCommand,
                                            ctu.DbConnectionStrKyc);
                                    if (inputTableValue.Equals("noExperience"))
                                        ctu.UpdateFtdUser(insertSignedRiskAnswerCommand2,
                                            ctu.DbConnectionStrKyc);

                                    res.Returnresult = true;

                                }
                                else
                                {


                                    res.Message =
                                        "Failed to edit user's signed risk disc status because riskDiscQuest was not specified correctly";
                                    res.Returnresult = false;
                                }
                            }
                            catch (Exception exception)
                            {


                                //Logger.Failed("Failed to edit user's signed risk disc status" + exception.Message);
                                // LocalLogFailure(exception.Message, exception, Constants.Error);
                                res.Message = "Failed to edit user's signed risk disc status. " + exception.Message;
                                res.Returnresult = false;
                                res.exc = exception;

                            }
                            break;

                        case 1054: //update docs status in BackOffice

                            string updateDocsStatusCommand =
                                @"Update [RealMirrorQA].[BackOffice].[Customer] Set [DocumentStatusID] = " +
                                inputTableValue +
                                @" Where CID =  " + GetLastCreatedValue("CID");



                            try
                            {
                                var ctu = new CreateTestUser();
                                if (ctu.UpdateFtdUser(updateDocsStatusCommand, ctu.DbConnectionStrGlobalRegistry) != 1)
                                {


                                    //Logger.Failed("Failed to edit user's signed risk disc status" + exception.Message);
                                    // LocalLogFailure(exception.Message, exception, Constants.Error);
                                    res.Message = " The Expected result of changing doc status is wrong actual: 0" +
                                                  " vs expected : 1" + inputTableValue;
                                    res.Returnresult = false;
                                }
                                else
                                {
                                    res.Returnresult = true;
                                }

                            }
                            catch (Exception exception)
                            {


                                //Logger.Failed("Failed to edit user's signed risk disc status" + exception.Message);
                                // LocalLogFailure(exception.Message, exception, Constants.Error);
                                res.Message = "Failed to edit user's signed risk disc status. " + exception.Message;
                                res.Returnresult = false;
                                res.exc = exception;



                            }
                            break;

                        case 1055: //update player status in BackOffice (usually to deposit block)

                            string updatePlayerStatusCommand =
                                @"Update [RealMirrorQA].[Customer].[Customer] Set [PlayerStatusID] = " + inputTableValue +
                                @" Where CID =  " + GetLastCreatedValue("CID");
                            try
                            {
                                var ctu = new CreateTestUser();
                                if (ctu.UpdateFtdUser(updatePlayerStatusCommand, ctu.DbConnectionStrGlobalRegistry) !=
                                    1)
                                {


                                    res.Message = "The Expected result of changing playerID status is wrong actual: 0" +
                                                  " vs expected : 1" + inputTableValue;
                                    res.Returnresult = false;

                                }
                                else
                                {
                                    res.Returnresult = true;
                                }
                            }
                            catch (Exception exception)
                            {

                                res.Message = "Failed to edit user's playerID status. " + exception.Message;
                                res.Returnresult = false;
                                res.exc = exception;

                            }
                            break;
                        case 1056: //update Identity Check  in BackOffice (usually to 2 sources)

                            //string updateIdentityCheckCommand =
                            //        @"Update [RealMirrorQA].[BackOffice].[ElectronicIdentityCheck] Set [ElectronicIdentityCheckID ] = " + inputTableValue +
                            //       @" Where CID =  " + GetLastCreatrdValue("CID");


                            string updateIdentityCheckCommand =
                                @"INSERT INTO [RealMirrorQA].[BackOffice].[ElectronicIdentityCheck]  (CID, ElectronicIdentityCheckID, ElectronicIdentityProviderID, TransactionID, TransactionDate) " +
                                @"VALUES (" + GetLastCreatedValue("CID") + ", 2, 1, 'dadadad' ,GETDATE() )";


                            try
                            {
                                var ctu = new CreateTestUser();
                                if (ctu.UpdateFtdUser(updateIdentityCheckCommand, ctu.DbConnectionStrGlobalRegistry) !=
                                    1)
                                {
                                    res.Returnresult = false;
                                    res.Message = "The Expected result of changing playerID status is wrong actual: 0" +
                                                  " vs expected : 1" + inputTableValue;


                                }
                                else
                                {
                                    res.Returnresult = true;
                                }
                            }
                            catch (Exception exception)
                            {
                                res.Message = "Failed to edit user's playerID status. " + exception.Message;
                                res.Returnresult = false;
                                res.exc = exception;


                            }
                            break;
                        case 1057: //edit user's verification level

                            string updateVerificationLevelCommand =
                                @"Update [RealMirrorQA].[BackOffice].[Customer] Set [VerificationLevelID] = " +
                                inputTableValue +
                                @" Where CID =  " + GetLastCreatedValue("CID");
                            try
                            {
                                var ctu = new CreateTestUser();
                                if (
                                    ctu.UpdateFtdUser(updateVerificationLevelCommand, ctu.DbConnectionStrGlobalRegistry) !=
                                    1)
                                {


                                    res.Message = "The Expected result of changing playerID status is wrong actual: 0" +
                                                  " vs expected : 1" + inputTableValue;
                                    res.Returnresult = false;

                                }
                                else
                                {
                                    res.Returnresult = true;
                                }
                            }
                            catch (Exception exception)
                            {

                                res.Message = " Failed to edit user's verification level. " + exception.Message;
                                res.Returnresult = false;

                                res.exc = exception;


                            }
                            break;

                        case 1058: // add/reduce amount to users account
                            int cid = Int32.Parse(GetLastCreatedValue("CID"));

                            if (CreateTestUser.AddRemoveAmount(10000, cid) == false)
                            {

                                res.Message = " Failed to add/reduce amount";
                                res.Returnresult = false;


                            }
                            else
                            {
                                res.Returnresult = true;
                            }

                            break;

                        case 1059: // create new and original username and email

                            string userName = "Front" + Guid.NewGuid().ToString().Substring(0, 7);
                            string email = userName + "@aa.com";
                            res = SetLastCreatedValue("userNameFront", userName);
                            res = SetLastCreatedValue("emailFront", email);


                            break;

                        case 1060: // Is Element Enabled
                            res.Returnresult = driver.FindElement(webelement).Enabled;
                            if (res.Returnresult)
                            {
                                res.Message = "element is enabled as expected";
                            }
                            else
                            {
                                res.Message = "expected the element to be enabled but it is disabled";
                            }
                            break;

                        case 1061: // is disabled
                            res.Returnresult = !(driver.FindElement(webelement).Enabled);
                            if (res.Returnresult)
                            {
                                res.Message = "element is disabled as expected";
                            }
                            else
                            {
                                res.Message = "expected the element to be disabled but it is inabled";
                            }
                            break;

                        case 1062: // Get text from div tag and store in memory
                            SetLastCreatedValue(Constants.Memory,
                                driver.FindElement((By) webelement).GetAttribute("textContent"));
                            res.Returnresult = true;
                            break;

                        case 1071:

                            switch (guiMapTagTypeValue)
                            {

                                case "post":
                                    string[] userAndPass = inputTableValue.Split('!');
                                    StreamsApiRequest sar = new StreamsApiRequest();
                                    string token = sar.login(userAndPass[0], userAndPass[1]);
                                    int numOfIterations = Int32.Parse(userAndPass[2]);

                                    for (int i = 0; i <= numOfIterations; i++)
                                    {
                                        StreamsApiResultModel.PostMessageResultModel resultMod =
                                            sar.PostMessageRequest(Constants.ACTION_DISCUSSION, token, userAndPass[0],
                                                "Automation Message");
                                        if (resultMod != null)
                                        {
                                            res.Message = " Executing API Call of type: " + guiMapTagTypeValue;
                                            res.Returnresult = true;
                                        }

                                        else
                                        {
                                            res.Message = "Couldn't post a message";
                                            res.Returnresult = false;
                                        }
                                    }
                                    break;
                                case "like":
                                    break;
                                case "get":
                                    break;
                                case "token":
                                    break;
                            }
                            break;



                        case 1065: // compare api to memory
                            conStringQuery = guimapadapter.GetTagTypeValue(guiMapId);
                            conStringQuery = GetDbElement(conStringQuery, inputTableValue);
                            conStringQuerySplit = conStringQuery.Split('!');
                            dbapiResult = HttpRequestExtensions.GetJsonValue(conStringQuerySplit[0],
                                conStringQuerySplit[1], 60000);
                            if (String.CompareOrdinal(dbapiResult, GetLastCreatedValue(Constants.Memory)) != 0)
                            {

                                res.Returnresult = false;
                                res.Message = "The text comparison is wrong. ACTUAL: " + dbapiResult +
                                              " vs EXPECTED: " + GetLastCreatedValue(Constants.Memory);


                            }
                            else
                            {
                                res.Returnresult = true;
                            }

                            break;

                        case 1068: // save db to memory

                            conStringQuery = guimapadapter.GetTagTypeValue(guiMapId);
                            conStringQuery = GetDbElement(conStringQuery, inputTableValue);
                            conStringQuerySplit = conStringQuery.Split('!');
                            dbapiResult = sql.GetDbSingleValue(conStringQuerySplit[0], conStringQuerySplit[1]);
                            res = SetLastCreatedValue(Constants.Memory, dbapiResult);

                            break;
                        case 1069: // svae db to Dictionary
                            conStringQuery = guimapadapter.GetTagTypeValue(guiMapId);
                            conStringQuery = GetDbElement(conStringQuery, inputTableValue);
                            conStringQuerySplit = conStringQuery.Split('!');
                            dbapiResult = sql.GetDbSingleValue(conStringQuerySplit[0], conStringQuerySplit[1]);
                            res = SetLastCreatedValue(inputDataColumn, dbapiResult);
                            break;

                        case 1070: //ComapreDB to... savedText or value in memory
                            // connect string + query split

                            conStringQuery = guimapadapter.GetTagTypeValue(guiMapId);
                            conStringQuery = GetDbElement(conStringQuery, inputTableValue);
                            conStringQuerySplit = conStringQuery.Split('!');


                            dbapiResult = sql.GetDbSingleValue(conStringQuerySplit[0], conStringQuerySplit[1]);
                            if (String.CompareOrdinal(dbapiResult, GetLastCreatedValue(Constants.Memory)) == 0)
                            {
                                res.Returnresult = false;
                                res.Message = "The text not equal comparison is wrong. ACTUAL: " + dbapiResult +
                                              " vs EXPECTED: " + GetLastCreatedValue(Constants.Memory);


                            }


                            res.Message = "Not equal Compared " + conStringQuerySplit[1] + " to " + inputTableValue;
                            res.Returnresult = true;
                            break;
                        case 1072: // copy value from memory into dictionary

                            var memVal = GetLastCreatedValue(Constants.Memory);
                            res = SetLastCreatedValue(inputDataColumn, memVal);

                            break;

                        case 1073: // copy value from dictionary into memory


                            var dicVal = GetLastCreatedValue(inputDataColumn);
                            res = SetLastCreatedValue(Constants.Memory, dicVal);

                            break;
                        case 1074: // not found element
                            bool found = Waituntilfindelement(webelement, waitElementDisplayed, driver);
                            if (!found)
                            {
                                res.Message = "Element not found (as expected) - " + webelement;
                                res.Returnresult = true;
                            }
                            else
                            {
                                res.Message = "Element found (Not as expected) - " + webelement;
                                res.Returnresult = false;
                            }
                            break;

                        case 1075: // not found element

                            bool isUp = false;

                            System.ServiceProcess.ServiceController[] services =
                                System.ServiceProcess.ServiceController.GetServices("ISR-SR-QA-WEB-1.trad.local");

                            foreach (ServiceController scTemp in services)
                            {
                                if (scTemp.ServiceName.Equals(inputTableValue) &&
                                    scTemp.Status == ServiceControllerStatus.Running)
                                {
                                    isUp = true;
                                    res.Message = "Service " + webelement + " is up and running ";
                                    break;
                                }
                            }

                            res.Returnresult = isUp;

                            if (!res.Returnresult)
                            {
                                res.Message = "Service " + webelement + " is NOT running ";
                            }
                            break;

                        case 1076: //Compare 2 input table values - 1 from the input and 1 from dictionary


                            if (String.CompareOrdinal(inputTableValue, GetLastCreatedValue(Constants.Memory)) == 0)
                            {
                                res.Returnresult = true;
                                res.Message = "The text is equal comparison succeeded. Memory value is: " +
                                              GetLastCreatedValue(Constants.Memory) +
                                              " and input table value that is : " + inputTableValue;


                            }

                            else
                            {
                                res.Message = "Not equal. Compared memory: " + GetLastCreatedValue(Constants.Memory) +
                                              " to input table value: " + inputTableValue;
                                res.Returnresult = false;
                            }

                            break;

                        case 1077: // SSH commands
                            try
                            {
                                var ssh = new ssh_client();

                                string cmd = guiMapTagTypeValue.Trim();
                                cmd = jp.replaceVariable(cmd);

                                jp.AddKeyToMemory(Constants.Memory, ssh.run_cmd(cmd));
                                res.Returnresult = true;

                            }
                            catch (Exception ex)
                            {
                                res.Returnresult = false;
                                jp.AddKeyToMemory(Constants.Memory, "SSH ERROR");
                                res.Message = "SSH connection problem : " + ex.Message;
                            }
                            break;
                        case 1078: // Configuration Include - from file
                            try
                            {
                                string cmd = guiMapTagTypeValue.Trim();

                                cmd = jp.replaceVariable(cmd);

                                if (File.Exists(cmd))
                                {
                                    jp.AddConfigToMemory(cmd);
                                }
                                res.Returnresult = true;

                            }
                            catch (Exception ex)
                            {
                                res.Returnresult = false;
                                res.Message = "Could not attach new configuration : " + ex.Message;
                            }
                            break;
                        case 1079: //Variables functions
                            try
                            {
                                string cmd = guiMapTagTypeValue.Trim();
                                jp.VariableFunctions (cmd);
                                res.Returnresult = true;
                            }
                            catch (Exception ex)
                            {
                                res.Returnresult = false;
                                res.Message = ex.Message;
                            }
                            break;
                       
                        case 1080: //Validation regex compare

                                string str = guiMapTagTypeValue.Trim();
                                string validationRegex = jp.ValidationRegex(str);
                                if (validationRegex.EndsWith("OK"))
                                {
                                    res.Returnresult = true;
                                    res.Message = "The regex compare is equal. Comparison succeeded.";
                                }

                                else
                                {
                                    res.Returnresult = false;
                                    res.Message = validationRegex;
                                }
                            break;
                        case 1081: //VSH Validation - line compare

                                 str = guiMapTagTypeValue.Trim();
                                Boolean validationLine = jp.ValidationLineCompare(str);

                                if (validationLine)
                                {
                                    res.Returnresult = true;
                                    res.Message = "The text is equal. Comparison succeeded.";
                                }

                                else
                                {
                                    res.Returnresult = false;
                                    res.Message = "The text is not equal. Comparison failed.";
                                }
                            break;
                        case 1084: // VSH: DB Client Cmd

                            string db_cmd = guiMapTagTypeValue.Trim();
                            db_cmd = jp.replaceVariable(db_cmd);


                            jp.AddKeyToMemory(Constants.Memory, sql.ExecuteCMDQuery(db_cmd));
                            res.Returnresult = true;
                            break;

                        case 1085: // VSH: HTTP Client Cmd
                            try
                            {
                                string url = guiMapTagTypeValue.Trim();
                                url = jp.replaceVariable(url);
                                res.Returnresult = true;
                                jp.AddKeyToMemory(Constants.Memory, HttpRequestExtensions.GetHTTPrequest(url));
                            }
                            catch
                            {
                                res.Returnresult = false;
                            }
                            break;
                        case 1087: // VSH: Advanced HTTP Client Cmd
                            try
                            {
                                string cmd = guiMapTagTypeValue.Trim();
                                cmd = jp.replaceVariable(cmd);
                                res.Returnresult = true;
                                jp.AddKeyToMemory(Constants.Memory, HttpRequestExtensions.GetHTTPrequestPost(cmd));
                            }
                            catch
                            {
                                res.Returnresult = false;
                            }
                            break;
                        case 1090://start transaction - mesure time                         
                            try
                            {
                               //get current time 
                                DateTime starttime = DateTime.Now;
                                String strStartTime = starttime.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                                                         CultureInfo.InvariantCulture);
                                jp.AddKeyToMemory(inputTableValue, strStartTime);
                                res.Message = "|StartTransaction=" + inputTableValue + "|StartTime=" + strStartTime;
                                res.Returnresult = true;
                            }
                            catch
                            {
                                res.Returnresult = false;
                            }
                            break;
                        case 1091: //end transaction -mesure time and print log 
                            try
                            {
                                //get current time 
                                DateTime endtime = DateTime.Now;
                                String strEndTime = endtime.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                                                        CultureInfo.InvariantCulture);
                                DateTime starttime = Convert.ToDateTime(Constants.MemoryConf[inputTableValue]);
                                

                                var totaltime = endtime.Subtract(starttime);

                                res.Message = "|EndTransaction=" + inputTableValue + "|EndTime=" + strEndTime + "|TotalTransactionTime=" + totaltime;
                                res.Returnresult = true;
                            }
                            catch
                            {
                                res.Returnresult = false;
                            }
                            break;
                    }

                }
                else
                {
                    res.Returnresult = false;
                    res.Message = guiMapCommandId + ": web element was not found : " + guiMapTagTypeValue.Trim();

                }
            }
            catch (Exception exception)
            {

                // In case the exception is of the following type - mark the scenario as passed. (BUG IN SELENIUM FRAMEWORK)
                if (exception.TargetSite.ToString().Contains("OpenQA.Selenium.Remote.Response CreateResponse"))
                {
                    // Still log the error 
                    res.Message = "OpenQA.Selenium.Remote.Response issue happened";
                    res.Returnresult = true;
                    res.exc = exception;
                    // The flag that makes the entire scenario pass even though there was an error.
                    Applenium._4____Infrustructure.Utilities.skipSCN = true;
                }

                else
                {
                    res.Message = "While working on element - (" + guiMapTagTypeValue + "), Exception has occured in: " +
                                  exception.Message;
                    res.Returnresult = false;
                    res.exc = exception;
                }

            }

            return res;
        }

        private void UploadUserDocExternal(object sender, System.EventArgs e, RemoteWebDriver driver)
            //actually sends the path of the file to windows file system windows.
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

        private void UploadUserDocInternal(object sender, System.EventArgs e, RemoteWebDriver driver,
            string guiMapTagTypeValue)
            //uses html file element to inject the file, without actually use the windows file system.
        {
            // string cssSelectorStr = guiMapTagTypeValue;

            //IWebElement fileInput = driver.FindElement(By.CssSelector(".passport-container"));
            IWebElement fileInput = driver.FindElement(By.CssSelector(guiMapTagTypeValue));
            fileInput.SendKeys("\\\\192.168.11.4\\Data\\QA\\KYC2.0\\id.jpg");
        }


        private void sleep(int strSleepTime)
        {
           
            int sleeptime = Convert.ToInt32(strSleepTime);
            Thread.Sleep(sleeptime*1000);
        }

        private bool CreateUser(int guiMapId, string veficationLevel)
        {
            try
            {
                string facebookAppSecret = ConfigurationManager.AppSettings["FacebookAppSecret"];
                string facebookAppId = ConfigurationManager.AppSettings["FacebookAppId"];
                bool result = false;
                var ctu = new CreateTestUser();
                var guimapadapter = new GuiMapTableAdapter();
               
                string usertype = guimapadapter.GetTagTypeValue(guiMapId).Trim();

                NewUser newUser = ctu.CreateUser(usertype, veficationLevel, GetLastCreatedValue("userNameFront"));
                    //affWiz,KYC usage (input is actually a verification level of the user: for affWiz input = 3, for KYC input = 0,1,2,3)
                    if (newUser != null)
                    {
                        SetLastCreatedValue("UserName", newUser.UserName);
                        SetLastCreatedValue("Password", newUser.Password);
                        SetLastCreatedValue("CID", newUser.Real_CID.ToString());
                        SetLastCreatedValue("GCID", newUser.GCID.ToString());
                        sleep(1000);
                        if (usertype.Equals(Constants.FtdUserWithAff) && (ctu.AffId != 0))
                            SetLastCreatedValue("AffiliateId", ctu.AffId.ToString());
                        result = true;
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

                string remote = Constants.MemoryConf["RemoteNode"];

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
                            string appium = Constants.MemoryConf["Appium"];
                            string AppiumServerHost = Constants.MemoryConf["AppiumServerHost"];
                            string emulatorName = Constants.MemoryConf["AndroidEmulatorDefaultName"];
                            string newCommandTimeout = Constants.MemoryConf["AndroidNewCommandTimeout"];
                            string appPackage = Constants.MemoryConf["app-package"];
                            string appActivity = Constants.MemoryConf["app-activity"];
                            string app = Constants.MemoryConf["app"];
                            string androidEmulatorType = Constants.MemoryConf["AndroidEmulatorType"];



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



                            //run cmd 
                            if (AppiumServerHost == "127.0.0.1" || AppiumServerHost == "localhost")
                            {
                                //run emulator and waiting for up
                                Process newProcess;
                                if (emulatorName != string.Empty)
                                {
                                    if (androidEmulatorType == "Google")
                                    {
                                        emulatorExe = Constants.MemoryConf["AndroidEmulatorExe"];
                                        newProcess = Process.Start(emulatorExe, "-avd " + emulatorName);
                                        Thread.Sleep(90000);
                                    }
                                    else
                                    {
                                        emulatorExe = Constants.MemoryConf["AndroidEmulatorGenymotionExe"];
                                        newProcess = Process.Start(emulatorExe, "--vm-name " + emulatorName);
                                        Thread.Sleep(30000);
                                    }
                                }

                                Process.Start(appium);
                                Thread.Sleep(10000);
                            }
                            driver = new ScreenShotRemoteWebDriver(new Uri("http://" + AppiumServerHost + ":4723/wd/hub/"), capability, TimeSpan.FromSeconds(120));                            
                            break;
                        case "5":
                        case "safari":

                            
                            driver = new SafariDriver();
                            break;
                        case "6":
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
                            string appPackage = Constants.MemoryConf["app-package"];
                            capability.SetCapability("app-package", appPackage);
                            capability.SetCapability("browserName", "");
                            capability.SetCapability("device", "Android");
                            string appActivity = Constants.MemoryConf["app-activity"];
                            //capability.SetCapability("app-activity", "com.etoro.mobileclient.Views.Login");
                            capability.SetCapability("app-activity", appActivity);
                            string appWaitActivity = Constants.MemoryConf["app-wait-activity"];
                            //capability.SetCapability("app-activity", "com.etoro.mobileclient.Views.Login");
                            capability.SetCapability("app-activity", appWaitActivity);
                            //capability.SetCapability("takesScreenshot", true);
                            //caps.SetCapability("version", "4.3.0");
                            capability.SetCapability("device ID", "uniquedeviceid");
                            //caps.SetCapability("app", @"C:\Temp\version 1.0.70-Maxim.apk");
                            string app = Constants.MemoryConf["app"];
                            if (app != string.Empty)
                            {

                                app = app.Replace(@"\\", @"\");
                                capability.SetCapability("app", app);
                            }

                            //driver = new RemoteWebDriver(new Uri("http://localhost:4723/wd/hub/"), capability);
                            break;
                        case "5":
                            capability = DesiredCapabilities.Safari();
                            break;
                      
                    }
                    string hubAddress = Constants.MemoryConf["HubAddress"];
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
                    driver.Manage().Window.Size = new System.Drawing.Size(5, 5);

                    var js = (IJavaScriptExecutor)driver;
                    string defaulturl = Constants.MemoryConf["DefaultURL"];
                    defaulturl = defaulturl.Replace(@"\", @"\\");
                    if ((driver != null) && (defaulturl != string.Empty))
                        driver.Navigate().GoToUrl(defaulturl);

                    string onboarding = Constants.MemoryConf["RunJavaScriptOnPageLoad"];

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
                String extendedTime = Constants.MemoryConf["ExpandedWaitFindElement"];

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