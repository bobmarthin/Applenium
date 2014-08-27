using System.Collections.Generic;

namespace Applenium
{
    /// <summary>
    ///     All constants is described in that class
    /// </summary>
    internal static class Constants
    {
        public const string StrGuiMap = "GuiMap";
        public const string StrGuiMapEdit = "GuiMapEdit";
        public const string StrGuiTest = "GuiTest";
        public const string StrGuiTestSteps = "GuiTestSteps";
        public const string StrInputTable = "InputTable";
        public const string StrInputScenarioTable = "InputScenarioTable";
        public const string StrInputBatchTable = "InputBatchTable";
        public const string StrGuiProjects = "GuiProjects";
        public const string StrTestStepsToSelenium = "TestStepsToSelenium";
        public const string StrGuiScenario = "GuiScenario";
        public const string StrGuiScenarioLogic = "GuiScenarioLogic";
        public const string StrGuiTagType = "GuiTagType";
        public const string StrPopUpNewTest = "PopUpNewTest";
        public const string StrGuiProjectsNewTest = "GuiProjectsNewTest";
        public const string StrGuiProjectsNewScenario = "GuiProjectsNewScenario";
        public const string StrPopUpNewScenario = "PopUpNewScenario";
        public const string StrScenarioTestsToSelenium = "ScenarioTestsToSelenium";
        public const string StrTables = "Tables";
        public const string StrColumns = "Columns";
        public const string StrGuiBatch = "GuiBatch";
        public const string StrGuiBatchLogic = "GuiBatchLogic";
        public const string StrPopUpNewBatch = "PopUpNewBatch";
        public const string StrBatchScenariosToSelenium = "BatchScenariosToSelenium";
        public const string StrTestViewer = "TestViewer";
        public const string StrScenarioViewer = "ScenarioViewer";
        public const string StrRowsId = "RowsID";
        public const string StrGuiProjectsNewBatch = "GuiProjectsNewBatch";
        public const string StrLogResults = "LogResults";
        public const string StrEnvironmentVersion = "EnvironmentVersion";
        public const string StrEnvironmentVersionClone = "StrEnvironmentVersionClone";
        public const string StrAnalayzing = "StrAnalayzing";
        public const string StrEnvironmentVersionMove = "StrEnvironmentVersionMove";

        
        public const int PASSED = 1;
        public const int FAILED = 2;
        public const int DONE = 3;
        public const int EXECPTION = 4;
        public const int ERROR = 5;
        public const int INFO = 6;
        public const int DEBUG = 7;
        public const int FATAL = 8;



        //public const int Running = 6;
        //public const int NotRunnig = 7;
        //public const int Pending = 8;
        public const int Checked = 1;
        public const int UnChecked = 0;


        public const string RegularExpressionAnyValue = "!.*";
        public const string RegularExpressionRandom = "!Rand";
        public const string RegularExpressionOutput = "!Out";
        public const string RegularExpressionTestingEnvironment = "!TestEnv";
        public const string RegularExpressionTestingEnvironmentBackend = "!BackEnv";


        public const string CompletedTestImage = "../checked_icon.png";
        public const string UncompletedTestImage = "../red_x.png";
        public const int ObOpenTradedInstrumentDisplayName = 211;
        public const int ObOpenTardesParentNameGuiMapId = 228;
        public const int ObOpenTradesUserName = 231;
        public const int ObOpenTardesInitRateStringGuimapid = 232;
        public const int ObOpenTradesIsBuyString = 233;
        public const int ObOpenTradesIsOverWeekendStocks = 256;
        public const int ObOpenTradesIsOverWeekendNonStocks = 257;
        public const int ObOpenTradesLimitRate = 258;
        public const int ObOpenTradeStopRate = 259;
        public const int ObOpenTradesInstrument = 260;
        public const int ObOpenTaredsInfoRowPending = 1768;


        public const string CommenterBadge = "CommenterBadge";
        public const string CommentReceiverBadge = "CommentReceiverBadge";
        public const string GuruBadge = "GuruBadge";
        public const string ConversationCreatorBadge = "ConversationCreatorBadge";
        public const string CopyTraderBadge = "CopyTraderBadge";
        public const string PioneerBadgeBadge = "PioneerBadgeBadge";

        public const string OpenBookUser = "OpenBook";
        public const string FtdUserNoAff = "FTDwithNoAffiliate";
        public const string FtdUserRespQuestionNoAff = "FTDwithRespQuestionNoAffiliate";
        public const string FtdUserWithAff = "FTDwithAffiliate";
        public const string FacebookTestUser = "FaceBookTestUser";
        public const string FacebookTestUserFriends = "FaceBookTestUserFriends";

        //public const string FacebookAppId = "166209726726710";
        //public const string FacebookAppSecret = "c779137599ec0fe1b9fb35975783fc9a";

        public const string LogExecutionId = "ExecutionID";
        public const string LogBatchName = "Batch";
        public const string LogScenarioName = "Scenario";
        public const string LogTestName = "Test";
        public const string LogStepName = "Step";
        public const string LogDescription = "Description";
        public const string LogScenarioStatus = "ScenarioStatus";
        public const string LogTestStatus = "TestStatus";
        public const string LogStepStatus = "StepStatus";
        public const string LogBatchStatus = "BatchStatus";
        public const string LogProjectName = "Project";
        public const string LogProjectPageName = "ProjectPage";
        public const string LogNothing = "N/A";

        public const string PROGRESS_STARTED = "Started";
        public const string PROGRESS_FINISHED = "Finished";
        public const string PROGRESS_PASSED = "Passed";
        public const string PROGRESS_FAILED = "Failed";

        // Streams calls

        public const string ACTION_DISCUSSION = "discussions";
        public const string ACTION_LIKE = "likes/";
        public const string ACTION_GET_USER_STREAM = "streams/users/";


        // User messages
        public const string STOPPED_EXPLICIT = "********* Stopped by user **********";
        public const string TEST_RUN_FAILED = "Test is failed. Press OK to see results. Cancel to stay on the same window. ";
        public const string TEST_RUN_PASSED = "Test is passed. Press OK to see results. Cancel to stay on the same window.";
        public const string SCENARIO_RUN_FAILED = "Scenario is failed. Press OK to see results. Cancel to stay on the same window. ";
        public const string SCENARIO_RUN_PASSED = "Scenario is passed. Press OK to see results. Cancel to stay on the same window.";
        public const string SCENARIO_RUN_TITLE = "Run evaluation result";
        public const string WEBDRIVER_BUSY = "WebDriver is busy. Wait a few seconds and try again!";
        public const string WEBDRIVER_BUSY_TITLE = "WebDriver Busy";
        public const string WEBDRIVER_EXECUTING = "WebDriver is still executing, do you wanna stop execution and run this test?";


        // ProgressLabelUpdate message types
        public const int UpdateProgress_REGULAR = 0;
        public const int UpdateProgress_STOPPED = 1;
        public const int UpdateProgress_EMPTY = 2;


        public const string Memory = "memory";
        //command constant 
        public const string SshTemplate = "ssh";
        public const string XpathTemplate = "xpath";
        public const string IdTemplate = "id";
        public const string NameTemplate = "name";
        public const string CssTemplate = "css";
        public const string CmdNameTemplate = "cmdname";

        /// <summary>
        ///     list for save global variables during test execution
        /// </summary>
        public static Dictionary<string, string> MemoryConf = new Dictionary<string, string>();
       
        

    }
}