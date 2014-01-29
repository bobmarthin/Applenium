using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Applenium.DataSetAutoTestTableAdapters;
using Applenium.Properties;

namespace Applenium
{
    internal class Sql
    {
        private readonly string _batchScenariosToSelenium = SQLResources.Query_BatchScenariosToSelenium;
        private readonly string _columns = SQLResources.Query_Columns;
        private readonly string _guiBatch = SQLResources.Query_GuiBatchSelect;
        private readonly string _guiBatchLogic = SQLResources.Query_GuiBatchLogicSelect;
        private readonly string _guiMap = SQLResources.Query_GuiMapSelect;
        private readonly string _guiProject = SQLResources.Query_GuiProjectsSelect;
        private readonly string _guiScenario = SQLResources.Query_GuiScenarioSelect;
        private readonly string _guiScenarioLogic = SQLResources.Query_GuiScenarioLogicSelect;
        private readonly string _guiTagType = SQLResources.Query_TagTypeSelect;
        private readonly string _guiTest = SQLResources.Query_GuiTestSelect;
        private readonly string _guiTestSteps = SQLResources.Query_GuiTestStepsSelect;
        private readonly string _inputTable = SQLResources.Query_InputTableNameSelect;
        private readonly string _rowsId = SQLResources.Query_RowsID;
        private readonly string _scenarioTestsToSelenium = SQLResources.Query_ScenarioTestsToSelenium;
        private readonly string _tables = SQLResources.Query_Tables;
        private readonly string _testStepsToSlenium = SQLResources.Query_TestStepsToSlenium;
        private string conString = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
        public DataTable GetDataTable(string tableName, string input)
        {
            
            var dt = new DataTable();
            bool isTable = true;
            try
            {
                using (var con = new SqlConnection(conString))
                {
                    string query = string.Empty;
                    switch (tableName)
                    {
                        case Constants.StrGuiMap:
                            query = _guiMap;
                            break;
                        case Constants.StrGuiMapEdit:
                            query = _guiMap;

                            break;
                        case Constants.StrPopUpNewTest:
                            query = _guiTest;
                            break;
                        case Constants.StrPopUpNewScenario:
                            query = _guiScenario;
                            break;
                        case Constants.StrPopUpNewBatch:
                            query = _guiBatch;
                            break;
                        case Constants.StrGuiTest:
                            query = _guiTest;
                            break;
                        case Constants.StrGuiTagType:
                            query = _guiTagType;
                            break;
                        case Constants.StrGuiScenario:
                            query = _guiScenario;
                            break;
                        case Constants.StrGuiBatch:
                            query = _guiBatch;
                            break;
                        case Constants.StrGuiTestSteps:
                            query = _guiTestSteps;
                            if (!string.IsNullOrEmpty(input))
                            {
                                query = query + " Where GuiTestID= '" + input + "'";
                            }
                            break;
                        case Constants.StrTestViewer:
                            query = _guiTestSteps;
                            if (!string.IsNullOrEmpty(input))
                            {
                                query = query + " where GuiTestID= '" + input + "'";
                            }
                            break;
                        case Constants.StrGuiScenarioLogic:
                            query = _guiScenarioLogic;
                            if (!string.IsNullOrEmpty(input))
                            {
                                query = query + " Where GuiScenarioID= '" + input + "'";
                            }
                            break;
                        case Constants.StrGuiBatchLogic:
                            query = _guiBatchLogic;
                            if (!string.IsNullOrEmpty(input))
                            {
                                query = query + " Where BatchID= '" + input + "'";
                            }
                            break;
                        case Constants.StrInputTable:
                        case Constants.StrInputScenarioTable:
                            query = _inputTable;
                            if (!string.IsNullOrEmpty(input))
                            {
                                string testInputTableName = GetInputDataTableName(input, true);
                                if (testInputTableName == string.Empty)
                                {
                                    testInputTableName = GetTestName(input);
                                }
                                if (IsTableExists(testInputTableName, false))
                                {
                                    query = "select * from QA_Autotest.Test." + testInputTableName;
                                }
                                else
                                {
                                    isTable = false;
                                }
                            }
                            break;

                        case Constants.StrTables:
                            query = _tables;
                            break;
                        case Constants.StrColumns:
                            query = _columns + " where TABLE_NAME = '" + input + "'";
                            break;
                        case Constants.StrRowsId:

                            if (IsTableExists(input, false))
                            {
                                query = _rowsId + "QA_Autotest.Test." + input;
                            }
                            else
                            {
                                isTable = false;
                            }

                            break;
                        case Constants.StrGuiProjects:
                            query = _guiProject;
                            break;
                        case Constants.StrGuiProjectsNewTest:
                            query = _guiProject;
                            break;
                        case Constants.StrGuiProjectsNewScenario:
                            query = _guiProject;
                            break;
                        case Constants.StrGuiProjectsNewBatch:
                            query = _guiProject;
                            break;
                        case Constants.StrTestStepsToSelenium:
                            query = _testStepsToSlenium;
                            if (!string.IsNullOrEmpty(input))
                            {
                                query = query + " Where GuiTestID= '" + input + "' ORDER BY StepsOrder";
                            }
                            break;
                        case Constants.StrScenarioTestsToSelenium:
                            query = _scenarioTestsToSelenium;
                            if (!string.IsNullOrEmpty(input))
                            {
                                query = query + " Where GuiScenarioID= '" + input + "' ORDER BY StepsOrder";
                            }
                            break;
                        case Constants.StrBatchScenariosToSelenium:
                            query = _batchScenariosToSelenium;
                            if (!string.IsNullOrEmpty(input))
                            {
                                query = query + " Where BatchID= '" + input + "'";
                            }
                            break;
                        case Constants.StrLogResults:
                            query = _guiProject;

                            break;
                        default:
                            Logger.Error("Default case");
                            break;
                    }
                    string cmdString = query;
                    var cmd = new SqlCommand(cmdString, con);
                    var sda = new SqlDataAdapter(cmd);
                    if (isTable)
                    {
                        dt = new DataTable(tableName);
                        sda.Fill(dt);
                    }
                    else
                    {
                        dt = null;
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message, exception);
            }

            return dt;
        }

        public string GetInputDataTableName(string testId, bool returnTestNameIfNotExists)
        {
            string inputTableName;
            using (var adapterTest = new TestTableAdapter())
            {
                inputTableName = adapterTest.GetInputTableName(Convert.ToInt32(testId));
            }
            if (inputTableName == null)
            {
                if (returnTestNameIfNotExists)
                    inputTableName = GetTestName(testId);
            }

            return inputTableName;
        }

        private bool IsTableExists(string tableName, bool isCreateNew)
        {
            bool returnval;
            //string conString = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;

            var con = new SqlConnection(conString);
            string cmdString = SQLResources.Query_Tables + " where name = '" + tableName + "'";

            var cmd = new SqlCommand(cmdString, con);
            con.Open();
            using (con)
            {
                object data = cmd.ExecuteScalar();
                if (data != null)
                {
                    returnval = true;
                }
                else
                {
                    if (isCreateNew)
                    {
                        var stc = new SqlTableCreator(con);
                        stc.Create(tableName, 10);
                    }
                    returnval = false;
                }
            }

            return returnval;
        }

        public string GetTestName(string testId)
        {
            string query = _guiTest + " where TestId= '" + testId + "'";

            return ExecuteQuery(query);
        }

        private string GetDbCellValue(string query)
        {
            return ExecuteQuery(query);
        }


        public string GetDbSingleValue(string ConString, string query)
        {
            string returnval = string.Empty;
            var con = new SqlConnection(ConString);
            string cmdString = query;
            var cmd = new SqlCommand(cmdString, con);
            con.Open();
            using (con)
            {
                object data = cmd.ExecuteScalar();
                if (data != null)
                {
                    returnval = data.ToString().Trim();
                }
            }
            return returnval;
        }
        private string ExecuteQuery(string query)
        {
            string returnval = string.Empty;
            var con = new SqlConnection(conString);
            string cmdString = query;
            var cmd = new SqlCommand(cmdString, con);
            con.Open();
            using (con)
            {
                object data = cmd.ExecuteScalar();
                if (data != null)
                {
                    returnval = data.ToString().Trim();
                }
            }
            return returnval;
        }


        public string InputDataValue(string tablename, string rownumber, string columnname)
        {
            string value;
            if ((columnname != string.Empty) && (rownumber != string.Empty))
            {
                string query = "select " + columnname + " from QA_Autotest.Test." + tablename + " where " + "rowID =" +
                               rownumber;
                value = GetDbCellValue(query);
            }
            else
            {
                value = string.Empty;
            }
            return value;
        }


        public bool CreateNewTable(string tableName)
        {
           // string conString = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
            var con = new SqlConnection(conString);
            con.Open();
            var stc = new SqlTableCreator(con);

            object obj = stc.Create(tableName, 0);
            if (obj != null)
                return true;
            return false;
        }

        public int UpdateTestTable(int testid, string testTable)
        {
            var adapterTest = new TestTableAdapter();
            int result = adapterTest.UpdateTestTable(testTable, testid);
            return result;
        }


        public string AddColumn(string tableName, string columnName,string columnType)
        {
            columnName = ValidateColumnName(columnName);
            string query = "ALTER TABLE QA_Autotest.Test." + tableName + " ADD " + columnName + " "+columnType;
           
            return ExecuteQuery(query);
        }

        public string CloneColumn(string tableName, string columnNameFrom, string columnNameToo, string columnType)
        {
            //create column 

            AddColumn(tableName, columnNameToo, columnType);
            columnNameToo = ValidateColumnName(columnNameToo);
            columnNameFrom = ValidateColumnName(columnNameFrom);

            //clone colums
            string query = "UPDATE QA_Autotest.Test." + tableName + " SET  " + columnNameToo + "=" + columnNameFrom;

            return ExecuteQuery(query);
        }


        public string CopyColumn(string tableName, string columnNameFrom, string columnNameToo)
        {
            //create column 

           
            columnNameToo = ValidateColumnName(columnNameToo);
            columnNameFrom = ValidateColumnName(columnNameFrom);

            //clone colums
            string query = "UPDATE QA_Autotest.Test." + tableName + " SET  " + columnNameToo + "=" + columnNameFrom;

            return ExecuteQuery(query);
        }

        //copy step using version column 
        public string CopyStepByVersion(string guiTestId, string stepsOrder,string versionColumn)
        {
            //copy step 
            string query = "INSERT INTO Test.GuiTestSteps (GuiTestID, GuiMapID, GuiMapCommandID, InputDataColumn, IsValidate, StepsOrder) SELECT GuiTestID, GuiMapID, GuiMapCommandID, InputDataColumn, IsValidate, StepsOrder FROM Test.GuiTestSteps WHERE (GuiTestID = " + guiTestId + ") AND (StepsOrder = " + stepsOrder + ") AND (" + versionColumn + " = 1)";
            return ExecuteQuery(query);
        }

        //GetStepsIdByVersion with version column 
        public string GetStepsIdByVersion(string guiTestId, string stepsOrder, string versionColumn)
        {
            //get step 
            string query = "SELECT GuiTestStepsID FROM Test.GuiTestSteps WHERE (GuiTestID = " + guiTestId + ") AND (StepsOrder = " + stepsOrder + ") AND (" + versionColumn + " = 1)";

            return ExecuteQuery(query);
        }

        //copy step using version column 
        public string CopyObjectByVersion(string guiMapId, string versionColumn)
        {
            //copy step 
            string query = "INSERT INTO Test.GuiMap (GuiMapObjectName, TagTypeID, TagTypeValue, GuiProjectID) SELECT GuiMapObjectName, TagTypeID, TagTypeValue, GuiProjectID FROM Test.GuiMap WHERE (GuiMapID = " + guiMapId + ")  AND (" + versionColumn + " = 1)";
            return ExecuteQuery(query);
        }

        //GetStepsIdByVersion with version column 
        public string GetGuiMapIdByVersion(string guiMapId, string versionColumn)
        {
            //get step 
            string query = "SELECT GuiMapID FROM Test.GuiMap WHERE (GuiMapID = " + guiMapId + ") AND (" + versionColumn + " = 1)";

            return ExecuteQuery(query);
        }

        //this function update Version column with true 
        //In 

        public string UpdateVersion(string tableName ,string byColumnName, string byColumnValue, string versionColumn,int ischeck)
        {       

            //Update
            string query = "UPDATE QA_Autotest.Test." + tableName + " SET  [" + versionColumn + "]="+ischeck+" where [" + byColumnName+"] ="+byColumnValue;

            return ExecuteQuery(query);
        }

        //public string AddSparceColumn(string tableName, string columnName)
        //{
        //    string query = "ALTER TABLE " + tableName + " ADD " + columnName + "bit null";

        //    return ExecuteQuery(query);
        //}

        public void InsertInputTestTable(string testTable, DataRowView rowView)
        {
            string cmdString = "INSERT INTO QA_Autotest.Test." + testTable + " (";
            for (int i = 1; i < rowView.Row.Table.Columns.Count; i++)
            {
                cmdString = cmdString + rowView.Row.Table.Columns[i].ColumnName + ",";
            }
            cmdString = cmdString.TrimEnd(new[] {',', '\n'});

            cmdString = cmdString + ") VALUES (";

            for (int i = 1; i < rowView.Row.Table.Columns.Count; i++)
            {
                string temprowvalue = rowView.Row[i].ToString().Replace("'", "''");
                cmdString = cmdString + "'" + temprowvalue + "',";
            }
            cmdString = cmdString.TrimEnd(new[] {',', '\n'});

            cmdString = cmdString + ")";


            ExecuteQuery(cmdString);
        }

        public string UpdateInputTestTable(string testTable, DataRowView rowView)
        {
            string cmdString = "UPDATE QA_Autotest.Test." + testTable + " SET ";
            for (int i = 1; i < rowView.Row.Table.Columns.Count; i++)
            {
                string temprowvalue = rowView.Row[i].ToString().Replace("'", "''");
                cmdString = cmdString + rowView.Row.Table.Columns[i].ColumnName + "='" + temprowvalue + "',";
            }
            cmdString = cmdString.TrimEnd(new[] {',', '\n'});

            cmdString = cmdString + " WHERE (" + rowView.Row.Table.Columns[0].ColumnName + "='" + rowView.Row[0] + "')";

            return ExecuteQuery(cmdString);
        }

        private string ValidateColumnName(string columnName)
        {
            if (columnName.Contains("_") || columnName.Contains("-"))
                columnName = "[" + columnName + "]";
            return columnName;
        }

        public string CopyStepAndChangeVersion(int currentStepsOrder, int guiTestId, string versionColumn)
        {
            //copy step
            var sql = new Sql();
            var guiTestStepsTableAdapter = new GuiTestStepsTableAdapter();

            sql.CopyStepByVersion(guiTestId.ToString(), currentStepsOrder.ToString(), versionColumn);
            string testStepsIdold = sql.GetStepsIdByVersion(guiTestId.ToString(), currentStepsOrder.ToString(), versionColumn);
            // unselect from previos version
            sql.UpdateVersion("GuiTestSteps", "GuiTestStepsID", testStepsIdold, versionColumn, 0);


            string testStepsId = guiTestStepsTableAdapter.GetLastGuiTestStepsID().ToString();

            //select version in new step                       
            sql.UpdateVersion("GuiTestSteps", "GuiTestStepsID", testStepsId, versionColumn, 1);
            return testStepsId;

        }
    }
}