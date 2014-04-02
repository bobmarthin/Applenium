using System;
using System.Reflection;
using Applenium._3___DAL.DataSetAutoTestTableAdapters;
using log4net;
using log4net.Config;
using LoggerDLL;

namespace Applenium
{
    /// <summary>
    ///     Logger - public class to write Logs
    /// </summary>
    public class AppleniumLogger
    {


        /// <summary>
        /// Default constractor
        /// </summary>
        public AppleniumLogger()
        {

        }

        /// <summary>
        /// Prints the results of a step execution
        /// </summary>
        /// <returns>return a flag to indicate success or error</returns>
        public int Print(LogObject log)
        {

            DateTime occurred = DateTime.Now.ToUniversalTime();
            int result = -1;

            string dateFromUnixTime = ConvertFromUnixTimestamp(log.ExecutionID).ToString("dd/MM/yyyy_HH:mm:ss");

         dateFromUnixTime = dateFromUnixTime.Replace(" ", "_");

            switch (log.StatusTag)
            {

                case ResultStatus.INFO: ClientLogger logger = new ClientLogger();
                    logger.Description = log.Description;
                    logger.StatusTag = log.StatusTag;
                    logger.printLog(log.StatusTag);
                    insertToDB(log);
                    break;

                case ResultStatus.ERROR: ClientLogger loggerError = new ClientLogger();
                    loggerError.Description = log.Description + " - " + log.StepName;
                    loggerError.StatusTag = log.StatusTag;
                    loggerError.ExecutionID = log.ExecutionID;
                    loggerError.BatchName = log.BatchName;
                    loggerError.BatchStatus = log.BatchStatus;
                    loggerError.ScnearioName = log.ScnearioName;
                    loggerError.ScenarioStatus = log.ScenarioStatus;
                    loggerError.TestName = log.TestName;
                    loggerError.SnapShot = log.Snapshot;
                    loggerError.TestStatus = log.TestStatus;
                    loggerError.StepName = log.StepName;
                    loggerError.StepStatus = log.StepStatus;
                    loggerError.ProjectName = log.ProjectName;
                    loggerError.ProjectPageName = log.ProjectPageName;
                    loggerError.ExecutionTimeStamp = dateFromUnixTime;
                    loggerError.printLog(log.StatusTag);
                    insertToDB(log);
                    break;


                case ResultStatus.FAILED: ClientLogger loggerFailed = new ClientLogger();
                    loggerFailed.Description = log.Description;
                    loggerFailed.StatusTag = log.StatusTag;
                    loggerFailed.ExecutionID = log.ExecutionID;
                    loggerFailed.BatchName = log.BatchName;
                    loggerFailed.BatchStatus = log.BatchStatus;
                    loggerFailed.ScnearioName = log.ScnearioName;
                    loggerFailed.ScenarioStatus = log.ScenarioStatus;
                    loggerFailed.TestName = log.TestName;
                    loggerFailed.TestStatus = log.TestStatus;
                    loggerFailed.SnapShot = log.Snapshot;
                    loggerFailed.StepName = log.StepName;
                    loggerFailed.StepStatus = log.StepStatus;
                    loggerFailed.ProjectName = log.ProjectName;
                    loggerFailed.ProjectPageName = log.ProjectPageName;
                    loggerFailed.ExecutionTimeStamp = dateFromUnixTime;
                    loggerFailed.printLog(log.StatusTag);
                    insertToDB(log);
                    break;

                case ResultStatus.PASSED: ClientLogger loggerPassed = new ClientLogger();
                    loggerPassed.Description = log.Description;
                    loggerPassed.StatusTag = log.StatusTag;
                    loggerPassed.ExecutionID = log.ExecutionID;
                    loggerPassed.BatchName = log.BatchName;
                    loggerPassed.BatchStatus = log.BatchStatus;
                    loggerPassed.ScnearioName = log.ScnearioName;
                    loggerPassed.ScenarioStatus = log.ScenarioStatus;
                    loggerPassed.TestName = log.TestName;
                    loggerPassed.TestStatus = log.TestStatus;
                    loggerPassed.StepName = log.StepName;
                    loggerPassed.StepStatus = log.StepStatus;
                    loggerPassed.ProjectName = log.ProjectName;
                    loggerPassed.ProjectPageName = log.ProjectPageName;
                    loggerPassed.ExecutionTimeStamp = dateFromUnixTime;
                    loggerPassed.printLog(log.StatusTag);
                    insertToDB(log);
                    break;

                case ResultStatus.DONE: ClientLogger loggerDone = new ClientLogger();
                    loggerDone.Description = log.Description;
                    loggerDone.StatusTag = log.StatusTag;
                    loggerDone.ExecutionID = log.ExecutionID;
                    loggerDone.BatchName = log.BatchName;
                    loggerDone.BatchStatus = log.BatchStatus;
                    loggerDone.ScnearioName = log.ScnearioName;
                    loggerDone.ScenarioStatus = log.ScenarioStatus;
                    loggerDone.TestName = log.TestName;
                    loggerDone.TestStatus = log.TestStatus;
                    loggerDone.StepName = log.StepName;
                    loggerDone.StepStatus = log.StepStatus;
                    loggerDone.ProjectName = log.ProjectName;
                    loggerDone.ProjectPageName = log.ProjectPageName;
                    loggerDone.ExecutionTimeStamp = dateFromUnixTime;
                    loggerDone.printLog(log.StatusTag);
                    insertToDB(log);
                    break;

                default: ClientLogger defaultLog = new ClientLogger();
                    defaultLog.Description = "No status code on log event.";
                    defaultLog.StatusTag = 5;
                    defaultLog.ScnearioName = log.ScnearioName;
                    defaultLog.printLog(5);
                    break;




            }


            return result;
        }

        private int insertToDB(LogObject log)
        {
            int result = -1;
            DateTime occurred = DateTime.Now.ToUniversalTime();
            // Insert log to database
            using (var adapterLogresult = new TestResultsTableAdapter())
            {
                result = adapterLogresult.Insert(log.ExecutionID, log.BatchID, log.ScenarioID, log.TestID, log.StepID, log.Description,
                                                 log.StatusTag, "0", log.ProjectID, log.ProjectPageID, occurred);
            }


            return result;

        }

        private DateTime ConvertFromUnixTimestamp(double timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddSeconds(timestamp);
        }
    }

    public class LogObject
    {
        protected int _LogExecutionId;
        protected string _logExecutionTimeStamp = "N/A";
        protected string _LogBatchName = "N/A";
        protected string _LogBatchProgress = "N/A";
        protected int _LogBatchID;
        protected string _LogScenarioName = "N/A";
        protected int _LogScenarioID;
        protected string _LogTestName = "N/A";
        protected int _LogTestID;
        protected string _LogStepName = "N/A";
        protected int _LogStepID = 0;
        protected string _PassedParameters1 = "N/A";
        protected string _PassedParameters2 = "N/A";
        protected string _GuiCommandName = "N/A";
        protected string _LogDescription = "N/A";
        protected string _LogScenarioStatus = "N/A";
        protected string _LogTestStatus = "N/A";
        protected string _SnapShot = "N/A";
        protected string _IISserver = "N/A";
        protected string _LogStepStatus = "N/A";
        protected string _LogBatchStatus = "N/A";
        protected string _LogProjectName = "N/A";
        protected string _LogProjectPageName = "N/A";
        protected string _LogNothing = "N/A";
        protected int _ProjectPageID;
        protected int _ProjectID;
        protected Exception _LogException = null;
        protected int _StatusTag = 0;
        protected string BrowserType = "N/A";

        public string ExecutionTimeStamp { get { return _logExecutionTimeStamp; } set { _logExecutionTimeStamp = value; } }
        public int BatchID { get { return _LogBatchID; } set { _LogBatchID = value; } }
        public int ScenarioID { get { return _LogScenarioID; } set { _LogScenarioID = value; } }
        public int TestID { get { return _LogTestID; } set { _LogTestID = value; } }
        public int StepID { get { return _LogStepID; } set { _LogStepID = value; } }
        public string Snapshot { get { return _SnapShot; } set { _SnapShot = value; } }
        public string Parameter1 { get { return _PassedParameters1; } set { _PassedParameters1 = value; } }
        public string Parameter2 { get { return _PassedParameters2; } set { _PassedParameters2 = value; } }
        public string CommandName { get { return _GuiCommandName; } set { _GuiCommandName = value; } }
        public string IISserver { get { return _IISserver; } set { _IISserver = value; } }
        public int ExecutionID { get { return _LogExecutionId; } set { _LogExecutionId = value; } }
        public string BatchName { get { return _LogBatchName; } set { _LogBatchName = value; } }
        public string ScnearioName { get { return _LogScenarioName; } set { _LogScenarioName = value; } }
        public string TestName { get { return _LogTestName; } set { _LogTestName = value; } }
        public string StepName { get { return _LogStepName; } set { _LogStepName = value; } }
        public string Description { get { return _LogDescription; } set { _LogDescription = value; } }
        public string ScenarioStatus { get { return _LogScenarioStatus; } set { _LogScenarioStatus = value; } }
        public string TestStatus { get { return _LogTestStatus; } set { _LogTestStatus = value; } }
        public string StepStatus { get { return _LogStepStatus; } set { _LogStepStatus = value; } }
        public string BatchStatus { get { return _LogBatchStatus; } set { _LogBatchStatus = value; } }
        public string ProjectName { get { return _LogProjectName; } set { _LogProjectName = value; } }
        public string ProjectPageName { get { return _LogProjectPageName; } set { _LogProjectPageName = value; } }
        public string LogNothing { get { return _LogNothing; } set { _LogNothing = value; } }
        public Exception Exception { get { return _LogException; } set { _LogException = value; } }
        public int StatusTag { get { return _StatusTag; } set { _StatusTag = value; } }
        public int ProjectID { get { return _ProjectID; } set { _ProjectID = value; } }
        public int ProjectPageID { get { return _ProjectPageID; } set { _ProjectPageID = value; } }
        public string BatchProgress { get { return _LogBatchProgress; } set { _LogBatchProgress = value; } }
        public string Browser { get { return BrowserType; } set { BrowserType = value; } }

    }

}