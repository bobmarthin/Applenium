using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Applenium.DataSetAutoTestTableAdapters;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

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


        private readonly MainWindow guiInstance;


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


            string guimapName = string.Empty;
            string guiMapCommandName = string.Empty;
            bool result;
            int guiTestId = Convert.ToInt32(dr["GuiTestID"].ToString().Trim());
            string inputTableValue = string.Empty;
            int guiMapId = Convert.ToInt32(dr["GuiMapID"].ToString().Trim());
            int guiMapCommandId = Convert.ToInt32(dr["GuiMapCommandID"].ToString().Trim());
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

                _projectPageId = Convert.ToInt32(adapterTest.GetProjectPageID(Convert.ToInt32(guiTestId)));
                //_projectId = Convert.ToInt32(adapterGuiproject.GetProjectID(_projectPageId));
                string inputTableValueDescription = string.Empty;
                if ((dr["InputDataRow"].ToString().Trim() != string.Empty) &&
                    (dr["InputDataRow"].ToString().IndexOf('-') < 0) && (dr["InputDataRow"].ToString().IndexOf(',') < 0))
                {
                    string inputDataRow = dr["InputDataRow"].ToString().Trim();
                    inputDataColumn = dr["InputDataColumn"].ToString().Trim();
                    string inputTableName = sql.GetInputDataTableName(guiTestId.ToString(CultureInfo.InvariantCulture),
                                                                      returnTestNameIfNotExists: true);
                    inputTableValue = sql.InputDataValue(inputTableName, inputDataRow, inputDataColumn);

                    inputTableValueDescription = " with DataInput=" + inputTableValue + "in DataRow=" + inputDataRow;
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


                result = sl.ExecuteOneStep(dr, inputDataColumn, inputTableValue, driver);

                adapterTest.GetTestName(guiTestId);
                if (guiMapId != 0)
                    guimapName = adapterGuimap.GetGuiMapName(guiMapId).Trim();
                if (guiMapCommandId != 0)
                {
                    guiMapCommandName = adapterCommand.GetTestCommandName(guiMapCommandId).Trim();
                }
                string description;
                int status;
                string ss=string.Empty;

                if (result)
                {
                    description = string.Format("{0}={1}", Constants.LogStepStatus, "Passed");
                    status = Constants.Passed;
                }
                    //LogResult(_runExecutionId, _batchId, _flowId, guiTestId, guiMapId,"Step Passed: " + guiMapCommandName + " => " + guimapName + inputTableValueDescription,Constants.Passed, "0", _projectId, _projectPageId);
                else
                {
                    //ss = sl.ScreenShot(driver, Directory.GetCurrentDirectory() + "\\Logs");
                    ss = sl.ScreenShot(driver, "\\\\" + Environment.MachineName + "\\Logs");
                    string pagesource = sl.SourcePage(driver);
                    TextParser tp=new TextParser();
                    //get iis server
                    string iisServer = tp.GetIis(pagesource);
                    description = string.Format("{0}={1}\t{2}={3}\t{4}={5}\t{6}", Constants.LogStepStatus, "Failed",
                                                                              "SnapShot", ss, "IISServer", iisServer, sl.LastFailureMessage);
                    status = Constants.Failed;
                    
                    //LogResult(_runExecutionId, _batchId, _flowId, guiTestId, guiMapId,"Step Failed: " + guiMapCommandName + " => " + guimapName + inputTableValueDescription +"\n" + sl.LastFailureMessage + "\n" + ss, Constants.Failed, "0", _projectId,_projectPageId);
                }
                LogResult(_runExecutionId, _batchId, _flowId, guiTestId, guiMapId, guiMapCommandName + " " + description + inputTableValueDescription , status, "0", _projectId, _projectPageId);
            }

            catch (Exception exception)
            {
                Logger.Error(exception.Message, exception);
                result = false;
            }


            return result;
        }

        internal bool ExecuteOneTest(string testId, string inputDataRow, RemoteWebDriver driver,ref Dictionary<string, int> testStatus)
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
                string testname = adapterTest.GetTestName(Convert.ToInt32(testId));

                if (guiInstance != null && isBatchRun == false)
                {
                    guiInstance.UpdateProgressLabel(testname);
                }

                LogResult(_runExecutionId, _batchId, _flowId, Convert.ToInt32(testId),0,
                          "TestStatus=Started with DataRow=" + inputDataRow, Constants.Done, "0", _projectId,
                          _projectPageId);
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
                            testresult = ExecuteOneTest(testId, key, driver,ref testStatus);
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
                            LogResult(_runExecutionId, _batchId, _flowId, Convert.ToInt32(testId), 0,
                                      " Previous Step is Failed => TestStatus=Skiped moving to next test",Constants.Error, "0", _projectId, _projectPageId);
                            break;
                        }
                    }
                }
                adapterTest.GetTestName(Convert.ToInt32(testId));
                int projectPageSectionId = Convert.ToInt32(adapterTest.GetProjectPageID(Convert.ToInt32(Convert.ToInt32(testId))));
                _projectPageId = Convert.ToInt32(adapteGuiPageSection.GetGuiPageID(projectPageSectionId));
                _projectId = Convert.ToInt32(adapterGuiproject.GetProjectID(_projectPageId));
                string description;
                int status;
                if (testresult)
                {
                    description = string.Format("{0}={1}", Constants.LogTestStatus, "Passed");
                    status = Constants.Passed;
                    if (testStatus.ContainsKey("PassedTests"))
                    {
                        testStatus["PassedTests"] = testStatus["PassedTests"] + 1;

                    }
                    else
                    {
                        testStatus.Add("PassedTests", 1);
                    }
               
                    //LogResult(_runExecutionId, _batchId, _flowId, Convert.ToInt32(testId), 0,testname + " Test Passed  with data row : " + inputDataRow, Constants.Passed, "0",_projectId,_projectPageId);
                }
                    
                else
                {
                    //LogResult(_runExecutionId, _batchId, _flowId, Convert.ToInt32(testId), 0,testname + " Test Failed  with data row : " + inputDataRow, Constants.Failed, "0",_projectId,_projectPageId);
                    description = string.Format("{0}={1}", Constants.LogTestStatus, "Failed");
                    status = Constants.Failed;
                    if (testStatus.ContainsKey("FailedTests"))
                    {
                        testStatus["FailedTests"] = testStatus["FailedTests"] + 1;

                    }
                    else
                    {
                        testStatus.Add("FailedTests", 1);
                    }
                   
                }
                LogResult(_runExecutionId, _batchId, _flowId, Convert.ToInt32(testId), 0,description+" with DataRow=" + inputDataRow, status, "0",_projectId,_projectPageId);
                return testresult;
            }
            catch (Exception exception)
            {

                Logger.Error(exception.Message, exception);
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
                string scenarioname = adapterScenario.GetScenarioName(_flowId);
                LogResult(_runExecutionId, _batchId, _flowId, 0, 0, String.Format("{0}={1}", Constants.LogScenarioStatus, "Started"), Constants.Done,
                          "0",
                          _projectId, _projectPageId);
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
                int status;
                if (scenarioresult)
                {
                    description = String.Format("{0}={1}", Constants.LogScenarioStatus, "Passed");
                    status = Constants.Passed;
                    //     LogResult(_runExecutionId, _batchId, _flowId, 0, 0, scenarioname + " Scenario Passed",Constants.Passed,"0", _projectId, 0);
                }
               else
                {
                    //LogResult(_runExecutionId, _batchId, _flowId, 0, 0, scenarioname + " Scenario Failed",Constants.Failed,"0", _projectId, 0);
                    description = String.Format("{0}={1}", Constants.LogScenarioStatus, "Failed");
                    status = Constants.Failed;
                }
                LogResult(_runExecutionId, _batchId, _flowId, 0, 0, description + " : PassedTest=" + testStatus["PassedTests"] + " FailedTest=" + testStatus["FailedTests"] + " TotalTest=" + testStatus["TotalTests"], status, "0", _projectId, 0);
                return scenarioresult;
            }

            catch (Exception exception)
            {

                Logger.Error(exception.Message, exception);
                return  false;
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
                int timeoutAllScenarios = Convert.ToInt32(jp.ReadJson("TimeoutAllScenarios"))*1000;
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
                string batchname = adapterBatches.GetBatchName(_batchId);
                var adapterScenario = new ScenarioTableAdapter();

                _projectId = Convert.ToInt32(adapterBatches.GetProjectID(Convert.ToInt32(_batchId)));

                LogResult(_runExecutionId, Convert.ToInt32(_batchId), 0, 0, 0, Constants.LogBatchStatus+"=Started",
                          Constants.Done,
                          "0", _projectId, 0);

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
                                while( ( driver==null ) && ( elapsed < timeoutAllScenarios ) )
                                {
                                    //generate random number 30-60 
                                    Random r = new Random();
                                    int rInt = r.Next(30000, 60000); //for ints

                                    Thread.Sleep(rInt);
                                    driver = sel.SetWebDriverBrowser(null, browserId, false);
                                    elapsed += rInt;                                     
                                }


                                string scenarioId = row["ScenarioID"].ToString();

                                // adapterBatchLogic.Update(_batchId, Convert.ToInt32(scenarioId), Convert.ToInt32(browserId),Constants.Running, Convert.ToInt32(batchLogicId));

                                string scenarioname = adapterScenario.GetScenarioName(Convert.ToInt32(scenarioId));
                                if (guiInstance != null && isBatchRun == true)
                                {
                                    guiInstance.UpdateProgressLabel(scenarioname);
                                }
                                LogResult(_runExecutionId, Convert.ToInt32(_batchId), Convert.ToInt32(scenarioId), 0, 0,
                                          scenarioname + " ScenarioBrowser=" + browserName,
                                          Constants.Done,
                                          "0", _projectId, _projectPageId);

                                // Do long work here
                                bool scanearioresult = ExecuteOneScenario(scenarioId, driver);
                               
                                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Normal, (EventHandler)
                                                                                               delegate
                                                                                                   {
                                                                                                       if (scanearioresult ==false)
                                                                                                       {
                                                                                                           myInstance.BatchResult= false;
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

                if (myInstance.BatchResult)
                {
                    description = string.Format("{0}={1}", Constants.LogBatchName, "Passed");
                    status = Constants.Passed;
                    //LogResult(_runExecutionId, Convert.ToInt32(_batchId), 0, 0, 0, batchname + " Batch Passed",Constants.Passed, "0", _projectId, 0);
                }
                
                else
                {
                    //LogResult(_runExecutionId, Convert.ToInt32(_batchId), 0, 0, 0, batchname + " Batch Failed",Constants.Failed, "0", _projectId, 0);
                    description = string.Format("{0}={1}", Constants.LogBatchStatus, "Failed");
                    status = Constants.Failed;
                }
                LogResult(_runExecutionId, Convert.ToInt32(_batchId), 0, 0, 0, description, status, "0", _projectId, 0);

                //SetAllScenariusStatusToRunorNot(dtScenario, Constants.NotRunnig);
                return myInstance.BatchResult;
            }
            catch (Exception exception)
            {

                Logger.Error(exception.Message, exception);
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

        internal int LogResult(int runExecutionId, int batchId, int scenarioId, int testId, int stepId,
                                     string description, int status, string outOfFlow, int projectId, int projectPageId)
        {
            try
            {
                int result;
                string batchName=string.Empty;
                string scenarioName = string.Empty;
                string testName = string.Empty;
                string stepName = string.Empty;
                string projectName = string.Empty;
                string projectPageName = string.Empty;
                DateTime occurred = DateTime.Now.ToUniversalTime();
                using (var adapterLogresult = new TestResultsTableAdapter())
                {
                    result = adapterLogresult.Insert(runExecutionId, batchId, scenarioId, testId, stepId, description,
                                                     status, outOfFlow, projectId, projectPageId, occurred);
                }

                using (var adapterBatch = new BatchesTableAdapter())
                {
                    if (batchId != 0)
                        batchName = adapterBatch.GetBatchName(batchId);
                    else
                    {
                        batchName = Constants.LogNothing;
                    }
                }

                using (var adapterScenario = new ScenarioTableAdapter())
                {
                    if (scenarioId != 0)
                        scenarioName = adapterScenario.GetScenarioName(scenarioId);
                    else
                    {
                        scenarioName = Constants.LogNothing;
                    }
                }

                using (var adapterTest = new TestTableAdapter())
                {
                    if (testId!=0)
                        testName = adapterTest.GetTestName(testId);
                    else
                    {
                        testName = Constants.LogNothing;
                    }
                }

                using (var adapterGuiMap = new GuiMapTableAdapter())
                {
                    if (stepId!=0)
                        stepName = adapterGuiMap.GetGuiMapName(stepId).Trim();
                    else
                    {
                        stepName = Constants.LogNothing;
                    }
                }

                using (var adapterProject = new ProjectsTableAdapter())
                {
                    
                    if(projectId!=0)
                        projectName = adapterProject.GetProjectName(projectId);
                    else
                    {
                        projectName = Constants.LogNothing;
                    }
                }

                using (var adapterProjectPage = new GuiProjectPageTableAdapter())
                {
                    if (projectPageId!=0)
                        projectPageName = adapterProjectPage.GetProjectPageName(projectPageId);
                    else
                    {
                        projectPageName = Constants.LogNothing;
                    }
                }


                string logString = String.Format("{0}={1}\t{2}={3}\t{4}={5}\t{6}={7}\t{8}={9}\t{10}=( {11} )\t{12}={13}\t{14}={15}",
                                                 Constants.LogExecutionId, runExecutionId,
                                                 Constants.LogBatchName, batchName,
                                                 Constants.LogScenarioName, scenarioName,
                                                 Constants.LogTestName, testName,
                                                 Constants.LogStepName, stepName,
                                                 Constants.LogDescription, description,
                                                 Constants.LogProjectName, projectName,
                                                 Constants.LogProjectPageName, projectPageName);
                if (status == Constants.Passed)
                    Logger.Passed(logString);
                if (status == Constants.Done)
                    Logger.Done(logString);
                if (status == Constants.Failed)
                    Logger.Failed(logString);

                return result;
            }
            catch (Exception exception)
            {

                Logger.Error(exception.Message, exception);
                return 0;
            }

        }
    }
}