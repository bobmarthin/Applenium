﻿/*
Deployment script for QA_Autotest

This code was generated by a tool.
Changes to this file may cause incorrect behavior and will be lost if
the code is regenerated.
*/

GO
SET ANSI_NULLS, ANSI_PADDING, ANSI_WARNINGS, ARITHABORT, CONCAT_NULL_YIELDS_NULL, QUOTED_IDENTIFIER ON;

SET NUMERIC_ROUNDABORT OFF;


GO
:setvar DatabaseName "QA_Autotest"
:setvar DefaultFilePrefix "QA_Autotest"
:setvar DefaultDataPath "C:\Users\maximgu\AppData\Local\Microsoft\Microsoft SQL Server Local DB\Instances\v11.0\"
:setvar DefaultLogPath "C:\Users\maximgu\AppData\Local\Microsoft\Microsoft SQL Server Local DB\Instances\v11.0\"

GO
:on error exit
GO
/*
Detect SQLCMD mode and disable script execution if SQLCMD mode is not supported.
To re-enable the script after enabling SQLCMD mode, execute the following:
SET NOEXEC OFF; 
*/
:setvar __IsSqlCmdEnabled "True"
GO
IF N'$(__IsSqlCmdEnabled)' NOT LIKE N'True'
    BEGIN
        PRINT N'SQLCMD mode must be enabled to successfully execute this script.';
        SET NOEXEC ON;
    END


GO
USE [$(DatabaseName)];


GO
/*
The column [Test].[GuiTestSteps].[11-OB_2.6.3.9] is being dropped, data loss could occur.
*/

--IF EXISTS (select top 1 1 from [Test].[GuiTestSteps])
--    RAISERROR (N'Rows were detected. The schema update is terminating because data loss might occur.', 16, 127) WITH NOWAIT

GO
PRINT N'Starting rebuilding table [Test].[BatchLogic]...';


GO
BEGIN TRANSACTION;

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

SET XACT_ABORT ON;

CREATE TABLE [Test].[tmp_ms_xx_BatchLogic] (
    [BatchLogicID]      INT IDENTITY (1, 1) NOT NULL,
    [BatchID]           INT NOT NULL,
    [ScenarioID]        INT NOT NULL,
    [BrowserID]         INT NULL,
    [ExecutionStatusID] INT NULL
);

CREATE CLUSTERED INDEX [tmp_ms_xx_index_BatchLogicID]
    ON [Test].[tmp_ms_xx_BatchLogic]([BatchLogicID] ASC) ;

IF EXISTS (SELECT TOP 1 1 
           FROM   [Test].[BatchLogic])
    BEGIN
        SET IDENTITY_INSERT [Test].[tmp_ms_xx_BatchLogic] ON;
        INSERT INTO [Test].[tmp_ms_xx_BatchLogic] ([BatchLogicID], [BatchID], [ScenarioID], [BrowserID], [ExecutionStatusID])
        SELECT   [BatchLogicID],
                 [BatchID],
                 [ScenarioID],
                 [BrowserID],
                 [ExecutionStatusID]
        FROM     [Test].[BatchLogic]
        ORDER BY [BatchLogicID] ASC;
        SET IDENTITY_INSERT [Test].[tmp_ms_xx_BatchLogic] OFF;
    END

DROP TABLE [Test].[BatchLogic];

EXECUTE sp_rename N'[Test].[tmp_ms_xx_BatchLogic]', N'BatchLogic';

EXECUTE sp_rename N'[Test].[BatchLogic].[tmp_ms_xx_index_BatchLogicID]', N'BatchLogicID', N'INDEX';

COMMIT TRANSACTION;

SET TRANSACTION ISOLATION LEVEL READ COMMITTED;


GO
PRINT N'Starting rebuilding table [Test].[Browsers]...';


GO
BEGIN TRANSACTION;

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

SET XACT_ABORT ON;

CREATE TABLE [Test].[tmp_ms_xx_Browsers] (
    [BrowserID]   INT            IDENTITY (1, 1) NOT NULL,
    [BrowserName] NVARCHAR (MAX) NOT NULL
);

CREATE CLUSTERED INDEX [tmp_ms_xx_index_BrowserID]
    ON [Test].[tmp_ms_xx_Browsers]([BrowserID] ASC) ;

IF EXISTS (SELECT TOP 1 1 
           FROM   [Test].[Browsers])
    BEGIN
        SET IDENTITY_INSERT [Test].[tmp_ms_xx_Browsers] ON;
        INSERT INTO [Test].[tmp_ms_xx_Browsers] ([BrowserID], [BrowserName])
        SELECT   [BrowserID],
                 [BrowserName]
        FROM     [Test].[Browsers]
        ORDER BY [BrowserID] ASC;
        SET IDENTITY_INSERT [Test].[tmp_ms_xx_Browsers] OFF;
    END

DROP TABLE [Test].[Browsers];

EXECUTE sp_rename N'[Test].[tmp_ms_xx_Browsers]', N'Browsers';

EXECUTE sp_rename N'[Test].[Browsers].[tmp_ms_xx_index_BrowserID]', N'BrowserID', N'INDEX';

COMMIT TRANSACTION;

SET TRANSACTION ISOLATION LEVEL READ COMMITTED;


GO
PRINT N'Starting rebuilding table [Test].[GuiMap]...';


GO
BEGIN TRANSACTION;

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

SET XACT_ABORT ON;

CREATE TABLE [Test].[tmp_ms_xx_GuiMap] (
    [GuiMapID]         INT            IDENTITY (1, 1) NOT NULL,
    [GuiMapObjectName] NVARCHAR (MAX) NOT NULL,
    [TagTypeID]        INT            NOT NULL,
    [TagTypeValue]     NVARCHAR (MAX) NOT NULL,
    [GuiProjectID]     INT            NOT NULL,
    CONSTRAINT [tmp_ms_xx_constraint_PK_GuiMapObject1] PRIMARY KEY CLUSTERED ([GuiMapID] ASC) 
);

IF EXISTS (SELECT TOP 1 1 
           FROM   [Test].[GuiMap])
    BEGIN
        SET IDENTITY_INSERT [Test].[tmp_ms_xx_GuiMap] ON;
        INSERT INTO [Test].[tmp_ms_xx_GuiMap] ([GuiMapID], [GuiMapObjectName], [TagTypeID], [TagTypeValue], [GuiProjectID])
        SELECT   [GuiMapID],
                 [GuiMapObjectName],
                 [TagTypeID],
                 [TagTypeValue],
                 [GuiProjectID]
        FROM     [Test].[GuiMap]
        ORDER BY [GuiMapID] ASC;
        SET IDENTITY_INSERT [Test].[tmp_ms_xx_GuiMap] OFF;
    END

DROP TABLE [Test].[GuiMap];

EXECUTE sp_rename N'[Test].[tmp_ms_xx_GuiMap]', N'GuiMap';

EXECUTE sp_rename N'[Test].[tmp_ms_xx_constraint_PK_GuiMapObject1]', N'PK_GuiMapObject1', N'OBJECT';

COMMIT TRANSACTION;

SET TRANSACTION ISOLATION LEVEL READ COMMITTED;


GO
PRINT N'Starting rebuilding table [Test].[GuiPageSection]...';


GO
BEGIN TRANSACTION;

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

SET XACT_ABORT ON;

CREATE TABLE [Test].[tmp_ms_xx_GuiPageSection] (
    [GuiPageSectionID]   INT            IDENTITY (1, 1) NOT NULL,
    [GuiPageID]          INT            NOT NULL,
    [GuiPageSectionName] NVARCHAR (MAX) NOT NULL
);

CREATE CLUSTERED INDEX [tmp_ms_xx_index_GuiPageSectionID]
    ON [Test].[tmp_ms_xx_GuiPageSection]([GuiPageSectionID] ASC) ;

IF EXISTS (SELECT TOP 1 1 
           FROM   [Test].[GuiPageSection])
    BEGIN
        SET IDENTITY_INSERT [Test].[tmp_ms_xx_GuiPageSection] ON;
        INSERT INTO [Test].[tmp_ms_xx_GuiPageSection] ([GuiPageSectionID], [GuiPageID], [GuiPageSectionName])
        SELECT   [GuiPageSectionID],
                 [GuiPageID],
                 [GuiPageSectionName]
        FROM     [Test].[GuiPageSection]
        ORDER BY [GuiPageSectionID] ASC;
        SET IDENTITY_INSERT [Test].[tmp_ms_xx_GuiPageSection] OFF;
    END

DROP TABLE [Test].[GuiPageSection];

EXECUTE sp_rename N'[Test].[tmp_ms_xx_GuiPageSection]', N'GuiPageSection';

EXECUTE sp_rename N'[Test].[GuiPageSection].[tmp_ms_xx_index_GuiPageSectionID]', N'GuiPageSectionID', N'INDEX';

COMMIT TRANSACTION;

SET TRANSACTION ISOLATION LEVEL READ COMMITTED;


GO
PRINT N'Starting rebuilding table [Test].[GuiProjectPage]...';


GO
BEGIN TRANSACTION;

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

SET XACT_ABORT ON;

CREATE TABLE [Test].[tmp_ms_xx_GuiProjectPage] (
    [GuiProjectPageID] INT            IDENTITY (1, 1) NOT NULL,
    [ProjectID]        INT            NOT NULL,
    [ProjectPageName]  NVARCHAR (MAX) NOT NULL,
    [PageURL]          NVARCHAR (MAX) NULL,
    CONSTRAINT [tmp_ms_xx_constraint_PK_GuiProject_1] PRIMARY KEY CLUSTERED ([GuiProjectPageID] ASC) 
);

IF EXISTS (SELECT TOP 1 1 
           FROM   [Test].[GuiProjectPage])
    BEGIN
        SET IDENTITY_INSERT [Test].[tmp_ms_xx_GuiProjectPage] ON;
        INSERT INTO [Test].[tmp_ms_xx_GuiProjectPage] ([GuiProjectPageID], [ProjectID], [ProjectPageName], [PageURL])
        SELECT   [GuiProjectPageID],
                 [ProjectID],
                 [ProjectPageName],
                 [PageURL]
        FROM     [Test].[GuiProjectPage]
        ORDER BY [GuiProjectPageID] ASC;
        SET IDENTITY_INSERT [Test].[tmp_ms_xx_GuiProjectPage] OFF;
    END

DROP TABLE [Test].[GuiProjectPage];

EXECUTE sp_rename N'[Test].[tmp_ms_xx_GuiProjectPage]', N'GuiProjectPage';

EXECUTE sp_rename N'[Test].[tmp_ms_xx_constraint_PK_GuiProject_1]', N'PK_GuiProject_1', N'OBJECT';

COMMIT TRANSACTION;

SET TRANSACTION ISOLATION LEVEL READ COMMITTED;


GO
PRINT N'Starting rebuilding table [Test].[GuiScenarioLogic]...';


GO
BEGIN TRANSACTION;

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

SET XACT_ABORT ON;

CREATE TABLE [Test].[tmp_ms_xx_GuiScenarioLogic] (
    [GuiScenarioLogicID] INT            IDENTITY (1, 1) NOT NULL,
    [GuiScenarioID]      INT            NOT NULL,
    [GuiTestID]          INT            NOT NULL,
    [InputDataRow]       NVARCHAR (MAX) NULL,
    [StepsOrder]         INT            NOT NULL,
    CONSTRAINT [tmp_ms_xx_constraint_PK_GuiScenarioLogic] PRIMARY KEY CLUSTERED ([GuiScenarioLogicID] ASC) 
);

IF EXISTS (SELECT TOP 1 1 
           FROM   [Test].[GuiScenarioLogic])
    BEGIN
        SET IDENTITY_INSERT [Test].[tmp_ms_xx_GuiScenarioLogic] ON;
        INSERT INTO [Test].[tmp_ms_xx_GuiScenarioLogic] ([GuiScenarioLogicID], [GuiScenarioID], [GuiTestID], [InputDataRow], [StepsOrder])
        SELECT   [GuiScenarioLogicID],
                 [GuiScenarioID],
                 [GuiTestID],
                 [InputDataRow],
                 [StepsOrder]
        FROM     [Test].[GuiScenarioLogic]
        ORDER BY [GuiScenarioLogicID] ASC;
        SET IDENTITY_INSERT [Test].[tmp_ms_xx_GuiScenarioLogic] OFF;
    END

DROP TABLE [Test].[GuiScenarioLogic];

EXECUTE sp_rename N'[Test].[tmp_ms_xx_GuiScenarioLogic]', N'GuiScenarioLogic';

EXECUTE sp_rename N'[Test].[tmp_ms_xx_constraint_PK_GuiScenarioLogic]', N'PK_GuiScenarioLogic', N'OBJECT';

COMMIT TRANSACTION;

SET TRANSACTION ISOLATION LEVEL READ COMMITTED;


GO
PRINT N'Starting rebuilding table [Test].[GuiTagType]...';


GO
BEGIN TRANSACTION;

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

SET XACT_ABORT ON;

CREATE TABLE [Test].[tmp_ms_xx_GuiTagType] (
    [TagTypeID] INT        IDENTITY (1, 1) NOT NULL,
    [TagType]   NCHAR (50) NOT NULL,
    CONSTRAINT [tmp_ms_xx_constraint_PK_GUIObjectDefenitionType] PRIMARY KEY CLUSTERED ([TagTypeID] ASC) 
);

IF EXISTS (SELECT TOP 1 1 
           FROM   [Test].[GuiTagType])
    BEGIN
        SET IDENTITY_INSERT [Test].[tmp_ms_xx_GuiTagType] ON;
        INSERT INTO [Test].[tmp_ms_xx_GuiTagType] ([TagTypeID], [TagType])
        SELECT   [TagTypeID],
                 [TagType]
        FROM     [Test].[GuiTagType]
        ORDER BY [TagTypeID] ASC;
        SET IDENTITY_INSERT [Test].[tmp_ms_xx_GuiTagType] OFF;
    END

DROP TABLE [Test].[GuiTagType];

EXECUTE sp_rename N'[Test].[tmp_ms_xx_GuiTagType]', N'GuiTagType';

EXECUTE sp_rename N'[Test].[tmp_ms_xx_constraint_PK_GUIObjectDefenitionType]', N'PK_GUIObjectDefenitionType', N'OBJECT';

COMMIT TRANSACTION;

SET TRANSACTION ISOLATION LEVEL READ COMMITTED;


GO
PRINT N'Starting rebuilding table [Test].[GuiTestSteps]...';


GO
BEGIN TRANSACTION;

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

SET XACT_ABORT ON;

CREATE TABLE [Test].[tmp_ms_xx_GuiTestSteps] (
    [GuiTestStepsID]  INT            IDENTITY (1, 1) NOT NULL,
    [GuiTestID]       INT            NOT NULL,
    [GuiMapID]        INT            NOT NULL,
    [GuiMapCommandID] INT            NOT NULL,
    [InputDataColumn] NVARCHAR (50)  NULL,
    [IsValidate]      INT            NULL,
    [InputDataRow]    NVARCHAR (MAX) NULL,
    [StepsOrder]      INT            NOT NULL,
    CONSTRAINT [tmp_ms_xx_constraint_PK_GuiTestSteps1] PRIMARY KEY CLUSTERED ([GuiTestStepsID] ASC) 
);

IF EXISTS (SELECT TOP 1 1 
           FROM   [Test].[GuiTestSteps])
    BEGIN
        SET IDENTITY_INSERT [Test].[tmp_ms_xx_GuiTestSteps] ON;
        INSERT INTO [Test].[tmp_ms_xx_GuiTestSteps] ([GuiTestStepsID], [GuiTestID], [GuiMapID], [GuiMapCommandID], [InputDataColumn], [IsValidate], [InputDataRow], [StepsOrder])
        SELECT   [GuiTestStepsID],
                 [GuiTestID],
                 [GuiMapID],
                 [GuiMapCommandID],
                 [InputDataColumn],
                 [IsValidate],
                 [InputDataRow],
                 [StepsOrder]
        FROM     [Test].[GuiTestSteps]
        ORDER BY [GuiTestStepsID] ASC;
        SET IDENTITY_INSERT [Test].[tmp_ms_xx_GuiTestSteps] OFF;
    END

DROP TABLE [Test].[GuiTestSteps];

EXECUTE sp_rename N'[Test].[tmp_ms_xx_GuiTestSteps]', N'GuiTestSteps';

EXECUTE sp_rename N'[Test].[tmp_ms_xx_constraint_PK_GuiTestSteps1]', N'PK_GuiTestSteps1', N'OBJECT';

COMMIT TRANSACTION;

SET TRANSACTION ISOLATION LEVEL READ COMMITTED;


GO
PRINT N'Starting rebuilding table [Test].[LogStatus]...';


GO
BEGIN TRANSACTION;

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

SET XACT_ABORT ON;

CREATE TABLE [Test].[tmp_ms_xx_LogStatus] (
    [LogStatusID]          INT           IDENTITY (1, 1) NOT NULL,
    [LogStatusDescription] NVARCHAR (50) NOT NULL
);

CREATE CLUSTERED INDEX [tmp_ms_xx_index_LogStatusID]
    ON [Test].[tmp_ms_xx_LogStatus]([LogStatusID] ASC) ;

IF EXISTS (SELECT TOP 1 1 
           FROM   [Test].[LogStatus])
    BEGIN
        SET IDENTITY_INSERT [Test].[tmp_ms_xx_LogStatus] ON;
        INSERT INTO [Test].[tmp_ms_xx_LogStatus] ([LogStatusID], [LogStatusDescription])
        SELECT   [LogStatusID],
                 [LogStatusDescription]
        FROM     [Test].[LogStatus]
        ORDER BY [LogStatusID] ASC;
        SET IDENTITY_INSERT [Test].[tmp_ms_xx_LogStatus] OFF;
    END

DROP TABLE [Test].[LogStatus];

EXECUTE sp_rename N'[Test].[tmp_ms_xx_LogStatus]', N'LogStatus';

EXECUTE sp_rename N'[Test].[LogStatus].[tmp_ms_xx_index_LogStatusID]', N'LogStatusID', N'INDEX';

COMMIT TRANSACTION;

SET TRANSACTION ISOLATION LEVEL READ COMMITTED;


GO
PRINT N'Starting rebuilding table [Test].[nums]...';


GO
BEGIN TRANSACTION;

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

SET XACT_ABORT ON;

CREATE TABLE [Test].[tmp_ms_xx_nums] (
    [id]  INT        NULL,
    [str] NCHAR (10) NULL
)
;

IF EXISTS (SELECT TOP 1 1 
           FROM   [Test].[nums])
    BEGIN
        INSERT INTO [Test].[tmp_ms_xx_nums] ([id], [str])
        SELECT [id],
               [str]
        FROM   [Test].[nums];
    END

DROP TABLE [Test].[nums];

EXECUTE sp_rename N'[Test].[tmp_ms_xx_nums]', N'nums';

COMMIT TRANSACTION;

SET TRANSACTION ISOLATION LEVEL READ COMMITTED;


GO
PRINT N'Starting rebuilding table [Test].[Projects]...';


GO
BEGIN TRANSACTION;

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

SET XACT_ABORT ON;

CREATE TABLE [Test].[tmp_ms_xx_Projects] (
    [GuiProjectID]          INT           IDENTITY (1, 1) NOT NULL,
    [GuiProjectName]        NVARCHAR (50) NOT NULL,
    [GuiProjectDescription] NVARCHAR (50) NULL,
    [GuiProjectURL]         NVARCHAR (50) NULL,
    [Prefix]                NCHAR (10)    NULL,
    CONSTRAINT [tmp_ms_xx_constraint_PK_GuiProject] PRIMARY KEY CLUSTERED ([GuiProjectID] ASC) 
);

IF EXISTS (SELECT TOP 1 1 
           FROM   [Test].[Projects])
    BEGIN
        SET IDENTITY_INSERT [Test].[tmp_ms_xx_Projects] ON;
        INSERT INTO [Test].[tmp_ms_xx_Projects] ([GuiProjectID], [GuiProjectName], [GuiProjectDescription], [GuiProjectURL], [Prefix])
        SELECT   [GuiProjectID],
                 [GuiProjectName],
                 [GuiProjectDescription],
                 [GuiProjectURL],
                 [Prefix]
        FROM     [Test].[Projects]
        ORDER BY [GuiProjectID] ASC;
        SET IDENTITY_INSERT [Test].[tmp_ms_xx_Projects] OFF;
    END

DROP TABLE [Test].[Projects];

EXECUTE sp_rename N'[Test].[tmp_ms_xx_Projects]', N'Projects';

EXECUTE sp_rename N'[Test].[tmp_ms_xx_constraint_PK_GuiProject]', N'PK_GuiProject', N'OBJECT';

COMMIT TRANSACTION;

SET TRANSACTION ISOLATION LEVEL READ COMMITTED;


GO
PRINT N'Starting rebuilding table [Test].[Scenario]...';


GO
BEGIN TRANSACTION;

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

SET XACT_ABORT ON;

CREATE TABLE [Test].[tmp_ms_xx_Scenario] (
    [ScenarioID]   INT            IDENTITY (1, 1) NOT NULL,
    [ScenarioName] NVARCHAR (MAX) NOT NULL,
    [IsAPI]        INT            NULL,
    [Description]  NVARCHAR (MAX) NULL,
    [ProjectID]    INT            NULL,
    CONSTRAINT [tmp_ms_xx_constraint_PK_Flow] PRIMARY KEY CLUSTERED ([ScenarioID] ASC) 
);

IF EXISTS (SELECT TOP 1 1 
           FROM   [Test].[Scenario])
    BEGIN
        SET IDENTITY_INSERT [Test].[tmp_ms_xx_Scenario] ON;
        INSERT INTO [Test].[tmp_ms_xx_Scenario] ([ScenarioID], [ScenarioName], [IsAPI], [Description], [ProjectID])
        SELECT   [ScenarioID],
                 [ScenarioName],
                 [IsAPI],
                 [Description],
                 [ProjectID]
        FROM     [Test].[Scenario]
        ORDER BY [ScenarioID] ASC;
        SET IDENTITY_INSERT [Test].[tmp_ms_xx_Scenario] OFF;
    END

DROP TABLE [Test].[Scenario];

EXECUTE sp_rename N'[Test].[tmp_ms_xx_Scenario]', N'Scenario';

EXECUTE sp_rename N'[Test].[tmp_ms_xx_constraint_PK_Flow]', N'PK_Flow', N'OBJECT';

COMMIT TRANSACTION;

SET TRANSACTION ISOLATION LEVEL READ COMMITTED;


GO
PRINT N'Starting rebuilding table [Test].[Test]...';


GO
BEGIN TRANSACTION;

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

SET XACT_ABORT ON;

CREATE TABLE [Test].[tmp_ms_xx_Test] (
    [TestId]               INT            IDENTITY (1, 1) NOT NULL,
    [TestName]             NVARCHAR (50)  NOT NULL,
    [ProjectID]            INT            NULL,
    [InputTableName]       NVARCHAR (50)  NULL,
    [IsAPI]                INT            NULL,
    [Description]          NVARCHAR (MAX) NULL,
    [StatusCompletedImage] NVARCHAR (MAX) NULL,
    CONSTRAINT [tmp_ms_xx_constraint_PK_Test] PRIMARY KEY CLUSTERED ([TestId] ASC) 
);

IF EXISTS (SELECT TOP 1 1 
           FROM   [Test].[Test])
    BEGIN
        SET IDENTITY_INSERT [Test].[tmp_ms_xx_Test] ON;
        INSERT INTO [Test].[tmp_ms_xx_Test] ([TestId], [TestName], [ProjectID], [InputTableName], [IsAPI], [Description], [StatusCompletedImage])
        SELECT   [TestId],
                 [TestName],
                 [ProjectID],
                 [InputTableName],
                 [IsAPI],
                 [Description],
                 [StatusCompletedImage]
        FROM     [Test].[Test]
        ORDER BY [TestId] ASC;
        SET IDENTITY_INSERT [Test].[tmp_ms_xx_Test] OFF;
    END

DROP TABLE [Test].[Test];

EXECUTE sp_rename N'[Test].[tmp_ms_xx_Test]', N'Test';

EXECUTE sp_rename N'[Test].[tmp_ms_xx_constraint_PK_Test]', N'PK_Test', N'OBJECT';

COMMIT TRANSACTION;

SET TRANSACTION ISOLATION LEVEL READ COMMITTED;


GO
PRINT N'Starting rebuilding table [Test].[TestCommand]...';


GO
BEGIN TRANSACTION;

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

SET XACT_ABORT ON;

CREATE TABLE [Test].[tmp_ms_xx_TestCommand] (
    [TestCommandID]   INT           IDENTITY (1, 1) NOT NULL,
    [TestCommandName] NVARCHAR (50) NOT NULL,
    [SeleniumCommand] NVARCHAR (50) NULL,
    CONSTRAINT [tmp_ms_xx_constraint_PK_TestCommand] PRIMARY KEY CLUSTERED ([TestCommandID] ASC) 
);

IF EXISTS (SELECT TOP 1 1 
           FROM   [Test].[TestCommand])
    BEGIN
        SET IDENTITY_INSERT [Test].[tmp_ms_xx_TestCommand] ON;
        INSERT INTO [Test].[tmp_ms_xx_TestCommand] ([TestCommandID], [TestCommandName], [SeleniumCommand])
        SELECT   [TestCommandID],
                 [TestCommandName],
                 [SeleniumCommand]
        FROM     [Test].[TestCommand]
        ORDER BY [TestCommandID] ASC;
        SET IDENTITY_INSERT [Test].[tmp_ms_xx_TestCommand] OFF;
    END

DROP TABLE [Test].[TestCommand];

EXECUTE sp_rename N'[Test].[tmp_ms_xx_TestCommand]', N'TestCommand';

EXECUTE sp_rename N'[Test].[tmp_ms_xx_constraint_PK_TestCommand]', N'PK_TestCommand', N'OBJECT';

COMMIT TRANSACTION;

SET TRANSACTION ISOLATION LEVEL READ COMMITTED;


GO

GO
PRINT N'Starting rebuilding table [Test].[TestResults]...';


GO
BEGIN TRANSACTION;

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

SET XACT_ABORT ON;

CREATE TABLE [Test].[tmp_ms_xx_TestResults] (
    [ResultsIndex]   INT            IDENTITY (1, 1) NOT NULL,
    [RunExecutionID] INT            NOT NULL,
    [BatchID]        INT            NOT NULL,
    [ScenarioID]     INT            NOT NULL,
    [TestID]         INT            NOT NULL,
    [StepID]         INT            NOT NULL,
    [Description]    NVARCHAR (MAX) NOT NULL,
    [Status]         INT            NOT NULL,
    [OutOfFlow]      NVARCHAR (50)  NOT NULL,
    [TimeStamp]      ROWVERSION     NOT NULL,
    [ProjectID]      INT            NOT NULL,
    [ProjectPageID]  INT            NOT NULL,
    [Occured]        DATETIME       NULL,
    [FailureLogID]   INT            NULL,
    [IsRelease]      TINYINT        NULL
);

CREATE CLUSTERED INDEX [tmp_ms_xx_index_ResultsIndex]
    ON [Test].[tmp_ms_xx_TestResults]([ResultsIndex] ASC) ;

IF EXISTS (SELECT TOP 1 1 
           FROM   [Test].[TestResults])
    BEGIN
        SET IDENTITY_INSERT [Test].[tmp_ms_xx_TestResults] ON;
        INSERT INTO [Test].[tmp_ms_xx_TestResults] ([ResultsIndex], [RunExecutionID], [BatchID], [ScenarioID], [TestID], [StepID], [Description], [Status], [OutOfFlow], [ProjectID], [ProjectPageID], [Occured], [FailureLogID], [IsRelease])
        SELECT   [ResultsIndex],
                 [RunExecutionID],
                 [BatchID],
                 [ScenarioID],
                 [TestID],
                 [StepID],
                 [Description],
                 [Status],
                 [OutOfFlow],
                 [ProjectID],
                 [ProjectPageID],
                 [Occured],
                 [FailureLogID],
                 [IsRelease]
        FROM     [Test].[TestResults]
        ORDER BY [ResultsIndex] ASC;
        SET IDENTITY_INSERT [Test].[tmp_ms_xx_TestResults] OFF;
    END

DROP TABLE [Test].[TestResults];

EXECUTE sp_rename N'[Test].[tmp_ms_xx_TestResults]', N'TestResults';

EXECUTE sp_rename N'[Test].[TestResults].[tmp_ms_xx_index_ResultsIndex]', N'ResultsIndex', N'INDEX';

COMMIT TRANSACTION;

SET TRANSACTION ISOLATION LEVEL READ COMMITTED;


GO
PRINT N'Altering [Test].[Batches].[BatchID]...';


GO
ALTER INDEX [BatchID]
    ON [Test].[Batches] REBUILD ;



GO
PRINT N'Update complete.';


GO