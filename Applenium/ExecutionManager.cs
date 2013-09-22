using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using OpenQA.Selenium;
using Applenium.DataSetAutoTestTableAdapters;
using Applenium;

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


        private bool ExecuteStep(DataRow dr, IWebDriver driver)
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
                string inputDataColumn=string.Empty;
                var adapterGuiproject = new GuiProjectPageTableAdapter();

                _projectPageId = Convert.ToInt32(adapterTest.GetProjectPageID(Convert.ToInt32(guiTestId)));
                _projectId = Convert.ToInt32(adapterGuiproject.GetProjectID(_projectPageId));
                string inputTableValueDescription = string.Empty;
                if ((dr["InputDataRow"].ToString().Trim() != string.Empty) &&
                    (dr["InputDataRow"].ToString().IndexOf('-') < 0) && (dr["InputDataRow"].ToString().IndexOf(',') < 0))
                {
                    string inputDataRow = dr["InputDataRow"].ToString().Trim();
                    inputDataColumn = dr["InputDataColumn"].ToString().Trim();
                    string inputTableName = sql.GetInputDataTableName(guiTestId.ToString(CultureInfo.InvariantCulture),
                                                                      returnTestNameIfNotExists: true);
                    inputTableValue = sql.InputDataValue(inputTableName, inputDataRow, inputDataColumn);

                    inputTableValueDescription = " with data input: " + inputTableValue + "in row : " + inputDataRow;
                    if (inputTableValue.IndexOf(Constants.RegularExpressionOutput, 0, StringComparison.Ordinal) >= 0)
                    {
                        inputTableValue = sl.GetLastCreatrdValue(inputDataColumn);
                    }
                }


                if (inputTableValue.IndexOf(Constants.RegularExpressionRandom, 0, StringComparison.Ordinal) >= 0)
                {
                    string rnd = Guid.NewGuid().ToString().Substring(2, 16);
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


                if (result)
                    LogResult(_runExecutionId, _batchId, _flowId, guiTestId, guiMapId,
                              "Step Passed: " + guiMapCommandName + " => " + guimapName + inputTableValueDescription,
                              Constants.Passed, "0", _projectId, _projectPageId);
                else
                {
                    string ss = sl.ScreenShot(driver, Directory.GetCurrentDirectory() + "\\Logs");
                    LogResult(_runExecutionId, _batchId, _flowId, guiTestId, guiMapId,
                              "Step Failed: " + guiMapCommandName + " => " + guimapName + inputTableValueDescription +
                              "\n" + sl.LastFailureMessage + "\n" + ss, Constants.Failed, "0", _projectId,
                              _projectPageId);
                }
            }

            catch (Exception exception)
            {
                Logger.Error(exception.Message, exception);
                result = false;
            }


            return result;
        }

        internal bool ExecuteOneTest(string testId, string inputDataRow, IWebDriver driver)
        {
            if (testId == string.Empty)
            {
                MessageBox.Show("Select test first", "Applenium");
                return false;
            }
            var sql = new Sql();

            DataTable dt = sql.GetDataTable(Constants.StrTestStepsToSelenium, testId);
            bool testresult = true;
            bool recursivetestresult = true;
            var adapterGuiproject = new GuiProjectPageTableAdapter();
            var adapterTest = new TestTableAdapter();
            string testname = adapterTest.GetTestName(Convert.ToInt32(testId));
            LogResult(_runExecutionId, _batchId, _flowId, Convert.ToInt32(testId), 0,
                      testname + " Test Started  with data row : " + inputDataRow, Constants.Done, "0", _projectId,
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
                        testresult = ExecuteOneTest(testId, i.ToString(CultureInfo.InvariantCulture), driver);
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
                        testresult = ExecuteOneTest(testId, key, driver);
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
                                  " Previous Step is Failed => Skipping test " + testname + " moving to next test : ",
                                  Constants.Error, "0", _projectId, _projectPageId);
                        break;
                    }
                }
            }
            adapterTest.GetTestName(Convert.ToInt32(testId));
            _projectPageId = Convert.ToInt32(adapterTest.GetProjectPageID(Convert.ToInt32(Convert.ToInt32(testId))));

            _projectId = Convert.ToInt32(adapterGuiproject.GetProjectID(_projectPageId));

            if (testresult)
                LogResult(_runExecutionId, _batchId, _flowId, Convert.ToInt32(testId), 0,
                          testname + " Test Passed  with data row : " + inputDataRow, Constants.Passed, "0", _projectId,
                          _projectPageId);
            else
            {
                LogResult(_runExecutionId, _batchId, _flowId, Convert.ToInt32(testId), 0,
                          testname + " Test Failed  with data row : " + inputDataRow, Constants.Failed, "0", _projectId,
                          _projectPageId);
                //LogFailiure(_runExecutionID, flowID, null, Convert.ToInt32(TestId), test_name, "Description in the step");
            }
            return testresult;
        }

        internal bool ExecuteOneScenario(string scenarioId, IWebDriver driver)
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
            LogResult(_runExecutionId, _batchId, _flowId, 0, 0, scenarioname + " Scenario Started", Constants.Done, "0",
                      _projectId, 0);
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

                bool testresult = ExecuteOneTest(testId, row["InputDataRow"].ToString(), driver);
                if (testresult == false)
                    scenarioresult = false;
            }

            if (scenarioresult)
                LogResult(_runExecutionId, _batchId, _flowId, 0, 0, scenarioname + " Scenario Passed", Constants.Passed,
                          "0", _projectId, 0);
            else
            {
                LogResult(_runExecutionId, _batchId, _flowId, 0, 0, scenarioname + " Scenario Failed", Constants.Failed,
                          "0", _projectId, 0);
                //LogFailiure(_runExecutionID, flowID, scenario_name, 0, null, "Description in the test");
            }
            return scenarioresult;
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
            SetAllScenariusStatusToRunorNot(dtScenario, Constants.Pending);
            var adapterBatches = new BatchesTableAdapter();
            var adapterBatchLogic = new BatchLogicTableAdapter();
            var adapterBrowser = new BrowsersTableAdapter();
            string batchname = adapterBatches.GetBatchName(_batchId);
            var adapterScenario = new ScenarioTableAdapter();

            _projectId = Convert.ToInt32(adapterBatches.GetProjectID(Convert.ToInt32(_batchId)));

            LogResult(_runExecutionId, Convert.ToInt32(_batchId), 0, 0, 0, batchname + " Batch Started", Constants.Done,
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
                threadFinishEvents.Add(threadFinish);
                string browserId = row["BrowserID"].ToString();
                string batchLogicId = row["BatchLogicID"].ToString();
                
                
                string browserName = adapterBrowser.GetBrowserName(Convert.ToInt32(browserId));
                IWebDriver driver = null;
                ThreadStart ts = delegate
                    {
                        driver = sel.SetWebDriverBrowser(driver, browserId, false);
                        if (driver == null)
                        {
                            return;
                        }
                        

                        string scenarioId = row["ScenarioID"].ToString();

                        adapterBatchLogic.Update(_batchId, Convert.ToInt32(scenarioId), Convert.ToInt32(browserId), Constants.Running,Convert.ToInt32(batchLogicId));
                        
                        string scenarioname = adapterScenario.GetScenarioName(Convert.ToInt32(scenarioId));
                        LogResult(_runExecutionId, Convert.ToInt32(_batchId), 0, 0, 0,
                                  scenarioname + " Scenario will run on " + browserName + " browser.", Constants.Done,
                                  "0", _projectId, 0);


                        // Do long work here
                        bool scanearioresult = ExecuteOneScenario(scenarioId, driver);
                        Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Normal, (EventHandler)
                            delegate
                                    {
                                        if (scanearioresult == false)
                                        {
                                            myInstance.BatchResult = false;
                                            adapterBatchLogic.Update(_batchId, Convert.ToInt32(scenarioId),
                                                                     Convert.ToInt32(browserId), Constants.Failed,
                                                                     Convert.ToInt32(batchLogicId));
                                        }
                                        else
                                        {
                                            adapterBatchLogic.Update(_batchId, Convert.ToInt32(scenarioId), Convert.ToInt32(browserId), Constants.Passed, Convert.ToInt32(batchLogicId));
                                        }
                                        driver.Quit();
                                        threadFinish.Set();
                                    }, null, null);
                    };

                ts.BeginInvoke(delegate(IAsyncResult aysncResult) { ts.EndInvoke(aysncResult); }, null);
                if (remote == "no")
                    WaitHandle.WaitAll(threadFinishEvents.ToArray(), Convert.ToInt32(timeoutAllScenarios));
            }

            WaitHandle.WaitAll(threadFinishEvents.ToArray(), Convert.ToInt32(timeoutAllScenarios));
            if (myInstance.BatchResult)
                LogResult(_runExecutionId, Convert.ToInt32(_batchId), 0, 0, 0, batchname + " Batch Passed",
                          Constants.Passed, "0", _projectId, 0);
            else
            {
                LogResult(_runExecutionId, Convert.ToInt32(_batchId), 0, 0, 0, batchname + " Batch Failed",
                          Constants.Failed, "0", _projectId, 0);
                //LogFailiure(_runExecutionID, 0, null, 0, null, "Description in the scenario");
            }

            SetAllScenariusStatusToRunorNot(dtScenario,Constants.NotRunnig);
            return myInstance.BatchResult;
        }


        internal bool HighLight(int guiMapTagTypeId, string guiMapTagTypeValue, IWebDriver driver)
        {
            if (guiMapTagTypeValue == null) throw new ArgumentNullException("guiMapTagTypeValue");
            var sl = new Selenium();
            bool result = sl.DrawBorder(guiMapTagTypeId, guiMapTagTypeValue, driver);
            if (result == false)
                MessageBox.Show("can't highlight this object", "Applenium");
            return result;
        }

        private static int LogResult(int runExecutionId, int batchId, int scenarioId, int testId, int stepId,
                                     string description, int status, string outOfFlow, int projectId, int projectPageId)
        {
            int result;
            DateTime occurred = DateTime.Now.ToUniversalTime();
            using (var adapterLogresult = new TestResultsTableAdapter())
            {
                result = adapterLogresult.Insert(runExecutionId, batchId, scenarioId, testId, stepId, description,
                                                 status, outOfFlow, projectId, projectPageId, occurred);
            }
            if (status == Constants.Passed)
                Logger.Passed(runExecutionId + "," + batchId + "," + scenarioId + "," + testId + "," + stepId + "," +
                              description + "," + status + "," + outOfFlow + "," + projectId + "," + projectPageId);
            if (status == Constants.Done)
                Logger.Done(runExecutionId + "," + batchId + "," + scenarioId + "," + testId + "," + stepId + "," +
                              description + "," + status + "," + outOfFlow + "," + projectId + "," + projectPageId);
            if(status==Constants.Failed)
                 Logger.Failed(runExecutionId + "," + batchId + "," + scenarioId + "," + testId + "," + stepId + "," +
                              description + "," + status + "," + outOfFlow + "," + projectId + "," + projectPageId);
            
            return result;
        }
    }
}