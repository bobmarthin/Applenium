using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Applenium._3___DAL.DataSetAutoTestTableAdapters;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using LoggerDLL;
using log4net;
using log4net.Appender;

namespace Applenium
{
    /// <summary>
    /// Run execution manager from outside 
    /// </summary>
    public class ExecutionManager
    {
        // private IWebDriver _driver;
        private readonly int _runExecutionId;
        private int _batchId;
        private int _flowId;
        private int _projectId;
        private int _projectPageId;
        private bool isBatchRun;
        private int _projectSectionId;
        private string _testname = Constants.LogNothing;
        private string _batchname = Constants.LogNothing;
        private string _scenarioname = Constants.LogNothing;
        private string _stepname = Constants.LogNothing;
        private string ss = Constants.LogNothing;
        private string _projectName = Constants.LogNothing;
        private readonly MainWindow guiInstance;
        public AppleniumLogger logger = new AppleniumLogger();

        /// <summary>
        /// Constructor
        /// </summary>
        public ExecutionManager()
        {
            //_driver = driver;

        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="runExecutionId"></param>
        public ExecutionManager(int runExecutionId)
        {

            //_driver = driver;
            _runExecutionId = runExecutionId;

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="runExecutionId"></param>
        /// <param name="guiInstanceParam"> Passes an instance of the UI to manipulate</param>
        /// <param name="batch">shows different status on toolbar based on batch parameter</param>
        public ExecutionManager(int runExecutionId, MainWindow guiInstanceParam, bool batch)
        {
            //_driver = driver;

            _runExecutionId = runExecutionId;
            guiInstance = guiInstanceParam;
            isBatchRun = batch;

        }


        private bool ExecuteStep(DataRow dr, RemoteWebDriver driver)
        {


            AppleniumLogger logger = new AppleniumLogger();


            string guiMapCommandName = string.Empty;
            bool result;
            int guiTestId = Convert.ToInt32(dr["GuiTestID"].ToString().Trim());
            string inputTableValue = string.Empty;
            int guiMapId = Convert.ToInt32(dr["GuiMapID"].ToString().Trim());
            int guiMapCommandId = Convert.ToInt32(dr["GuiMapCommandID"].ToString().Trim());
            string iisServer = string.Empty;
            string inputDataRow = string.Empty;
            try
            {
                var sql = new Sql();
                var sl = new Selenium();
                var adapterGuimap = new GuiMapTableAdapter();
                var adapterTest = new TestTableAdapter();
                var adapterCommand = new TestCommandTableAdapter();
                //var adapter_test_failurelog = new DataSetAutoTestTableAdapters.TestFailiureLogTableAdapter();
                string inputDataColumn = string.Empty;

                var adapterGuiproject = new GuiProjectPageTableAdapter();
                _projectSectionId = Convert.ToInt32(adapterTest.GetPageSectionID(Convert.ToInt32(guiTestId)));
                _projectPageId = Convert.ToInt32(new GuiPageSectionTableAdapter().GetGuiPageID(_projectSectionId));
                _projectId = Convert.ToInt32(adapterGuiproject.GetProjectID(_projectPageId));
                _projectName = new ProjectsTableAdapter().GetProjectName(_projectId);


                string inputTableValueDescription = string.Empty;
                if ((dr["InputDataRow"].ToString().Trim() != string.Empty) &&
                    (dr["InputDataRow"].ToString().IndexOf('-') < 0) && (dr["InputDataRow"].ToString().IndexOf(',') < 0))
                {
                    inputDataRow = dr["InputDataRow"].ToString().Trim();
                    inputDataColumn = dr["InputDataColumn"].ToString().Trim();
                    string inputTableName = sql.GetInputDataTableName(guiTestId.ToString(CultureInfo.InvariantCulture),
                                                                      returnTestNameIfNotExists: true);
                    inputTableValue = sql.InputDataValue(inputTableName, inputDataRow, inputDataColumn);

                    //inputTableValueDescription = "Used DataInput Parameters: " + inputTableValue + "\nFrom DataRow: " + inputDataRow + "\n\n";
                    if (inputTableValue.IndexOf(Constants.RegularExpressionOutput, 0, StringComparison.Ordinal) >= 0)
                    {
                        inputTableValue = sl.GetLastCreatrdValue(inputDataColumn);

                    }
                }


                if (inputTableValue.IndexOf(Constants.RegularExpressionRandom, 0, StringComparison.Ordinal) >= 0)
                {
                    string rnd = Guid.NewGuid().ToString().Substring(2, 13);
                    inputTableValue = inputTableValue.Replace(Constants.RegularExpressionRandom, rnd);
                }


                //result = sl.ExecuteOneStep(dr, inputDataColumn, inputTableValue, driver);
                ResultModel resultMod = sl.ExecuteOneStep(dr, inputDataColumn, inputTableValue, driver);
                result = resultMod.Returnresult;

                adapterTest.GetTestName(guiTestId);
                if (guiMapId != 0)
                    _stepname = adapterGuimap.GetGuiMapName(guiMapId).Trim();
                if (guiMapCommandId != 0)
                {
                    guiMapCommandName = adapterCommand.GetTestCommandName(guiMapCommandId).Trim();
                }



                LogObject logObject = new LogObject();
                if (resultMod.Returnresult)
                {

                    logObject.StepName = _stepname;
                    logObject.CommandName = guiMapCommandName;
                    logObject.Description = "("+resultMod.Message+")." + " Completed successfully.";
                    logObject.StepStatus = "Passed";
                    logObject.StatusTag = Constants.PASSED;

                }

                else
                {
                    string filepath = (LogManager.GetCurrentLoggers()[0].Logger.Repository.GetAppenders()[0] as FileAppender).File;
                    string folderpath = System.IO.Directory.GetParent(filepath).ToString();
                    
                   // ss = sl.ScreenShot(driver, "\\\\" + Environment.MachineName + "\\Logs");
                    ss = sl.ScreenShot(driver, folderpath);
                    string pagesource = sl.SourcePage(driver);
                    TextParser tp = new TextParser();
                    //get iis server
                    iisServer = tp.GetIis(pagesource);

                    //string info = string.Format("\n{0}={1}\n{2}={3}\n{4}={5}\n{6}", Constants.LogStepStatus, "Failed",
                    //                                                          "SnapShot", ss, "IISServer", iisServer, sl.LastFailureMessage);

                    logObject.Snapshot = ss;
                    logObject.IISserver = iisServer;
                    logObject.StepStatus = "Failed";
                    logObject.StatusTag = Constants.FAILED;
                    logObject.Description = "Couldn't complete execution of step. Reason - " + resultMod.Message;


                }


                logObject.BatchID = _batchId;
                logObject.ScenarioID = _flowId;



                logObject.TestID = guiTestId;
                logObject.StepID = guiMapId;
                logObject.Parameter1 = "Used DataInput Parameters: " + inputTableValue;
                logObject.Parameter2 = "From DataRow: " + inputDataRow;
                logObject.CommandName = guiMapCommandName;
                logObject.BatchName = _batchname;
                logObject.ScnearioName = _scenarioname;
                logObject.TestName = _testname;
                logObject.ExecutionID = _runExecutionId;
                logObject.StepName = _stepname;
                logObject.ProjectPageID = _projectPageId;
                logObject.ProjectID = _projectId;
                logObject.ProjectName = _projectName;

                logger.Print(logObject);


            }

            catch (Exception exception)
            {

                LogObject logObject = new LogObject();
                logObject.Description = exception.Message;
                logObject.CommandName = guiMapCommandName;
                logObject.StatusTag = Constants.ERROR;
                logObject.Exception = exception;
                logger.Print(logObject);



                //AppleniumLogger.LogResult(guiMapCommandName,exception.Message, Constants.Error, exception);
                result = false;
            }


            return result;
        }

        internal bool ExecuteOneTest(string testId, string inputDataRow, RemoteWebDriver driver, ref Dictionary<string, int> testStatus)
        {
            try
            {



                if (testId == string.Empty)
                {
                    MessageBox.Show("Select test first", "Applenium");
                    return false;
                }
                if (testStatus.ContainsKey("TotalTests"))
                {
                    testStatus["TotalTests"] = testStatus["TotalTests"] + 1;

                }
                else
                {
                    testStatus.Add("TotalTests", 1);
                }

                var sql = new Sql();
                DataTable dt = sql.GetDataTable(Constants.StrTestStepsToSelenium, testId);
                bool testresult = true;
                bool recursivetestresult = true;
                var adapterGuiproject = new GuiProjectPageTableAdapter();
                var adapterTest = new TestTableAdapter();
                var adapteGuiPageSection = new GuiPageSectionTableAdapter();
                _testname = adapterTest.GetTestName(Convert.ToInt32(testId));

                if (guiInstance != null && isBatchRun == false)
                {
                    guiInstance.UpdateProgressLabel(_testname);
                }


                LogObject logObject = new LogObject();



                logObject.Description = "*-------------------------------------------------------------------------------------------------------------\n" + "\t\t\t\t\t\t\t\t\t\t" + _testname + " Test Started\n-------------------------------------------------------------------------------------------------------------\n";
                logObject.StatusTag = Constants.INFO;
                logger.Print(logObject);



                LogObject loggg = new LogObject();
                loggg.StepName = _stepname;
                loggg.ExecutionID = _runExecutionId;
                loggg.TestStatus = Constants.PROGRESS_STARTED;
                loggg.TestName = _testname;
                loggg.Description = Constants.LogTestName + "=" + Constants.PROGRESS_STARTED;
                loggg.ScnearioName = _scenarioname;
                loggg.ProjectName = _projectName;
                loggg.BatchName = _batchname;
                loggg.Parameter1 = inputDataRow;
                loggg.StatusTag = Constants.DONE;
                loggg.Exception = null;
                logger.Print(loggg);




                var jp = new JsonParser();
                string skipTestOnStepFail = jp.ReadJson("SkipTestOnStepFail");


                Singleton myInstance = Singleton.Instance; // Will always be the same instance...

                if (inputDataRow != null)
                {


                    if (inputDataRow.IndexOf("-", 0, StringComparison.Ordinal) >= 0)
                    {
                        string[] rows = inputDataRow.Split('-');
                        for (int i = Convert.ToInt32(rows[0].ToString(CultureInfo.InvariantCulture));
                             i <= Convert.ToInt32(rows[1].ToString(CultureInfo.InvariantCulture));
                             i++)
                        {
                            testresult = ExecuteOneTest(testId, i.ToString(CultureInfo.InvariantCulture), driver, ref testStatus);
                            if (testresult == false)
                                recursivetestresult = false;
                        }
                        return recursivetestresult;
                    }

                    if (inputDataRow.IndexOf(",", 0, StringComparison.Ordinal) >= 0)
                    {

                        string[] rows = inputDataRow.Split(',');
                        foreach (string key in rows)
                        {
                            testresult = ExecuteOneTest(testId, key, driver, ref testStatus);
                            if (testresult == false)
                                recursivetestresult = false;
                        }

                        return recursivetestresult;
                    }
                    foreach (DataRow testrow in dt.Rows)
                    {
                        testrow["InputDataRow"] = inputDataRow;
                    }
                }
                foreach (DataRow row in dt.Rows)
                {
                    if (myInstance.StopExecution)
                    {
                        testresult = false;
                        break;
                    }
                    bool stepresult = ExecuteStep(row, driver);
                    if (stepresult == false)
                    {
                        testresult = false;
                        if (skipTestOnStepFail == "yes")
                        {

                            LogObject logObject2 = new LogObject();

                            logObject2.Description = "Previous Step is Failed => TestStatus=Skiped moving to next test";
                            logObject2.StatusTag = Constants.ERROR;
                            logObject2.ExecutionID = _runExecutionId;
                            logObject2.Exception = null;
                            logger.Print(logObject2);

                            break;
                        }
                    }
                }
                adapterTest.GetTestName(Convert.ToInt32(testId));
                int projectPageSectionId = Convert.ToInt32(adapterTest.GetPageSectionID(Convert.ToInt32(Convert.ToInt32(testId))));
                _projectPageId = Convert.ToInt32(adapteGuiPageSection.GetGuiPageID(projectPageSectionId));
                _projectId = Convert.ToInt32(adapterGuiproject.GetProjectID(_projectPageId));

                LogObject logObject3 = new LogObject();
                if (testresult)
                {
                    //description = string.Format("{0}={1}", Constants.LogTestStatus, "Passed");
                    //status = Constants.Passed;


                    logObject3.TestStatus = Constants.PROGRESS_PASSED;
                    logObject3.StatusTag = Constants.PASSED;


                    if (testStatus.ContainsKey("PassedTests"))
                    {
                        testStatus["PassedTests"] = testStatus["PassedTests"] + 1;

                    }
                    else
                    {
                        testStatus.Add("PassedTests", 1);
                    }

                }

                else
                {



                    logObject3.TestStatus = Constants.PROGRESS_FAILED;
                    logObject3.Description = _testname + " has failed";
                    logObject3.StatusTag = Constants.FAILED;



                    if (testStatus.ContainsKey("FailedTests"))
                    {
                        testStatus["FailedTests"] = testStatus["FailedTests"] + 1;

                    }
                    else
                    {
                        testStatus.Add("FailedTests", 1);
                    }

                }

                logObject3.Parameter1 = "";
                logObject3.Parameter2 = "";
                logObject3.Snapshot = ss;
                logObject3.BatchID = _batchId;
                logObject3.ScenarioID = _flowId;
                logObject3.TestID = Convert.ToInt32(testId);
                logObject3.TestName = _testname;
                logObject3.ScnearioName = _scenarioname;
                logObject3.BatchName = _batchname;
                logObject3.StepID = -1;
                logObject3.ExecutionID = _runExecutionId;
                logObject3.ProjectPageID = _projectPageId;
                logObject3.ProjectName = _projectName;
                logObject3.ProjectID = _projectId;
                logger.Print(logObject3);


                //AppleniumLogger.LogResult(_runExecutionId, _batchId, _flowId, Convert.ToInt32(testId), 0, description + " with DataRow=" + inputDataRow, status, "0", _projectId, _projectPageId);
                return testresult;
            }
            catch (Exception exception)
            {
                LogObject logObject4 = new LogObject();
                logObject4.Description = exception.Message;
                logObject4.CommandName = string.Empty;
                logObject4.StatusTag = Constants.EXECPTION;
                logObject4.Exception = exception;
                logger.Print(logObject4);

                //AppleniumLogger.LogResult(string.Empty,exception.Message, Constants.Error, exception);
                return false;
            }
        }

        internal bool ExecuteOneScenario(string scenarioId, RemoteWebDriver driver)
        {




            //init test status count
            var testStatus = new Dictionary<string, int>();
            if (testStatus.ContainsKey("PassedTests"))
            {
                testStatus["PassedTests"] = 0;

            }
            else
            {
                testStatus.Add("PassedTests", 0);

            }

            if (testStatus.ContainsKey("FailedTests"))
            {
                testStatus["FailedTests"] = 0;

            }
            else
            {
                testStatus.Add("FailedTests", 0);

            }
            if (testStatus.ContainsKey("TotalTests"))
            {
                testStatus["TotalTests"] = 0;

            }
            else
            {
                testStatus.Add("TotalTests", 0);

            }

            try
            {


                if (scenarioId == string.Empty)
                {
                    MessageBox.Show("Select Scenario first", "Applenium");
                    return false;
                }

                _flowId = Convert.ToInt32(scenarioId);

                var sql = new Sql();
                DataTable dtScenario = sql.GetDataTable(Constants.StrScenarioTestsToSelenium, scenarioId);

                var adapterScenario = new ScenarioTableAdapter();
                _projectId = Convert.ToInt32(adapterScenario.GetProjectID(_flowId));
                _scenarioname = adapterScenario.GetScenarioName(_flowId);


                LogObject logObject = new LogObject();
                logObject.Description = "-------------------------------------------------------------------------------------------------------------\n" + "\t\t\t\t\t\t\t\t\t\t" + _scenarioname + " Scenario Started\n-------------------------------------------------------------------------------------------------------------\n";
                logObject.StatusTag = Constants.INFO;
                logger.Print(logObject);

                //                AppleniumLogger.LogResult(string.Empty,
                //"-------------------------------------------------------------------------------------------------------------\n" + "\t\t\t\t\t\t\t\t\t\t" + scenarioname + " Scenario Started\n-------------------------------------------------------------------------------------------------------------\n", Constants.Info, null);

                LogObject logObject2 = new LogObject();
                logObject2.ExecutionID = _runExecutionId;
                logObject2.BatchID = _batchId;
                logObject2.BatchName = _batchname;
                logObject2.ProjectName = _projectName;
                logObject2.ScenarioID = _flowId;
                logObject2.ScenarioStatus = Constants.PROGRESS_STARTED;
                logObject2.StatusTag = Constants.DONE;
                logObject2.Description = Constants.LogScenarioStatus + "=" + Constants.PROGRESS_STARTED;
                logger.Print(logObject2);

                //AppleniumLogger.LogResult(_runExecutionId, _batchId, _flowId, 0, 0, String.Format("{0}={1}", Constants.LogScenarioStatus, "Started"), Constants.Done,
                //          "0",
                //          _projectId, _projectPageId);
                Singleton myInstance = Singleton.Instance; // Will always be the same instance...
                bool scenarioresult = true;
                foreach (DataRow row in dtScenario.Rows)
                {
                    if (myInstance.StopExecution)
                    {
                        scenarioresult = false;
                        break;
                    }
                    string testId = row["GuiTestID"].ToString();

                    bool testresult = ExecuteOneTest(testId, row["InputDataRow"].ToString(), driver, ref testStatus);
                    if (testresult == false)
                        scenarioresult = false;
                }
                string description;

                LogObject result = new LogObject();

                if (scenarioresult)
                {
                    description = String.Format("{0}={1}", Constants.LogScenarioStatus, "Passed");

                    result.ScenarioStatus = Constants.PROGRESS_PASSED;
                    result.StatusTag = Constants.PASSED;
                }
                else
                {
                    result.ScenarioStatus = Constants.PROGRESS_FAILED;
                    result.StatusTag = Constants.FAILED;

                }
                result.ExecutionID = _runExecutionId;
                result.BatchID = _batchId;
                result.BatchName = _batchname;
                result.ScenarioID = _flowId;
                result.ScnearioName = _scenarioname;
                result.ProjectName = _projectName;
                result.ProjectID = _projectId;
                result.ProjectPageID = _projectPageId;
                result.BatchName = _batchname;
                result.Description = " | PassedTest=" + testStatus["PassedTests"] + " | FailedTest=" + testStatus["FailedTests"] + " | TotalTest=" + testStatus["TotalTests"];
                result.ProjectID = _projectId;
                logger.Print(result);


                //AppleniumLogger.LogResult(_runExecutionId, _batchId, _flowId, 0, 0, description + " : PassedTest=" + testStatus["PassedTests"] + " FailedTest=" + testStatus["FailedTests"] + " TotalTest=" + testStatus["TotalTests"], status, "0", _projectId, 0);
                return scenarioresult;
            }

            catch (Exception exception)
            {
                LogObject logObject3 = new LogObject();
                logObject3.Exception = exception;
                logObject3.StatusTag = Constants.ERROR;
                logObject3.Description = exception.Message;
                logger.Print(logObject3);
                //AppleniumLogger.LogResult(string.Empty, exception.Message, Constants.Error, exception);
                return false;
            }

        }


        private bool SetAllScenariusStatusToRunorNot(DataTable dtScenario, int status)
        {
            var adapterBatchLogic = new BatchLogicTableAdapter();
            foreach (DataRow row in dtScenario.Rows)
            {
                adapterBatchLogic.Update(Convert.ToInt32(row["BatchID"]), Convert.ToInt32(row["ScenarioID"]),
                                         Convert.ToInt32(row["BrowserID"]), status,
                                         Convert.ToInt32(row["BatchLogicID"]));
            }
            return true;
        }
        /// <summary>
        /// Unitest will cal this one
        /// </summary>
        /// <param name="batchId"></param>
        /// <returns></returns>
        public bool ExecuteOneBatch(string batchId)
        {
            try
            {

                var jp = new JsonParser();
                string remote = jp.ReadJson("RemoteNode");
                int timeoutAllScenarios = Convert.ToInt32(jp.ReadJson("TimeoutAllScenarios")) * 1000;
                var sel = new Selenium();
                if (batchId == string.Empty)
                {
                    MessageBox.Show("Select Batch first", "Applenium");
                    return false;
                }
                var sql = new Sql();
                _batchId = Convert.ToInt32(batchId);
                DataTable dtScenario = sql.GetDataTable(Constants.StrBatchScenariosToSelenium,
                                                        _batchId.ToString(CultureInfo.InvariantCulture));
                //SetAllScenariusStatusToRunorNot(dtScenario, Constants.Pending);
                var adapterBatches = new BatchesTableAdapter();
                var adapterBatchLogic = new BatchLogicTableAdapter();
                var adapterBrowser = new BrowsersTableAdapter();
                _batchname = adapterBatches.GetBatchName(_batchId);
                var adapterScenario = new ScenarioTableAdapter();

                _projectId = Convert.ToInt32(adapterBatches.GetProjectID(Convert.ToInt32(_batchId)));


                LogObject logObject = new LogObject();
                logObject.ExecutionID = _runExecutionId;
                logObject.BatchID = _batchId;
                logObject.Description = Constants.LogBatchStatus + "=" + Constants.PROGRESS_STARTED;
                logObject.BatchName = _batchname;
                logObject.BatchProgress = Constants.PROGRESS_STARTED;
                logObject.StatusTag = Constants.DONE;
                logObject.ProjectID = _projectId;

                logger.Print(logObject);

                Singleton myInstance = Singleton.Instance;
                myInstance.BatchResult = true;
                var threadFinishEvents = new List<EventWaitHandle>();
                foreach (DataRow row in dtScenario.Rows)
                {
                    if (myInstance.StopExecution)
                    {
                        myInstance.BatchResult = false;
                        break;
                    }
                    var threadFinish = new EventWaitHandle(false, EventResetMode.ManualReset);

                    string browserId = row["BrowserID"].ToString();
                    string batchLogicId = row["BatchLogicID"].ToString();
                    int executionStatus = Convert.ToInt32(row["ExecutionStatusID"].ToString());

                    if (executionStatus == 1) //run only selected scenarios 
                    {
                        threadFinishEvents.Add(threadFinish);
                        string browserName = adapterBrowser.GetBrowserName(Convert.ToInt32(browserId));



                        ThreadStart ts = delegate
                            {

                                //wait till hub return available browser 

                                RemoteWebDriver driver = null;
                                int elapsed = 0;
                                while ((driver == null) && (elapsed < timeoutAllScenarios))
                                {
                                    if (remote == "yes")
                                    {
                                        //generate random number 30-60 
                                        Random r = new Random();
                                        int rInt = r.Next(30000, 60000); //for ints

                                        Thread.Sleep(rInt);
                                        elapsed += rInt;
                                    }
                                    driver = sel.SetWebDriverBrowser(null, browserId, false);

                                }


                                string scenarioId = row["ScenarioID"].ToString();

                                // adapterBatchLogic.Update(_batchId, Convert.ToInt32(scenarioId), Convert.ToInt32(browserId),Constants.Running, Convert.ToInt32(batchLogicId));

                                string scenarioname = adapterScenario.GetScenarioName(Convert.ToInt32(scenarioId));
                                if (guiInstance != null && isBatchRun == true)
                                {
                                    guiInstance.UpdateProgressLabel(scenarioname);
                                }

                                LogObject logObject2 = new LogObject();
                                logObject2.ExecutionID = _runExecutionId;
                                logObject2.BatchID = _batchId;
                                logObject2.ScenarioID = Convert.ToInt32(scenarioId);
                                logObject2.ScnearioName = scenarioname;
                                logObject2.Browser = browserName;
                                logObject2.ProjectID = _projectId;
                                logObject2.ProjectName = _projectName;
                                logObject2.BatchName = _batchname;
                                logObject2.ProjectPageID = _projectPageId;
                                logObject2.StatusTag = Constants.DONE;
                                logger.Print(logObject2);


                                // Do long work here
                                bool scanearioresult = ExecuteOneScenario(scenarioId, driver);

                                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Normal, (EventHandler)
                                                                                               delegate
                                                                                               {
                                                                                                   if (scanearioresult == false)
                                                                                                   {
                                                                                                       myInstance.BatchResult = false;
                                                                                                       //adapterBatchLogic.Update(_batchId,Convert.ToInt32(scenarioId),Convert.ToInt32(browserId),Constants.Failed,Convert.ToInt32(batchLogicId));
                                                                                                   }


                                                                                                   driver.Quit();
                                                                                                   threadFinish.Set();

                                                                                               }, null, null);
                            };

                        ts.BeginInvoke(delegate(IAsyncResult aysncResult) { ts.EndInvoke(aysncResult); }, null);
                        if (remote == "no")
                            WaitHandle.WaitAll(threadFinishEvents.ToArray(), Convert.ToInt32(timeoutAllScenarios));
                    }
                }

                WaitHandle.WaitAll(threadFinishEvents.ToArray(), Convert.ToInt32(timeoutAllScenarios));
                string description;
                int status;
                string bStatus = Constants.PROGRESS_FAILED;

                if (myInstance.BatchResult)
                {
                    description = string.Format("{0}={1}", Constants.LogBatchName, "Passed");
                    status = Constants.PASSED;
                    bStatus = Constants.PROGRESS_PASSED;
                    //LogResult(_runExecutionId, Convert.ToInt32(_batchId), 0, 0, 0, batchname + " Batch Passed",Constants.Passed, "0", _projectId, 0);
                }

                else
                {
                    //LogResult(_runExecutionId, Convert.ToInt32(_batchId), 0, 0, 0, batchname + " Batch Failed",Constants.Failed, "0", _projectId, 0);
                    description = string.Format("{0}={1}", Constants.LogBatchStatus, "Failed");
                    status = Constants.FAILED;
                    bStatus = Constants.PROGRESS_FAILED;

                }

                LogObject logObject3 = new LogObject();
                logObject3.ExecutionID = _runExecutionId;
                logObject3.BatchID = Convert.ToInt32(_batchId);
                logObject3.Description = description;
                logObject3.StatusTag = status;
                logObject3.BatchStatus = bStatus;
                logObject3.ProjectID = _projectId;
                logger.Print(logObject3);


                //SetAllScenariusStatusToRunorNot(dtScenario, Constants.NotRunnig);
                return myInstance.BatchResult;
            }
            catch (Exception exception)
            {

                LogObject exceptionLog = new LogObject();
                exceptionLog.Description = exception.Message;
                exceptionLog.StatusTag = Constants.ERROR;
                logger.Print(exceptionLog);

                return false;
            }
        }


        internal bool HighLight(int guiMapTagTypeId, string guiMapTagTypeValue, RemoteWebDriver driver)
        {
            if (guiMapTagTypeValue == null) throw new ArgumentNullException("guiMapTagTypeValue");
            var sl = new Selenium();
            bool result = sl.DrawBorder(guiMapTagTypeId, guiMapTagTypeValue, driver);
            if (result == false)
                MessageBox.Show("can't highlight this object", "Applenium");
            return result;
        }







    }
}