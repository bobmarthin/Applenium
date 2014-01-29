using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Applenium.DataSetAutoTestTableAdapters;
using Microsoft.VisualBasic;
using OpenQA.Selenium;
using System.Linq;
using OpenQA.Selenium.Remote;

namespace Applenium
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private int _batchid;
        private RemoteWebDriver _driver;
        private bool _editinggrid;
        private int _guimapId;
        private int _pageSectionid;
        private int _projectPageid;
        private int _projectid;
        private int _scenarioid;
        private bool _showAllSections;
        private int _testid;
        private bool nullDriver = false;
        private string _upmessage=string.Empty;
        private bool _msgDisplayed = false;

        /// <summary>
        ///     This is main window of eToroAutoStudio
        /// </summary>
        public MainWindow()
        {
            try
            {
                InitializeComponent();
                var sel = new Selenium();
                var jp = new JsonParser();
                string browser = jp.ReadJson("DefaultBrowser");

                Logger.Info(
                    "-------------------------------------------------------------------------------------------------------------\n                                             eToroAutoStudioStarted\n-------------------------------------------------------------------------------------------------------------");

                newBrowserThread(browser);

              
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message, exception);
            }
        }


        private void DataGridFill(string tableName, string input)
        {
            try
            {
                var sql = new Sql();
                var dataset = new DataSetAutoTest();
                var adapterPageSection = new GuiPageSectionTableAdapter();
                var adapterTag = new GuiTagTypeTableAdapter();
                var adapterProject = new GuiProjectPageTableAdapter();
                var adapterGuimap = new GuiMapTableAdapter();
                var adapterTest = new TestTableAdapter();
                var adapterProjectsTest = new ProjectsTableAdapter();
                var adapterScenario = new ScenarioTableAdapter();
                var adapterBatch = new BatchesTableAdapter();
                var adapterTeststeps = new GuiTestStepsTableAdapter();
                var adapterBatchlogic = new BatchLogicTableAdapter();
                var adapterTestsclogic = new GuiScenarioLogicTableAdapter();
                var adapterTc = new TestCommandTableAdapter();
                var adapterResults = new TestResultsTableAdapter();
                var adapterAgent = new AgentMachinesTableAdapter();
                var adapterBrowser = new BrowsersTableAdapter();
                var adapterStatus = new LogStatusTableAdapter();
                var adapterTcv = new TestCommandTableAdapter();
                var adapterEnvVer = new EnvironmentVersionTableAdapter();

                var relProjectPage =
                    new DataRelation("ProjectPage", dataset.Tables["Projects"].Columns["GuiProjectID"],
                                     dataset.Tables["GuiProjectPage"].Columns["ProjectID"]);

                var relPageSection =
                    new DataRelation("PageSection", dataset.Tables["GuiProjectPage"].Columns["GuiProjectPageID"],
                                     dataset.Tables["GuiPageSection"].Columns["GuiPageID"]);

                var relSectionTest =
                    new DataRelation("SectionTest", dataset.Tables["GuiPageSection"].Columns["GuiPageSectionID"],
                                     dataset.Tables["Test"].Columns["ProjectID"]);


                var relProjectScenario =
                    new DataRelation("ProjectScenario", dataset.Tables["Projects"].Columns["GuiProjectID"],
                                     dataset.Tables["Scenario"].Columns["ProjectID"], false);

                var relProjectBatch =
                    new DataRelation("ProjectBatch", dataset.Tables["Projects"].Columns["GuiProjectID"],
                                     dataset.Tables["Batches"].Columns["ProjectID"], false);
                DataTable dt = sql.GetDataTable(tableName, input);
                DataView view = dt != null
                                    ? dt.DefaultView
                                    : null;

                DataTable dataTable=new DataTable();
                var jp = new JsonParser();
                string showTestsObjectsFromAllProjects = jp.ReadJson("ShowTestsObjectsFromAllProjects");
              
                string testingEnvironmentVersion = jp.ReadJson("TestingEnvironmentVersion");
                //build column name 
                string testingEnvironmentVersionColumn;
                
                if (adapterEnvVer.GetEnvironmnetVersionIDByVersionName(testingEnvironmentVersion) != null)
                {
                    string environmentVersionId = adapterEnvVer.GetEnvironmnetVersionIDByVersionName(testingEnvironmentVersion).ToString();
                    testingEnvironmentVersionColumn = adapterEnvVer.GetColumnByID(Convert.ToInt32(environmentVersionId)).ToString();
                    _upmessage = "Current Environmnet Version: " + testingEnvironmentVersion +
                                " . Please select  environment version of your choice .";
                }
                else
                {

                    testingEnvironmentVersionColumn = "Ver1";
                    _upmessage = "The Environmnet Version: " + testingEnvironmentVersion +
                                 " doesn't exist. Please select other environment version";

                }
                switch (tableName)
                {
                    case Constants.StrGuiMap:
                       
                        adapterGuimap.FillBy(dataset.GuiMap, Convert.ToInt32(input));
                        if( dataset.GuiMap.Select("[" + testingEnvironmentVersionColumn + "]=1").Any())
                        {
                            dataTable =
                                dataset.GuiMap.Select("[" + testingEnvironmentVersionColumn + "]=1").CopyToDataTable();
                        }
                        DataGridGuiMap.ItemsSource = dataTable.DefaultView;

                        //DataGridGuiMap.ItemsSource = dataset.GuiMap.DefaultView;

                        adapterTag.Fill(dataset.GuiTagType);
                        DataGridComboboxColumnType.ItemsSource = dataset.GuiTagType.DefaultView;

                        adapterProject.Fill(dataset.GuiProjectPage);


                        
                        break;
                    case Constants.StrGuiMapEdit:

                        adapterGuimap.FillByGuiMapID(dataset.GuiMap, Convert.ToInt32(input));
                        DataGridGuiMapEditorHidenTable.ItemsSource = dataset.GuiMap.DefaultView;
                        adapterTag.Fill(dataset.GuiTagType);
                        DataGridComboboxColumnTypeEdit.ItemsSource = dataset.GuiTagType.DefaultView;
                        if (_showAllSections)
                            adapterPageSection.FillByPageandSection(dataset.GuiPageSection, _projectid);
                        else
                            adapterPageSection.FillBy(dataset.GuiPageSection, _projectPageid);

                        DataGridComboboxColumnProjectEdit.ItemsSource = dataset.GuiPageSection.DefaultView;

                        break;
                    case Constants.StrGuiTest:
                        adapterTest.FillBy(dataset.Test, 0);
                        adapterPageSection.Fill(dataset.GuiPageSection);
                        adapterProject.Fill(dataset.GuiProjectPage);
                        adapterProjectsTest.Fill(dataset.Projects);
                        dataset.Relations.Add(relSectionTest);
                        dataset.Relations.Add(relPageSection);
                        dataset.Relations.Add(relProjectPage);
                        TreeViewTests.ItemsSource = dataset.Tables["Projects"].DefaultView;

                        break;
                    case Constants.StrPopUpNewTest:
                        adapterTest.FillByProjectID(dataset.Test, 0, _pageSectionid);
                        if (_projectPageid == 0)
                        {
                            DataGridNewTest.IsReadOnly = true;
                            DataGridNewTest.Background = Brushes.LightGray;
                        }

                        else
                            DataGridNewTest.IsReadOnly = false;
                        DataGridNewTest.ItemsSource = dataset.Test.DefaultView;
                        dt = sql.GetDataTable(Constants.StrTables, null);
                        view = dt.DefaultView;
                        if (view != null)
                        {
                            DataGridComboboxColumnTableName.ItemsSource = view;
                        }
                        if (_showAllSections)
                            adapterPageSection.FillByPageandSection(dataset.GuiPageSection, _projectid);
                        else
                            adapterPageSection.FillBy(dataset.GuiPageSection, _projectPageid);

                        DataGridComboboxColumnPageSection.ItemsSource = dataset.GuiPageSection.DefaultView;

                        break;
                    case Constants.StrPopUpNewScenario:
                        if (input == null)
                            adapterScenario.FillByProject(dataset.Scenario, _projectid);
                        else
                            adapterScenario.FillByProject(dataset.Scenario, Convert.ToInt32(input));

                        if (_projectid == 0)
                        {
                            DataGridNewScenario.IsReadOnly = true;
                            DataGridNewScenario.Background = Brushes.LightGray;
                        }
                        else
                            DataGridNewScenario.IsReadOnly = false;
                        DataGridNewScenario.ItemsSource = dataset.Scenario.DefaultView;
                        break;
                    case Constants.StrPopUpNewBatch:

                        adapterBatch.FillBy(dataset.Batches, _projectid);
                        adapterProjectsTest.Fill(dataset.Projects);
                        if (_projectid == 0)
                        {
                            DataGridNewBatch.IsReadOnly = true;
                            DataGridNewBatch.Background = Brushes.LightGray;
                        }
                        else
                            DataGridNewBatch.IsReadOnly = false;
                        DataGridNewBatch.ItemsSource = dataset.Batches.DefaultView;

                        break;
                    case Constants.StrGuiScenario:

                        adapterProjectsTest.Fill(dataset.Projects);
                        //dataset.Clear();
                        //dataset.EnforceConstraints = false;
                        //adapterScenario.FillByProject(dataset.Scenario, 4);
                        adapterScenario.Fill(dataset.Scenario);

                        dataset.Relations.Add(relProjectScenario);
                        TreeViewScenarios.ItemsSource = dataset.Tables["Projects"].DefaultView;
                        //dataset.Clear();
                        //dataset.EnforceConstraints = true;

                        break;
                    case Constants.StrGuiBatch:

                        adapterProjectsTest.Fill(dataset.Projects);
                        adapterBatch.Fill(dataset.Batches);

                        dataset.Relations.Add(relProjectBatch);
                        TreeViewBatch.ItemsSource = dataset.Tables["Projects"].DefaultView;

                        break;

                    case Constants.StrGuiTestSteps:

                        adapterTeststeps.FillBy(dataset.GuiTestSteps, Convert.ToInt32(input));
                        if (Convert.ToInt32(input) == 0)
                        {
                            DataGridTestEditor.IsReadOnly = true;
                            DataGridTestEditor.Background = Brushes.LightGray;
                        }
                        else
                            DataGridTestEditor.IsReadOnly = false;
                       
                            
                        dataTable = dataset.GuiTestSteps.Select("["+testingEnvironmentVersionColumn + "]=1").CopyToDataTable();
                        DataGridTestEditor.ItemsSource = dataTable.DefaultView;
                        //DataGridTestEditor.ItemsSource = dataset.GuiTestSteps.DefaultView;

                        adapterGuimap.Fill(dataset.GuiMap);
                        DataGridComboboxColumnGuiMap.ItemsSource = dataset.GuiMap.DefaultView;

                        adapterTc.Fill(dataset.TestCommand);
                        DataGridComboboxColumnCommand.ItemsSource = dataset.TestCommand.DefaultView;
                        string inputTableName = sql.GetInputDataTableName(input, true);

                        dt = sql.GetDataTable(Constants.StrColumns, inputTableName);
                        string inputTableNameifExists = sql.GetInputDataTableName(input, false);
                        view = dt.DefaultView;
                        if (view != null)
                        {
                            DataGridComboboxInputDataColumn.ItemsSource = view;
                        }


                        LabelTestDescription.Content = adapterTest.GetTestDescription(Convert.ToInt32(input));
                        LabelTableDescriptionCopy.Content = inputTableNameifExists;


                        break;
                    case Constants.StrTestViewer:
                        adapterTeststeps.FillBy(dataset.GuiTestSteps, Convert.ToInt32(input));
                        DataGridTestViewer.ItemsSource = dataset.GuiTestSteps.DefaultView;

                        adapterGuimap.Fill(dataset.GuiMap);
                        DataGridComboboxColumnGuiMapTestViewer.ItemsSource = dataset.GuiMap.DefaultView;

                        adapterTcv.Fill(dataset.TestCommand);
                        DataGridComboboxColumnCommandTestViewer.ItemsSource = dataset.TestCommand.DefaultView;
                        break;


                    case Constants.StrGuiScenarioLogic:

                        adapterTestsclogic.FillBy(dataset.GuiScenarioLogic, Convert.ToInt32(input));
                        if (Convert.ToInt32(input) == 0)
                        {
                            DataGridScenarioEditor.IsReadOnly = true;
                        }
                        else
                            DataGridScenarioEditor.IsReadOnly = false;
                        DataGridScenarioEditor.ItemsSource = dataset.GuiScenarioLogic.DefaultView;
                        if (showTestsObjectsFromAllProjects == "yes")
                        {
                            adapterTest.FillByAllProjects(dataset.Test);
                        }
                        else
                        {
                            adapterTest.FillByProjectIDJoined(dataset.Test, _projectid);
                        }




                        DataGridComboboxColumnTest.ItemsSource = dataset.Test.DefaultView;

                        LabelScenarioDescription.Content = adapterScenario.GetScenarioDescription(Convert.ToInt32(input));


                        break;

                    case Constants.StrGuiBatchLogic:

                        if (Convert.ToInt32(input) == 0)
                            DataGridBatchEditor.IsReadOnly = true;
                        else
                            DataGridBatchEditor.IsReadOnly = false;
                        adapterBatchlogic.FillBy(dataset.BatchLogic, Convert.ToInt32(_batchid));

                        DataGridBatchEditor.ItemsSource = dataset.BatchLogic.DefaultView;
                        _projectid = Convert.ToInt32(adapterBatch.GetProjectID(Convert.ToInt32(_batchid)));
                        adapterScenario.FillByProject(dataset.Scenario, _projectid);
                        DataGridComboboxColumnScenario.ItemsSource = dataset.Scenario.DefaultView;
                        adapterBrowser.Fill(dataset.Browsers);
                        DataGridComboboxColumnBrowser.ItemsSource = dataset.Browsers.DefaultView;
                        adapterStatus.Fill(dataset.LogStatus);
                        //DataGridComboboxColumnExecutionStatus.ItemsSource = dataset.LogStatus.DefaultView;

                        adapterAgent.Fill(dataset.AgentMachines);
                        //DataGridComboboxColumnMachine.ItemsSource =dataset.AgentMachines.DefaultView;

                        break;
                    case Constants.StrInputTable:
                        if (view != null)
                        {
                            DataGridTestEditorInputTable.ItemsSource = view;
                            DataGridTestEditorInputTable.Visibility = Visibility.Visible;
                            BtnAddInputTable.Visibility = Visibility.Hidden;

                            foreach (DataGridTextColumn column in DataGridTestEditorInputTable.Columns)
                            {
                                if (
                                    column.Header.ToString() == "rowID")
                                {
                                    column.IsReadOnly = true;
                                }
                                column.Binding = new Binding(column.Header.ToString())
                                    {
                                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                                    };
                            }
                        }
                        else
                        {
                            DataGridTestEditorInputTable.Visibility = Visibility.Hidden;
                            BtnAddInputTable.Visibility = Visibility.Visible;
                        }
                        break;
                    case Constants.StrInputScenarioTable:

                        if (view != null)
                        {
                            DataGridScenarioEditorInputTable.Visibility = Visibility.Visible;
                            DataGridScenarioEditorInputTable.ItemsSource = view;
                            string inputtablename = sql.GetInputDataTableName
                                (input, false);
                            LabelTableDescriptionScenario.Content = inputtablename;
                        }
                        else
                        {
                            DataGridScenarioEditorInputTable.Visibility = Visibility.Hidden;
                            LabelTableDescriptionScenario.Content = string.Empty;
                        }

                        break;
                    case Constants.StrGuiProjects:
                        adapterPageSection.Fill(dataset.GuiPageSection);
                        adapterProject.Fill(dataset.GuiProjectPage);
                        adapterProjectsTest.Fill(dataset.Projects);

                        dataset.Relations.Add(relPageSection);
                        dataset.Relations.Add(relProjectPage);
                        TreeViewProjects.ItemsSource = dataset.Tables["Projects"].DefaultView;

                        break;
                    case Constants.StrGuiProjectsNewTest:
                        adapterPageSection.Fill(dataset.GuiPageSection);
                        adapterProject.Fill(dataset.GuiProjectPage);
                        adapterProjectsTest.Fill(dataset.Projects);

                        dataset.Relations.Add(relPageSection);
                        dataset.Relations.Add(relProjectPage);
                        TreeViewProjectsNewTest.ItemsSource = dataset.Tables["Projects"].DefaultView;
                        break;
                    case Constants.StrGuiProjectsNewScenario:
                        //dataset.Clear();
                        //dataset.EnforceConstraints = false;
                        adapterScenario.Fill(dataset.Scenario);
                        adapterProjectsTest.Fill(dataset.Projects);
                        dataset.Relations.Add(relProjectScenario);
                        TreeViewProjectsNewScenario.ItemsSource = dataset.Tables["Projects"].DefaultView;
                        //dataset.Clear();
                        //dataset.EnforceConstraints = true;
                        break;
                    case Constants.StrGuiProjectsNewBatch:

                        adapterProjectsTest.Fill(dataset.Projects);

                        TreeViewProjectsNewBatch.ItemsSource = dataset.Tables["Projects"].DefaultView;
                        break;
                    case Constants.StrLogResults:
                        if (input != null)
                            adapterResults.FillBy(dataset.TestResults, Convert.ToInt32(input));
                        else
                            adapterResults.Fill(dataset.TestResults);
                        adapterBatch.Fill(dataset.Batches);
                        adapterScenario.Fill(dataset.Scenario);
                        adapterTest.Fill(dataset.Test);
                        adapterGuimap.Fill(dataset.GuiMap);
                        adapterStatus.Fill(dataset.LogStatus);


                        DataGridTestResults.ItemsSource = dataset.TestResults.DefaultView;

                        DataGridComboboxColumnBatchId.ItemsSource = dataset.Batches.DefaultView;
                        DataGridComboboxColumnScenarioId.ItemsSource = dataset.Scenario.DefaultView;
                        DataGridComboboxColumnTestId.ItemsSource = dataset.Test.DefaultView;
                        DataGridComboboxColumnStepId.ItemsSource = dataset.GuiMap.DefaultView;
                        DataGridComboboxColumnStatus.ItemsSource = dataset.LogStatus.DefaultView;

                        break;
                    case Constants.StrEnvironmentVersion:
                        adapterEnvVer.Fill(dataset.EnvironmentVersion);
                        ComboboxTestingEnvironment.ItemsSource = dataset.EnvironmentVersion.DefaultView;

                        break;
                    case Constants.StrEnvironmentVersionClone:
                        adapterEnvVer.Fill(dataset.EnvironmentVersion);
                        ComboboxTestingEnvironmentFromClone.ItemsSource = dataset.EnvironmentVersion.DefaultView;
                        break;
                    default:
                        Logger.Error("Default case");
                        break;
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message, exception);
            }
        }


        private void btnHighLight_Click(object sender, RoutedEventArgs e)
        {
            var manager = new ExecutionManager();

            var drv = (DataRowView)DataGridGuiMap.SelectedItem;

            try
            {
                int guiMapTagTypeId = Convert.ToInt32(drv.Row[2].ToString());
                string guiMapTagTypeValue = drv.Row[3].ToString().Trim();
                manager.HighLight(guiMapTagTypeId, guiMapTagTypeValue, _driver);
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message, exception);
            }
        }

        private void DataGridGuiMap_Selected_CellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            BtnHighLight.IsEnabled = true;

            var dataGrid = (DataGrid)sender;
            //var InputTableName = "Test";

            string cellitem = string.Empty;
            if (dataGrid.SelectedItem != null)
            {
                cellitem = dataGrid.SelectedItem.ToString();
            }
            try
            {
                if ((cellitem != "{NewItemPlaceholder}") & (cellitem != string.Empty))
                {
                    var row = (DataRowView)dataGrid.SelectedItem;


                    if (row != null)
                    {
                        int guiMapId = Convert.ToInt32(row["GuiMapID"].ToString());
                        _guimapId = guiMapId;
                        DataGridFill(Constants.StrGuiMapEdit, guiMapId.ToString(CultureInfo.InvariantCulture));
                    }
                    DataGridGuiMapEditorHidenTable.IsReadOnly = false;
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message, exception);
            }
        }

        private void DataGridGuiMap_Selected_CellsChanged_Edit(object sender, SelectedCellsChangedEventArgs e)
        {
            BtnHighLight.IsEnabled = true;

            BtnUpdateObject.IsEnabled = true;
            var dataGrid = (DataGrid)sender;

            string cellitem = string.Empty;
            if (dataGrid.SelectedItem != null)
            {
                cellitem = dataGrid.SelectedItem.ToString();
            }
            try
            {
                if ((cellitem != "{NewItemPlaceholder}") & (cellitem != string.Empty))
                {
                    var row = (DataRowView)dataGrid.SelectedItem;

                    Debug.Assert(row != null, "row != null");
                    if (row != null)
                    {
                        int guiMapId = Convert.ToInt32(row["GuiMapID"].ToString());
                        _guimapId = guiMapId;
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message, exception);
            }
        }

        private void TreeViewProjects_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                DataGridFill(Constants.StrGuiProjects, null);
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message, exception);
            }
        }

        private void SetProjectPageScetionId(object sender)
        {
            var treeView = (TreeView)sender;
            var row = (DataRowView)treeView.SelectedItem;
            var adapterTest = new TestTableAdapter();
            var adapterPageSection = new GuiPageSectionTableAdapter();
            var adapterGuiproject = new GuiProjectPageTableAdapter();
            var adapterBatch = new BatchesTableAdapter();
            var adapterScenario = new ScenarioTableAdapter();
            try
            {
                if (row.DataView.Table.Columns.Contains("TestId"))
                {
                    _testid = Convert.ToInt32(row["TestId"].ToString());
                    _pageSectionid = Convert.ToInt32(adapterTest.GetProjectPageID(_pageSectionid));
                    _projectPageid = Convert.ToInt32(adapterPageSection.GetGuiPageID(_pageSectionid));
                    _projectid = Convert.ToInt32(adapterGuiproject.GetProjectID(_projectPageid));
                    _batchid = 0;
                    _scenarioid = 0;
                }
                else if (row.DataView.Table.Columns.Contains("GuiPageSectionID"))
                {
                    _pageSectionid = Convert.ToInt32(row["GuiPageSectionID"].ToString());
                    _projectPageid = Convert.ToInt32(adapterPageSection.GetGuiPageID(_pageSectionid));
                    _projectid = Convert.ToInt32(adapterGuiproject.GetProjectID(_projectPageid));
                    _testid = 0;
                    _batchid = 0;
                    _scenarioid = 0;
                }
                else if (row.DataView.Table.Columns.Contains("GuiProjectPageID"))
                {
                    _projectPageid = Convert.ToInt32(row["GuiProjectPageID"].ToString());
                    _projectid = Convert.ToInt32(adapterGuiproject.GetProjectID(_projectPageid));
                    _pageSectionid = 0;
                    _testid = 0;
                    _batchid = 0;
                    _scenarioid = 0;
                }
                else if (row.DataView.Table.Columns.Contains("GuiProjectID"))
                {
                    _projectid = Convert.ToInt32(row["GuiProjectID"].ToString());
                    _projectPageid = 0;
                    _pageSectionid = 0;
                    _testid = 0;
                    _batchid = 0;
                    _scenarioid = 0;
                }
                else if (row.DataView.Table.Columns.Contains("BatchID"))
                {
                    _batchid = Convert.ToInt32(row["BatchID"].ToString());
                    _projectid = Convert.ToInt32(adapterBatch.GetProjectID(_batchid));
                    _projectPageid = 0;
                    _pageSectionid = 0;
                    _scenarioid = 0;
                    _testid = 0;
                }

                else if (row.DataView.Table.Columns.Contains("ScenarioID"))
                {
                    _scenarioid = Convert.ToInt32(row["ScenarioID"].ToString());
                    _projectid = Convert.ToInt32(adapterScenario.GetProjectID(_scenarioid));
                    _projectPageid = 0;
                    _pageSectionid = 0;

                    _batchid = 0;
                    _testid = 0;
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message, exception);
            }
        }

        private void TreeViewProjects_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                SetProjectPageScetionId(sender);
                DataGridFill(Constants.StrGuiMap, _pageSectionid.ToString(CultureInfo.InvariantCulture));
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message, exception);
            }
        }

        private void DataGridGuiMap_RowEditing(object sender, DataGridRowEditEndingEventArgs e)
        {
            var rowView = e.Row.Item as DataRowView;
            if (rowView == null) throw new ArgumentNullException("sender");
            try
            {
                using (var adapterGuiMap = new GuiMapTableAdapter())
                {
                    string guiMapObjectName;
                    int tagTypeId;
                    string tagTypeValue;
                    string projectPrefix;
                    string actualPrefix;
                    int guiMapId;
                    using (var adapterProject = new ProjectsTableAdapter())
                    {
                        actualPrefix = string.Empty;


                        if ((rowView.Row["GuiMapObjectName"].ToString() == string.Empty) ||
                            (rowView.Row["TagTypeID"].ToString() == string.Empty) ||
                            (rowView.Row["TagTypeValue"].ToString() == string.Empty))
                        {
                            throw new Exception("The Gui Map Object values can't be empty. Please check");
                        }
                        guiMapId = Convert.ToInt32(rowView.Row["GuiMapID"].ToString());
                        guiMapObjectName = rowView.Row["GuiMapObjectName"].ToString();
                        tagTypeId = Convert.ToInt32(rowView.Row["TagTypeID"].ToString());
                        tagTypeValue = rowView.Row["TagTypeValue"].ToString();
                        projectPrefix = adapterProject.GetProjectPrefix(_projectid).Trim();
                    }
                    if (projectPrefix.Length < guiMapObjectName.Length)
                        actualPrefix = guiMapObjectName.Substring(0, projectPrefix.Length);
                    if (actualPrefix != projectPrefix)
                        guiMapObjectName = projectPrefix + guiMapObjectName;
                    if (guiMapId == -1)
                    {
                        adapterGuiMap.Insert(guiMapObjectName, tagTypeId, tagTypeValue, _projectPageid);
                    }
                    else
                    {
                        adapterGuiMap.UpdateBy(guiMapObjectName, tagTypeId, tagTypeValue, _projectPageid, guiMapId);
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Applenium");
                Logger.Error(exception.Message, exception);
            }

            finally
            {
                DataGridFill(Constants.StrGuiMap, _projectPageid.ToString(CultureInfo.InvariantCulture));
            }
        }

        private void DataGridGuiMap_RowEditing_Edit(object sender, DataGridRowEditEndingEventArgs e)
        {
            var sql = new Sql();
            var jsonParser = new JsonParser();
            string verid;
            string versionColumn;
            string guiMapIdlast;
            var adapterEnvVer = new EnvironmentVersionTableAdapter();
            try
            {
                string actualPrefix = string.Empty;
                var rowView = e.Row.Item as DataRowView;

                var guiMapTableAdapter = new GuiMapTableAdapter();

                if (rowView != null &&
                    ((rowView.Row["GuiMapObjectName"].ToString() == string.Empty) ||
                     (rowView.Row["TagTypeID"].ToString() == string.Empty) ||
                     (rowView.Row["TagTypeValue"].ToString() == string.Empty)))
                {
                    throw new Exception("The Gui Map Object values can't be empty. Please check");
                }
                if (rowView != null)
                {
                    int guiMapId = Convert.ToInt32(rowView.Row["GuiMapID"].ToString());
                    string guiMapObjectName = rowView.Row["GuiMapObjectName"].ToString();
                    int tagTypeId = Convert.ToInt32(rowView.Row["TagTypeID"].ToString());
                    string tagTypeValue = rowView.Row["TagTypeValue"].ToString();
                    int pageSectionId;
                    if (rowView.Row["GuiProjectID"].ToString() != string.Empty)
                        pageSectionId = Convert.ToInt32(rowView.Row["GuiProjectID"].ToString());
                    else
                        pageSectionId = _pageSectionid;
                    string projectPrefix;
                    using (var adapterProject = new ProjectsTableAdapter())
                    {
                        projectPrefix = adapterProject.GetProjectPrefix(_projectid).Trim();
                    }
                    if (projectPrefix.Length < guiMapObjectName.Length)
                        actualPrefix = guiMapObjectName.Substring(0, projectPrefix.Length);
                    if (actualPrefix != projectPrefix)
                        guiMapObjectName = projectPrefix + guiMapObjectName;

                    if (guiMapId == -1)
                    {
                        guiMapTableAdapter.Insert(guiMapObjectName, tagTypeId, tagTypeValue, pageSectionId);
                        
                        //create new function in SQL that update 
                        verid = adapterEnvVer.GetEnvironmnetVersionIDByVersionName(jsonParser.ReadJson("TestingEnvironmentVersion"))
                                     .ToString();
                        versionColumn = adapterEnvVer.GetColumnByID(Convert.ToInt32(verid)).ToString();
                        guiMapIdlast = guiMapTableAdapter.GetLastGuiMapId().ToString();
                        sql.UpdateVersion("GuiMap", "GuiMapID", guiMapIdlast, versionColumn, 1);

                        DataGridFill(Constants.StrGuiMapEdit, "0");
                    }
                    else
                    {
                        
                        verid = adapterEnvVer.GetEnvironmnetVersionIDByVersionName(jsonParser.ReadJson("TestingEnvironmentVersion"))
                                    .ToString();
                        versionColumn = adapterEnvVer.GetColumnByID(Convert.ToInt32(verid)).ToString();
                        //copy test first
                        sql.CopyObjectByVersion(guiMapId.ToString(),  versionColumn);
                        string guiMapIdold = sql.GetGuiMapIdByVersion(guiMapId.ToString(), versionColumn);
                        // unselect from previos version
                        sql.UpdateVersion("GuiMap", "GuiMapID", guiMapIdold, versionColumn, 0);


                        guiMapIdlast = guiMapTableAdapter.GetLastGuiMapId().ToString();

                        //select version in new step                       
                        guiMapTableAdapter.UpdateBy(guiMapObjectName, tagTypeId, tagTypeValue, pageSectionId, Convert.ToInt32(guiMapIdlast));
                        sql.UpdateVersion("GuiMap", "GuiMapID", guiMapIdlast, versionColumn, 1);
                        DataGridFill(Constants.StrGuiMapEdit, guiMapIdlast.ToString(CultureInfo.InvariantCulture));
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Applenium");
                Logger.Error(exception.Message, exception);
            }

            finally
            {
                DataGridFill(Constants.StrGuiMap, _pageSectionid.ToString(CultureInfo.InvariantCulture));

                DataGridGuiMapEditorHidenTable.IsReadOnly = false;
            }
        }

        private void dataGridGuiMap_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                var dataGrid = (DataGrid)sender;
                var rowView = (DataRowView)dataGrid.SelectedItem;
                var guiMapTableAdapter = new GuiMapTableAdapter();
                if (e.Key == Key.Delete)
                {
                    if (_editinggrid == false)
                    {
                        int guiMapId = Convert.ToInt32(rowView.Row["GuiMapID"].ToString());
                        string guiMapObjectName = rowView.Row["GuiMapObjectName"].ToString().Trim();
                        int tagTypeId = Convert.ToInt32(rowView.Row["TagTypeID"].ToString());
                        int guiProjectId = Convert.ToInt32(rowView.Row["GuiProjectID"].ToString());
                        string tagTypeValue = rowView.Row["TagTypeValue"].ToString();

                        if (
                            MessageBox.Show("Delete " + guiMapObjectName + "?", "eToroAutoStudio",
                                            MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                            guiMapTableAdapter.Delete(guiMapId, tagTypeId,guiProjectId, tagTypeValue,  guiMapObjectName);


                    }
                }
                if (e.Key == Key.Enter)
                {
                    e.Handled = true;
                    var eInsertBack = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0,
                                                       Key.Tab);
                    eInsertBack.RoutedEvent = KeyDownEvent;
                    InputManager.Current.ProcessInput(eInsertBack);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message, exception);
            }
        }

        private void dataGridGuiMap_PreviewKeyDown_Edit(object sender, KeyEventArgs e)
        {
            var adapterEnvVer = new EnvironmentVersionTableAdapter();
            var jsonParser = new JsonParser();
            var sql = new Sql();

            try
            {
                if (e.Key == Key.Delete)
                {
                    var dataGrid = (DataGrid)sender;
                    var rowView = (DataRowView)dataGrid.SelectedItem;

                    using (var guiMapTableAdapter = new GuiMapTableAdapter())
                    {
                        if (_editinggrid == false)
                        {
                            int guiMapId = Convert.ToInt32(rowView.Row["GuiMapID"].ToString());
                            string guiMapObjectName = rowView.Row["GuiMapObjectName"].ToString().Trim();
                            int tagTypeId = Convert.ToInt32(rowView.Row["TagTypeID"].ToString());
                            int guiProjectId = Convert.ToInt32(rowView.Row["GuiProjectID"].ToString());
                            string tagTypeValue = rowView.Row["TagTypeValue"].ToString();
                            if (
                                MessageBox.Show("Delete " + guiMapObjectName + "?", "eToroAutoStudio",
                                                MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                            {
                                //adapterTest.Delete(guiMapId, tagTypeId, guiProjectId, tagTypeValue,  guiMapObjectName);
                                string verid = adapterEnvVer.GetEnvironmnetVersionIDByVersionName(
                                    jsonParser.ReadJson("TestingEnvironmentVersion"))
                                                            .ToString();
                                string versionColumn = adapterEnvVer.GetColumnByID(Convert.ToInt32(verid)).ToString();
                                
                               
                                string guiMapIdold = sql.GetGuiMapIdByVersion(guiMapId.ToString(), versionColumn);
                                // unselect from previos version
                                sql.UpdateVersion("GuiMap", "GuiMapID", guiMapIdold, versionColumn, 0);

                              
                                dataGrid.IsReadOnly = false;
                                DataGridFill(Constants.StrGuiMapEdit, "0");
                                DataGridFill(Constants.StrGuiMap, _pageSectionid.ToString(CultureInfo.InvariantCulture));
                            }
                        }
                    }
                }
                if (e.Key == Key.Enter)
                {
                    e.Handled = true;
                    var eInsertBack = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0,
                                                       Key.Tab);
                    eInsertBack.RoutedEvent = KeyDownEvent;
                    InputManager.Current.ProcessInput(eInsertBack);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message, exception);
            }
        }


        private void DataGridGuiMap_BeginingEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            _editinggrid = true;
        }

        private void DataGridGuiMap_BeginingEdit_Edit(object sender, DataGridBeginningEditEventArgs e)
        {
            _editinggrid = true;
        }

        private void DataGridGuiMap_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            _editinggrid = false;
        }

        private void DataGridGuiMap_CellEditEnding_Edit(object sender, DataGridCellEditEndingEventArgs e)
        {
            _editinggrid = false;
        }


        private void TreeViewTests_Loaded(object sender, RoutedEventArgs e)
        {
            DataGridFill(Constants.StrGuiTest, null);
        }

        private void TreeViewTests_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SetProjectPageScetionId(sender);
            try
            {
                DataGridFill(Constants.StrGuiTestSteps, _testid.ToString(CultureInfo.InvariantCulture));
                DataGridFill(Constants.StrInputTable, _testid.ToString(CultureInfo.InvariantCulture));
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message, exception);
            }
        }


        private void DataGridTestEditor_RowEditing(object sender, DataGridRowEditEndingEventArgs e)
        {
            var sql=new Sql();
            var jsonParser=new JsonParser();

            int guiMapId = 0;
            try
            {
                var rowView = e.Row.Item as DataRowView;

                var adapterEnvVer = new EnvironmentVersionTableAdapter();
                using (var guiTestStepsTableAdapter = new GuiTestStepsTableAdapter())
                {
                    if (rowView != null && rowView.Row["GuiMapID"].ToString() == string.Empty)
                    {
                        if (rowView.Row["GuiMapCommandID"].ToString() != "3" &&
                            rowView.Row["GuiMapCommandID"].ToString() != "15" &&
                            rowView.Row["GuiMapCommandID"].ToString() != "18" &&
                            rowView.Row["GuiMapCommandID"].ToString() != "19" &&
                            rowView.Row["GuiMapCommandID"].ToString() != "20" &&
                            rowView.Row["GuiMapCommandID"].ToString() != "22" &&
                            rowView.Row["GuiMapCommandID"].ToString() != "23" &&
                            rowView.Row["GuiMapCommandID"].ToString() != "24" &&
                            rowView.Row["GuiMapCommandID"].ToString() != "25" &&
                            rowView.Row["GuiMapCommandID"].ToString() != "26" &&
                            rowView.Row["GuiMapCommandID"].ToString() != "28" &&
                            rowView.Row["GuiMapCommandID"].ToString() != "29" &&
                            rowView.Row["GuiMapCommandID"].ToString() != "30" &&
                            rowView.Row["GuiMapCommandID"].ToString() != "33" &&
                            rowView.Row["GuiMapCommandID"].ToString() != "34" &&
                            rowView.Row["GuiMapCommandID"].ToString() != "35" &&
                            rowView.Row["GuiMapCommandID"].ToString() != "1041" &&
                            rowView.Row["GuiMapCommandID"].ToString() != "1042" &&
                            rowView.Row["GuiMapCommandID"].ToString() != "36")
                            throw new Exception("The Test Map Object can't be empty. Please check");
                    }
                    else if (rowView != null) guiMapId = Convert.ToInt32(rowView.Row["GuiMapID"].ToString());


                    if (rowView != null && rowView.Row["GuiMapCommandID"].ToString() == string.Empty)
                        throw new Exception("The Test Map Object Command  can't be empty. Please check");
                    if (rowView != null)
                    {
                        int guiTestStepsId;
                        if (rowView.Row["GuiTestStepsID"].ToString() == string.Empty)
                            guiTestStepsId = -1;
                        else
                            guiTestStepsId = Convert.ToInt32(rowView.Row["GuiTestStepsID"].ToString());

                        if (guiTestStepsId == -1)
                        {
                            rowView.Row["IsValidate"] = 0;
                            rowView.Row["GuiTestID"] = _testid;
                        }

                        int guiMapCommandId = Convert.ToInt32(rowView.Row["GuiMapCommandID"].ToString());
                        string inputDataRow;
                        if (rowView.Row["InputDataRow"].ToString() == string.Empty)
                        {
                            inputDataRow = null;
                        }
                        else
                        {
                            inputDataRow = rowView.Row["InputDataRow"].ToString();
                        }
                        string inputDataColumn = rowView.Row["InputDataColumn"].ToString();
                        int isValidate = Convert.ToInt32(rowView.Row["IsValidate"].ToString());
                        int guiTestId = Convert.ToInt32(rowView.Row["GuiTestID"].ToString());
                        int stepsOrder;

                        string versionColumn;
                        string verid;
                        string testStepsId;
                        if (guiTestStepsId == -1)
                        {
                            rowView.Row["IsValidate"] = 0;
                            stepsOrder = Convert.ToInt32(guiTestStepsTableAdapter.LastStepsOrder(guiTestId)) + 1;

                            guiTestStepsTableAdapter.Insert(guiTestId, guiMapId, guiMapCommandId, inputDataColumn, isValidate,
                                               stepsOrder);
                            

                            //add version to new step

                            //create new function in SQL that update 
                            verid=adapterEnvVer.GetEnvironmnetVersionIDByVersionName(jsonParser.ReadJson("TestingEnvironmentVersion"))
                                         .ToString();
                            versionColumn = adapterEnvVer.GetColumnByID(Convert.ToInt32(verid)).ToString();
                            testStepsId = guiTestStepsTableAdapter.GetLastGuiTestStepsID().ToString();
                            sql.UpdateVersion("GuiTestSteps", "GuiTestStepsID", testStepsId, versionColumn,1);
                        }
                        else
                        {
                          
                            stepsOrder = Convert.ToInt32(rowView.Row["StepsOrder"].ToString());
                            verid = adapterEnvVer.GetEnvironmnetVersionIDByVersionName(jsonParser.ReadJson("TestingEnvironmentVersion"))
                                        .ToString();
                            versionColumn = adapterEnvVer.GetColumnByID(Convert.ToInt32(verid)).ToString();
                            //copy test first
                            sql.CopyStepByVersion(guiTestId.ToString(), stepsOrder.ToString(),versionColumn);
                            string testStepsIdold = sql.GetStepsIdByVersion(guiTestId.ToString(), stepsOrder.ToString(), versionColumn);
                            // unselect from previos version
                            sql.UpdateVersion("GuiTestSteps", "GuiTestStepsID", testStepsIdold, versionColumn, 0);


                            testStepsId = guiTestStepsTableAdapter.GetLastGuiTestStepsID().ToString();
                            
                            //select version in new step                       
                            sql.UpdateVersion("GuiTestSteps", "GuiTestStepsID", testStepsId, versionColumn, 1);
                            guiTestStepsTableAdapter.UpdateBy(guiTestId, guiMapId, guiMapCommandId, inputDataRow, inputDataColumn, isValidate, stepsOrder, Convert.ToInt32(testStepsId));
                            
                        }
                    }
                }
            }

            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Applenium");
                Logger.Error(exception.Message, exception);
            }

            finally
            {
                DataGridFill(Constants.StrGuiTestSteps, _testid.ToString(CultureInfo.InvariantCulture));
            }
        }


        private void btnNewTest_Click(object sender, RoutedEventArgs e)
        {
            PopupNewTest.IsOpen = true;
        }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {

            ShowStopBtn();

            if (nullDriver == true)
            {
                MessageBox.Show("WebDriver is busy. Wait a few seconds and try again!", "No WebDriver");
                HideStopBtn();
            }

            else if (nullDriver != null)
            {

                string rowNumber = "1";
                int runExecutionId;
                using (var adapterTestresult = new TestResultsTableAdapter())
                {
                    var sql = new Sql();
                    string selectedItem = string.Empty;
                    bool result = false;
                    try
                    {
                        if (TreeViewTests.SelectedItem != null)
                        {
                            var row = (DataRowView)TreeViewTests.SelectedItem;
                            selectedItem = row["TestId"].ToString();
                        }
                        if (sql.GetInputDataTableName(selectedItem, false) != null)
                            rowNumber = Interaction.InputBox("Please select on what line to execute the test",
                                                             "Before executing tests navigate browser to right URL", "1");
                        runExecutionId = Convert.ToInt32(adapterTestresult.LastRunExecutionID()) + 1;

                        ThreadStart ts = delegate
                            {
                                // Do long work here
                                if (rowNumber != string.Empty)
                                {
                                    result = ExecuteTest(selectedItem, rowNumber.Trim(), runExecutionId, _driver);
                                }
                                Dispatcher.BeginInvoke(DispatcherPriority.Normal, (EventHandler)
                                                                                  delegate
                                                                                  {
                                                                                      HideStopBtn();
                                                                                      MessageBoxResult mbr;
                                                                                      DataGridFill(
                                                                                          Constants.StrLogResults,
                                                                                          runExecutionId.ToString(
                                                                                              CultureInfo
                                                                                                  .InvariantCulture));
                                                                                      if (result)
                                                                                          mbr =
                                                                                              MessageBox.Show(
                                                                                                  "Test is passed. Press OK to see results. Cancel to stay on the same window. ",
                                                                                                  "Run evaluation result",
                                                                                                  MessageBoxButton
                                                                                                      .OKCancel,
                                                                                                  MessageBoxImage.None,
                                                                                                  MessageBoxResult.OK,
                                                                                                  MessageBoxOptions
                                                                                                      .DefaultDesktopOnly);
                                                                                      else
                                                                                          mbr =
                                                                                              MessageBox.Show(
                                                                                                  "Test is failed. Press OK to see results. Cancel to stay on the same window. ",
                                                                                                  "Run evaluation result",
                                                                                                  MessageBoxButton
                                                                                                      .OKCancel,
                                                                                                  MessageBoxImage.Error);

                                                                                      if (mbr == MessageBoxResult.OK)
                                                                                          TabItemAnalyzingZone
                                                                                              .IsSelected = true;

                                                                                      Singleton myInstance =
                                                                                          Singleton.Instance;
                                                                                      // Will always be the same instance...
                                                                                      myInstance.StopExecution = false;
                                                                                  }, null, null);
                            };

                        ts.BeginInvoke(delegate(IAsyncResult aysncResult) { ts.EndInvoke(aysncResult); }, null);
                    }
                    catch (Exception exception)
                    {
                        Logger.Error(exception.Message, exception);
                    }
                }
            }
        }

        private void ShowStopBtn()
        {
            BtnStopTest.IsEnabled = true;
            BtnRun.IsEnabled = false;
        }

        private void HideStopBtn()
        {
            BtnStopTest.IsEnabled = false;
            BtnRun.IsEnabled = true;
        }


        private void btnStopTest_Click(object sender, RoutedEventArgs e)
        {
            Singleton myInstance = Singleton.Instance; // Will always be the same instance...
            myInstance.StopExecution = true;
        }

        private void btnStopScenario_Click(object sender, RoutedEventArgs e)
        {
            Singleton myInstance = Singleton.Instance; // Will always be the same instance...
            myInstance.StopExecution = true;
            UpdateProgressLabel("");
        }

        private void btnStopBatch_Click(object sender, RoutedEventArgs e)
        {
            Singleton myInstance = Singleton.Instance; // Will always be the same instance...
            myInstance.StopExecution = true;
            UpdateProgressLabel("");
        }

        private bool ExecuteTest(string selectedItem, string rowNumber, int runExecutionId, RemoteWebDriver driver)
        {
            var exman = new ExecutionManager(runExecutionId, this, false);
            var testStatus = new Dictionary<string, int>();
            bool result = exman.ExecuteOneTest(selectedItem, rowNumber, driver, ref testStatus);

            UpdateProgressLabel("");
            return result;
        }


       
        private void dataGridTestEditor_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var adapterEnvVer = new EnvironmentVersionTableAdapter();
            var jsonParser = new JsonParser();
            
            string verid = adapterEnvVer.GetEnvironmnetVersionIDByVersionName(jsonParser.ReadJson("TestingEnvironmentVersion"))
                    .ToString();
            string versionColumn = adapterEnvVer.GetColumnByID(Convert.ToInt32(verid)).ToString();
           
            try
            {
                if (e.Key == Key.Delete)
                {
                    var dataGrid = (DataGrid)sender;
                    var rowView = (DataRowView)dataGrid.SelectedItem;

                    using (var guiTestStepsTableAdapter = new GuiTestStepsTableAdapter())
                    {
                        var sql = new Sql();
                        
                        if (_editinggrid == false)
                        {
                            int guiTestStepsId = Convert.ToInt32(rowView.Row["GuiTestStepsID"].ToString());
                            int guiTestId = Convert.ToInt32(rowView.Row["GuiTestID"].ToString());
                            int guiMapId = Convert.ToInt32(rowView.Row["GuiMapID"].ToString());
                            //   int guiMapCommandId = Convert.ToInt32(rowView.Row["GuiMapCommandID"].ToString());
                            // string inputDataColumn = rowView.Row["InputDataColumn"].ToString();
                            
                            if (MessageBox.Show("Delete Selected Step?", "eToroAutoStudio", MessageBoxButton.YesNo) ==
                                MessageBoxResult.Yes)
                            {
                                //copy step

                                int currentStepsOrder = Convert.ToInt32(rowView.Row["StepsOrder"].ToString());
                                string  newguiTestStepsId= sql.CopyStepAndChangeVersion(currentStepsOrder, guiTestId, versionColumn);
                                

                                //reorder all steps by copieng befor reorder :
                                
                                int selectItem = currentStepsOrder - 1;
                                while (sql.GetStepsIdByVersion(guiTestId.ToString(), (currentStepsOrder + 1).ToString(), versionColumn) != string.Empty)
                                {
                                    int nextStepsOrder = currentStepsOrder + 1;
                                    //int nextGuiTestStepsId =Convert.ToInt32(guiTestStepsTableAdapter.GetStepId(guiTestId, nextStepsOrder));
                                    //copy next step
                                    int nextGuiTestStepsId= Convert.ToInt32(sql.CopyStepAndChangeVersion(nextStepsOrder, guiTestId, versionColumn));
                                    guiTestStepsTableAdapter.UpdateStepsOrder(currentStepsOrder, nextGuiTestStepsId);
                                    currentStepsOrder = nextStepsOrder;
                                }
                                //adapterTest.Delete(guiTestStepsId, guiTestId, guiMapId);

                                
                                //uncheck 
 
                                sql.UpdateVersion("GuiTestSteps", "GuiTestStepsID", newguiTestStepsId, versionColumn, 0);

                                DataGridFill(Constants.StrGuiTestSteps, _testid.ToString(CultureInfo.InvariantCulture));
                                dataGrid.SelectedItem = dataGrid.Items[selectItem];
                            }
                        }
                    }
                }
                if (e.Key == Key.Enter)
                {
                    e.Handled = true;
                    var eInsertBack = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0,
                                                       Key.Tab);
                    eInsertBack.RoutedEvent = KeyDownEvent;
                    InputManager.Current.ProcessInput(eInsertBack);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message, exception);
            }
        }

        private void DataGridTestEditor_BeginingEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            _editinggrid = true;
        }

        private void DataGridTestEditoer_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            _editinggrid = false;
        }


        private void MenuItem_AddColumn_Click(object sender, RoutedEventArgs e)
        {
            var sql = new Sql();
            try
            {
                string columname = Interaction.InputBox("Please select Column Name ", "Column Name", "Column1");
                if (columname != string.Empty)
                {
                    string tableName = sql.GetInputDataTableName(_testid.ToString(CultureInfo.InvariantCulture),
                                                                 returnTestNameIfNotExists: true);
                    sql.AddColumn(tableName, columname, "[nvarchar](MAX)");
                    DataGridFill(Constants.StrInputTable, _testid.ToString(CultureInfo.InvariantCulture));
                    DataGridFill(Constants.StrGuiTestSteps, _testid.ToString(CultureInfo.InvariantCulture));
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message, exception);
            }
        }


        private void MenuItem_AddNewTest_Click(object sender, RoutedEventArgs e)
        {
            btnNewTest_Click(null, null);
        }


        private void btnAddInputTable_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sql = new Sql();
                string tableName = sql.GetTestName(_testid.ToString(CultureInfo.InvariantCulture));
                if (tableName == null) throw new ArgumentNullException("sender");
                sql.CreateNewTable("QA_Autotest.Test." + tableName);
                sql.UpdateTestTable(_testid, tableName);
                DataGridFill(Constants.StrInputTable, _testid.ToString(CultureInfo.InvariantCulture));
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message, exception);
            }
        }


        private void DataGridTestEditor_InputTable_RowEditEditing(object sender, DataGridRowEditEndingEventArgs e)
        {
            var rowView = e.Row.Item as DataRowView;

            var sql = new Sql();
            //var TableName = sql.getTestName(testid.ToString());
            string tableName = sql.GetInputDataTableName(_testid.ToString(CultureInfo.InvariantCulture), true);
            try
            {
                if (rowView != null && rowView.Row[0].ToString() == string.Empty)
                {
                    sql.InsertInputTestTable(tableName, rowView);
                }

                else

                    sql.UpdateInputTestTable(tableName, rowView);
            }

            catch (Exception exception)
            {
                Logger.Error(exception.Message, exception);
            }

            finally
            {
                DataGridFill(Constants.StrInputTable, _testid.ToString(CultureInfo.InvariantCulture));
            }
        }

        private void DataGridNewTest_RowEditing(object sender, DataGridRowEditEndingEventArgs e)
        {
            try
            {
                var rowView = e.Row.Item as DataRowView;
                var adapterPageSection = new GuiPageSectionTableAdapter();
                var adapterprojectpage = new GuiProjectPageTableAdapter();
                var adapterTest = new TestTableAdapter();

                if (rowView != null && rowView.Row["TestName"].ToString() == string.Empty)
                {
                    throw new Exception("The Test name can't be empty. Please check");
                }
                const string statusCompleted = Constants.UncompletedTestImage;
                if (rowView != null)
                {
                    int testId = Convert.ToInt32(rowView.Row["TestId"].ToString());
                    string testName = rowView.Row["TestName"].ToString();
                    string inputTableName = rowView.Row["InputTableName"].ToString();
                    const int isApi = 0;

                    int pageSectionId;
                    if (rowView.Row["ProjectID"].ToString() != string.Empty)
                        pageSectionId = Convert.ToInt32(rowView.Row["ProjectID"].ToString());
                    else
                        pageSectionId = _pageSectionid;

                    string description = rowView.Row["Description"].ToString();

                    string projectPrefix;
                    using (var adapterProject = new ProjectsTableAdapter())
                    {
                        int projectPageid = Convert.ToInt32(adapterPageSection.GetGuiPageID(pageSectionId));
                        int projectid = Convert.ToInt32(adapterprojectpage.GetProjectID(projectPageid));
                        projectPrefix = adapterProject.GetProjectPrefix(projectid).Trim();
                    }

                    string actualPrefix = testName.Substring(0, projectPrefix.Length);
                    if (actualPrefix != projectPrefix)
                        testName = projectPrefix + testName;


                    if (testId == -1)
                    {
                        adapterTest.Insert(testName, pageSectionId, inputTableName, isApi, description, statusCompleted);
                    }
                    else
                    {
                        adapterTest.UpdateBy(testName, pageSectionId, inputTableName, isApi, description,
                                             statusCompleted, testId);
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "eToroAutoStudio");
                Logger.Error(exception.Message, exception);
            }

            finally
            {
                DataGridFill(Constants.StrPopUpNewTest, _projectPageid.ToString(CultureInfo.InvariantCulture));
                DataGridFill(Constants.StrGuiTest, null);
            }
        }


        private void TreeViewProjectsNewTest_loaded(object sender, RoutedEventArgs e)
        {
            DataGridFill(Constants.StrGuiProjectsNewTest, null);
        }


        private void TreeViewProjectsNewTest_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                SetProjectPageScetionId(sender);
                //var row = (DataRowView) TreeViewProjectsNewTest.SelectedItem;
                //_projectPageid = row.DataView.Table.Columns.Contains("GuiPageSectionID")
                //                            ? Convert.ToInt32(row["GuiPageSectionID"].ToString())
                //                            : 0;
                DataGridFill(Constants.StrPopUpNewTest, _projectid.ToString(CultureInfo.InvariantCulture));
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message, exception);
            }
        }

        private void btnSaveTest_Click(object sender, RoutedEventArgs e)
        {
            PopupNewTest.IsOpen = false;
        }


        private void TreeViewScenario_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                //var row = (DataRowView) TreeViewScenarios.SelectedItem;
                //if (row.DataView.Table.Columns.Contains("ScenarioID"))
                //{
                //    _scenarioid = Convert.ToInt32(row["ScenarioID"].ToString());
                //}
                //else
                //{
                //    _projectid = Convert.ToInt32(row["GuiProjectID"]);
                //    _scenarioid = 0;
                //}
                SetProjectPageScetionId(sender);
                DataGridFill(Constants.StrGuiScenarioLogic, _scenarioid.ToString(CultureInfo.InvariantCulture));
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message, exception);
            }
        }


        private void btnRunScenario_Click(object sender, RoutedEventArgs e)
        {
            bool result;
            ShowStopBtnScenario();

            if (nullDriver == true)
            {
                MessageBox.Show("WebDriver is busy. Wait a few seconds and try again!", "No WebDriver");
                HideStopBtnScenario();
            }

            else if (nullDriver != null)
            {
                using (var adapterTestresult = new TestResultsTableAdapter())
                {
                    try
                    {
                        int runExecutionId = Convert.ToInt32(adapterTestresult.LastRunExecutionID()) + 1;

                        string selectedItem = string.Empty;

                        if (TreeViewScenarios.SelectedItem != null)
                        {
                            var row = (DataRowView)TreeViewScenarios.SelectedItem;
                            selectedItem = row["ScenarioID"].ToString();
                        }

                        ThreadStart ts = delegate
                            {
                                // Do long work here
                                result = ExecuteScenario(selectedItem, runExecutionId, _driver);
                                Dispatcher.BeginInvoke(DispatcherPriority.Normal, (EventHandler)
                                                                                  delegate
                                                                                  {
                                                                                      HideStopBtnScenario();
                                                                                      DataGridFill(
                                                                                          Constants.StrLogResults,
                                                                                          runExecutionId.ToString(
                                                                                              CultureInfo
                                                                                                  .InvariantCulture));
                                                                                      MessageBoxResult mbr;
                                                                                      if (result)
                                                                                          mbr =
                                                                                              MessageBox.Show(
                                                                                                  "Scenario is passed. Press OK to see results. Cancel to stay on the same window.",
                                                                                                  "Run evaluation result",
                                                                                                  MessageBoxButton
                                                                                                      .OKCancel,
                                                                                                  MessageBoxImage
                                                                                                      .Asterisk,
                                                                                                  MessageBoxResult.OK,
                                                                                                  MessageBoxOptions
                                                                                                      .DefaultDesktopOnly);
                                                                                      else
                                                                                          mbr =
                                                                                              MessageBox.Show(
                                                                                                  "Scenario  is failed. Press OK to see results. Cancel to stay on the same window.",
                                                                                                  "Run evaluation result",
                                                                                                  MessageBoxButton
                                                                                                      .OKCancel,
                                                                                                  MessageBoxImage.Error);
                                                                                      if (mbr == MessageBoxResult.OK)
                                                                                          TabItemAnalyzingZone
                                                                                              .IsSelected = true;

                                                                                      Singleton myInstance =
                                                                                          Singleton.Instance;
                                                                                      // Will always be the same instance...
                                                                                      myInstance.StopExecution = false;
                                                                                  }, null, null);
                            };

                        ts.BeginInvoke(delegate(IAsyncResult aysncResult) { ts.EndInvoke(aysncResult); }, null);
                    }
                    catch (Exception exception)
                    {
                        Logger.Error(exception.Message, exception);
                    }
                }
            }
        }

        private void ShowStopBtnScenario()
        {
            BtnStopScenario.IsEnabled = true;
            BtnRunScenario.IsEnabled = false;
        }

        private void HideStopBtnScenario()
        {
            BtnStopScenario.IsEnabled = false;
            BtnRunScenario.IsEnabled = true;
        }


        private bool ExecuteScenario(string selectedItem, int runExecutionId, RemoteWebDriver driver)
        {
            var exman = new ExecutionManager(runExecutionId, this, false);
            bool result = exman.ExecuteOneScenario(selectedItem, driver);
            UpdateProgressLabel("");
            return result;
        }


        private void DataGridScenarioEditor_RowEditing(object sender, DataGridRowEditEndingEventArgs e)
        {
            try
            {
                var rowView = e.Row.Item as DataRowView;

                using (var adapterSl = new GuiScenarioLogicTableAdapter())
                {
                    if (rowView != null && rowView.Row["GuiTestID"].ToString() == string.Empty)
                    {
                        throw new Exception("The Test name can't be empty. Please check");
                    }
                    if (rowView != null)
                    {
                        int guiScenarioLogicId = Convert.ToInt32(rowView.Row["GuiScenarioLogicID"].ToString());
                        if (guiScenarioLogicId == -1)
                        {
                            rowView.Row["GuiScenarioID"] = _scenarioid;
                        }

                        int guiScenarioId = Convert.ToInt32(rowView.Row["GuiScenarioID"].ToString());

                        string inputDataRow;
                        if (rowView.Row["InputDataRow"].ToString() == string.Empty)
                        {
                            Logger.Error("Fill value in DataRow . Can not leave it empty:");
                            inputDataRow = string.Empty;
                        }
                        else
                        {
                            inputDataRow = rowView.Row["InputDataRow"].ToString();
                        }
                        int guiTestId = Convert.ToInt32(rowView.Row["GuiTestID"].ToString());


                        int stepsOrder;
                        if (guiScenarioLogicId == -1)
                        {
                            stepsOrder = Convert.ToInt32(adapterSl.LastStepsOrder(guiScenarioId)) + 1;


                            adapterSl.Insert(guiScenarioId, guiTestId, inputDataRow, stepsOrder);
                        }
                        else
                        {
                            stepsOrder = Convert.ToInt32(rowView.Row["StepsOrder"].ToString());
                            adapterSl.UpdateBy(guiScenarioId, guiTestId, inputDataRow, stepsOrder, guiScenarioLogicId);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "eToroAutoStudio");
                Logger.Error(exception.Message, exception);
            }

            finally
            {
                DataGridFill(Constants.StrGuiScenarioLogic, _scenarioid.ToString(CultureInfo.InvariantCulture));
            }
        }

        private void btnNewScenario_Click(object sender, RoutedEventArgs e)
        {
            PopupNewScenario.IsOpen = true;
            DataGridFill(Constants.StrPopUpNewScenario, null);
        }


        private void btnDumpToTable_BeCarefull_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var myDv = (DataView)DataGridScenarioEditor.ItemsSource;
                DataTable myDt = DataViewAsDataTable(myDv);
                var myNewDt = new DataTable();


                var adapterTest = new TestTableAdapter();
                var adapterSc = new ScenarioTableAdapter();
                string guiScenarioName = string.Empty;
                string conString = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
                var sq = new SqlTableCreator();

                foreach (DataRow row in myDt.Rows)
                {
                    int guiTestId = Convert.ToInt32(row["GuiTestID"].ToString());
                    int guiScenarioId = Convert.ToInt32(row["GuiScenarioID"].ToString());
                    guiScenarioName = adapterSc.GetScenarioName(guiScenarioId);
                    string guiTestName = adapterTest.GetTestName(guiTestId);

                    if (myNewDt.Columns.Contains(guiTestName) == false)
                    {
                        myNewDt.Columns.Add(guiTestName, typeof(int));
                    }
                }


                int columnscount = myNewDt.Columns.Count;
                for (int rowindex = 0; rowindex < myDt.Rows.Count; rowindex = rowindex + columnscount)
                {
                    DataRow newrow = myNewDt.NewRow();
                    for (int i = 0; i < columnscount; i++)
                    {
                        newrow[myNewDt.Columns[i].ColumnName.ToString(CultureInfo.InvariantCulture)] =
                            Convert.ToInt32(myDt.Rows[rowindex + i]["InputDataRow"].ToString());
                    }

                    myNewDt.Rows.Add(newrow);
                }

                var con = new SqlConnection(conString);
                con.Open();
                sq.Connection = con;
                string scenarioTableName = "[QA_Autotest].[Test].[" + guiScenarioName + "]";
                sq.DestinationTableName = scenarioTableName;
                sq.CreateFromDataTable(myNewDt);
                sq.BulkInsertDataTable(conString, scenarioTableName, myNewDt);
            }

            catch (Exception exception)
            {
                Logger.Error(exception.Message, exception);
            }
        }

        /// <summary>
        ///     Show data view from datagrid as DataTable
        /// </summary>
        private static DataTable DataViewAsDataTable(DataView dv)
        {
            DataTable dt = dv.Table.Clone();
            foreach (DataRowView drv in dv)
            {
                dt.ImportRow(drv.Row);
            }
            return dt;
        }

        private void DataGridScenario_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            BtnMoveStepUpScenario.IsEnabled = true;
            BtnMoveStepDownScenario.IsEnabled = true;

            string cellitem = string.Empty;
            if (DataGridScenarioEditor.SelectedItem != null)
            {
                cellitem = DataGridScenarioEditor.SelectedItem.ToString();
            }
            try
            {
                if ((cellitem != "{NewItemPlaceholder}") & (cellitem != string.Empty))
                {
                    var rowView = (DataRowView)DataGridScenarioEditor.SelectedItem;

                    if (rowView != null)
                    {
                        int guiTestId = Convert.ToInt32(rowView.Row["GuiTestID"].ToString());

                        DataGridFill(Constants.StrInputScenarioTable, guiTestId.ToString(CultureInfo.InvariantCulture));
                        DataGridFill(Constants.StrTestViewer, guiTestId.ToString(CultureInfo.InvariantCulture));
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message, exception);
            }
        }

        private void dataGridScenarioEditor_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Delete)
                {
                    var dataGrid = (DataGrid)sender;
                    var rowView = (DataRowView)dataGrid.SelectedItem;

                    var adapterScenario = new GuiScenarioLogicTableAdapter();
                    if (_editinggrid == false)
                    {
                        int guiScenarioLogicId = Convert.ToInt32(rowView.Row["GuiScenarioLogicID"].ToString());
                        int guiScenarioId = Convert.ToInt32(rowView.Row["GuiScenarioID"].ToString());
                        //int guiTestId = Convert.ToInt32(rowView.Row["GuiTestID"].ToString());
                        //string inputDataRow = rowView.Row["InputDataRow"].ToString();


                        if (MessageBox.Show("Delete Selected Test? ", "eToroAutoStudio", MessageBoxButton.YesNo) ==
                            MessageBoxResult.Yes)
                        {
                            int currentStepsOrder = Convert.ToInt32(rowView.Row["StepsOrder"].ToString());
                            int selectItem = currentStepsOrder - 1;
                            while (adapterScenario.GetStepId(guiScenarioId, currentStepsOrder + 1) != null)
                            {
                                int nextStepsOrder = currentStepsOrder + 1;
                                int nextGuiTestStepsId =
                                    Convert.ToInt32(adapterScenario.GetStepId(guiScenarioId, nextStepsOrder));
                                adapterScenario.UpdateStepsOrder(currentStepsOrder, nextGuiTestStepsId);
                                currentStepsOrder = nextStepsOrder;
                            }
                            adapterScenario.Delete(guiScenarioLogicId, guiScenarioId);
                            DataGridFill(Constants.StrGuiScenarioLogic,
                                         guiScenarioId.ToString(CultureInfo.InvariantCulture));
                            dataGrid.SelectedItem = dataGrid.Items[selectItem];
                        }
                    }
                }
                if (e.Key == Key.Enter)
                {
                    e.Handled = true;
                    var eInsertBack = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0,
                                                       Key.Tab);
                    eInsertBack.RoutedEvent = KeyDownEvent;
                    InputManager.Current.ProcessInput(eInsertBack);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message, exception);
            }
        }

        private void DataGridScenarioEditor_BeginingEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            _editinggrid = true;
        }

        private void DataGridScenarioEditor_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            _editinggrid = false;
            int guiTestId = 0;
            try
            {
                var rowView = e.Row.Item as DataRowView;

                var adapterSl = new GuiScenarioLogicTableAdapter();
                if (rowView != null && rowView.Row["GuiTestID"].ToString() == string.Empty)
                {
                    throw new Exception("The Test name can't be empty. Please check");
                }
                if (rowView != null)
                {
                    int guiScenarioLogicId = Convert.ToInt32(rowView.Row["GuiScenarioLogicID"].ToString());
                    if (guiScenarioLogicId == -1)
                    {
                        rowView.Row["GuiScenarioID"] = _scenarioid;
                    }

                    int guiScenarioId = Convert.ToInt32(rowView.Row["GuiScenarioID"].ToString());

                    string inputDataRow;
                    if (rowView.Row["InputDataRow"].ToString() == string.Empty)
                    {
                        Logger.Error("Fill value in DataRow . Can not leave it empty:");
                        inputDataRow = string.Empty;
                    }
                    else
                    {
                        inputDataRow = rowView.Row["InputDataRow"].ToString();
                    }
                    guiTestId = Convert.ToInt32(rowView.Row["GuiTestID"].ToString());


                    int stepsOrder;
                    if (guiScenarioLogicId == -1)
                    {
                        stepsOrder = Convert.ToInt32(adapterSl.LastStepsOrder(guiScenarioId)) + 1;
                        adapterSl.Insert(guiScenarioId, guiTestId, inputDataRow, stepsOrder);
                    }
                    else
                    {
                        stepsOrder = Convert.ToInt32(rowView.Row["StepsOrder"].ToString());
                        adapterSl.UpdateBy(guiScenarioId, guiTestId, inputDataRow, stepsOrder, guiScenarioLogicId);
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "eToroAutoStudio");
                Logger.Error(exception.Message, exception);
            }

            finally
            {
                DataGridFill(Constants.StrGuiScenarioLogic, _scenarioid.ToString(CultureInfo.InvariantCulture));


                DataGridFill(Constants.StrInputScenarioTable, guiTestId.ToString(CultureInfo.InvariantCulture));
                DataGridFill(Constants.StrTestViewer, guiTestId.ToString(CultureInfo.InvariantCulture));
            }
        }

        private void DataGridNewScenario_RowEditing(object sender, DataGridRowEditEndingEventArgs e)
        {
            string actualPrefix = string.Empty;
            try
            {
                var rowView = e.Row.Item as DataRowView;

                using (var adapterScenario = new ScenarioTableAdapter())
                {
                    if (rowView != null && rowView.Row["ScenarioID"].ToString() == string.Empty)
                    {
                        throw new Exception("The Scenario name can't be empty. Please check");
                    }
                    if (rowView != null)
                    {
                        int scenarioId = Convert.ToInt32(rowView.Row["ScenarioID"].ToString());
                        string scenarioName = rowView.Row["ScenarioName"].ToString();

                        int isApi;
                        if (rowView.Row["IsAPI"].ToString() == "")
                            isApi = 0;
                        else
                            isApi = Convert.ToInt32(rowView.Row["IsAPI"].ToString());
                        string description = rowView.Row["Description"].ToString();
                        int projectId;
                        if (rowView.Row["ProjectID"].ToString() == string.Empty)

                            projectId = _projectid;
                        else
                            projectId = Convert.ToInt32(rowView.Row["ProjectID"].ToString());

                        string projectPrefix;
                        using (var adapterProject = new ProjectsTableAdapter())
                        {
                            projectPrefix = adapterProject.GetProjectPrefix(projectId).Trim();
                        }
                        if (projectPrefix.Length < scenarioName.Length)
                            actualPrefix = scenarioName.Substring(0, projectPrefix.Length);

                        if (actualPrefix != projectPrefix)
                            scenarioName = projectPrefix + scenarioName;
                        if (scenarioId == -1)
                        {
                            adapterScenario.Insert(scenarioName, isApi, description, projectId);
                        }
                        else
                        {
                            adapterScenario.UpdateQuery(scenarioName, isApi, description, projectId, scenarioId);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "eToroAutoStudio");
                Logger.Error(exception.Message, exception);
            }

            finally
            {
                DataGridFill(Constants.StrPopUpNewScenario, null);
                DataGridFill(Constants.StrGuiScenario, null);
            }
        }

        private void btnSaveScenario_Click(object sender, RoutedEventArgs e)
        {
            PopupNewScenario.IsOpen = false;
        }


        private void DataGridBatchEditor_RowEditing(object sender, DataGridRowEditEndingEventArgs e)
        {
            try
            {
                var rowView = e.Row.Item as DataRowView;

                var adapterSl = new BatchLogicTableAdapter();
                if (rowView != null && rowView.Row["ScenarioID"].ToString() == string.Empty)
                {
                    throw new Exception("The Scenario name can't be empty. Please check");
                }
                if (rowView != null)
                {
                    int guiBatchLogicId = Convert.ToInt32(rowView.Row["BatchLogicID"].ToString());
                    if (guiBatchLogicId == -1)
                    {
                        rowView.Row["BatchID"] = _batchid;
                    }

                    int batchId = Convert.ToInt32(rowView.Row["BatchID"].ToString());

                    int guiScenarioId = Convert.ToInt32(rowView.Row["ScenarioID"].ToString());

                    int browserId = Convert.ToInt32(rowView.Row["BrowserID"].ToString());
                    int executionStatusId;
                    if (rowView.Row["ExecutionStatusID"].ToString() == string.Empty)
                        executionStatusId = Constants.Checked;
                    else
                        executionStatusId = Convert.ToInt32(rowView.Row["ExecutionStatusID"].ToString());


                    if (guiBatchLogicId == -1)
                    {
                        adapterSl.Insert(batchId, guiScenarioId, browserId, executionStatusId);
                    }
                    else
                    {
                        adapterSl.Update(batchId, guiScenarioId, browserId, executionStatusId, guiBatchLogicId);
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "eToroAutoStudio");
                Logger.Error(exception.Message, exception);
            }

            finally
            {
                DataGridFill(Constants.StrGuiBatchLogic, _batchid.ToString(CultureInfo.InvariantCulture));
            }
        }

        private void btnNewBatch_Click(object sender, RoutedEventArgs e)
        {
            PopupNewBatch.IsOpen = true;
            DataGridFill(Constants.StrPopUpNewBatch, null);
        }


        private void DataGridBatch_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (DataGridBatchEditor.SelectedItem != null)
            {
            }
            try
            {
                //if ((cellitem != "{NewItemPlaceholder}") & (cellitem != string.Empty))
                //{
                //    var rowView = (DataRowView)DataGridBatchEditor.SelectedItem;
                //    var dataset = new DataSetAutoTest();
                //    var adapter_Scenario = new DataSetAutoTestTableAdapters.ScenarioTableAdapter();
                //}
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message, exception);
            }
        }

        private void dataGridBatchEditor_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Delete)
                {
                    var dataGrid = (DataGrid)sender;
                    var rowView = (DataRowView)dataGrid.SelectedItem;

                    using (var adapterScenario = new BatchLogicTableAdapter())
                    {
                        if (_editinggrid == false)
                        {
                            int guiBatchLogicId = Convert.ToInt32(rowView.Row["BatchLogicID"].ToString());
                            if (
                                MessageBox.Show("Delete Selected Scenario? ", "eToroAutoStudio", MessageBoxButton.YesNo) ==
                                MessageBoxResult.Yes)
                                adapterScenario.Delete(guiBatchLogicId);
                        }
                    }
                }
                if (e.Key == Key.Enter)
                {
                    e.Handled = true;
                    var eInsertBack = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0,
                                                       Key.Tab);
                    eInsertBack.RoutedEvent = KeyDownEvent;
                    InputManager.Current.ProcessInput(eInsertBack);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message, exception);
            }
        }

        private void DataGridBatchEditor_BeginingEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            _editinggrid = true;
        }

        private void DataGridBatchEditor_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            _editinggrid = false;
        }

        private void DataGridNewBatch_RowEditing(object sender, DataGridRowEditEndingEventArgs e)
        {
            try
            {
                string actualPrefix = string.Empty;
                var rowView = e.Row.Item as DataRowView;

                using (var adapterScenario = new BatchesTableAdapter())
                {
                    if (rowView != null && rowView.Row["BatchID"].ToString() == string.Empty)
                    {
                        throw new Exception("The Batch name can't be empty. Please check");
                    }
                    if (rowView != null)
                    {
                        int batchId = Convert.ToInt32(rowView.Row["BatchID"].ToString());
                        string batchName = rowView.Row["BatchName"].ToString();

                        string projectPrefix;
                        using (var adapterProject = new ProjectsTableAdapter())
                        {
                            projectPrefix = adapterProject.GetProjectPrefix(_projectid).Trim();
                        }

                        if (projectPrefix.Length < batchName.Length)
                            actualPrefix = batchName.Substring(0, projectPrefix.Length);


                        if (actualPrefix != projectPrefix)
                            batchName = projectPrefix + batchName;
                        if (batchId == -1)
                        {
                            adapterScenario.Insert(batchName, _projectid);
                        }
                        else
                        {
                            adapterScenario.UpdateBy(batchName, batchId);
                        }
                    }
                }
            }

            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "eToroAutoStudio");
                Logger.Error(exception.Message, exception);
            }

            finally
            {
                DataGridFill(Constants.StrPopUpNewBatch, null);
                DataGridFill(Constants.StrGuiBatch, null);
            }
        }

        private void btnSaveBatch_Click(object sender, RoutedEventArgs e)
        {
            PopupNewBatch.IsOpen = false;
        }


        private void WindoweToroAutoStudio_Closed(object sender, EventArgs e)
        {
            try
            {
                closingBrowserThread();
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message, exception);
            }
            Logger.Info(
                "-------------------------------------------------------------------------------------------------------------\n                                             eToroAutoStudio Closed with  driver  \n-------------------------------------------------------------------------------------------------------------");
        }

        private void btnAddNewObject_Click(object sender, RoutedEventArgs e)
        {
            if (_pageSectionid != 0)
            {

                DataGridFill(Constants.StrGuiMapEdit, "0");
                DataGridGuiMapEditorHidenTable.IsReadOnly = false;
            }
            else
                MessageBox.Show("Select Project Page first", "eToroAutoStudio");
        }

        private void btnUpdateObject_Click(object sender, RoutedEventArgs e)
        {
            DataGridGuiMapEditorHidenTable.IsReadOnly = false;
        }

        private void btnDeleteObject_Click(object sender, RoutedEventArgs e)
        {
            DataGridGuiMapEditorHidenTable.IsReadOnly = false;


            MessageBox.Show("Select row (object) and press delete button on keyboard", "eToroAutoStudio");
        }

        private void DataGridTestEditor_InputTable_PreviewKewDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    e.Handled = true;
                    var eInsertBack = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0,
                                                       Key.Tab);
                    eInsertBack.RoutedEvent = KeyDownEvent;
                    InputManager.Current.ProcessInput(eInsertBack);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message, exception);
            }
        }

        private void DataGridNewTest_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    e.Handled = true;
                    var eInsertBack = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0,
                                                       Key.Tab);
                    eInsertBack.RoutedEvent = KeyDownEvent;
                    InputManager.Current.ProcessInput(eInsertBack);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message, exception);
            }
        }

        private void MenuUpadteTest_Click(object sender, RoutedEventArgs e)
        {
            btnNewTest_Click(null, null);
        }

        private void MenuItem_DeleteTest_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("You can't delete test plaese ask Automation Manager", "eToroAutoStudio");
        }

        private void TreeViewScenarios_Loaded(object sender, RoutedEventArgs e)
        {
            DataGridFill(Constants.StrGuiScenario, null);
        }


        private void MenuItem_AddNewScenario_Click(object sender, RoutedEventArgs e)
        {
            btnNewScenario_Click(null, null);
        }

        private void MenuItemUpadteScenario_Click(object sender, RoutedEventArgs e)
        {
            btnNewScenario_Click(null, null);
        }

        private void MenuItem_DeleteScenario_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("You can't delete Scenario please ask Automation Manager", "eToroAutoStudio");
        }

        private void TreeViewProjectsNewScenario_loaded(object sender, RoutedEventArgs e)
        {
            DataGridFill(Constants.StrGuiProjectsNewScenario, null);
        }

        private void TreeViewProjectsNewScenario_SelectedItemChanged(object sender,
                                                                     RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                SetProjectPageScetionId(sender);

                DataGridFill(Constants.StrPopUpNewScenario, _projectid.ToString(CultureInfo.InvariantCulture));
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message, exception);
            }
        }


        private void TreeViewBatch_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                //using (var adapterBatch = new BatchesTableAdapter())
                //{
                //    var row = (DataRowView) TreeViewBatch.SelectedItem;
                //    if (row.DataView.Table.Columns.Contains("BatchID"))
                //    {
                //        _batchid = Convert.ToInt32(row["BatchID"].ToString());
                //        _projectid = Convert.ToInt32(adapterBatch.GetProjectID(_batchid));
                //    }
                //    else
                //    {
                //        _projectid = Convert.ToInt32(row["GuiProjectID"]);
                //        _batchid = 0;
                //    }
                SetProjectPageScetionId(sender);

                DataGridFill(Constants.StrGuiBatchLogic, _batchid.ToString(CultureInfo.InvariantCulture));
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message, exception);
            }
        }

        private void TreeViewBatch_Loaded(object sender, RoutedEventArgs e)
        {
            DataGridFill(Constants.StrGuiBatch, null);
        }

        private void MenuItem_AddNewBatch_Click(object sender, RoutedEventArgs e)
        {
            btnNewBatch_Click(null, null);
        }

        private void MenuItemUpadteBatch_Click(object sender, RoutedEventArgs e)
        {
            btnNewBatch_Click(null, null);
        }

        private void MenuItem_DeleteBatch_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("You can't delete Batch please ask Automation Manager", "eToroAutoStudio");
        }

        private void MenuItem_RunBatch_Click(object sender, RoutedEventArgs e)
        {
            btnRunBatch_Click(null, null);
        }

        private void MenuItem_AddnewObject_Click(object sender, RoutedEventArgs e)
        {
            if (_projectPageid != 0)
                btnAddNewObject_Click(null, null);
            else
                MessageBox.Show("Select Project Page first", "eToroAutoStudio");
        }

        private void MenuItem_AddNewPage_Click(object sender, RoutedEventArgs e)
        {
            GuiProjectPageTableAdapter adapterProjectpage;
            string projectname;
            string projectPrefix;
            using (var adapterProject = new ProjectsTableAdapter())
            {
                adapterProjectpage = new GuiProjectPageTableAdapter();
                projectname = adapterProject.GetProjectName(_projectid);
                projectPrefix = adapterProject.GetProjectPrefix(_projectid).Trim();
            }
            string pageName = Interaction.InputBox("Please Enter Page Name under project " + projectname,
                                                   "Add project Page", projectPrefix + "NewPage");
            if (pageName != string.Empty)
            {
                adapterProjectpage.Insert1(_projectid, pageName.Trim());
                DataGridFill(Constants.StrGuiProjects, null);
                DataGridFill(Constants.StrGuiTest, null);
            }
        }

        private void MenuItem_AddNewPageSection_Click(object sender, RoutedEventArgs e)
        {
            string projectPrefix;
            string pagename;


            var adapterPagesection = new GuiPageSectionTableAdapter();
            using (var adapterProject = new ProjectsTableAdapter())
            {
                var adapterProjectpage = new GuiProjectPageTableAdapter();
                pagename = adapterProjectpage.GetProjectPageName(_projectPageid).ToString().Trim();
                projectPrefix = adapterProject.GetProjectPrefix(_projectid).Trim();
            }
            string pageSectionName = Interaction.InputBox("Please Enter PageSection Name under Page " + pagename,
                                                          "Add  Page Section ", projectPrefix + "NewPageSection");
            if (pageSectionName != string.Empty)
            {
                adapterPagesection.Insert(_projectPageid, pageSectionName.Trim());
                DataGridFill(Constants.StrGuiProjects, null);
                DataGridFill(Constants.StrGuiTest, null);
            }
        }

        private void MenuItem_RenamePage_Click(object sender, RoutedEventArgs e)
        {
            if (_projectPageid != 0)
            {
                GuiProjectPageTableAdapter adapterProjectpage;
                string projectname;
                string projectPagename;
                string projectPrefix;
                using (var adapterProject = new ProjectsTableAdapter())
                {
                    adapterProjectpage = new GuiProjectPageTableAdapter();
                    projectname = adapterProject.GetProjectName(_projectid);
                    projectPagename = adapterProjectpage.GetProjectPageName(_projectPageid).ToString();
                    projectPrefix = adapterProject.GetProjectPrefix(_projectid).Trim();
                }
                string pageName =
                    Interaction.InputBox(
                        "Please Enter New Name for Page: " + projectPagename + " under project " + projectname,
                        "Rename project Page", projectPrefix + "NewPageName");
                if (pageName != string.Empty)
                {
                    adapterProjectpage.UpdateBy(pageName.Trim(), _projectPageid);
                    DataGridFill(Constants.StrGuiProjects, null);
                    DataGridFill(Constants.StrGuiTest, null);
                }
            }
            else
                MessageBox.Show("Select Project Page first", "eToroAutoStudio");
        }

        private void MenuItem_RenamePageSection_Click(object sender, RoutedEventArgs e)
        {
            if (_pageSectionid != 0)
            {
                string projectPagename;
                string projectPrefix;
                string pageSectionName;
                var adapterPageSection = new GuiPageSectionTableAdapter();
                using (var adapterProject = new ProjectsTableAdapter())
                {
                    var adapterProjectpage = new GuiProjectPageTableAdapter();
                    pageSectionName = adapterPageSection.GetPageSectionName(_pageSectionid);
                    projectPagename = adapterProjectpage.GetProjectPageName(_projectPageid).ToString();
                    projectPrefix = adapterProject.GetProjectPrefix(_projectid).Trim();
                }
                string pageName =
                    Interaction.InputBox(
                        "Please Enter New Name for PageSection: " + pageSectionName + " under Page " + projectPagename,
                        "Rename project Page", projectPrefix + "NewPageSectionName");
                if (pageName != string.Empty)
                {
                    adapterPageSection.Update(pageName.Trim(), _pageSectionid);
                    DataGridFill(Constants.StrGuiProjects, null);
                    DataGridFill(Constants.StrGuiTest, null);
                }
            }
            else
                MessageBox.Show("Select  Page Section first", "eToroAutoStudio");
        }

        private void btnRunBatch_Click(object sender, RoutedEventArgs e)
        {
            ShowStopBtnBatch();
            try
            {
                bool result;
                //var sl = new Selenium(driver);
                var adapterTestresult = new TestResultsTableAdapter();
                int runExecutionId = Convert.ToInt32(adapterTestresult.LastRunExecutionID()) + 1;

                string selectedItem = string.Empty;

                if (TreeViewBatch.SelectedItem != null)
                {
                    var row = (DataRowView)TreeViewBatch.SelectedItem;
                    selectedItem = row["BatchID"].ToString();
                }

                ThreadStart ts = delegate
                    {
                        // Do long work here
                        result = ExecuteBatch(selectedItem, runExecutionId);
                        Dispatcher.BeginInvoke(DispatcherPriority.Normal, (EventHandler)
                                                                          delegate
                                                                          {
                                                                              HideStopBtnBatch();
                                                                              MessageBoxResult mbr;
                                                                              DataGridFill(Constants.StrLogResults,
                                                                                           runExecutionId.ToString(
                                                                                               CultureInfo
                                                                                                   .InvariantCulture));
                                                                              if (result)
                                                                                  mbr =
                                                                                      MessageBox.Show(
                                                                                          "Batch is passed. Press OK to see results. Cancel to stay on the same window.",
                                                                                          "Run evaluation result",
                                                                                          MessageBoxButton.OKCancel,
                                                                                          MessageBoxImage.Asterisk,
                                                                                          MessageBoxResult.OK,
                                                                                          MessageBoxOptions
                                                                                              .DefaultDesktopOnly);
                                                                              else
                                                                                  mbr =
                                                                                      MessageBox.Show(
                                                                                          "Batch  is failed. Press OK to see results. Cancel to stay on the same window.",
                                                                                          "Run evaluation result",
                                                                                          MessageBoxButton.OKCancel,
                                                                                          MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);

                                                                              if (mbr == MessageBoxResult.OK)
                                                                                  TabItemAnalyzingZone.IsSelected =
                                                                                      true;
                                                                              Singleton myInstance =
                                                                                  Singleton.Instance;
                                                                              // Will always be the same instance...
                                                                              myInstance.StopExecution = false;
                                                                          }, null, null);
                    };

                ts.BeginInvoke(delegate(IAsyncResult aysncResult) { ts.EndInvoke(aysncResult); }, null);
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message, exception);
            }
        }

        private void ShowStopBtnBatch()
        {
            BtnStopBatch.IsEnabled = true;
            BtnRunBatch.IsEnabled = false;
        }

        private void HideStopBtnBatch()
        {
            BtnStopBatch.IsEnabled = false;
            BtnRunBatch.IsEnabled = true;
        }


        private bool ExecuteBatch(string selectedItem, int runExecutionId)
        {
            var exman = new ExecutionManager(runExecutionId, this, true);
            bool result = exman.ExecuteOneBatch(selectedItem);



            return result;
        }

        private void MenuItem_RunScenario_Click(object sender, RoutedEventArgs e)
        {
            btnRunScenario_Click(null, null);
        }

        private void MenuItem_CopyScenario_Click(object sender, RoutedEventArgs e)
        {
            CopyScenario(sender, e);
        }

        private void WindoweToroAutoStudio_Closing(object sender, CancelEventArgs e)
        {
            closingBrowserThread();
        }

        private void WindoweToroAutoStudio_Unloaded(object sender, RoutedEventArgs e)
        {
            closingBrowserThread();
        }

        private void TreeViewProjectsNewBatch_SelectedItemChanged(object sender,
                                                                  RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                SetProjectPageScetionId(sender);
                DataGridFill(Constants.StrPopUpNewBatch, _projectid.ToString(CultureInfo.InvariantCulture));
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message, exception);
            }
        }

        private void TreeViewProjectsNewBatch_loaded(object sender, RoutedEventArgs e)
        {
            DataGridFill(Constants.StrGuiProjectsNewBatch, null);
        }


        private void DataGrid_Configuration_Loaded(object sender, RoutedEventArgs e)
        {
            var jp = new JsonParser();
            DataTable dt = jp.ReadJsonToDt();
            DataGridConfiguration.ItemsSource = dt.DefaultView;

            if (_msgDisplayed == false)
            {
                MessageBox.Show(_upmessage
                         , "Applenium");
                _msgDisplayed = true;
            }
       

        }

        private void DataGrid_Configuration_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            var rowView = e.Row.Item as DataRowView;
            if (rowView != null)
            {
                string name = rowView["Name"].ToString();
                string value = rowView["Value"].ToString();
                var jp = new JsonParser();
                jp.WriteJson(name, value);
                if (rowView["Name"].ToString() == "DefaultBrowser")

                    newBrowserThread( value);
            }


        }

        private void WebBrowser_ReportDashboard_Loaded(object sender, RoutedEventArgs e)
        {
            var jp = new JsonParser();
            var oUri = new Uri(jp.ReadJson("DashboardURL"));

            WebBrowserReportDashboard.Navigate(oUri);
        }

        private void btnMoveStepUp_Click(object sender, RoutedEventArgs e)
        {
            MoveStepsUpTest(DataGridTestEditor, Constants.StrGuiTestSteps);
        }

        private void btnMoveStepDown_Click(object sender, RoutedEventArgs e)
        {
            MoveStepDownTest(DataGridTestEditor, Constants.StrGuiTestSteps);
        }

        private void btnMoveStepUpScenario_Click(object sender, RoutedEventArgs e)
        {
            MoveStepsUpScenario(DataGridScenarioEditor, Constants.StrGuiScenarioLogic);
        }

        private void btnMoveStepDownScenario_Click(object sender, RoutedEventArgs e)
        {
            MoveStepDownScenario(DataGridScenarioEditor, Constants.StrGuiScenarioLogic);
        }


        private void MoveStepsUpScenario(DataGrid datagrid, string dataGridEditor)
        {
            int selectedItem = 0;
            var scenarioLogicTableAdapter = new GuiScenarioLogicTableAdapter();


            try
            {
                var rowView = (DataRowView)datagrid.SelectedItem;
                int scenarioOrTestId;
                if (dataGridEditor == Constants.StrGuiScenarioLogic)
                {
                    scenarioOrTestId = Convert.ToInt32(rowView.Row["GuiScenarioID"].ToString());
                }
                else
                {
                    scenarioOrTestId = Convert.ToInt32(rowView.Row["GuiTestID"].ToString());
                }

                if (rowView != null)
                {
                    //CurrentguiTestStepsId = Convert.ToInt32(rowView.Row["GuiTestStepsID"].ToString());
                    // int guiTestId = Convert.ToInt32(rowView.Row["GuiTestID"].ToString());


                    int currentStepsOrder = Convert.ToInt32(rowView.Row["StepsOrder"].ToString());
                    int currentguiTestStepsId = Convert.ToInt32(scenarioLogicTableAdapter.GetStepId(scenarioOrTestId, currentStepsOrder));
                    int previesStepsOrder = currentStepsOrder - 1;
                    if (scenarioLogicTableAdapter.GetStepId(scenarioOrTestId, previesStepsOrder) != null)
                    {
                        int previesGuiTestStepsId =
                            Convert.ToInt32(scenarioLogicTableAdapter.GetStepId(scenarioOrTestId, previesStepsOrder));
                        scenarioLogicTableAdapter.UpdateStepsOrder(previesStepsOrder, currentguiTestStepsId);
                        scenarioLogicTableAdapter.UpdateStepsOrder(currentStepsOrder, previesGuiTestStepsId);
                        selectedItem = previesStepsOrder - 1;
                    }
                    if (currentStepsOrder == 1)
                    {
                        currentguiTestStepsId = Convert.ToInt32(scenarioLogicTableAdapter.GetStepId(scenarioOrTestId, currentStepsOrder));
                        while (scenarioLogicTableAdapter.GetStepId(scenarioOrTestId, currentStepsOrder + 1) != null)
                        {
                            int nextStepsOrder = currentStepsOrder + 1;
                            int nextGuiTestStepsId =
                                Convert.ToInt32(scenarioLogicTableAdapter.GetStepId(scenarioOrTestId, nextStepsOrder));
                            scenarioLogicTableAdapter.UpdateStepsOrder(currentStepsOrder, nextGuiTestStepsId);
                            currentStepsOrder = nextStepsOrder;
                        }
                        scenarioLogicTableAdapter.UpdateStepsOrder(datagrid.Items.Count - 1, currentguiTestStepsId);
                        selectedItem = datagrid.Items.Count - 2;
                    }
                }
                DataGridFill(dataGridEditor, scenarioOrTestId.ToString(CultureInfo.InvariantCulture));
                datagrid.SelectedItem = datagrid.Items[selectedItem];
            }

            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "eToroAutoStudio");
                Logger.Error(exception.Message, exception);
            }
        }

        private void MoveStepDownScenario(DataGrid datagrid, string dataGridEditor)
        {
            int selectedItem = 0;
            int scenarioOrTestId = 0;
            try
            {
                var rowView = (DataRowView)datagrid.SelectedItem;

                using (var scenarioLogicTableAdapter = new GuiScenarioLogicTableAdapter())
                {
                    if (rowView != null)
                    {
                        //int currentguiTestStepsId = Convert.ToInt32(rowView.Row["GuiTestStepsID"].ToString());
                        if (dataGridEditor == Constants.StrGuiScenarioLogic)
                            scenarioOrTestId = Convert.ToInt32(rowView.Row["GuiScenarioID"].ToString());
                        else
                        {
                            scenarioOrTestId = Convert.ToInt32(rowView.Row["GuiTestID"].ToString());
                        }


                        int currentStepsOrder = Convert.ToInt32(rowView.Row["StepsOrder"].ToString());
                        int currentguiTestStepsId =
                            Convert.ToInt32(scenarioLogicTableAdapter.GetStepId(scenarioOrTestId, currentStepsOrder));
                        int nextStepsOrder = currentStepsOrder + 1;
                        int nextGuiTestStepsId;
                        if (scenarioLogicTableAdapter.GetStepId(scenarioOrTestId, nextStepsOrder) != null)
                        {
                            nextGuiTestStepsId = Convert.ToInt32(scenarioLogicTableAdapter.GetStepId(scenarioOrTestId, nextStepsOrder));
                            scenarioLogicTableAdapter.UpdateStepsOrder(nextStepsOrder, currentguiTestStepsId);
                            scenarioLogicTableAdapter.UpdateStepsOrder(currentStepsOrder, nextGuiTestStepsId);
                            selectedItem = nextStepsOrder - 1;
                        }
                        if (currentStepsOrder == datagrid.Items.Count - 1)
                        {
                            currentguiTestStepsId =
                                Convert.ToInt32(scenarioLogicTableAdapter.GetStepId(scenarioOrTestId, currentStepsOrder));
                            while (scenarioLogicTableAdapter.GetStepId(scenarioOrTestId, currentStepsOrder - 1) != null)
                            {
                                nextStepsOrder = currentStepsOrder - 1;
                                nextGuiTestStepsId =
                                    Convert.ToInt32(scenarioLogicTableAdapter.GetStepId(scenarioOrTestId, nextStepsOrder));
                                scenarioLogicTableAdapter.UpdateStepsOrder(currentStepsOrder, nextGuiTestStepsId);
                                currentStepsOrder = nextStepsOrder;
                            }
                            scenarioLogicTableAdapter.UpdateStepsOrder(1, currentguiTestStepsId);
                            selectedItem = 0;
                        }
                    }
                    DataGridFill(dataGridEditor, scenarioOrTestId.ToString(CultureInfo.InvariantCulture));
                    datagrid.SelectedItem = datagrid.Items[selectedItem];
                }
            }

            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "eToroAutoStudio");
                Logger.Error(exception.Message, exception);
            }
        }

        private void MoveStepsUpTest(DataGrid datagrid, string dataGridEditor)
        {
            var adapterEnvVer = new EnvironmentVersionTableAdapter();
            var jsonParser = new JsonParser();

            string verid = adapterEnvVer.GetEnvironmnetVersionIDByVersionName(jsonParser.ReadJson("TestingEnvironmentVersion"))
                    .ToString();
            string versionColumn = adapterEnvVer.GetColumnByID(Convert.ToInt32(verid)).ToString();
            int selectedItem = 0;
            var testStepsTableAdapter = new GuiTestStepsTableAdapter();
            var sql = new Sql();

            try
            {
                var rowView = (DataRowView)datagrid.SelectedItem;
                int scenarioOrTestId;
                if (dataGridEditor == Constants.StrGuiScenarioLogic)
                {
                    scenarioOrTestId = Convert.ToInt32(rowView.Row["GuiScenarioID"].ToString());
                }
                else
                {
                    scenarioOrTestId = Convert.ToInt32(rowView.Row["GuiTestID"].ToString());
                }

                if (rowView != null)
                {
                    //CurrentguiTestStepsId = Convert.ToInt32(rowView.Row["GuiTestStepsID"].ToString());
                    // int guiTestId = Convert.ToInt32(rowView.Row["GuiTestID"].ToString());
                    int currentStepsOrder = Convert.ToInt32(rowView.Row["StepsOrder"].ToString());
                    //int currentguiTestStepsId = Convert.ToInt32(testStepsTableAdapter.GetStepId(scenarioOrTestId, currentStepsOrder));
                    //copy currentguiTestStep

                    int currentguiTestStepsId = Convert.ToInt32(sql.CopyStepAndChangeVersion(currentStepsOrder,scenarioOrTestId, versionColumn));

                    int previesStepsOrder = currentStepsOrder - 1;
                    if (sql.GetStepsIdByVersion(scenarioOrTestId.ToString(), previesStepsOrder.ToString(),versionColumn)!=string.Empty) //if (testStepsTableAdapter.GetStepId(scenarioOrTestId, previesStepsOrder) != null)
                    {
                        //int previesGuiTestStepsId =Convert.ToInt32(testStepsTableAdapter.GetStepId(scenarioOrTestId, previesStepsOrder));
                       // int previesGuiTestStepsId =Convert.ToInt32(sql.GetStepsIdByVersion(scenarioOrTestId.ToString(),previesStepsOrder.ToString(), versionColumn));

                        //before update copy 

                        int previesGuiTestStepsId = Convert.ToInt32(sql.CopyStepAndChangeVersion(previesStepsOrder, scenarioOrTestId, versionColumn));
                        testStepsTableAdapter.UpdateStepsOrder(previesStepsOrder, currentguiTestStepsId);
                        testStepsTableAdapter.UpdateStepsOrder(currentStepsOrder, previesGuiTestStepsId);
                        selectedItem = previesStepsOrder - 1;
                    }
                    if (currentStepsOrder == 1)
                    {
                        //currentguiTestStepsId =Convert.ToInt32(testStepsTableAdapter.GetStepId(scenarioOrTestId, currentStepsOrder));
                        if (sql.GetStepsIdByVersion(currentStepsOrder.ToString(),scenarioOrTestId.ToString(),versionColumn)!=string.Empty)
                        {
                        //currentguiTestStepsId = Convert.ToInt32(sql.GetStepsIdByVersion(currentStepsOrder.ToString(),scenarioOrTestId.ToString(),versionColumn));                            
                            currentguiTestStepsId = Convert.ToInt32(sql.CopyStepAndChangeVersion(currentStepsOrder, scenarioOrTestId, versionColumn));                            
                        }

                        while (sql.GetStepsIdByVersion(scenarioOrTestId.ToString(),(currentStepsOrder + 1).ToString(),versionColumn)!=string.Empty)//(testStepsTableAdapter.GetStepId(scenarioOrTestId, currentStepsOrder + 1) != null)
                        {
                            int nextStepsOrder = currentStepsOrder + 1;
                            //int nextGuiTestStepsId =Convert.ToInt32(testStepsTableAdapter.GetStepId(scenarioOrTestId, nextStepsOrder));
                            //int nextGuiTestStepsId =Convert.ToInt32(sql.GetStepsIdByVersion(scenarioOrTestId.ToString(),nextStepsOrder.ToString(), versionColumn));
                            //before update copy step
                            int nextGuiTestStepsId = Convert.ToInt32(sql.CopyStepAndChangeVersion(nextStepsOrder, scenarioOrTestId, versionColumn));
                            testStepsTableAdapter.UpdateStepsOrder(currentStepsOrder, nextGuiTestStepsId);
                            currentStepsOrder = nextStepsOrder;
                        }

                        testStepsTableAdapter.UpdateStepsOrder(datagrid.Items.Count - 1, currentguiTestStepsId);
                        selectedItem = datagrid.Items.Count - 2;
                    }
                }
                DataGridFill(dataGridEditor, scenarioOrTestId.ToString(CultureInfo.InvariantCulture));
                datagrid.SelectedItem = datagrid.Items[selectedItem];
            }

            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Applenium");
                Logger.Error(exception.Message, exception);
            }
        }

        private void MoveStepDownTest(DataGrid datagrid, string dataGridEditor)
        {
            var adapterEnvVer = new EnvironmentVersionTableAdapter();
            var jsonParser = new JsonParser();

            string verid = adapterEnvVer.GetEnvironmnetVersionIDByVersionName(jsonParser.ReadJson("TestingEnvironmentVersion"))
                    .ToString();
            string versionColumn = adapterEnvVer.GetColumnByID(Convert.ToInt32(verid)).ToString();
            
            
            var sql = new Sql();

            int selectedItem = 0;
            int scenarioOrTestId = 0;
            try
            {
                var rowView = (DataRowView)datagrid.SelectedItem;

                using (var testStepsTableAdapter = new GuiTestStepsTableAdapter())
                {
                    if (rowView != null)
                    {
                        //int currentguiTestStepsId = Convert.ToInt32(rowView.Row["GuiTestStepsID"].ToString());
                        if (dataGridEditor == Constants.StrGuiScenarioLogic)
                            scenarioOrTestId = Convert.ToInt32(rowView.Row["GuiScenarioID"].ToString());
                        else
                        {
                            scenarioOrTestId = Convert.ToInt32(rowView.Row["GuiTestID"].ToString());
                        }


                        int currentStepsOrder = Convert.ToInt32(rowView.Row["StepsOrder"].ToString());
                        //int currentguiTestStepsId =Convert.ToInt32(testStepsTableAdapter.GetStepId(scenarioOrTestId, currentStepsOrder));

                        //copy currentguiTestStep

                        int currentguiTestStepsId = Convert.ToInt32(sql.CopyStepAndChangeVersion(currentStepsOrder, scenarioOrTestId, versionColumn));


                        int nextStepsOrder = currentStepsOrder + 1;
                        int nextGuiTestStepsId;
                        if (sql.GetStepsIdByVersion(scenarioOrTestId.ToString(), nextStepsOrder.ToString(), versionColumn) != string.Empty)// (testStepsTableAdapter.GetStepId(scenarioOrTestId, nextStepsOrder) != null)
                        {
                            //nextGuiTestStepsId =Convert.ToInt32(testStepsTableAdapter.GetStepId(scenarioOrTestId, nextStepsOrder));

                            //before update copy 

                            nextGuiTestStepsId = Convert.ToInt32(sql.CopyStepAndChangeVersion(nextStepsOrder, scenarioOrTestId, versionColumn));

                            testStepsTableAdapter.UpdateStepsOrder(nextStepsOrder, currentguiTestStepsId);
                            testStepsTableAdapter.UpdateStepsOrder(currentStepsOrder, nextGuiTestStepsId);
                            selectedItem = nextStepsOrder - 1;
                        }
                        if (currentStepsOrder == datagrid.Items.Count - 1)
                        {
                            //currentguiTestStepsId =Convert.ToInt32(testStepsTableAdapter.GetStepId(scenarioOrTestId, currentStepsOrder));
                            currentguiTestStepsId = Convert.ToInt32(sql.CopyStepAndChangeVersion(currentStepsOrder, scenarioOrTestId, versionColumn));

                            while (sql.GetStepsIdByVersion(scenarioOrTestId.ToString(), (currentStepsOrder - 1).ToString(), versionColumn) != string.Empty)//(testStepsTableAdapter.GetStepId(scenarioOrTestId, currentStepsOrder - 1) != null)
                            {
                                nextStepsOrder = currentStepsOrder - 1;
                                //nextGuiTestStepsId =Convert.ToInt32(testStepsTableAdapter.GetStepId(scenarioOrTestId, nextStepsOrder));
                                //before update copy step
                                nextGuiTestStepsId = Convert.ToInt32(sql.CopyStepAndChangeVersion(nextStepsOrder, scenarioOrTestId, versionColumn));
                                testStepsTableAdapter.UpdateStepsOrder(currentStepsOrder, nextGuiTestStepsId);
                                currentStepsOrder = nextStepsOrder;
                            }
                            testStepsTableAdapter.UpdateStepsOrder(1, currentguiTestStepsId);
                            selectedItem = 0;
                        }
                    }
                    DataGridFill(dataGridEditor, scenarioOrTestId.ToString(CultureInfo.InvariantCulture));
                    datagrid.SelectedItem = datagrid.Items[selectedItem];
                }
            }

            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "eToroAutoStudio");
                Logger.Error(exception.Message, exception);
            }




        }

        private void DataGridTestEditor_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            BtnMoveStepUp.IsEnabled = true;
            BtnMoveStepDown.IsEnabled = true;
        }

        private void DataGridScenarioEditor_LostFocus(object sender, RoutedEventArgs e)
        {
            //BtnMoveStepUpScenario.IsEnabled = false;
            //BtnMoveStepDownScenario.IsEnabled = false;
        }

        private void DataGridTestEditor_LostFocus(object sender, RoutedEventArgs e)
        {
            //BtnMoveStepUp.IsEnabled = false;
            //BtnMoveStepDown.IsEnabled = false;
        }


        private void TreeViewTests_OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);

            if (treeViewItem != null)
            {
                treeViewItem.Focus();
                e.Handled = true;
            }
        }

        private static TreeViewItem VisualUpwardSearch(DependencyObject source)
        {
            while (source != null && !(source is TreeViewItem))
                source = VisualTreeHelper.GetParent(source);

            return source as TreeViewItem;
        }

        private void TreeViewProjects_OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);

            if (treeViewItem != null)
            {
                treeViewItem.Focus();
                e.Handled = true;
            }
        }

        private void TreeViewProjectsNewTest_OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);

            if (treeViewItem != null)
            {
                treeViewItem.Focus();
                e.Handled = true;
            }
        }

        private void ShowAllPageSections(object sender, RoutedEventArgs e)
        {
            _showAllSections = true;
        }

        private void ShowOnlyPageSections(object sender, RoutedEventArgs e)
        {
            _showAllSections = false;
        }

        private void StatusCompleted(object sender, RoutedEventArgs e)
        {
            var adapterTest = new TestTableAdapter();
            adapterTest.UpdateStatusCompleted(Constants.CompletedTestImage, _testid);
            DataGridFill(Constants.StrGuiTest, null);
        }

        private void StatusUnCompleted(object sender, RoutedEventArgs e)
        {
            var adapterTest = new TestTableAdapter();

            adapterTest.UpdateStatusCompleted(Constants.UncompletedTestImage, _testid);
            DataGridFill(Constants.StrGuiTest, null);
        }

        private void CopyTest(object sender, RoutedEventArgs e)
        {
            var adapterTest = new TestTableAdapter();
            var adapterTestSteps = new GuiTestStepsTableAdapter();

            string testname = adapterTest.GetTestName(_testid);

            string newtestname = Interaction.InputBox("Test " + testname + " Coppied to...",
                                                      "Enter new name for your test ", "Copy Of_" + testname);

            adapterTest.CopyTest(newtestname.Trim(), Constants.UncompletedTestImage, _testid);
            int? newtestid = (int)adapterTest.GetLastTestId();
            adapterTestSteps.CopyTestSteps(newtestid.ToString(), _testid);

            adapterTest.UpdateStatusCompleted(Constants.UncompletedTestImage, _testid);
            DataGridFill(Constants.StrGuiTest, null);
        }

        private void CopyScenario(object sender, RoutedEventArgs e)
        {
            var adapterScenario = new ScenarioTableAdapter();
            var adapterScenarioLogic = new GuiScenarioLogicTableAdapter();

            string scenarioName = adapterScenario.GetScenarioName(_scenarioid);

            string newScenarioName = Interaction.InputBox("Scenario " + scenarioName + " Copied to...",
                                                      "Enter new name for your scenario ", "Copy Of_" + scenarioName);

            adapterScenario.CopyScenario(newScenarioName.Trim(), scenarioName);

            int? newScenarioId = adapterScenario.GetLastScnerioID();

            adapterScenarioLogic.CopyScenarioLogic(newScenarioId.ToString(), _scenarioid);
            DataGridFill(Constants.StrGuiScenario, null);
        }

        private void CopyObject(object sender, RoutedEventArgs e)
        {
            var adapterTest = new TestTableAdapter();
            var adapterTestSteps = new GuiTestStepsTableAdapter();

            string testname = adapterTest.GetTestName(_testid);

            string newtestname = Interaction.InputBox("Test " + testname + " Coppied to...",
                                                      "Enter new name for your test ", "Copy Of_" + testname);

            adapterTest.CopyTest(newtestname.Trim(), Constants.UncompletedTestImage, _testid);
            int? newtestid = (int)adapterTest.GetLastTestId();
            adapterTestSteps.CopyTestSteps(newtestid.ToString(), _testid);

            adapterTest.UpdateStatusCompleted(Constants.UncompletedTestImage, _testid);
            DataGridFill(Constants.StrGuiTest, null);
        }


        private void JumpToNode(TreeViewItem tvi, string NodeName)
        {
            if (tvi.Name == NodeName)
            {
                tvi.IsExpanded = true;
                tvi.BringIntoView();
                return;
            }
            else
            {
                tvi.IsExpanded = false;
            }

            if (tvi.HasItems)
            {
                foreach (object item in tvi.Items)
                {
                    var temp = item as TreeViewItem;
                    JumpToNode(temp, NodeName);
                }
            }
        }

        private void JumpToFolder(TreeView tv, string node)
        {
            bool done = false;
            ItemCollection ic = tv.Items;

            while (!done)
            {
                bool found = false;

                foreach (TreeViewItem tvi in ic)
                {
                    if (node.StartsWith(tvi.Tag.ToString()))
                    {
                        found = true;
                        tvi.IsExpanded = true;
                        ic = tvi.Items;
                        if (node == tvi.Tag.ToString()) done = true;
                        break;
                    }
                }

                done = (found == false && done == false);
            }
        }

        /// <summary>
        /// This fuction sets ongoing status for each scenario
        /// </summary>
        /// <param name="status"></param>
        /// <param name="row"></param>
        public void DataGridBatchEditor_SetStatus(int status, int row)
        {

        }



        // Switch Windows button, to easily switch between opened windows
        int counter = 0;
        private void ButtonSwitchWindow_Click(object sender, RoutedEventArgs e)
        {
            string currentTitle = string.Empty;
            try
            {


                Button btn = (Button)sender;
                List<string> list = new List<string>();
                foreach (string handle in _driver.WindowHandles)
                {
                    list.Add(handle);
                }

                int index = counter % list.Count;

                if (index < list.Count)
                {
                    _driver.SwitchTo().Window(list[index]);
                    counter++;
                }


                currentTitle = _driver.Title;



                Applenium.Title = "Applenium. Current window - " + currentTitle;
            }
            catch (Exception)
            {

                Applenium.Title = "Applenium. Current window- " + currentTitle;
            }

        }

        /// <summary>
        /// Update progress bar with current test\scenario
        /// </summary>
        /// <param name="message"></param>
        public void UpdateProgressLabel(String message)
        {

            this.Dispatcher.BeginInvoke(
            (Action)delegate()
        {
            if (!message.Equals("", StringComparison.OrdinalIgnoreCase))
            {
                LblCurrentlyRunning.Content = "Running - " + message;
            }
            else LblCurrentlyRunning.Content = "";
        });

        }

        private void ComboboxTestingEnvironment_Loaded(object sender, RoutedEventArgs e)
        {

            DataGridFill(Constants.StrEnvironmentVersion, null);
        }


        private void ComboboxTestingEnvironment_TextChanged(object sender, KeyEventArgs e)
        {
            ComboBox comboBoxTestingEnvironment = (ComboBox) sender;
            using (var adapterEnvVer = new EnvironmentVersionTableAdapter())
            {
                if (e.Key == Key.Enter)
                {
                    string newTestEnv = comboBoxTestingEnvironment.Text;

                    if (adapterEnvVer.GetEnvironmnetVersionIDByVersionName(newTestEnv) == null)
                    {

                        PopupCloneFromTestingEnviromnet.IsOpen = true;
                    }


                    JsonParser json = new JsonParser();
                    json.WriteJson("TestingEnvironmentVersion", newTestEnv);
                    DataGrid_Configuration_Loaded(null, null);
                }
            }
            
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {

            using (var adapterEnvVer = new EnvironmentVersionTableAdapter())
            {
                //get next empty column version 

                string versionName = ComboboxTestingEnvironment.Text;
                string envVerId = adapterEnvVer.GetNextColumnVersion().ToString();
                adapterEnvVer.Update(versionName, Convert.ToInt32(envVerId));
                
                //add ver sparce column to GuiSteps
                var sql = new Sql();
                string toColumn = adapterEnvVer.GetColumnByID(Convert.ToInt32(envVerId)).ToString();
                string oldenvVerId =
                    adapterEnvVer.GetEnvironmnetVersionIDByVersionName(ComboboxTestingEnvironmentFromClone.Text).ToString();
                string fromColumn = adapterEnvVer.GetColumnByID(Convert.ToInt32(oldenvVerId)).ToString() ;
                sql.CopyColumn("GuiTestSteps",fromColumn,toColumn);
                sql.CopyColumn("GuiMap", fromColumn, toColumn);

                //clone checkboxes 

                PopupCloneFromTestingEnviromnet.IsOpen = false;


            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            PopupCloneFromTestingEnviromnet.IsOpen = false;
        }

        private void PopupCloneFromTestingEnviromnet_Opened(object sender, EventArgs e)
        {
            LabelConfigurationPopup.Content =
                "You are going to create new version of tests. \n Select from which version to clone tests ";
            DataGridFill(Constants.StrEnvironmentVersionClone, null);
        }


       private void newBrowserThread(string browser)
        {
            Thread t = new Thread(delegate(){
                 nullDriver = true;
                 Selenium selenium = new Selenium();
                 _driver = selenium.SetWebDriverBrowser(_driver, browser, true);
                 nullDriver = false;

             });
            t.Start();
        

        }

        private void closingBrowserThread()
        {
            new Thread(delegate()
            {
                try
                {
                    _driver.Quit();
                    nullDriver = true;
                }
                catch (NullReferenceException)
                {
                    Logger.Error("Failed to close driver in new thread");
                }
            }).Start();

        }




    }
}