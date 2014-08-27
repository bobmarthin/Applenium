﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Transactions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Applenium._2___Business_Logic;
using Applenium._3___DAL;
using Applenium._3___DAL.DataSetAutoTestTableAdapters;
using System.Deployment.Application;
using Microsoft.VisualBasic;
using OpenQA.Selenium;
using System.Linq;
using OpenQA.Selenium.Remote;
using Applenium._4____Infrustructure;
using OpenQA.Selenium.Interactions;
using System.Windows.Controls.Primitives;
using System.Text.RegularExpressions;


namespace Applenium
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml.
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
        private int _runexecutionid;
        private bool nullDriver = false;
        private string _upmessage = string.Empty;
        private bool _msgDisplayed = false;
        private AppleniumLogger logger = new AppleniumLogger();
        private string selectedItem = string.Empty;
        private Thread executionThread = null;
        private string browser;
        private string rowNumber = "1";
        private bool _guiMapIsEmpty = false;
        private bool _guiOntheFly = false;
        private string _guiMapValue;
        private string _loadtestid = string.Empty;
        private int _loadtestduration = 600;
        private string _loderioAppKey = string.Empty;

        //get Assembly version
        private string GetAssemblyVersion()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

            Version version = assembly.GetName().Version;
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                version = ApplicationDeployment.CurrentDeployment.CurrentVersion;
            }
            string name = assembly.GetName().Name;



            return name + " " + version;
        }

        /// <summary>
        ///     This is main window of eToroAutoStudio
        /// </summary>
        /// 
        public MainWindow()
        {
            try
            {

                InitializeComponent();

                Applenium.Title = GetAssemblyVersion();

                //verify if Configuration File exists 
                string configurationfile = ConfigurationManager.AppSettings["ConfigurationJsonFile"];
                if (File.Exists(configurationfile) == false)
                {
                    _upmessage = "No Configuration File!";

                    MessageBox.Show(_upmessage
                                    , "Applenium");
                    _msgDisplayed = true;

                    //copy configuration.json to app.cnfg file name 
                    File.Copy("Configuration.json", @configurationfile, true);
                }


                var jp = new JsonParser();
                Boolean res = jp.AddConfigToMemory("");

                browser = Constants.MemoryConf["DefaultBrowser"];


                LogObject infoLogger = new LogObject();
                infoLogger.Description =
                    "-------------------------------------------------------------------------------------------------------------\n                                             Applenium Started\n-------------------------------------------------------------------------------------------------------------";
                infoLogger.StatusTag = Constants.INFO;
                logger.Print(infoLogger);


                NewBrowserThread(browser);

            }
            catch (Exception exception)
            {

                LogObject exceptionLogger = new LogObject();
                exceptionLogger.StatusTag = Constants.ERROR;
                exceptionLogger.Description = exception.Message;
                exceptionLogger.Exception = exception;
                logger.Print(exceptionLogger);
            }
        }


        //********************** Initialization Process ******************************//

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
                var relProjectLog =
                    new DataRelation("ProjectLogs", dataset.Tables["Projects"].Columns["GuiProjectID"],
                                     dataset.Tables["TestResults"].Columns["ProjectID"], false);
                DataTable dt = new DataTable(); //= sql.GetDataTable(tableName, input);
                DataView view = new DataView(); //= dt != null ? dt.DefaultView : null;

                DataTable dataTable = new DataTable();
                var jp = new JsonParser();
                string showTestsObjectsFromAllProjects = Constants.MemoryConf["ShowTestsObjectsFromAllProjects"];

                string testingEnvironmentVersion = Constants.MemoryConf["TestingEnvironmentVersion"];
                //build column name 
                string testingEnvironmentVersionColumn;

                if (adapterEnvVer.GetEnvironmnetVersionIDByVersionName(testingEnvironmentVersion) != null)
                {
                    string environmentVersionId =
                        adapterEnvVer.GetEnvironmnetVersionIDByVersionName(testingEnvironmentVersion).ToString();
                    testingEnvironmentVersionColumn =
                        adapterEnvVer.GetColumnByID(Convert.ToInt32(environmentVersionId)).ToString();
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
                        if (dataset.GuiMap.Select("[" + testingEnvironmentVersionColumn + "]=1").Any())
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
                            DataGridNewScenario.IsReadOnly = false;
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
                            DataGridNewBatch.IsReadOnly = false;
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
                            DataGridTestEditor.IsReadOnly = false;
                            DataGridTestEditor.Background = Brushes.LightGray;
                        }
                        else
                            DataGridTestEditor.IsReadOnly = false;


                        if (dataset.GuiTestSteps.Select("[" + testingEnvironmentVersionColumn + "]=1").Any())
                        {
                            dataTable =
                                dataset.GuiTestSteps.Select("[" + testingEnvironmentVersionColumn + "]=1")
                                       .CopyToDataTable();
                            DataGridTestEditor.ItemsSource = dataTable.DefaultView;
                        }
                        else
                        {
                            adapterTeststeps.FillBy(dataset.GuiTestSteps, -1);
                            DataGridTestEditor.ItemsSource = dataset.GuiTestSteps.DefaultView;
                        }



                        //DataGridTestEditor.ItemsSource = dataset.GuiTestSteps.DefaultView;

                        adapterGuimap.Fill(dataset.GuiMap);
                        if (dataset.GuiMap.Select("[" + testingEnvironmentVersionColumn + "]=1").Any())
                        {
                            dataTable =
                                dataset.GuiMap.Select("[" + testingEnvironmentVersionColumn + "]=1").CopyToDataTable();
                            DataGridComboboxColumnGuiMap.ItemsSource = dataTable.DefaultView;
                            DataGridComboboxColumnGuiMapValue.ItemsSource = dataTable.DefaultView;
                            //DataGridComboboxColumnGuiMap.ItemsSource = dataset.GuiMap.DefaultView;
                        }
                        else
                        {
                            DataGridComboboxColumnGuiMap.ItemsSource = dataset.GuiMap.DefaultView;
                            DataGridComboboxColumnGuiMapValue.ItemsSource = dataset.GuiMap.DefaultView;
                        }


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

                        dataTable =
                            dataset.GuiTestSteps.Select("[" + testingEnvironmentVersionColumn + "]=1").CopyToDataTable();
                        // DataGridTestViewer.ItemsSource = dataset.GuiTestSteps.DefaultView;
                        DataGridTestViewer.ItemsSource = dataTable.DefaultView;

                        adapterGuimap.Fill(dataset.GuiMap);
                        DataGridComboboxColumnGuiMapTestViewer.ItemsSource = dataset.GuiMap.DefaultView;

                        adapterTcv.Fill(dataset.TestCommand);
                        DataGridComboboxColumnCommandTestViewer.ItemsSource = dataset.TestCommand.DefaultView;
                        break;
                    case Constants.StrScenarioViewer:
                        adapterTeststeps.FillBy(dataset.GuiTestSteps, Convert.ToInt32(input));

                        dataTable =
                            dataset.GuiTestSteps.Select("[" + testingEnvironmentVersionColumn + "]=1").CopyToDataTable();
                        // DataGridTestViewer.ItemsSource = dataset.GuiTestSteps.DefaultView;
                        DataGridTestViewer.ItemsSource = dataTable.DefaultView;

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
                        dt = sql.GetDataTable(tableName, input);
                        view = dt != null ? dt.DefaultView : null;
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

                        dt = sql.GetDataTable(tableName, input);
                        view = dt != null ? dt.DefaultView : null;
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
                        case Constants.StrInputBatchTable:

                        dt = sql.GetDataTable(tableName, input);
                        view = dt != null ? dt.DefaultView : null;
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

                        adapterScenario.Fill(dataset.Scenario);
                        adapterProjectsTest.Fill(dataset.Projects);
                        dataset.Relations.Add(relProjectScenario);
                        TreeViewProjectsNewScenario.ItemsSource = dataset.Tables["Projects"].DefaultView;

                        break;
                    case Constants.StrGuiProjectsNewBatch:

                        adapterProjectsTest.Fill(dataset.Projects);

                        TreeViewProjectsNewBatch.ItemsSource = dataset.Tables["Projects"].DefaultView;
                        break;
                    case Constants.StrLogResults:
                        if ((input != null) && (input != "0"))
                        {
                            dt = sql.GetDataTable(Constants.StrLogResults, input);
                            view = dt != null
                                       ? dt.DefaultView
                                       : null;
                            //adapterResults.FillBy  GroupByRunExecutionID(dataset.TestResults);
                            DataGridTestResults.ItemsSource = view;
                            //var cvs = CollectionViewSource.GetDefaultView(DataGridTestResults.ItemsSource);
                            //if (cvs != null)
                            //{
                            //    cvs.GroupDescriptions.Add(new PropertyGroupDescription("ScenarioName"));

                            //    cvs.GroupDescriptions.Add(new PropertyGroupDescription("TestName"));

                            //    cvs.Refresh();
                            //}
                            DataGridTestResults.IsEnabled = true;

                        }

                        else
                        {

                            DataGridTestResults.IsEnabled = false;
                        }





                        break;
                    case Constants.StrEnvironmentVersion:
                        adapterEnvVer.Fill(dataset.EnvironmentVersion);
                        ComboboxTestingEnvironment.ItemsSource = dataset.EnvironmentVersion.DefaultView;

                        break;
                    case Constants.StrEnvironmentVersionClone:
                        adapterEnvVer.Fill(dataset.EnvironmentVersion);
                        ComboboxTestingEnvironmentFromClone.ItemsSource = dataset.EnvironmentVersion.DefaultView;
                        break;
                    case Constants.StrEnvironmentVersionMove:
                        adapterEnvVer.Fill(dataset.EnvironmentVersion);
                        ComboboxSourceVersion.ItemsSource = dataset.EnvironmentVersion.DefaultView;
                        break;
                    case Constants.StrAnalayzing:
                        adapterProjectsTest.Fill(dataset.Projects);
                        adapterResults.FillByGroupByRunExecutionID(dataset.TestResults);
                        adapterBatch.Fill(dataset.Batches);

                        dataset.Relations.Add(relProjectLog);
                        TreeViewLogs.ItemsSource = dataset.Tables["Projects"].DefaultView;
                        break;
                    default:


                        LogObject loggerException = new LogObject();
                        loggerException.Description = "Default case";
                        loggerException.StatusTag = Constants.DEBUG;
                        logger.Print(loggerException);
                        break;
                }
            }
            catch (Exception exception)
            {

                LogObject exceptionLogger2 = new LogObject();
                exceptionLogger2.StatusTag = Constants.ERROR;
                exceptionLogger2.Description = exception.Message;
                exceptionLogger2.Exception = exception;
                logger.Print(exceptionLogger2);
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
                LogObject exceptionLogger2 = new LogObject();
                exceptionLogger2.StatusTag = Constants.ERROR;
                exceptionLogger2.Description = exception.Message;
                exceptionLogger2.Exception = exception;
                logger.Print(exceptionLogger2);
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
                LogObject exceptionLogger2 = new LogObject();
                exceptionLogger2.StatusTag = Constants.ERROR;
                exceptionLogger2.Description = exception.Message;
                exceptionLogger2.Exception = exception;
                logger.Print(exceptionLogger2);
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
                LogObject exceptionLogger2 = new LogObject();
                exceptionLogger2.StatusTag = Constants.ERROR;
                exceptionLogger2.Description = exception.Message;
                exceptionLogger2.Exception = exception;
                logger.Print(exceptionLogger2);
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
            var adapterTestResult = new TestResultsTableAdapter();
            try
            {
                if (row.DataView.Table.Columns.Contains("RunExecutionID"))
                {
                    _scenarioid = 0;
                    _runexecutionid = Convert.ToInt32(row["RunExecutionID"].ToString());
                    _projectid = Convert.ToInt32(adapterTestResult.GetProjectID(_runexecutionid));
                    _projectPageid = 0;
                    _pageSectionid = 0;

                    _batchid = 0;
                    _testid = 0;
                }
                else if (row.DataView.Table.Columns.Contains("TestId"))
                {
                    _testid = Convert.ToInt32(row["TestId"].ToString());
                    _pageSectionid = Convert.ToInt32(adapterTest.GetPageSectionID(_pageSectionid));
                    _projectPageid = Convert.ToInt32(adapterPageSection.GetGuiPageID(_pageSectionid));
                    _projectid = Convert.ToInt32(adapterGuiproject.GetProjectID(_projectPageid));
                    _batchid = 0;
                    _scenarioid = 0;
                    _runexecutionid = 0;
                }
                else if (row.DataView.Table.Columns.Contains("GuiPageSectionID"))
                {
                    _pageSectionid = Convert.ToInt32(row["GuiPageSectionID"].ToString());
                    _projectPageid = Convert.ToInt32(adapterPageSection.GetGuiPageID(_pageSectionid));
                    _projectid = Convert.ToInt32(adapterGuiproject.GetProjectID(_projectPageid));
                    _testid = 0;
                    _batchid = 0;
                    _scenarioid = 0;
                    _runexecutionid = 0;
                }
                else if (row.DataView.Table.Columns.Contains("GuiProjectPageID"))
                {
                    _projectPageid = Convert.ToInt32(row["GuiProjectPageID"].ToString());
                    _projectid = Convert.ToInt32(adapterGuiproject.GetProjectID(_projectPageid));
                    _pageSectionid = 0;
                    _testid = 0;
                    _batchid = 0;
                    _scenarioid = 0;
                    _runexecutionid = 0;
                }
                else if (row.DataView.Table.Columns.Contains("GuiProjectID"))
                {
                    _projectid = Convert.ToInt32(row["GuiProjectID"].ToString());
                    _projectPageid = 0;
                    _pageSectionid = 0;
                    _testid = 0;
                    _batchid = 0;
                    _scenarioid = 0;
                    _runexecutionid = 0;
                }
                else if (row.DataView.Table.Columns.Contains("BatchID"))
                {
                    _batchid = Convert.ToInt32(row["BatchID"].ToString());
                    _projectid = Convert.ToInt32(adapterBatch.GetProjectID(_batchid));
                    _projectPageid = 0;
                    _pageSectionid = 0;
                    _scenarioid = 0;
                    _testid = 0;
                    _runexecutionid = 0;
                }

                else if (row.DataView.Table.Columns.Contains("ScenarioID"))
                {
                    _scenarioid = Convert.ToInt32(row["ScenarioID"].ToString());
                    _projectid = Convert.ToInt32(adapterScenario.GetProjectID(_scenarioid));
                    _projectPageid = 0;
                    _pageSectionid = 0;
                    _runexecutionid = 0;
                    _batchid = 0;
                    _testid = 0;
                }


            }
            catch (Exception exception)
            {
                LogObject exceptionLogger2 = new LogObject();
                exceptionLogger2.StatusTag = Constants.ERROR;
                exceptionLogger2.Description = exception.Message;
                exceptionLogger2.Exception = exception;
                logger.Print(exceptionLogger2);
            }
        }

        private void TreeViewTests_Loaded(object sender, RoutedEventArgs e)
        {
            DataGridFill(Constants.StrGuiTest, null);
        }

        //********************** Editing and Updating *******************************//

        private void TreeViewProjects_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                SetProjectPageScetionId(sender);
                DataGridFill(Constants.StrGuiMap, _pageSectionid.ToString(CultureInfo.InvariantCulture));
            }
            catch (Exception exception)
            {
                LogObject exceptionLogger2 = new LogObject();
                exceptionLogger2.StatusTag = Constants.ERROR;
                exceptionLogger2.Description = exception.Message;
                exceptionLogger2.Exception = exception;
                logger.Print(exceptionLogger2);
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
                LogObject exceptionLogger2 = new LogObject();
                exceptionLogger2.StatusTag = Constants.ERROR;
                exceptionLogger2.Description = exception.Message;
                exceptionLogger2.Exception = exception;
                logger.Print(exceptionLogger2);
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
                        verid =
                            adapterEnvVer.GetEnvironmnetVersionIDByVersionName(
                                jsonParser.ReadJson("TestingEnvironmentVersion"))
                                         .ToString();
                        versionColumn = adapterEnvVer.GetColumnByID(Convert.ToInt32(verid)).ToString();
                        guiMapIdlast = guiMapTableAdapter.GetLastGuiMapId().ToString();
                        sql.UpdateVersion("GuiMap", "GuiMapID", guiMapIdlast, versionColumn, 1);

                        DataGridFill(Constants.StrGuiMapEdit, "0");
                    }
                    else
                    {

                        verid =
                            adapterEnvVer.GetEnvironmnetVersionIDByVersionName(
                                jsonParser.ReadJson("TestingEnvironmentVersion"))
                                         .ToString();
                        versionColumn = adapterEnvVer.GetColumnByID(Convert.ToInt32(verid)).ToString();
                        //copy test first
                        sql.CopyObjectByVersion(guiMapId.ToString(), versionColumn);
                        string guiMapIdold = sql.GetGuiMapIdByVersion(guiMapId.ToString(), versionColumn);
                        // unselect from previos version
                        sql.UpdateVersion("GuiMap", "GuiMapID", guiMapIdold, versionColumn, 0);


                        guiMapIdlast = guiMapTableAdapter.GetLastGuiMapId().ToString();

                        //select version in new step                       
                        guiMapTableAdapter.UpdateBy(guiMapObjectName, tagTypeId, tagTypeValue, pageSectionId,
                                                    Convert.ToInt32(guiMapIdlast));
                        sql.UpdateVersion("GuiMap", "GuiMapID", guiMapIdlast, versionColumn, 1);
                        DataGridFill(Constants.StrGuiMapEdit, guiMapIdlast.ToString(CultureInfo.InvariantCulture));


                        //triger - update all test steps - where this object is used 

                        sql.UpdateTestStepsByGuiMap(guiMapId.ToString(), guiMapIdlast, versionColumn);
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Applenium");
                LogObject exceptionLogger2 = new LogObject();
                exceptionLogger2.StatusTag = Constants.ERROR;
                exceptionLogger2.Description = exception.Message;
                exceptionLogger2.Exception = exception;
                logger.Print(exceptionLogger2);
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
                            guiMapTableAdapter.Delete(guiMapId, tagTypeId, guiProjectId, tagTypeValue, guiMapObjectName);


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
                LogObject exceptionLogger2 = new LogObject();
                exceptionLogger2.StatusTag = Constants.ERROR;
                exceptionLogger2.Description = exception.Message;
                exceptionLogger2.Exception = exception;
                logger.Print(exceptionLogger2);
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
                                    Constants.MemoryConf["TestingEnvironmentVersion"])
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
                LogObject exceptionLogger2 = new LogObject();
                exceptionLogger2.StatusTag = Constants.ERROR;
                exceptionLogger2.Description = exception.Message;
                exceptionLogger2.Exception = exception;
                logger.Print(exceptionLogger2);
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
                LogObject exceptionLogger2 = new LogObject();
                exceptionLogger2.StatusTag = Constants.ERROR;
                exceptionLogger2.Description = exception.Message;
                exceptionLogger2.Exception = exception;
                logger.Print(exceptionLogger2);
            }
        }


        private void DataGridTestEditor_RowEditing(object sender, DataGridRowEditEndingEventArgs e)
        {
            var sql = new Sql();
            var jsonParser = new JsonParser();
            var guiMapTableAdapter = new GuiMapTableAdapter();
            string guiMapObjectName = string.Empty;

            int guiMapId = 0;
            try
            {
                var rowView = e.Row.Item as DataRowView;
                var adapterEnvVer = new EnvironmentVersionTableAdapter();
                using (var guiTestStepsTableAdapter = new GuiTestStepsTableAdapter())
                {
                    if ((rowView != null) &&
                        ((rowView.Row["GuiMapID"].ToString() == string.Empty) ||
                         (rowView.Row["GuiMapID"].ToString() == "0")))
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
                            rowView.Row["GuiMapCommandID"].ToString() != "36" &&
                            rowView.Row["GuiMapCommandID"].ToString() != "1041" &&
                            rowView.Row["GuiMapCommandID"].ToString() != "1042" &&
                            rowView.Row["GuiMapCommandID"].ToString() != "1072" &&
                            rowView.Row["GuiMapCommandID"].ToString() != "1073" &&
                            rowView.Row["GuiMapCommandID"].ToString() != "1076"

                            )
                        //throw new Exception("The Test Map Object can't be empty. Please check");
                        {

                            //parce _guiMapValue
                            var guimapArray = _guiMapValue.Split(new[] { "::" }, StringSplitOptions.None);
                            int tagTypeId = 1;
                            string tagTypeValue = _guiMapValue;

                            for (int i = 0; i < guimapArray.Count(); i++)

                                switch (guimapArray[i])
                                {
                                    case Constants.CssTemplate:
                                        tagTypeId = 1;
                                        tagTypeValue = guimapArray[i + 1];
                                        break;
                                    case Constants.NameTemplate:
                                        tagTypeId = 2;
                                        tagTypeValue = guimapArray[i + 1];
                                        break;
                                    case Constants.XpathTemplate:
                                        tagTypeId = 5;
                                        tagTypeValue = guimapArray[i + 1];
                                        break;
                                    case Constants.IdTemplate:
                                        tagTypeId = 6;
                                        tagTypeValue = guimapArray[i + 1];
                                        break;
                                    default:

                                        if ((i == 0) && (guimapArray.Count() == 1))
                                        {
                                            tagTypeValue = guimapArray[i];
                                            guiMapObjectName = tagTypeValue + Utilities.GetTimeStamp();
                                        }

                                        else if ((i == 0) && (guimapArray.Count() == 2))
                                        {
                                            tagTypeValue = guimapArray[i + 1];
                                            guiMapObjectName = guimapArray[i];
                                        }
                                        else if ((i == 0) && (guimapArray.Count() == 3))
                                            guiMapObjectName = guimapArray[i];

                                        break;

                                }
                            //popup would you like create new object 


                            //guiMapObjectName = Interaction.InputBox("Would you like to create new GuiMap Object "+ guiMapObjectName+" with value :" + tagTypeValue+". You can rename the Object Name :" , "New object/command", guiMapObjectName);
                            //if (guiMapObjectName != string.Empty)
                            //{
                            //    guiMapTableAdapter.Insert(guiMapObjectName, tagTypeId, tagTypeValue, _pageSectionid);

                            //    //create new function in SQL that update 
                            //    string verid =
                            //        adapterEnvVer.GetEnvironmnetVersionIDByVersionName(
                            //            Constants.MemoryConf["TestingEnvironmentVersion"])
                            //                     .ToString();
                            //    string versionColumn = adapterEnvVer.GetColumnByID(Convert.ToInt32(verid)).ToString();
                            //    string guiMapIdlast = guiMapTableAdapter.GetLastGuiMapId().ToString();
                            //    sql.UpdateVersion("GuiMap", "GuiMapID", guiMapIdlast, versionColumn, 1);
                            //    guiMapId = Convert.ToInt32(guiMapIdlast);
                            //}
                        }

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
                        verid = adapterEnvVer.GetEnvironmnetVersionIDByVersionName(Constants.MemoryConf["TestingEnvironmentVersion"])
                                         .ToString();
                        versionColumn = adapterEnvVer.GetColumnByID(Convert.ToInt32(verid)).ToString();
                        if (guiTestStepsId == -1)
                        {
                            rowView.Row["IsValidate"] = 0;
                            // stepsOrder = Convert.ToInt32(guiTestStepsTableAdapter.LastStepsOrder(guiTestId)) + 1;

                            stepsOrder = Convert.ToInt32(sql.GetLastStepOrder(guiTestId.ToString(), versionColumn)) + 1;

                            if (guiTestId != 0)
                            {
                                //if (guiMapCommandId==1077)
                                // add gui map id 
                                //guiMapTableAdapter.Insert(
                                //guiTestStepsTableAdapter.Insert(guiTestId, guiMapId, guiMapCommandId, inputDataColumn, isValidate,stepsOrder);
                                guiTestStepsTableAdapter.Insert(guiTestId, guiMapId, guiMapCommandId, inputDataColumn, isValidate,
                                                   stepsOrder);
                            }

                            else
                            {
                                MessageBox.Show("Please select a test first.", "Applenium");
                            }

                            //add version to new step

                            //create new function in SQL that update 

                            testStepsId = guiTestStepsTableAdapter.GetLastGuiTestStepsID().ToString();
                            sql.UpdateVersion("GuiTestSteps", "GuiTestStepsID", testStepsId, versionColumn, 1);
                        }
                        else
                        {

                            stepsOrder = Convert.ToInt32(rowView.Row["StepsOrder"].ToString());
                            verid = adapterEnvVer.GetEnvironmnetVersionIDByVersionName(Constants.MemoryConf["TestingEnvironmentVersion"])
                                        .ToString();
                            versionColumn = adapterEnvVer.GetColumnByID(Convert.ToInt32(verid)).ToString();
                            //copy test first
                            sql.CopyStepByVersion(guiTestId.ToString(), stepsOrder.ToString(), versionColumn);
                            string testStepsIdold = sql.GetStepsIdByVersion(guiTestId.ToString(), stepsOrder.ToString(),
                                                                            versionColumn);
                            // unselect from previos version
                            sql.UpdateVersion("GuiTestSteps", "GuiTestStepsID", testStepsIdold, versionColumn, 0);


                            testStepsId = guiTestStepsTableAdapter.GetLastGuiTestStepsID().ToString();

                            //select version in new step                       
                            sql.UpdateVersion("GuiTestSteps", "GuiTestStepsID", testStepsId, versionColumn, 1);


                            if (guiTestId != 0)
                            {
                                guiTestStepsTableAdapter.UpdateBy(guiTestId, guiMapId, guiMapCommandId, inputDataRow,
                                                                  inputDataColumn, isValidate, stepsOrder,
                                                                  Convert.ToInt32(testStepsId));
                            }

                            else
                            {
                                MessageBox.Show("Please select a test first.", "Applenium");
                            }


                        }
                    }
                }
            }

            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Applenium");
                LogObject exceptionLogger2 = new LogObject();
                exceptionLogger2.StatusTag = Constants.ERROR;
                exceptionLogger2.Description = exception.Message;
                exceptionLogger2.Exception = exception;
                logger.Print(exceptionLogger2);
            }

            finally
            {
                DataGridFill(Constants.StrGuiTestSteps, _testid.ToString(CultureInfo.InvariantCulture));
            }
        }

        private void dataGridTestEditor_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var adapterEnvVer = new EnvironmentVersionTableAdapter();
            var jsonParser = new JsonParser();

            string verid = adapterEnvVer.GetEnvironmnetVersionIDByVersionName(Constants.MemoryConf["TestingEnvironmentVersion"])
                    .ToString();
            string versionColumn = adapterEnvVer.GetColumnByID(Convert.ToInt32(verid)).ToString();

            try
            {
                var dataGrid = (DataGrid)sender;
                var rowView = (DataRowView)dataGrid.SelectedItem;
                if (e.Key == Key.Delete)
                {


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
                                string newguiTestStepsId = sql.CopyStepAndChangeVersion(currentStepsOrder, guiTestId, versionColumn);


                                //reorder all steps by copieng befor reorder :

                                int selectItem = currentStepsOrder - 1;
                                while (sql.GetStepsIdByVersion(guiTestId.ToString(), (currentStepsOrder + 1).ToString(), versionColumn) != string.Empty)
                                {
                                    int nextStepsOrder = currentStepsOrder + 1;
                                    //int nextGuiTestStepsId =Convert.ToInt32(guiTestStepsTableAdapter.GetStepId(guiTestId, nextStepsOrder));
                                    //copy next step
                                    int nextGuiTestStepsId =
                                        Convert.ToInt32(sql.CopyStepAndChangeVersion(nextStepsOrder, guiTestId,
                                                                                     versionColumn));
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
                LogObject exceptionLogger2 = new LogObject();
                exceptionLogger2.StatusTag = Constants.ERROR;
                exceptionLogger2.Description = exception.Message;
                exceptionLogger2.Exception = exception;
                logger.Print(exceptionLogger2);
            }
        }

        private void DataGridTestEditor_BeginingEdit(object sender, DataGridBeginningEditEventArgs e)
        {

            _editinggrid = true;
        }

        private void DataGridTestEditoer_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (PopupTestCommand.IsOpen == true)
            {
                return;
            }

            int i = 0;
            string cmd_val = "";
            string guimap_name = "";
            string guimap_val = "";
            string testId = "";

            string sql_cmd = "";

            string DBguimapName = "";
            string DBguiMapValue = "";

            string TagTypeID = "";
            string GuiMapID = "";
            string TagTypeName = "";
            string TestStepsGuiMapID = "";
            bool NameFlag = false;
            bool ValueFlag = false;


            string GuiMap = "[QA_Autotest].[Test].[GuiMap]";
            string GuiTestStepsTable = "[QA_Autotest].[Test].[GuiTestSteps]";
            string GuiTagType = "[QA_Autotest].[Test].[GuiTagType]";

            var dataGrid = (DataGrid)sender;

            DataGridColumn ColData = e.Column;
            DataGridRow RowData = e.Row;

            int row_index = ((DataGrid)sender).ItemContainerGenerator.IndexFromContainer(RowData);
            int col_index = ColData.DisplayIndex;

            if (col_index != 1 && col_index != 2)
            {
                _editinggrid = false;
                return;
            }

            DataGridTestEditor.ScrollIntoView(RowData, ColData);
            DataGridCellInfo cellInfo = new DataGridCellInfo(RowData, ColData);

            if (TreeViewTests.SelectedItem != null)
            {
                var row = (DataRowView)TreeViewTests.SelectedItem;
                testId = row["TestId"].ToString();

                foreach (DataGridColumn column in DataGridTestEditor.Columns)
                {
                    if (column.GetCellContent(RowData) is ComboBox)
                    {
                        ComboBox cellContent = column.GetCellContent(RowData) as ComboBox;
                        if (i == 0)
                        {
                            cmd_val = cellContent.Text;
                            if (cmd_val == "")
                            {

                                System.Windows.Forms.MessageBox.Show("Please choose command first.", "Warning", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                                _editinggrid = false;
                                return;
                            }
                        }
                        else if (i == 1)
                        {
                            guimap_name = cellContent.Text;
                        }
                        else if (i == 2)
                        {
                            guimap_val = cellContent.Text;
                        }
                        i++;
                    }
                }


                var sql = new Sql();
                string versionColumn = sql.GetVersionColumn(); //AND (" + versionColumn + " = 1)"

                //GET GuiMapID from test table
                sql_cmd = "GuiTestID ='" + testId + "' AND StepsOrder='" + (row_index + 1) + "' AND " + versionColumn + " = 1";
                TestStepsGuiMapID = sql.GetOneParameter("GuiMapID", GuiTestStepsTable, sql_cmd);

                TBTestStepID.Text = Convert.ToString(row_index + 1);
                TBTestID.Text = testId;
                TBVersionClnID.Text = versionColumn;

                if (col_index == 1) // guimap name
                {
                    sql_cmd = "GuiMapObjectName ='" + guimap_name + "'";
                    string C1_DBguimapName = sql.GetOneParameter("GuiMapObjectName", GuiMap, sql_cmd);

                    if (C1_DBguimapName != string.Empty)
                    {
                        _editinggrid = false;
                        return;
                    }
                    else
                    {
                        NameFlag = false;
                        ValueFlag = false;
                    }

                }
                else if (col_index == 2) // guimap name
                {
                    sql_cmd = "TagTypeValue ='" + guimap_val + "' AND GuiMapObjectName ='" + guimap_name + "'";
                    DBguimapName = sql.GetOneParameter("GuiMapObjectName", GuiMap, sql_cmd);
                    if (DBguimapName != string.Empty)
                    {
                        _editinggrid = false;
                        return;
                    }
                    sql_cmd = "TagTypeValue ='" + guimap_val + "'";
                    DBguiMapValue = sql.GetOneParameter("TagTypeValue", GuiMap, sql_cmd);
                    GuiMapID = sql.GetOneParameter("GuiMapID", GuiMap, sql_cmd);
                    TagTypeID = sql.GetOneParameter("TagTypeID", GuiMap, sql_cmd);
                    TagTypeName = sql.GetOneParameter("TagType", GuiTagType, "TagTypeID='" + TagTypeID + "'");

                    if (DBguiMapValue != string.Empty)
                    {
                        //if (DBguimapName == guimap_name)
                        //{
                        //    _editinggrid = false;
                        //    return;
                        //}
                        ValueFlag = true;
                        NameFlag = false;
                    }
                    else
                    {
                        NameFlag = false;
                        ValueFlag = false;
                    }

                }
                else
                {
                    _editinggrid = false;
                    return;
                }
                if (TestStepsGuiMapID == GuiMapID && TestStepsGuiMapID != string.Empty)
                {
                    _editinggrid = false;
                    return;
                }

                if (ValueFlag)// value cell - changed and new value exists in GuiMAP table
                {
                    string guimap_name_tmp = Interaction.InputBox("The GuiMap Command '" + DBguimapName + "' with value '" + DBguiMapValue + "' already exists. Enter 'Cancel' to use existing command?\n\nEnter New Gui Command Name for Create New", "GuiMaP Help Wizard", DBguimapName);
                    if (guimap_name_tmp == string.Empty)
                    {

                        // UPDATE [QA_Autotest].[Test].[GuiTestSteps] SET [GuiMapID]= '"+ GuiMapID +"'  WHERE GuiTestID=3837  and StepsOrder=2 and Ver1=1
                        sql_cmd = " GuiTestID='" + testId + "' AND StepsOrder='" + (row_index + 1) + "' AND " + versionColumn + " =1";
                        string giumap_tmp = sql.UpdateOneParameter("GuiMapID", GuiMapID, GuiTestStepsTable, sql_cmd);
                        if (giumap_tmp == GuiMapID)
                        {
                            DataGridFill(Constants.StrGuiTestSteps, _testid.ToString(CultureInfo.InvariantCulture));
                            _editinggrid = false;
                            return;
                        }
                    }
                    else
                    {
                        guimap_name = guimap_name_tmp;
                    }

                }

                TBGuiMapName.Text = "";
                TBGuiMapValue.Text = "";
                TBGuiMapTagTypeName.Text = "";

                TBGuiMapTagTypeName.Visibility = System.Windows.Visibility.Visible;
                LABGuiMapTagTypeName.Visibility = System.Windows.Visibility.Visible;

                Regex cmd_type = new Regex(@"^\s*(ssh|var|validation|http|include)");
                Match mcmd = cmd_type.Match(cmd_val);

                if (mcmd.Success)
                {
                    TBGuiMapTagTypeName.Visibility = System.Windows.Visibility.Hidden;
                    LABGuiMapTagTypeName.Visibility = System.Windows.Visibility.Hidden;

                }

                TBCmdName.Text = cmd_val;
                TBCmdName.IsEnabled = false;
                LABCmdName.IsEnabled = false;

                TBGuiMapName.Text = guimap_name;
                TBGuiMapValue.Text = guimap_val;
                TBGuiMapTagTypeName.Text = TagTypeName;

                PopupWizardName.Content = "Create New GuiMap Command Wizard";

                PopupTestCommand.IsOpen = true;

            }
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
                LogObject exceptionLogger2 = new LogObject();
                exceptionLogger2.StatusTag = Constants.ERROR;
                exceptionLogger2.Description = exception.Message;
                exceptionLogger2.Exception = exception;
                logger.Print(exceptionLogger2);
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
                LogObject exceptionLogger2 = new LogObject();
                exceptionLogger2.StatusTag = Constants.ERROR;
                exceptionLogger2.Description = exception.Message;
                exceptionLogger2.Exception = exception;
                logger.Print(exceptionLogger2);
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
                LogObject exceptionLogger2 = new LogObject();
                exceptionLogger2.StatusTag = Constants.ERROR;
                exceptionLogger2.Description = exception.Message;
                exceptionLogger2.Exception = exception;
                logger.Print(exceptionLogger2);
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
                        adapterTest.Insert(testName, pageSectionId, inputTableName, isApi, description, statusCompleted,
                                           pageSectionId);
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
                LogObject exceptionLogger2 = new LogObject();
                exceptionLogger2.StatusTag = Constants.ERROR;
                exceptionLogger2.Description = exception.Message;
                exceptionLogger2.Exception = exception;
                logger.Print(exceptionLogger2);
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
                LogObject exceptionLogger2 = new LogObject();
                exceptionLogger2.StatusTag = Constants.ERROR;
                exceptionLogger2.Description = exception.Message;
                exceptionLogger2.Exception = exception;
                logger.Print(exceptionLogger2);
            }
        }



        //************************ Execution And Commands ************************//

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


                LogObject exceptionLogger2 = new LogObject();
                exceptionLogger2.StatusTag = Constants.ERROR;
                exceptionLogger2.Description = exception.Message;
                exceptionLogger2.Exception = exception;
                logger.Print(exceptionLogger2);
            }
        }

        private void btnNewTest_Click(object sender, RoutedEventArgs e)
        {
            PopupNewTest.IsOpen = true;
        }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            int runExecutionId;
            bool alreadyRunning = executionThreaIsRunning();
            bool whatUserSaid = false;

            // Gather Needed Data         
            using (var adapterTestresult = new TestResultsTableAdapter())
            {
                var sql = new Sql();
                selectedItem = string.Empty;
                try
                {
                    if (TreeViewTests.SelectedItem != null)
                    {
                        var row = (DataRowView)TreeViewTests.SelectedItem;
                        selectedItem = row["TestId"].ToString();
                    }
                    runExecutionId = Utilities.GetTimeStamp();

                    // if thread is already running by another test/scenario/batch, ask user what to do next
                    if (alreadyRunning)
                    {

                        whatUserSaid = askWhatToDO();
                        if (whatUserSaid)
                        {
                            // Kill previously running execution
                            executionThread.Abort();

                            if (sql.GetInputDataTableName(selectedItem, false) != null)
                                rowNumber = Interaction.InputBox("Please select on what line to execute the test",
                                                                 "Before executing tests navigate browser to right URL",
                                                                 "1");

                            // Update UI
                            HideStopBtnScenario();
                            ShowStopBtn();
                            HideStopBtnBatch();
                            UpdateProgressLabel("", "", Constants.UpdateProgress_REGULAR);

                            // Work in background
                            executionThread = new Thread(() => RunThreaded("test"));
                            executionThread.Start();
                        }
                        // If user canceled, do nothing.
                        else HideStopBtnScenario();
                    }

                    else if (!alreadyRunning)
                    {
                        // Update UI
                        ShowStopBtn();
                        HideStopBtnScenario();
                        HideStopBtnBatch();
                        UpdateProgressLabel("", "", Constants.UpdateProgress_REGULAR);

                        if (sql.GetInputDataTableName(selectedItem, false) != null)
                            rowNumber = Interaction.InputBox("Please select on what line to execute the test",
                                                             "Before executing tests navigate browser to right URL", "1");

                        // Work In background
                        executionThread = new Thread(() => RunThreaded("test"));
                        executionThread.Start();
                    }

                }
                catch (Exception exception)
                {
                    LogObject exceptionLogger2 = new LogObject();
                    exceptionLogger2.StatusTag = Constants.ERROR;
                    exceptionLogger2.Description = exception.Message;
                    exceptionLogger2.Exception = exception;
                    logger.Print(exceptionLogger2);
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

            // Kill current executing thread
            executionThread.Abort();
            executionThread = null;
            //Update UI
            UpdateProgressLabel(Constants.STOPPED_EXPLICIT, "", Constants.UpdateProgress_STOPPED);
            HideStopBtn();
            HideStopBtnScenario();
            HideStopBtnBatch();

            LogObject StoppedExplicit = new LogObject();
            StoppedExplicit.StatusTag = Constants.ERROR;
            StoppedExplicit.Description = Constants.STOPPED_EXPLICIT;
            logger.Print(StoppedExplicit);
        }


        private void btnStopBatch_Click(object sender, RoutedEventArgs e)
        {
            Singleton myInstance = Singleton.Instance; // Will always be the same instance...
            myInstance.StopExecution = true;


            UpdateProgressLabel("", "", Constants.UpdateProgress_REGULAR);
        }

        private bool ExecuteTest(string selectedItem, string rowNumber, int runExecutionId, RemoteWebDriver driver)
        {
            var exman = new ExecutionManager(runExecutionId, this, false);
            var testStatus = new Dictionary<string, int>();
            bool result = exman.ExecuteOneTest(selectedItem, rowNumber, driver, ref testStatus);

            UpdateProgressLabel("", "", Constants.UpdateProgress_REGULAR);
            return result;
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
                LogObject exceptionLogger2 = new LogObject();
                exceptionLogger2.StatusTag = Constants.ERROR;
                exceptionLogger2.Description = exception.Message;
                exceptionLogger2.Exception = exception;
                logger.Print(exceptionLogger2);
            }
        }


        /// <summary>
        /// Stop running scenario
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStopScenario_Click(object sender, RoutedEventArgs e)
        {
            nullDriver = false;
            Singleton myInstance = Singleton.Instance; // Will always be the same instance...
            myInstance.StopExecution = true;
            // Kill running thread execution
            try
            {
                executionThread.Abort();
                executionThread = null;
            }

            catch (Exception exa)
            {


            }


            // Update UI
            UpdateProgressLabel(Constants.STOPPED_EXPLICIT, "", Constants.UpdateProgress_STOPPED);
            HideStopBtnScenario();
            HideStopBtn();
            HideStopBtnBatch();

            LogObject StoppedExplicit = new LogObject();
            StoppedExplicit.StatusTag = Constants.ERROR;
            StoppedExplicit.Description = Constants.STOPPED_EXPLICIT;
            logger.Print(StoppedExplicit);
        }

        /// <summary>
        /// Run selected scenario
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRunScenario_Click(object sender, RoutedEventArgs e)
        {

            bool alreadyRunning = executionThreaIsRunning();
            bool whatUserSaid = false;

            // Gather needed data
            using (var adapterTestresult = new TestResultsTableAdapter())
            {
                try
                {
                    int runExecutionId = Utilities.GetTimeStamp();
                    if (TreeViewScenarios.SelectedItem != null)
                    {
                        var row = (DataRowView)TreeViewScenarios.SelectedItem;
                        selectedItem = row["ScenarioID"].ToString();
                    }

                    // if thread is already running by another test/scenario/batch, ask user what to do next
                    if (alreadyRunning)
                    {
                        whatUserSaid = askWhatToDO();

                        // if user said "Okay", kill previously running execution and start a new one instead 
                        if (whatUserSaid)
                        {
                            // Kill previously running execution
                            executionThread.Abort();

                            // Update UI
                            ShowStopBtnScenario();
                            HideStopBtn();
                            HideStopBtnBatch();
                            UpdateProgressLabel("...", "", Constants.UpdateProgress_REGULAR);

                            try
                            {
                                // Work in background
                                executionThread = new Thread(() => RunThreaded("scenario"));
                                executionThread.Start();
                            }

                            catch (Exception ex)
                            {
                                //Console.WriteLine(ex.ToString());
                            }

                        }
                        // If user canceled, do nothing. Let the previous execution continue
                        else HideStopBtnScenario();
                    }

                    else if (!alreadyRunning)
                    {
                        // Update UI
                        ShowStopBtnScenario();
                        HideStopBtn();
                        HideStopBtnBatch();
                        UpdateProgressLabel("", "", Constants.UpdateProgress_REGULAR);
                        // Work In background
                        executionThread = new Thread(() => RunThreaded("scenario"));
                        executionThread.Start();
                    }


                }
                catch (Exception exception)
                {
                    LogObject exceptionLogger2 = new LogObject();
                    exceptionLogger2.StatusTag = Constants.ERROR;
                    exceptionLogger2.Description = exception.Message;
                    exceptionLogger2.Exception = exception;
                    logger.Print(exceptionLogger2);
                }
            }
        }

        /// <summary>
        ///  run a new scenario, usually called through a new Thread.
        /// </summary>
        private void RunThreaded(string testType)
        {
            try
            {
                int runExecutionId = Utilities.GetTimeStamp();
                JsonParser jp = new JsonParser();
                MessageBoxResult mbr;
                var ex = new ExecutionManager(runExecutionId, this, false);
                bool result = false;

                // Preper driver
                Selenium selenium = new Selenium();
                browser = Constants.MemoryConf["DefaultBrowser"];
                if ((testType != "loadtest") && (browser != "android"))
                {
                    _driver.Manage().Window.Maximize();
                }

                Singleton.Instance.StopExecution = false;

                // Choose what to run based on passed parameter
                switch (testType)
                {
                    case "scenario":
                        result = ex.ExecuteOneScenario(selectedItem, _driver);
                        break;
                    case "test":
                        result = ExecuteTest(selectedItem, rowNumber.Trim(), runExecutionId, _driver);
                        break;
                    case "loadtest":

                        string result_id = RunLoadTest(_loadtestid, _loadtestduration, _loderioAppKey);
                        break;
                }

                // Update UI - fill analayzing tab with results

                if (testType != "loadtest")
                {
                    Debug.WriteLine("Maxim-RunThreadedDispatcher");
                    Dispatcher.Invoke(
                        () =>
                        DataGridFill(Constants.StrLogResults, runExecutionId.ToString(CultureInfo.InvariantCulture)));
                }

                // if user choose so - go to anaylzing zone, otherwise stay where you are

                if (testType == "loadtest")
                {
                    mbr = MessageBox.Show("Loader.io execution was completed");
                    //redirect browser to right test results
                    if (mbr == MessageBoxResult.OK)
                    {
                        // Update UI
                        Dispatcher.Invoke(() => HideStopBtnLoadTest());

                        Dispatcher.Invoke(() => UpdateProgressLabel("", "", Constants.UpdateProgress_EMPTY));
                        // Kill the thread
                        executionThread = null;
                        Singleton.Instance.StopExecution = false;
                        Dispatcher.Invoke(() => NavigateWebBrowserLoadTest(_loadtestid));

                    }





                    else if (mbr == MessageBoxResult.Cancel)
                    {
                        // Update UI
                        Dispatcher.Invoke(() => HideStopBtnLoadTest());
                        Dispatcher.Invoke(() => UpdateProgressLabel("", "", Constants.UpdateProgress_REGULAR));

                        executionThread = null;
                        Singleton.Instance.StopExecution = false;


                    }
                }

                else
                {


                    if (result)
                        mbr =
                            MessageBox.Show(
                                Constants.SCENARIO_RUN_PASSED,
                                Constants.SCENARIO_RUN_TITLE,
                                MessageBoxButton
                                    .OKCancel,
                                MessageBoxImage
                                    .Asterisk,
                                MessageBoxResult.OK,
                                MessageBoxOptions
                                    .ServiceNotification);
                    else
                        mbr =
                            MessageBox.Show(
                                Constants.SCENARIO_RUN_FAILED,
                                Constants.SCENARIO_RUN_TITLE,
                                MessageBoxButton
                                    .OKCancel,
                                MessageBoxImage.Error, MessageBoxResult.No, MessageBoxOptions.DefaultDesktopOnly);

                    if (mbr == MessageBoxResult.OK)
                    {
                        // Update UI
                        Dispatcher.Invoke(() => HideStopBtnScenario());
                        Dispatcher.Invoke(() => HideStopBtn());
                        Dispatcher.Invoke(() => HideStopBtnBatch());
                        Dispatcher.Invoke(() => switchToAnalyzingTab());
                        Dispatcher.Invoke(() => UpdateProgressLabel("", "", Constants.UpdateProgress_EMPTY));
                        // Kill the thread
                        executionThread = null;
                        Singleton.Instance.StopExecution = false;

                    }


                    else if (mbr == MessageBoxResult.Cancel)
                    {
                        // Update UI
                        Dispatcher.Invoke(() => HideStopBtnScenario());
                        Dispatcher.Invoke(() => HideStopBtn());
                        Dispatcher.Invoke(() => HideStopBtnBatch());
                        Dispatcher.Invoke(() => UpdateProgressLabel("", "", Constants.UpdateProgress_REGULAR));

                        executionThread = null;
                        Singleton.Instance.StopExecution = false;


                    }
                }
            }
            catch (Exception exception)
            {

                //MessageBox.Show("RunThreaded", exception.Message);
            }


        }

        private void switchToAnalyzingTab()
        {
            TabItemAnalyzingZone.IsSelected = true;
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
            UpdateProgressLabel("", "", Constants.UpdateProgress_REGULAR);
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

                            LogObject exceptionLogger2 = new LogObject();
                            exceptionLogger2.StatusTag = Constants.DEBUG;
                            exceptionLogger2.Description = "Fill value in DataRow . Can not leave it empty:";
                            logger.Print(exceptionLogger2);

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
                MessageBox.Show(exception.Message, "Applenium");
                LogObject exceptionLogger2 = new LogObject();
                exceptionLogger2.StatusTag = Constants.ERROR;
                exceptionLogger2.Description = exception.Message;
                exceptionLogger2.Exception = exception;
                logger.Print(exceptionLogger2);
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
                LogObject exceptionLogger2 = new LogObject();
                exceptionLogger2.StatusTag = Constants.ERROR;
                exceptionLogger2.Description = exception.Message;
                exceptionLogger2.Exception = exception;
                logger.Print(exceptionLogger2);
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
                LogObject exceptionLogger2 = new LogObject();
                exceptionLogger2.StatusTag = Constants.ERROR;
                exceptionLogger2.Description = exception.Message;
                exceptionLogger2.Exception = exception;
                logger.Print(exceptionLogger2);
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
                LogObject exceptionLogger2 = new LogObject();
                exceptionLogger2.StatusTag = Constants.ERROR;
                exceptionLogger2.Description = exception.Message;
                exceptionLogger2.Exception = exception;
                logger.Print(exceptionLogger2);
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

                        LogObject debugger = new LogObject();
                        debugger.StatusTag = Constants.DEBUG;
                        debugger.Description = "Fill value in DataRow . Can not leave it empty:";
                        logger.Print(debugger);

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
                MessageBox.Show(exception.Message, "Applenium");
                LogObject exceptionLogger2 = new LogObject();
                exceptionLogger2.StatusTag = Constants.ERROR;
                exceptionLogger2.Description = exception.Message;
                exceptionLogger2.Exception = exception;
                logger.Print(exceptionLogger2);
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
                MessageBox.Show(exception.Message, "Applenium");
                LogObject exceptionLogger2 = new LogObject();
                exceptionLogger2.StatusTag = Constants.ERROR;
                exceptionLogger2.Description = exception.Message;
                exceptionLogger2.Exception = exception;
                logger.Print(exceptionLogger2);
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
                    int stepsOrder;

                    if (guiBatchLogicId == -1)
                    {
                        stepsOrder = Convert.ToInt32(adapterSl.LastScenarioOrder(batchId)) + 1;
                        adapterSl.Insert(batchId, guiScenarioId, browserId, executionStatusId, stepsOrder);
                    }
                    else
                    {
                        stepsOrder = Convert.ToInt32(rowView.Row["StepsOrder"].ToString());
                        adapterSl.Update(batchId, guiScenarioId, browserId, executionStatusId, stepsOrder, guiBatchLogicId);
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Applenium");
                LogObject exceptionLogger2 = new LogObject();
                exceptionLogger2.StatusTag = Constants.ERROR;
                exceptionLogger2.Description = exception.Message;
                exceptionLogger2.Exception = exception;
                logger.Print(exceptionLogger2);
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
            BtnMoveStepUpExecution.IsEnabled = true;
            BtnMoveStepDownExecution.IsEnabled = true;

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
                LogObject exceptionLogger2 = new LogObject();
                exceptionLogger2.StatusTag = Constants.ERROR;
                exceptionLogger2.Description = exception.Message;
                exceptionLogger2.Exception = exception;
                logger.Print(exceptionLogger2);
            }
       }






        private void dataGridBatchEditor_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Delete)
                {
                    var dataGrid = (DataGrid) sender;
                    var rowView = (DataRowView) dataGrid.SelectedItem;

                    using (var adapterBatch = new BatchLogicTableAdapter())
                    {
                        if (_editinggrid == false)
                        {
                            int guiBatchLogicId = Convert.ToInt32(rowView.Row["BatchLogicID"].ToString());
                            int guiBatchId = Convert.ToInt32(rowView.Row["BatchID"].ToString());

                            if (
                                MessageBox.Show("Delete Selected Scenario? ", "eToroAutoStudio", MessageBoxButton.YesNo) ==
                                MessageBoxResult.Yes)
                            {
                                int currentStepsOrder = Convert.ToInt32(rowView.Row["StepsOrder"].ToString());
                                int selectItem = currentStepsOrder - 1;
                               
                                while (adapterBatch.GetBatchLogicID(guiBatchId, currentStepsOrder + 1) != null)
                                {
                                    int nextStepsOrder = currentStepsOrder + 1;
                                    int nextGuiTestStepsId =
                                        Convert.ToInt32(adapterBatch.GetBatchLogicID(guiBatchId, nextStepsOrder));
                                    adapterBatch.UpdateScenariosOrder(currentStepsOrder, nextGuiTestStepsId);
                                    currentStepsOrder = nextStepsOrder;
                                }

                                adapterBatch.Delete(guiBatchLogicId);
                                DataGridFill(Constants.StrGuiScenarioLogic,
                                    guiBatchId.ToString(CultureInfo.InvariantCulture));
                                dataGrid.SelectedItem = dataGrid.Items[selectItem];
                            }
                        }
                    }
                    if (e.Key == Key.Enter)
                    {
                        e.Handled = true;
                        var eInsertBack = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource,
                            0,
                            Key.Tab);
                        eInsertBack.RoutedEvent = KeyDownEvent;
                        InputManager.Current.ProcessInput(eInsertBack);
                    }
                }
            }
            catch (Exception exception)
            {
                LogObject exceptionLogger2 = new LogObject();
                exceptionLogger2.StatusTag = Constants.ERROR;
                exceptionLogger2.Description = exception.Message;
                exceptionLogger2.Exception = exception;
                logger.Print(exceptionLogger2);
               
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
                MessageBox.Show(exception.Message, "Applenium");
                LogObject exceptionLogger2 = new LogObject();
                exceptionLogger2.StatusTag = Constants.ERROR;
                exceptionLogger2.Description = exception.Message;
                exceptionLogger2.Exception = exception;
                logger.Print(exceptionLogger2);
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
                ClosingBrowserThread();
            }
            catch (Exception exception)
            {
                LogObject exceptionLogger2 = new LogObject();
                exceptionLogger2.StatusTag = Constants.ERROR;
                exceptionLogger2.Description = exception.Message;
                exceptionLogger2.Exception = exception;
                logger.Print(exceptionLogger2);
            }


            LogObject closeMessage = new LogObject();
            closeMessage.StatusTag = Constants.INFO;
            closeMessage.Description =
                "-------------------------------------------------------------------------------------------------------------\n                                             Applenium Closed with driver  \n-------------------------------------------------------------------------------------------------------------";
            logger.Print(closeMessage);

        }

        private void btnAddNewObject_Click(object sender, RoutedEventArgs e)
        {
            if (_pageSectionid != 0)
            {

                DataGridFill(Constants.StrGuiMapEdit, "0");
                DataGridGuiMapEditorHidenTable.IsReadOnly = false;
            }
            else
                MessageBox.Show("Select Project Page first", "Applenium");
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
                LogObject exceptionLogger2 = new LogObject();
                exceptionLogger2.StatusTag = Constants.ERROR;
                exceptionLogger2.Description = exception.Message;
                exceptionLogger2.Exception = exception;
                logger.Print(exceptionLogger2);
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
                LogObject exceptionLogger2 = new LogObject();
                exceptionLogger2.StatusTag = Constants.ERROR;
                exceptionLogger2.Description = exception.Message;
                exceptionLogger2.Exception = exception;
                logger.Print(exceptionLogger2);
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
                LogObject exceptionLogger2 = new LogObject();
                exceptionLogger2.StatusTag = Constants.ERROR;
                exceptionLogger2.Description = exception.Message;
                exceptionLogger2.Exception = exception;
                logger.Print(exceptionLogger2);
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
                LogObject exceptionLogger2 = new LogObject();
                exceptionLogger2.StatusTag = Constants.ERROR;
                exceptionLogger2.Description = exception.Message;
                exceptionLogger2.Exception = exception;
                logger.Print(exceptionLogger2);
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
                MessageBox.Show("Select Project Page first", "Applenium");
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
                MessageBox.Show("Select  Page Section first", "Applenium");
        }

        private void btnRunBatch_Click(object sender, RoutedEventArgs e)
        {
            ShowStopBtnBatch();
            try
            {
                bool result;
                //var sl = new Selenium(driver);
                var adapterTestresult = new TestResultsTableAdapter();
                int runExecutionId = Utilities.GetTimeStamp();

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
                                                                                      MessageBoxImage.Error,
                                                                                      MessageBoxResult.OK,
                                                                                      MessageBoxOptions
                                                                                          .DefaultDesktopOnly);

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
                LogObject exceptionLogger2 = new LogObject();
                exceptionLogger2.StatusTag = Constants.ERROR;
                exceptionLogger2.Description = exception.Message;
                exceptionLogger2.Exception = exception;
                logger.Print(exceptionLogger2);
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
            ClosingBrowserThread();
        }

        private void WindoweToroAutoStudio_Unloaded(object sender, RoutedEventArgs e)
        {
            ClosingBrowserThread();
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
                LogObject exceptionLogger2 = new LogObject();
                exceptionLogger2.StatusTag = Constants.ERROR;
                exceptionLogger2.Description = exception.Message;
                exceptionLogger2.Exception = exception;
                logger.Print(exceptionLogger2);
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

            if (ConfigurationManager.AppSettings["ShowAppleniumTabs"] != String.Empty)
            {
                string showtabs = ConfigurationManager.AppSettings["ShowAppleniumTabs"].ToString();
                if (showtabs.Contains("Configuration"))
                {
                    if (_msgDisplayed == false)
                    {
                        MessageBox.Show(_upmessage
                                        , "Applenium");
                        _msgDisplayed = true;
                    }
                }


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
                //write to dictionary too
                jp.AddKeyToMemory(name, value);
                if (rowView["Name"].ToString() == "DefaultBrowser")

                    NewBrowserThread(value);
            }


        }

        private void WebBrowser_ReportDashboard_Loaded(object sender, RoutedEventArgs e)
        {
            var jp = new JsonParser();
            var oUri = new Uri(Constants.MemoryConf["DashboardURL"]);

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


        private void btnMoveStepUpExec_Click(object sender, RoutedEventArgs e)
        {
            MoveScenariosUpExec(DataGridBatchEditor, Constants.StrGuiBatchLogic);
        }

        private void btnMoveStepDownExec_Click(object sender, RoutedEventArgs e)
        {
            MoveScenariosDownExec(DataGridBatchEditor, Constants.StrGuiBatchLogic);
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
                    int currentguiTestStepsId =
                        Convert.ToInt32(scenarioLogicTableAdapter.GetStepId(scenarioOrTestId, currentStepsOrder));
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
                        currentguiTestStepsId =
                            Convert.ToInt32(scenarioLogicTableAdapter.GetStepId(scenarioOrTestId, currentStepsOrder));
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
                MessageBox.Show(exception.Message, "Applenium");
                LogObject exceptionLogger2 = new LogObject();
                exceptionLogger2.StatusTag = Constants.ERROR;
                exceptionLogger2.Description = exception.Message;
                exceptionLogger2.Exception = exception;
                logger.Print(exceptionLogger2);
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
                            nextGuiTestStepsId =
                                Convert.ToInt32(scenarioLogicTableAdapter.GetStepId(scenarioOrTestId, nextStepsOrder));
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
                MessageBox.Show(exception.Message, "Applenium");
                LogObject exceptionLogger2 = new LogObject();
                exceptionLogger2.StatusTag = Constants.ERROR;
                exceptionLogger2.Description = exception.Message;
                exceptionLogger2.Exception = exception;
                logger.Print(exceptionLogger2);
            }
        }

        private void MoveScenariosUpExec(DataGrid datagrid, string dataGridEditor)
        {
            int selectedItem = 0;
            var batchLogicTableAdapter = new BatchLogicTableAdapter();


            try
            {
                var rowView = (DataRowView)datagrid.SelectedItem;
                int batchId;

                batchId = Convert.ToInt32(rowView.Row["BatchID"].ToString());


                if (rowView != null)
                {
                    int currentScenarioOrder = Convert.ToInt32(rowView.Row["StepsOrder"].ToString());
                    int currentBatchLogicId = Convert.ToInt32(batchLogicTableAdapter.GetBatchLogicID(batchId, currentScenarioOrder));
                    int previousScenarioOrder = currentScenarioOrder - 1;
                    if (batchLogicTableAdapter.GetBatchLogicID(batchId, previousScenarioOrder) != null)
                    {
                        int previousScenarioId =
                            Convert.ToInt32(batchLogicTableAdapter.GetBatchLogicID(batchId, previousScenarioOrder));
                        batchLogicTableAdapter.UpdateScenariosOrder(previousScenarioOrder, currentBatchLogicId);
                        batchLogicTableAdapter.UpdateScenariosOrder(currentScenarioOrder, previousScenarioId);
                        selectedItem = previousScenarioOrder - 1;
                    }
                    if (currentScenarioOrder == 1)
                    {
                        currentBatchLogicId = Convert.ToInt32(batchLogicTableAdapter.GetBatchLogicID(batchId, currentScenarioOrder));
                        while (batchLogicTableAdapter.GetBatchLogicID(batchId, currentBatchLogicId + 1) != null)
                        {
                            int nextStepsOrder = currentScenarioOrder + 1;
                            int nextGuiTestStepsId =
                                Convert.ToInt32(batchLogicTableAdapter.GetBatchLogicID(batchId, nextStepsOrder));
                            batchLogicTableAdapter.UpdateScenariosOrder(currentScenarioOrder, nextGuiTestStepsId);
                            currentScenarioOrder = nextStepsOrder;
                        }
                        batchLogicTableAdapter.UpdateScenariosOrder(datagrid.Items.Count - 1, currentBatchLogicId);
                        selectedItem = datagrid.Items.Count - 2;
                    }
                }
                DataGridFill(dataGridEditor, batchId.ToString(CultureInfo.InvariantCulture));
                datagrid.SelectedItem = datagrid.Items[selectedItem];
            }

            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Applenium");
                LogObject exceptionLogger2 = new LogObject();
                exceptionLogger2.StatusTag = Constants.ERROR;
                exceptionLogger2.Description = exception.Message;
                exceptionLogger2.Exception = exception;
                logger.Print(exceptionLogger2);
            }
        }

        private void MoveScenariosDownExec(DataGrid datagrid, string dataGridEditor)
        {
            int selectedItem = 0;
            var batchLogicTableAdapter = new BatchLogicTableAdapter();


            try
            {
                var rowView = (DataRowView)datagrid.SelectedItem;
                int batchId;

                batchId = Convert.ToInt32(rowView.Row["BatchID"].ToString());


                if (rowView != null)
                {
                    int currentScenarioOrder = Convert.ToInt32(rowView.Row["StepsOrder"].ToString());
                    int currentBatchLogicId = Convert.ToInt32(batchLogicTableAdapter.GetBatchLogicID(batchId, currentScenarioOrder));
                    int nextScenarioOrder = currentScenarioOrder + 1;
                   
                    if (batchLogicTableAdapter.GetBatchLogicID(batchId, nextScenarioOrder) != null)
                    {
                        int nextScenarioId =
                            Convert.ToInt32(batchLogicTableAdapter.GetBatchLogicID(batchId, nextScenarioOrder));
                        batchLogicTableAdapter.UpdateScenariosOrder(nextScenarioOrder, currentBatchLogicId);
                        batchLogicTableAdapter.UpdateScenariosOrder(currentScenarioOrder, nextScenarioId);
                        selectedItem = nextScenarioOrder - 1;
                    }
                    if (currentScenarioOrder == datagrid.Items.Count - 1)
                    {
                        currentBatchLogicId = Convert.ToInt32(batchLogicTableAdapter.GetBatchLogicID(batchId, currentScenarioOrder));
                        while (batchLogicTableAdapter.GetBatchLogicID(batchId, currentScenarioOrder - 1) != null)
                        {

                            int nextStepsOrder = currentScenarioOrder - 1;
                            int nextGuiTestStepsId = Convert.ToInt32(batchLogicTableAdapter.GetBatchLogicID(batchId, nextStepsOrder));
                            batchLogicTableAdapter.UpdateScenariosOrder(currentScenarioOrder, nextGuiTestStepsId);
                            currentScenarioOrder = nextStepsOrder;
                        }
                        batchLogicTableAdapter.UpdateScenariosOrder(1, currentBatchLogicId);
                        selectedItem = 0;
                    }

                }
                DataGridFill(dataGridEditor, batchId.ToString(CultureInfo.InvariantCulture));
                datagrid.SelectedItem = datagrid.Items[selectedItem];
            }

            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Applenium");
                LogObject exceptionLogger2 = new LogObject();
                exceptionLogger2.StatusTag = Constants.ERROR;
                exceptionLogger2.Description = exception.Message;
                exceptionLogger2.Exception = exception;
                logger.Print(exceptionLogger2);
            }
        }


        private void MoveStepsUpTest(DataGrid datagrid, string dataGridEditor)
        {
            var adapterEnvVer = new EnvironmentVersionTableAdapter();
            var jsonParser = new JsonParser();

            string verid = adapterEnvVer.GetEnvironmnetVersionIDByVersionName(Constants.MemoryConf["TestingEnvironmentVersion"])
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

                    int currentguiTestStepsId =
                        Convert.ToInt32(sql.CopyStepAndChangeVersion(currentStepsOrder, scenarioOrTestId, versionColumn));

                    int previesStepsOrder = currentStepsOrder - 1;
                    if (
                        sql.GetStepsIdByVersion(scenarioOrTestId.ToString(), previesStepsOrder.ToString(), versionColumn) !=
                        string.Empty)
                    //if (testStepsTableAdapter.GetStepId(scenarioOrTestId, previesStepsOrder) != null)
                    {
                        //int previesGuiTestStepsId =Convert.ToInt32(testStepsTableAdapter.GetStepId(scenarioOrTestId, previesStepsOrder));
                        // int previesGuiTestStepsId =Convert.ToInt32(sql.GetStepsIdByVersion(scenarioOrTestId.ToString(),previesStepsOrder.ToString(), versionColumn));

                        //before update copy 

                        int previesGuiTestStepsId =
                            Convert.ToInt32(sql.CopyStepAndChangeVersion(previesStepsOrder, scenarioOrTestId,
                                                                         versionColumn));
                        testStepsTableAdapter.UpdateStepsOrder(previesStepsOrder, currentguiTestStepsId);
                        testStepsTableAdapter.UpdateStepsOrder(currentStepsOrder, previesGuiTestStepsId);
                        selectedItem = previesStepsOrder - 1;
                    }
                    if (currentStepsOrder == 1)
                    {
                        //currentguiTestStepsId =Convert.ToInt32(testStepsTableAdapter.GetStepId(scenarioOrTestId, currentStepsOrder));
                        if (
                            sql.GetStepsIdByVersion(currentStepsOrder.ToString(), scenarioOrTestId.ToString(),
                                                    versionColumn) != string.Empty)
                        {
                            //currentguiTestStepsId = Convert.ToInt32(sql.GetStepsIdByVersion(currentStepsOrder.ToString(),scenarioOrTestId.ToString(),versionColumn));                            
                            currentguiTestStepsId =
                                Convert.ToInt32(sql.CopyStepAndChangeVersion(currentStepsOrder, scenarioOrTestId,
                                                                             versionColumn));
                        }

                        while (
                            sql.GetStepsIdByVersion(scenarioOrTestId.ToString(), (currentStepsOrder + 1).ToString(),
                                                    versionColumn) != string.Empty)
                        //(testStepsTableAdapter.GetStepId(scenarioOrTestId, currentStepsOrder + 1) != null)
                        {
                            int nextStepsOrder = currentStepsOrder + 1;
                            //int nextGuiTestStepsId =Convert.ToInt32(testStepsTableAdapter.GetStepId(scenarioOrTestId, nextStepsOrder));
                            //int nextGuiTestStepsId =Convert.ToInt32(sql.GetStepsIdByVersion(scenarioOrTestId.ToString(),nextStepsOrder.ToString(), versionColumn));
                            //before update copy step
                            int nextGuiTestStepsId =
                                Convert.ToInt32(sql.CopyStepAndChangeVersion(nextStepsOrder, scenarioOrTestId,
                                                                             versionColumn));
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
                LogObject exceptionLogger2 = new LogObject();
                exceptionLogger2.StatusTag = Constants.ERROR;
                exceptionLogger2.Description = exception.Message;
                exceptionLogger2.Exception = exception;
                logger.Print(exceptionLogger2);
            }
        }

        private void MoveStepDownTest(DataGrid datagrid, string dataGridEditor)
        {
            var adapterEnvVer = new EnvironmentVersionTableAdapter();
            var jsonParser = new JsonParser();

            string verid = adapterEnvVer.GetEnvironmnetVersionIDByVersionName(Constants.MemoryConf["TestingEnvironmentVersion"])
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

                        int currentguiTestStepsId =
                            Convert.ToInt32(sql.CopyStepAndChangeVersion(currentStepsOrder, scenarioOrTestId,
                                                                         versionColumn));


                        int nextStepsOrder = currentStepsOrder + 1;
                        int nextGuiTestStepsId;
                        if (
                            sql.GetStepsIdByVersion(scenarioOrTestId.ToString(), nextStepsOrder.ToString(),
                                                    versionColumn) != string.Empty)
                        // (testStepsTableAdapter.GetStepId(scenarioOrTestId, nextStepsOrder) != null)
                        {
                            //nextGuiTestStepsId =Convert.ToInt32(testStepsTableAdapter.GetStepId(scenarioOrTestId, nextStepsOrder));

                            //before update copy 

                            nextGuiTestStepsId =
                                Convert.ToInt32(sql.CopyStepAndChangeVersion(nextStepsOrder, scenarioOrTestId,
                                                                             versionColumn));

                            testStepsTableAdapter.UpdateStepsOrder(nextStepsOrder, currentguiTestStepsId);
                            testStepsTableAdapter.UpdateStepsOrder(currentStepsOrder, nextGuiTestStepsId);
                            selectedItem = nextStepsOrder - 1;
                        }
                        if (currentStepsOrder == datagrid.Items.Count - 1)
                        {
                            //currentguiTestStepsId =Convert.ToInt32(testStepsTableAdapter.GetStepId(scenarioOrTestId, currentStepsOrder));
                            currentguiTestStepsId =
                                Convert.ToInt32(sql.CopyStepAndChangeVersion(currentStepsOrder, scenarioOrTestId,
                                                                             versionColumn));

                            while (
                                sql.GetStepsIdByVersion(scenarioOrTestId.ToString(), (currentStepsOrder - 1).ToString(),
                                                        versionColumn) != string.Empty)
                            //(testStepsTableAdapter.GetStepId(scenarioOrTestId, currentStepsOrder - 1) != null)
                            {
                                nextStepsOrder = currentStepsOrder - 1;
                                //nextGuiTestStepsId =Convert.ToInt32(testStepsTableAdapter.GetStepId(scenarioOrTestId, nextStepsOrder));
                                //before update copy step
                                nextGuiTestStepsId =
                                    Convert.ToInt32(sql.CopyStepAndChangeVersion(nextStepsOrder, scenarioOrTestId,
                                                                                 versionColumn));
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
                MessageBox.Show(exception.Message, "Applenium");
                LogObject exceptionLogger2 = new LogObject();
                exceptionLogger2.StatusTag = Constants.ERROR;
                exceptionLogger2.Description = exception.Message;
                exceptionLogger2.Exception = exception;
                logger.Print(exceptionLogger2);
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
            var sql = new Sql();
            var adapterTest = new TestTableAdapter();
            var adapterTestSteps = new GuiTestStepsTableAdapter();

            string testname = adapterTest.GetTestName(_testid);

            string newtestname = Interaction.InputBox("Test " + testname + " Coppied to...",
                                                      "Enter new name for your test ", "Copy Of_" + testname);

            if (testname != string.Empty)
            {
                adapterTest.CopyTest(newtestname.Trim(), Constants.UncompletedTestImage, _testid);
                int? newtestid = (int)adapterTest.GetLastTestId();
                //adapterTestSteps.CopyTestSteps(newtestid.ToString(), _testid);//!!!!!!!!!!

                sql.CopyStepsByVersion(_testid.ToString(), newtestid.ToString(), sql.GetVersionColumn());
                //update version for new steps

                adapterTest.UpdateStatusCompleted(Constants.UncompletedTestImage, _testid);
                DataGridFill(Constants.StrGuiTest, null);
            }
        }

        private void CopyScenario(object sender, RoutedEventArgs e)
        {
            var adapterScenario = new ScenarioTableAdapter();
            var adapterScenarioLogic = new GuiScenarioLogicTableAdapter();

            string scenarioName = adapterScenario.GetScenarioName(_scenarioid);

            string newScenarioName = Interaction.InputBox("Scenario " + scenarioName + " Copied to...",
                                                          "Enter new name for your scenario ", "Copy Of_" + scenarioName);
            if (newScenarioName != string.Empty)
            {

                adapterScenario.CopyScenario(newScenarioName.Trim(), scenarioName);

                int? newScenarioId = adapterScenario.GetLastScnerioID();

                adapterScenarioLogic.CopyScenarioLogic(newScenarioId.ToString(), _scenarioid);
                DataGridFill(Constants.StrGuiScenario, null);
            }
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
        private int counter = 0;

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
                    //  Console.WriteLine(handle.ToString());
                }

                int index = counter % list.Count;

                if (index < list.Count)
                {
                    _driver.SwitchTo().Window(list[index]);
                    counter++;
                }


                currentTitle = _driver.Title;

                string versionName = GetAssemblyVersion();
                Applenium.Title = versionName + " | Current window - " + currentTitle;
            }
            catch (Exception exception)
            {

                LogObject exceptionLogger2 = new LogObject();
                exceptionLogger2.StatusTag = Constants.ERROR;
                exceptionLogger2.Description = exception.Message;
                exceptionLogger2.Exception = exception;
                logger.Print(exceptionLogger2);
            }

        }

        /// <summary>
        /// Update progress bar with current test\scenario
        /// </summary>
        /// <param name="messageSCN">scenario name</param>
        /// <param name="messageTST">test name</param>
        /// <param name="type">type of message</param>
        public void UpdateProgressLabel(String messageSCN, string messageTST, int type)
        {
            string messageToPrint = String.Empty;
            switch (type)
            {
                case 0:
                    messageToPrint = "Running - " + messageSCN + " >>> " + messageTST;
                    break;
                case 1:
                    messageToPrint = Constants.STOPPED_EXPLICIT;
                    break;
                case 2:
                    messageToPrint = string.Empty;
                    break;
            }

            this.Dispatcher.BeginInvoke(
                (Action)delegate()
                {
                    // If both messages are not empty print it. Else, print empty string;
                    if (messageToPrint != string.Empty)
                    {
                        LblCurrentlyRunning.Content = messageToPrint;
                    }
                    else LblCurrentlyRunning.Content = string.Empty;
                });

        }

        private void ComboboxTestingEnvironment_Loaded(object sender, RoutedEventArgs e)
        {

            DataGridFill(Constants.StrEnvironmentVersion, null);
        }


        private void ComboboxTestingEnvironment_TextChanged(object sender, KeyEventArgs e)
        {
            ComboBox comboBoxTestingEnvironment = (ComboBox)sender;
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

        private void ComboboxGuiMap_LostFocus(object sender, RoutedEventArgs e)
        {

            ComboBox comboBoxGuiMap = (ComboBox)sender;
            _guiMapValue = comboBoxGuiMap.Text;
        }



        private void btnOK_Click(object sender, RoutedEventArgs e)
        {

            using (var adapterEnvVer = new EnvironmentVersionTableAdapter())
            {
                //get next empty column version 

                string versionName = ComboboxTestingEnvironment.Text;
                int? envVerId = adapterEnvVer.GetNextColumnVersion();
                adapterEnvVer.Update(versionName, Convert.ToInt32(envVerId));

                //add ver sparce column to GuiSteps
                var sql = new Sql();
                string toColumn = adapterEnvVer.GetColumnByID(Convert.ToInt32(envVerId)).ToString();
                string oldenvVerId =
                    adapterEnvVer.GetEnvironmnetVersionIDByVersionName(ComboboxTestingEnvironmentFromClone.Text)
                                 .ToString();
                string fromColumn = adapterEnvVer.GetColumnByID(Convert.ToInt32(oldenvVerId)).ToString();
                sql.CopyColumn("GuiTestSteps", fromColumn, toColumn);
                sql.CopyColumn("GuiMap", fromColumn, toColumn);
                // add message that created new environment 
                MessageBox.Show("New Version" + versionName + "Environment was created successfully", "Applenium");


                PopupCloneFromTestingEnviromnet.IsOpen = false;
                DataGridFill(Constants.StrEnvironmentVersion, null);

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


        /// <summary>
        /// Opens a new browser in a new thread
        /// </summary>
        /// <param name="browser">type of browser</param>
        private void NewBrowserThread(string browser)
        {
            Thread t = new Thread(delegate()
            {
                // driver is not ready yet so lock execution (run scenario/test) buttons
                Dispatcher.Invoke(() => ShowStopBtnScenario());
                Dispatcher.Invoke(() => ShowStopBtn());

                // preper the driver
                nullDriver = true;
                Selenium selenium = new Selenium();
                _driver = selenium.SetWebDriverBrowser(_driver, browser, true);
                nullDriver = false;

                // driver is ready, unlock buttons 
                Dispatcher.Invoke(() => HideStopBtnScenario());
                Dispatcher.Invoke(() => HideStopBtn());


            });
            t.Start();

        }

        /// <summary>
        /// Try to quit the driver in a new thread
        /// </summary>
        private void ClosingBrowserThread()
        {
            new Thread(delegate()
            {
                try
                {
                    _driver.Quit();
                    nullDriver = true;
                }
                catch (NullReferenceException ex)
                {
                    LogObject exceptionLogger2 = new LogObject();
                    exceptionLogger2.StatusTag = Constants.ERROR;
                    exceptionLogger2.Description = "Failed to close driver in new thread";
                    exceptionLogger2.Exception = ex;
                    logger.Print(exceptionLogger2);


                }
            }).Start();

        }


        //
        //VSH:start  VAX tools (Tools window)
        //Creation Date: 5.2.2014
        //Load all Applenium project from DB to ComboBox
        private void ComboBox_MOP_Loaded(object sender, RoutedEventArgs e)
        {

            var dataset = new DataSetAutoTest();
            var adapterProject = new ProjectsTableAdapter();
            adapterProject.Fill(dataset.Projects);
            ComboBox_MOP.ItemsSource = dataset.Projects.DefaultView;

        }

        //Enable element mapping (add data to Applenium DB)
        private void MOP_CheckBtn_Checked(object sender, RoutedEventArgs e)
        {
            ComboBox_MOP.IsEnabled = true;

        }

        //Disable element mapping (add data to Applenium DB)
        private void MOP_CheckBtn_Unchecked(object sender, RoutedEventArgs e)
        {
            ComboBox_MOP.IsEnabled = false;
        }

        //Choose Directory or File of Mobile Project
        private void MOP_OpenBtn_Click(object sender, RoutedEventArgs e)
        {
            MOP_RunBtn.IsEnabled = false;

            //Directory case
            if (Convert.ToBoolean(MOP_DirRbtn.IsChecked))
            {
                System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1 =
                    new System.Windows.Forms.FolderBrowserDialog();
                //FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
                if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    MOP_FileTbox.Text = folderBrowserDialog1.SelectedPath;
                    MOP_RunBtn.IsEnabled = true;
                }
            } // File case
            else if (Convert.ToBoolean(MOP_FileRbtn.IsChecked))
            {
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

                dlg.DefaultExt = ".xml"; // Set filter for file extension and default file extension
                dlg.Filter = "Text documents (.xml)|*.xml"; // Filter files by extension 

                // Display OpenFileDialog by calling ShowDialog method
                Nullable<bool> result = dlg.ShowDialog();

                // Get the selected file name and display in a TextBox
                if (result == true)
                {
                    // Open document
                    string filename = dlg.FileName;
                    MOP_FileTbox.Text = filename;
                    MOP_RunBtn.IsEnabled = true;
                }

            }
        }

        //Run mapping process
        private void MOP_RunBtn_Run(object sender, RoutedEventArgs e)
        {
            var str_time = DateTime.Now.ToString();
            string s =
                "------------------------------------------------------------------------------------------------------------\r\n";
            var MOPname = "";
            var MOPid = "";
            var MOPprefix = "";
            var MOPpageName = "";
            var MOPpageID = "";
            string testingEnvironmentVersionColumn = "Ver1";


            //Get Backup option
            Boolean BackupEnabled = Convert.ToBoolean(MOP_BackupChkBtn.IsChecked);

            //Get element mapping options
            Boolean MobileProjectEnabled = Convert.ToBoolean(MOP_CheckBtn.IsChecked);

            //Disable run button
            MOP_RunBtn.IsEnabled = false;

            IndexerTextArea.Text = s + "Start time: " + str_time + "\r\n" + s;


            /*
             * If mapping enabled - get project data
             */
            if (MobileProjectEnabled)
            {
                if (ComboBox_MOP.SelectionBoxItem.Equals(""))
                {
                    System.Windows.Forms.MessageBox.Show(
                        "Please choose Mobile Project or uncheck Projects Check Button ", "Warning",
                        System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                    MOP_RunBtn.IsEnabled = true;
                    return;
                }

                string environmentVersionId = null;


                var MOPobject = (DataRowView)ComboBox_MOP.SelectedValue;
                var dataset = new DataSetAutoTest();


                // Get Applenium Testing Version
                var jp = new JsonParser();
                string testingEnvironmentVersion = Constants.MemoryConf["TestingEnvironmentVersion"];
                var adapterEnvVer = new EnvironmentVersionTableAdapter();
                adapterEnvVer.Fill(dataset.EnvironmentVersion);

                environmentVersionId =
                    adapterEnvVer.GetEnvironmnetVersionIDByVersionName(testingEnvironmentVersion).ToString();
                if (String.IsNullOrEmpty(environmentVersionId))
                {
                    testingEnvironmentVersionColumn =
                        adapterEnvVer.GetColumnByID(Convert.ToInt32(environmentVersionId)).ToString();
                }
                // End version ID

                var adapterProjects = new ProjectsTableAdapter();
                var adapterProjectPage = new GuiProjectPageTableAdapter();

                MOPid = Convert.ToString(MOPobject.Row[0].ToString());
                MOPname = Convert.ToString(MOPobject.Row[1].ToString());
                MOPprefix = adapterProjects.GetProjectPrefix(Convert.ToInt32(MOPid));


                adapterProjects.Fill(dataset.Projects);
                adapterProjectPage.Fill(dataset.GuiProjectPage);

                DataTableReader dtReader = dataset.GuiProjectPage.CreateDataReader();
                while (dtReader.Read())
                {
                    var pr_num = dtReader.GetValue(1).ToString().Trim();
                    if (pr_num == MOPid)
                    {
                        MOPpageName = dtReader.GetValue(2).ToString().Trim();
                        MOPpageID = dtReader.GetValue(0).ToString().Trim();
                    }

                }
                dtReader.Close();

                IndexerTextArea.Text += "Mobile Project Name: " + MOPname + "\r\n";
                IndexerTextArea.Text += "Mobile Project ID: " + MOPid + "\r\n";
                IndexerTextArea.Text += "Mobile Project Prefix: " + MOPprefix + "\r\n";
                IndexerTextArea.Text += "Mobile Project Page name: " + MOPpageName + "\r\n";
                IndexerTextArea.Text += "Mobile Project Page ID: " + MOPpageID + "\r\n";
                IndexerTextArea.Text += "Version Name: " + testingEnvironmentVersion + "==" +
                                        testingEnvironmentVersionColumn + "\r\n";

            }


            if (Convert.ToBoolean(MOP_DirRbtn.IsChecked))
            {
                string dir_name = MOP_FileTbox.Text;

                IndexerTextArea.Text += "Directory Path: " + dir_name + "\r\n" + s + "\r\n";

                // Directory validation
                if (String.IsNullOrEmpty(dir_name))
                {
                    System.Windows.Forms.MessageBox.Show("Please insert Working Directory or File", "Warning",
                                                         System.Windows.Forms.MessageBoxButtons.OK,
                                                         System.Windows.Forms.MessageBoxIcon.Warning);
                    MOP_RunBtn.IsEnabled = true;
                    return;
                }

                if (!(System.IO.Directory.Exists(MOP_FileTbox.Text)))
                {
                    System.Windows.Forms.MessageBox.Show("Please fill Working Directory or File", "Warning",
                                                         System.Windows.Forms.MessageBoxButtons.OK,
                                                         System.Windows.Forms.MessageBoxIcon.Warning);
                    MOP_RunBtn.IsEnabled = true;
                    return;
                }

                //Get all dir's files
                string[] files = Directory.GetFiles(dir_name, "*.xml", SearchOption.AllDirectories);

                if (files.Count() < 1)
                {
                    System.Windows.Forms.MessageBox.Show("This is empty directory", "Warning",
                                                         System.Windows.Forms.MessageBoxButtons.OK,
                                                         System.Windows.Forms.MessageBoxIcon.Warning);
                    MOP_RunBtn.IsEnabled = true;
                    return;
                }

                //Start proccess for each file
                foreach (var file in files)
                {
                    if (String.IsNullOrEmpty(file))
                    {
                        continue;
                    }

                    if (!System.IO.File.Exists(file))
                    {
                        System.Windows.Forms.MessageBox.Show("File: " + file + "is not exists", "FILE ERROR\r\n",
                                                             System.Windows.Forms.MessageBoxButtons.OK,
                                                             System.Windows.Forms.MessageBoxIcon.Error);
                        MOP_RunBtn.IsEnabled = true;
                        return;
                    }

                    _XMLfileValidator fe = new _XMLfileValidator(file, MobileProjectEnabled, MOPid, MOPprefix, MOPpageID,
                                                                 testingEnvironmentVersionColumn, dir_name);

                    List<string> out_stat = fe.xml_element_id_validator(BackupEnabled);

                    foreach (var line in out_stat)
                    {
                        IndexerTextArea.Text += line;
                    }
                }

            }
            else
            {
                // File case: Get and Validate file
                string file = MOP_FileTbox.Text;

                FileInfo fi = new FileInfo(file);


                string dir_name = fi.DirectoryName;

                if (file == String.Empty)
                {
                    MOP_RunBtn.IsEnabled = true;
                    return;
                }

                if (!System.IO.File.Exists(file))
                {
                    System.Windows.Forms.MessageBox.Show("File: " + file + "is not exists", "FILE ERROR\r\n",
                                                         System.Windows.Forms.MessageBoxButtons.OK,
                                                         System.Windows.Forms.MessageBoxIcon.Error);
                    MOP_RunBtn.IsEnabled = true;
                    return;
                }

                IndexerTextArea.Text += "The " + file + " in works...\r\n";

                //Start proccess for the file
                _XMLfileValidator fe = new _XMLfileValidator(file, MobileProjectEnabled, MOPid, MOPprefix, MOPpageID,
                                                             testingEnvironmentVersionColumn, dir_name);

                List<string> out_stat = fe.xml_element_id_validator(BackupEnabled);

                foreach (var line in out_stat)
                {
                    IndexerTextArea.Text += line;
                }

            }
            MOP_RunBtn.IsEnabled = true;


        }

        //VSH:end  VAX tools (Tools window)

        //VSH:start Configuration save/open option
        //Creation Date: 5.2.2014
        private void SaveConfButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {


                //
                string initDir = Constants.MemoryConf["AppleniumConfDir"];
                // check if that folder exists 
                bool isExists = System.IO.Directory.Exists(initDir);

                if (!isExists)
                    System.IO.Directory.CreateDirectory(initDir);
                // Configure save file dialog box
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.FileName = "AppleniumConfiguration"; // Default file name
                dlg.DefaultExt = ".text"; // Default file extension
                dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                dlg.InitialDirectory = Path.GetFullPath(initDir);
                dlg.Filter = "Text documents (.txt)|*.txt"; // Filter files by extension

                // Show save file dialog box
                Nullable<bool> result = dlg.ShowDialog();

                // Process save file dialog box results
                if (result == true)
                {
                    // Save document
                    string filename = dlg.FileName;
                    File.Copy(@ConfigurationManager.AppSettings["ConfigurationJsonFile"], filename, true);
                }
                else
                {
                    return;
                }

            }
            catch (Exception exception)
            {

                MessageBox.Show(exception.Message, "Applenium");
            }
        }

        private void OpenConfButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {


                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                string initDir = Constants.MemoryConf["AppleniumConfDir"];
                bool isExists = System.IO.Directory.Exists(initDir);

                if (!isExists)
                    System.IO.Directory.CreateDirectory(initDir);
                dlg.DefaultExt = ".txt"; // Set filter for file extension and default file extension
                dlg.Filter = "Text documents (.txt)|*.txt"; // Filter files by extension 
                dlg.InitialDirectory = Path.GetFullPath(initDir);
                //dlg.InitialDirectory = Path.GetFullPath(@"Y:\MobileClients\Config");
                //dlg.InitialDirectory = Environment.GetFolderPath();
                // Display OpenFileDialog by calling ShowDialog method
                Nullable<bool> result = dlg.ShowDialog();

                // Get the selected file name and display in a TextBox
                if (result == true)
                {
                    // Open document
                    string filename = dlg.FileName;
                    File.Copy(filename, @ConfigurationManager.AppSettings["ConfigurationJsonFile"], true);
                    DataGrid_Configuration_Loaded(null, null);
                    var jp = new JsonParser();
                    Boolean res = jp.AddConfigToMemory("");
                }
            }
           
            catch (Exception exception)
            {

                MessageBox.Show(exception.Message, "Applenium");
            }


        }

        private void TreeViewLogs_Loaded(object sender, RoutedEventArgs e)
        {
            DataGridFill(Constants.StrAnalayzing, null);
        }

        private void TreeViewLogs_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SetProjectPageScetionId(sender);

            DataGridFill(Constants.StrLogResults, _runexecutionid.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Checks to see if current executing thread is still running
        /// </summary>
        /// <returns>answer boolean</returns>
        private bool executionThreaIsRunning()
        {
            bool answer = false;
            if (executionThread == null) answer = false;
            else if (executionThread != null) answer = true;
            return answer;
        }

        /// <summary>
        /// Opens a dialog box asking user what to do
        /// </summary>
        /// <returns>boolean answer</returns>
        private bool askWhatToDO()
        {
            bool answer = false;

            MessageBoxResult mbr =
                MessageBox.Show(
                    Constants.WEBDRIVER_EXECUTING,
                    Constants.WEBDRIVER_BUSY_TITLE,
                    MessageBoxButton
                        .OKCancel,
                    MessageBoxImage
                        .Exclamation,
                    MessageBoxResult.OK,
                    MessageBoxOptions
                        .DefaultDesktopOnly);


            if (mbr == MessageBoxResult.OK)
            {
                answer = true;
            }

            else answer = false;

            return answer;
        }


        private void ChangeTestVersionPopup(object sender, RoutedEventArgs e)
        {

            PopupMoveTestFromVersionToVersion.IsOpen = true;
        }

        private void ComboboxSourceVersion_Loaded(object sender, RoutedEventArgs e)
        {

            DataGridFill(Constants.StrEnvironmentVersionMove, null);
        }

        private void ChangeTestVersion(string chosenVerName, int testId)
        {
            Sql sql = new Sql();
            var jp = new JsonParser();
            string currentVerName = jp.ReadJson("TestingEnvironmentVersion");
            var adapterEnvVer = new EnvironmentVersionTableAdapter();
            var dataset = new DataSetAutoTest();

            adapterEnvVer.Fill(dataset.EnvironmentVersion);

            int currentEnvironmentVersionId =
                int.Parse(adapterEnvVer.GetEnvironmnetVersionIDByVersionName(currentVerName).ToString());
            int chosenEnvironmentVersionId =
                int.Parse(adapterEnvVer.GetEnvironmnetVersionIDByVersionName(chosenVerName).ToString());
            string currentEnvironmentVersionColumn = adapterEnvVer.GetColumnByID(currentEnvironmentVersionId).ToString();
            string chosenEnvironmentVersionColumn = adapterEnvVer.GetColumnByID(chosenEnvironmentVersionId).ToString();
            //  ArrayList currentVerTestStepsToDeleteId = sql.GetTestStepsInVersion(currentEnvironmentVersionColumn, testId);
            ArrayList chosenVerTestStepsToCopyId = sql.GetTestStepsInVersion(chosenEnvironmentVersionColumn, testId);

            if (!currentVerName.Equals(chosenVerName)) //GetVersionColumn()
            {
                //start transaction
                using (TransactionScope scope = new TransactionScope())
                {
                    //check if the GUIMAP element used in the step exists in the chosen version and if not - make him exist.
                    foreach (int testStepId in chosenVerTestStepsToCopyId)
                    {
                        int guiMapId = sql.GetGuiMapIdFromTestStep(testStepId);
                        bool isExistGuiMapIdInCurrentVersion = sql.IsExistGuiMapIdInCurrentVersion(guiMapId,
                                                                                                   currentEnvironmentVersionColumn);
                        if (!isExistGuiMapIdInCurrentVersion)
                        {
                            sql.UpdateGuiMapInCurrentVersion(currentEnvironmentVersionColumn, guiMapId, 1);
                        }
                    }
                    //delete steps in current version
                    sql.DeleteCurrentVersionTestSteps(currentEnvironmentVersionColumn, testId);

                    //copy steps from chosen version to current version  
                    sql.CopyChosenVersionTestStepsToCurrentVersion(currentEnvironmentVersionColumn,
                                                                   chosenEnvironmentVersionColumn, testId);

                    scope.Complete();

                }
                //end transaction
            }

        }

        private void PopupMoveTestFromVersionToVersionbtnCancel_Click(object sender, RoutedEventArgs e)
        {
            PopupMoveTestFromVersionToVersion.IsOpen = false;
        }

        private void PopupMoveTestFromVersionToVersionbtnOK_Click(object sender, RoutedEventArgs e)
        {

            string chosenVersionName = ComboboxSourceVersion.Text;
            ChangeTestVersion(chosenVersionName, _testid);
            PopupMoveTestFromVersionToVersion.IsOpen = false;
        }

        private void TabItemGuiMap_Loaded(object sender, RoutedEventArgs e)
        {
            TreeViewProjects_Loaded(null, null);
        }

        private void TabItemTestEditor_Loaded(object sender, RoutedEventArgs e)
        {
            TreeViewTests_Loaded(null, null);
        }

        private void TabItemScenario_Loaded(object sender, RoutedEventArgs e)
        {

            TreeViewScenarios_Loaded(null, null);
        }

        private void ExecutionTab_Loaded(object sender, RoutedEventArgs e)
        {

            TreeViewBatch_Loaded(null, null);
        }

        private void TabControlMain_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ConfigurationManager.AppSettings["ShowAppleniumTabs"] != String.Empty)
                {
                    string showtabs = ConfigurationManager.AppSettings["ShowAppleniumTabs"].ToString();

                    //Main TAb Control 
                    foreach (TabItem item in TabControlMain.Items)
                    {
                        string header = item.Header.ToString();
                        if (showtabs.Contains(header))
                        {
                            item.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            item.Content = null;
                            item.Visibility = Visibility.Collapsed;
                        }
                    }

                    //Tools Tab Control ToolsTabControl

                    foreach (TabItem item in ToolsTabControl.Items)
                    {
                        string header = item.Header.ToString();
                        if (showtabs.Contains(header))
                        {
                            item.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            item.Content = null;
                            item.Visibility = Visibility.Collapsed;
                        }
                    }

                    if (showtabs.Contains("Loader.io"))
                    {
                        TabItemTools.IsSelected = true;
                        TabItemLoaderIo.IsSelected = true;
                    }
                }
            }
            catch (Exception exception)
            {

                MessageBox.Show(exception.Message, "Applenium");

            }

        }



        private void Combobox_SelectLoadTest_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {

                //connect to Loader.io API and get all tests

                ComboBox comboBox = (ComboBox)sender;
                string url = string.Format("https://api.loader.io/v2/tests");
                string appKeyName = "loaderio-auth";
                string appKey = ConfigurationManager.AppSettings["LoaderIOAppKey"];


                //separate thread 

                ThreadStart ts = delegate
                {
                    // Do long work here
                    var result = HttpRequestExtensions.TryGetJson<List<LoaderIoResultModel>>(url, appKeyName, appKey,
                                                                                             300000);
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, (EventHandler)
                                                                      delegate
                                                                      {

                                                                          if (result != null)
                                                                          {
                                                                              comboBox.Text =
                                                                              "Loading tests was Completed. Please select the test";
                                                                              var newresult =
                                                                                  new List
                                                                                      <
                                                                                          LoaderIoResultModelConcatenate
                                                                                          >();
                                                                              foreach (
                                                                                  var loaderresultmodel in result)
                                                                              {

                                                                                  newresult.Add(
                                                                                      new LoaderIoResultModelConcatenate
                                                                                          (loaderresultmodel));
                                                                              }
                                                                              comboBox.ItemsSource = newresult;
                                                                          }
                                                                          else
                                                                          {
                                                                              comboBox.Text =
                                                                            "Can't load tests. Verify you Loader.io app key";
                                                                          }
                                                                      }, null, null);
                };

                ts.BeginInvoke(delegate(IAsyncResult aysncResult) { ts.EndInvoke(aysncResult); }, null);

            }
            catch (Exception exception)
            {

                MessageBox.Show(exception.Message);
                ;
            }

        }



        private void ButtonRunTest_Click(object sender, RoutedEventArgs e)
        {
            bool alreadyRunning = executionThreaIsRunning();
            bool whatUserSaid = false;

            // Gather needed data
            try
            {
                // if thread is already running by another test/scenario/batch, ask user what to do next
                if (alreadyRunning)
                {
                    whatUserSaid = askWhatToDO();

                    // if user said "Okay", kill previously running execution and start a new one instead 
                    if (whatUserSaid)
                    {
                        // Kill previously running execution
                        executionThread.Abort();

                        // Update UI
                        ShowStopBtnLoadTest();
                        UpdateProgressLabel("...", "", Constants.UpdateProgress_REGULAR);

                        try
                        {
                            // Work in background
                            _loadtestid = ComboboxSelectLoadTest.SelectedValue.ToString();
                            _loadtestduration = Convert.ToInt32(TextboxSelectDurationtime.Text);
                            _loderioAppKey = TextboxAppKey.Text.Trim();
                            executionThread = new Thread(() => RunThreaded("loadtest"));
                            executionThread.Start();
                            NavigateWebBrowserLoadTest(_loadtestid);
                        }

                        catch (Exception ex)
                        {
                            //Console.WriteLine(ex.ToString());
                        }

                    }
                    // If user canceled, do nothing. Let the previous execution continue
                    else HideStopBtnLoadTest();
                }

                else if (!alreadyRunning)
                {
                    // Update UI
                    ShowStopBtnLoadTest();
                    UpdateProgressLabel("", "", Constants.UpdateProgress_REGULAR);
                    // Work In background
                    _loadtestid = ComboboxSelectLoadTest.SelectedValue.ToString();
                    _loadtestduration = Convert.ToInt32(TextboxSelectDurationtime.Text);
                    _loderioAppKey = TextboxAppKey.Text.Trim();
                    executionThread = new Thread(() => RunThreaded("loadtest"));
                    executionThread.Start();
                    NavigateWebBrowserLoadTest(_loadtestid);
                }


            }
            catch (Exception exception)
            {
                LogObject exceptionLogger2 = new LogObject();
                exceptionLogger2.StatusTag = Constants.ERROR;
                exceptionLogger2.Description = exception.Message;
                exceptionLogger2.Exception = exception;
                logger.Print(exceptionLogger2);
            }
        }



        //private bool RunLoadTest()
        //{
        //    MessageBox.Show("running load tests");
        //    return result-id;
        //}

        private string RunLoadTest(string testId, int totalDuration, string appKey)
        {
            try
            {
                LoaderIoResultTestResultModel result = new LoaderIoResultTestResultModel();

                //multithread - make long work here 

                //call api and run test.


                //calculate how many times to run test:
                //1. get duration time from test_id
                string appKeyName = "loaderio-auth";
                // string appKey = TextboxAppKey.Text;
                //if (testId == string.Empty)
                //{
                //    testId = ComboboxSelectLoadTest.Text;
                //}


                string url = string.Format("https://api.loader.io/v2/tests/{0}", testId);

                LoaderIoResultModel loaderIoResultModelResult =
                    HttpRequestExtensions.TryGetJson<LoaderIoResultModel>(url, appKeyName, appKey, 300000);
                int testDuration = Convert.ToInt32(loaderIoResultModelResult.duration);

                //2. calculate nuber of time to run loder.io test
                int numberofTimesToRun = totalDuration / testDuration;

                for (int i = 0; i <= numberofTimesToRun; i++)
                {
                    url = string.Format("https://api.loader.io/v2/tests/{0}/run", testId);

                    string postData = string.Empty;
                    //need to get results id 
                    result = HttpRequestExtensions.TryPutJson<LoaderIoResultTestResultModel>(url, appKeyName, appKey,
                                                                                                postData, 300000);
                    //wait till test completed 
                    Thread.Sleep(testDuration * 1000);
                }
                return result.result_id;


            }
            catch (Exception exception)
            {
                Debug.WriteLine("5 .Maxim-RunLoadTest");
                //MessageBox.Show(exception.Message);
                return string.Empty;
            }


        }

        private void ButtonLoadTests_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string url = string.Format("https://api.loader.io/v2/tests");
                string appKeyName = "loaderio-auth";
                string appKey = TextboxAppKey.Text.Trim();
                ComboboxSelectLoadTest.Text = "Loading all tests from Loader.io...- This operation can take several minutes.";
                ThreadStart ts = delegate
                {
                    // Do long work here

                    var result = HttpRequestExtensions.TryGetJson<List<LoaderIoResultModel>>(url, appKeyName, appKey, 300000);
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, (EventHandler)
                                                                      delegate
                                                                      {

                                                                          if (result != null)
                                                                          {
                                                                              ComboboxSelectLoadTest.Text =
                                                                              "Loading tests was completed. Please select the test";
                                                                              var newresult =
                                                                                  new List
                                                                                      <
                                                                                          LoaderIoResultModelConcatenate
                                                                                          >();
                                                                              foreach (
                                                                                  var loaderresultmodel in result)
                                                                              {

                                                                                  newresult.Add(
                                                                                      new LoaderIoResultModelConcatenate
                                                                                          (loaderresultmodel));
                                                                              }
                                                                              ComboboxSelectLoadTest.ItemsSource = newresult;
                                                                          }
                                                                          else
                                                                          {
                                                                              ComboboxSelectLoadTest.Text =
                                                                            "Can't load tests. Verify you Loader.io app key";
                                                                          }
                                                                      }, null, null);
                };
                ts.BeginInvoke(delegate(IAsyncResult aysncResult) { ts.EndInvoke(aysncResult); }, null);

            }
            catch (Exception exception)
            {

                MessageBox.Show(exception.Message);
            }
        }

        private void TabItemLoaderIo_loaded(object sender, RoutedEventArgs e)
        {
            try
            {

                string showtabs = ConfigurationManager.AppSettings["ShowAppleniumTabs"].ToString();
                if (showtabs.Contains("Loader.io"))
                {
                    if (ConfigurationManager.AppSettings["LoaderIOAppKey"] == string.Empty)
                    {
                        var appKey = Interaction.InputBox("Enter you Loader.io App key it can be found under : https://loader.io/settings", "Applenium - Loader.io AppKey").Trim();
                        if (appKey == string.Empty)
                        {
                            appKey = "Replace Loader.io AppKey";
                        }
                        UpdateAppSettings("LoaderIOAppKey", appKey);
                        TextboxAppKey.Text = ConfigurationManager.AppSettings["LoaderIOAppKey"].ToString();
                    }
                    else
                    {
                        TextboxAppKey.Text = ConfigurationManager.AppSettings["LoaderIOAppKey"].ToString();
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }

        }

        private void UpdateAppSettings(string theKey, string theValue)
        {

            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (ConfigurationManager.AppSettings.AllKeys.Contains(theKey))
            {
                configuration.AppSettings.Settings[theKey].Value = theValue;
            }

            configuration.Save(ConfigurationSaveMode.Modified);

            ConfigurationManager.RefreshSection("appSettings");
        }

        private void TextboxAppKey_SelectionChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                UpdateAppSettings("LoaderIOAppKey", ((TextBox)sender).Text);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void WebBrowserLoaderio_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                WebBrowser wb = (WebBrowser)sender;
                dynamic activeX = wb.GetType().InvokeMember("ActiveXInstance",
                    BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                    null, wb, new object[] { });

                activeX.Silent = true;


                var oUri = new Uri("https://loader.io/tests");
                WebBrowserLoaderio.Navigate(oUri);
            }

            catch (Exception)
            {
            }

        }

        private void ShowStopBtnLoadTest()
        {
            ButtonStopLoadTest.IsEnabled = true;
            ButtonRunLoadTest.IsEnabled = false;
        }

        private void HideStopBtnLoadTest()
        {
            ButtonStopLoadTest.IsEnabled = false;
            ButtonRunLoadTest.IsEnabled = true;
        }

        private void NavigateWebBrowserLoadTest(string url)
        {
            var oUri = new Uri(string.Format("https://loader.io/tests/{0}#all-results", url));
            WebBrowserLoaderio.Navigate(oUri);
            WebBrowserLoaderio.Refresh();
        }

        private void ButtonStopTest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Singleton myInstance = Singleton.Instance; // Will always be the same instance...
                myInstance.StopExecution = true;

                executionThread.Abort();
                executionThread = null;
                //Update UI
                UpdateProgressLabel(Constants.STOPPED_EXPLICIT, "", Constants.UpdateProgress_STOPPED);
                HideStopBtnLoadTest();
            }
            catch (Exception)
            {


            }
        }
        /// VSH: Create GuiMap Commands from Popup Window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddNewCommandFromPopup(object sender, RoutedEventArgs e)
        {
            string GuiTagType = "[QA_Autotest].[Test].[GuiTagType]";
            string GuiMap = "[QA_Autotest].[Test].[GuiMap]";
            string GuiTestStepsTable = "[QA_Autotest].[Test].[GuiTestSteps]";
            string giumap_tmp = "";
            PopupTestCommand.IsOpen = false;

            string cmd_val = TBCmdName.Text;
            string guimap_name = TBGuiMapName.Text;
            string guimap_val = TBGuiMapValue.Text;
            string TagTypeName = TBGuiMapTagTypeName.Text;
            string TestStepID = TBTestStepID.Text;
            string testId = TBTestID.Text;
            string VersionClnID = TBVersionClnID.Text;

            var sql = new Sql();

            string sql_cmd = "TagTypeValue ='" + guimap_val + "'";
            string DBguimapName = sql.GetOneParameter("GuiMapObjectName", GuiMap, sql_cmd);

            if (DBguimapName != string.Empty)
            {
                string guimap_name_tmp = Interaction.InputBox("The GuiMap Command '" + DBguimapName + "' with value '" + guimap_val + "' already exists. Enter 'Cancel' to use existing command?\n\nEnter New Gui Command Name for Create New", "GuiMaP Help Wizard", DBguimapName);
                if (guimap_name_tmp == string.Empty)
                {
                    string GuiMapID = sql.GetOneParameter("GuiMapID", GuiMap, sql_cmd);

                    // UPDATE [QA_Autotest].[Test].[GuiTestSteps] SET [GuiMapID]= '"+ GuiMapID +"'  WHERE GuiTestID=3837  and StepsOrder=2 and Ver1=1
                    sql_cmd = " GuiTestID='" + testId + "' AND StepsOrder='" + TestStepID + "' AND " + VersionClnID + " =1";
                    giumap_tmp = sql.UpdateOneParameter("GuiMapID", GuiMapID, GuiTestStepsTable, sql_cmd);
                    if (giumap_tmp == GuiMapID)
                    {
                        DataGridFill(Constants.StrGuiTestSteps, _testid.ToString(CultureInfo.InvariantCulture));
                        return;
                    }
                }
                else
                {
                    guimap_name = guimap_name_tmp;
                }
            }

            string TagTypeID = sql.GetOneParameter("TagTypeID", GuiTagType, "TagType='" + TagTypeName + "'");
            if (TagTypeID == string.Empty)
            {
                TagTypeID = "10";
            }
            int GuiProjectID = 11;

            sql_cmd = @"( [GuiMapObjectName], [TagTypeID], [TagTypeValue], [GuiProjectID], [" + VersionClnID + "])";
            sql_cmd += " VALUES ('" + guimap_name + "', '" + Convert.ToInt32(TagTypeID) + "', '" + guimap_val + "', '" + GuiProjectID + "', '1')";


            //Create new GuiMap object
            string GuiMapID_CMD = sql.InsertNewData(GuiMap, sql_cmd);

            // UPDATE [QA_Autotest].[Test].[GuiTestSteps] SET [GuiMapID]= '"+ GuiMapID +"'  WHERE GuiTestID=3837  and StepsOrder=2 and Ver1=1
            sql_cmd = " GuiTestID='" + testId + "' AND StepsOrder='" + TestStepID + "' AND " + VersionClnID + " =1";
            giumap_tmp = sql.UpdateOneParameter("GuiMapID", GuiMapID_CMD, GuiTestStepsTable, sql_cmd);
            //DataGridFill(Constants.StrGuiTest, null);
            DataGridFill(Constants.StrGuiTestSteps, _testid.ToString(CultureInfo.InvariantCulture));
            _editinggrid = false;
        }

        private void CancelAddNewCommandFromPopup(object sender, RoutedEventArgs e)
        {
            PopupTestCommand.IsOpen = false;
            _editinggrid = false;
            return;
        }


    }
}