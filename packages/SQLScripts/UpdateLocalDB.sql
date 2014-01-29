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
    ON [Test].[tmp_ms_xx_BatchLogic]([BatchLogicID] ASC);

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
    ON [Test].[tmp_ms_xx_Browsers]([BrowserID] ASC);

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
    ON [Test].[tmp_ms_xx_GuiPageSection]([GuiPageSectionID] ASC);

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
    ON [Test].[tmp_ms_xx_LogStatus]([LogStatusID] ASC);

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
WITH (DATA_COMPRESSION = PAGE);

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
PRINT N'Starting rebuilding table [Test].[TestFailiureLog]...';


GO
BEGIN TRANSACTION;

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

SET XACT_ABORT ON;

CREATE TABLE [Test].[tmp_ms_xx_TestFailiureLog] (
    [FailiureID]      INT           IDENTITY (1, 1) NOT NULL,
    [FlowRunID]       INT           NULL,
    [FlowID]          INT           NULL,
    [FlowDescription] VARCHAR (MAX) NULL,
    [TestID]          INT           NULL,
    [TestDescription] VARCHAR (MAX) NULL,
    [FailDescription] VARCHAR (MAX) NULL,
    [Occurred]        DATETIME      CONSTRAINT [DF_TestTestFailiureLog_Occurred] DEFAULT (getutcdate()) NULL,
    CONSTRAINT [tmp_ms_xx_constraint_PK_TestFailiureLog] PRIMARY KEY CLUSTERED ([FailiureID] ASC)
);

IF EXISTS (SELECT TOP 1 1 
           FROM   [Test].[TestFailiureLog])
    BEGIN
        SET IDENTITY_INSERT [Test].[tmp_ms_xx_TestFailiureLog] ON;
        INSERT INTO [Test].[tmp_ms_xx_TestFailiureLog] ([FailiureID], [FlowRunID], [FlowID], [FlowDescription], [TestID], [TestDescription], [FailDescription], [Occurred])
        SELECT   [FailiureID],
                 [FlowRunID],
                 [FlowID],
                 [FlowDescription],
                 [TestID],
                 [TestDescription],
                 [FailDescription],
                 [Occurred]
        FROM     [Test].[TestFailiureLog]
        ORDER BY [FailiureID] ASC;
        SET IDENTITY_INSERT [Test].[tmp_ms_xx_TestFailiureLog] OFF;
    END

DROP TABLE [Test].[TestFailiureLog];

EXECUTE sp_rename N'[Test].[tmp_ms_xx_TestFailiureLog]', N'TestFailiureLog';

EXECUTE sp_rename N'[Test].[tmp_ms_xx_constraint_PK_TestFailiureLog]', N'PK_TestFailiureLog', N'OBJECT';

COMMIT TRANSACTION;

SET TRANSACTION ISOLATION LEVEL READ COMMITTED;


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
    ON [Test].[tmp_ms_xx_TestResults]([ResultsIndex] ASC);

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
    ON [Test].[Batches] REBUILD WITH(DATA_COMPRESSION = PAGE);


GO
PRINT N'Creating [Test].[Tr_DB_Audit]...';


GO
CREATE TRIGGER [Test].[Tr_DB_Audit] ON [Test].[GuiMap] FOR INSERT, UPDATE, DELETE AS
------------Triger Name and table of triger 
DECLARE @bit INT ,
       @field INT ,
       @maxfield INT ,
       @char INT ,
       @fieldname VARCHAR(128) ,
       @TableName VARCHAR(128) ,
       @PKCols VARCHAR(1000) ,
       @sql VARCHAR(2000), 
       @UpdateDate VARCHAR(21) ,
       @UserName VARCHAR(128) ,
       @Type CHAR(1) ,
       @PKSelect VARCHAR(1000)


--You will need to change @TableName to match the table to be [dbo].[Audit_GuiMap]ed
SELECT @TableName = 'GuiMap'------Monitoring Tabble 

-- date and user
SELECT         @UserName = SYSTEM_USER ,
       @UpdateDate = CONVERT(VARCHAR(8), GETDATE(), 112) 
               + ' ' + CONVERT(VARCHAR(12), GETDATE(), 114)

-- Action
IF EXISTS (SELECT * FROM inserted)
       IF EXISTS (SELECT * FROM deleted)
               SELECT @Type = 'U'
       ELSE
               SELECT @Type = 'I'
ELSE
       SELECT @Type = 'D'

-- get list of columns
SELECT * INTO #ins FROM inserted
SELECT * INTO #del FROM deleted

-- Get primary key columns for full outer join
SELECT @PKCols = COALESCE(@PKCols + ' and', ' on') 
               + ' i.' + c.COLUMN_NAME + ' = d.' + c.COLUMN_NAME
       FROM    INFORMATION_SCHEMA.TABLE_CONSTRAINTS PK ,

              INFORMATION_SCHEMA.KEY_COLUMN_USAGE c
       WHERE   PK.TABLE_NAME = @TableName
       AND     CONSTRAINT_TYPE = 'PRIMARY KEY'
       AND     c.TABLE_NAME = PK.TABLE_NAME
       AND     c.CONSTRAINT_NAME = PK.CONSTRAINT_NAME

-- Get primary key select for insert
SELECT @PKSelect = COALESCE(@PKSelect+'+','') 
       + '''<' + COLUMN_NAME 
       + '=''+convert(varchar(100),
coalesce(i.' + COLUMN_NAME +',d.' + COLUMN_NAME + '))+''>''' 
       FROM    INFORMATION_SCHEMA.TABLE_CONSTRAINTS PK ,
               INFORMATION_SCHEMA.KEY_COLUMN_USAGE c
       WHERE   PK.TABLE_NAME = @TableName
       AND     CONSTRAINT_TYPE = 'PRIMARY KEY'
       AND     c.TABLE_NAME = PK.TABLE_NAME
       AND     c.CONSTRAINT_NAME = PK.CONSTRAINT_NAME

IF @PKCols IS NULL
BEGIN
       RAISERROR('no PK on table %s', 16, -1, @TableName)
       RETURN
END

SELECT         @field = 0, 
       @maxfield = MAX(ORDINAL_POSITION) 
       FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TableName
WHILE @field < @maxfield
BEGIN
       SELECT @field = MIN(ORDINAL_POSITION) 
               FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = @TableName 
               AND ORDINAL_POSITION > @field
       SELECT @bit = (@field - 1 )% 8 + 1
       SELECT @bit = POWER(2,@bit - 1)
       SELECT @char = ((@field - 1) / 8) + 1
       IF SUBSTRING(COLUMNS_UPDATED(),@char, 1) & @bit > 0
                                       OR @Type IN ('I','D')
       BEGIN
               SELECT @fieldname = COLUMN_NAME 
                       FROM INFORMATION_SCHEMA.COLUMNS 
                       WHERE TABLE_NAME = @TableName 
                       AND ORDINAL_POSITION = @field
					   -------------Write name to what table enter audit cahnges
               SELECT @sql = '
insert  dbo.Audit_GuiMap (    Type, 
               TableName, 
               PK, 
               FieldName, 
               OldValue, 
               NewValue, 
               UpdateDate, 
               UserName)
select ''' + @Type + ''',''' 
       + @TableName + ''',' + @PKSelect
       + ',''' + @fieldname + ''''
       + ',convert(varchar(1000),d.' + @fieldname + ')'
       + ',convert(varchar(1000),i.' + @fieldname + ')'
       + ',''' + @UpdateDate + ''''
       + ',''' + @UserName + ''''
       + ' from #ins i full outer join #del d'
       + @PKCols
       + ' where i.' + @fieldname + ' <> d.' + @fieldname 
       + ' or (i.' + @fieldname + ' is null and  d.'
                                + @fieldname
                                + ' is not null)' 
       + ' or (i.' + @fieldname + ' is not null and  d.' 
                                + @fieldname
                                + ' is null)' 
               EXEC (@sql)
       END
END
GO
PRINT N'Creating [Test].[Tr_DB_Audit_GuiScenarioLogic]...';


GO
create TRIGGER [Test].[Tr_DB_Audit_GuiScenarioLogic] ON [Test].[GuiScenarioLogic] FOR INSERT, UPDATE, DELETE AS

DECLARE @bit INT ,
       @field INT ,
       @maxfield INT ,
       @char INT ,
       @fieldname VARCHAR(128) ,
       @TableName VARCHAR(128) ,
       @PKCols VARCHAR(1000) ,
       @sql VARCHAR(2000), 
       @UpdateDate VARCHAR(21) ,
       @UserName VARCHAR(128) ,
       @Type CHAR(1) ,
       @PKSelect VARCHAR(1000)


--You will need to change @TableName to match the table to be Audit_GuiScenarioLogic
SELECT @TableName = 'GuiScenarioLogic'

-- date and user
SELECT         @UserName = SYSTEM_USER ,
       @UpdateDate = CONVERT(VARCHAR(8), GETDATE(), 112) 
               + ' ' + CONVERT(VARCHAR(12), GETDATE(), 114)

-- Action
IF EXISTS (SELECT * FROM inserted)
       IF EXISTS (SELECT * FROM deleted)
               SELECT @Type = 'U'
       ELSE
               SELECT @Type = 'I'
ELSE
       SELECT @Type = 'D'

-- get list of columns
SELECT * INTO #ins FROM inserted
SELECT * INTO #del FROM deleted

-- Get primary key columns for full outer join
SELECT @PKCols = COALESCE(@PKCols + ' and', ' on') 
               + ' i.' + c.COLUMN_NAME + ' = d.' + c.COLUMN_NAME
       FROM    INFORMATION_SCHEMA.TABLE_CONSTRAINTS PK ,

              INFORMATION_SCHEMA.KEY_COLUMN_USAGE c
       WHERE   PK.TABLE_NAME = @TableName
       AND     CONSTRAINT_TYPE = 'PRIMARY KEY'
       AND     c.TABLE_NAME = PK.TABLE_NAME
       AND     c.CONSTRAINT_NAME = PK.CONSTRAINT_NAME

-- Get primary key select for insert
SELECT @PKSelect = COALESCE(@PKSelect+'+','') 
       + '''<' + COLUMN_NAME 
       + '=''+convert(varchar(100),
coalesce(i.' + COLUMN_NAME +',d.' + COLUMN_NAME + '))+''>''' 
       FROM    INFORMATION_SCHEMA.TABLE_CONSTRAINTS PK ,
               INFORMATION_SCHEMA.KEY_COLUMN_USAGE c
       WHERE   PK.TABLE_NAME = @TableName
       AND     CONSTRAINT_TYPE = 'PRIMARY KEY'
       AND     c.TABLE_NAME = PK.TABLE_NAME
       AND     c.CONSTRAINT_NAME = PK.CONSTRAINT_NAME

IF @PKCols IS NULL
BEGIN
       RAISERROR('no PK on table %s', 16, -1, @TableName)
       RETURN
END

SELECT         @field = 0, 
       @maxfield = MAX(ORDINAL_POSITION) 
       FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TableName
WHILE @field < @maxfield
BEGIN
       SELECT @field = MIN(ORDINAL_POSITION) 
               FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = @TableName 
               AND ORDINAL_POSITION > @field
       SELECT @bit = (@field - 1 )% 8 + 1
       SELECT @bit = POWER(2,@bit - 1)
       SELECT @char = ((@field - 1) / 8) + 1
       IF SUBSTRING(COLUMNS_UPDATED(),@char, 1) & @bit > 0
                                       OR @Type IN ('I','D')
       BEGIN
               SELECT @fieldname = COLUMN_NAME 
                       FROM INFORMATION_SCHEMA.COLUMNS 
                       WHERE TABLE_NAME = @TableName 
                       AND ORDINAL_POSITION = @field
               SELECT @sql = '
insert  Audit_GuiScenarioLogic (    Type, 
               TableName, 
               PK, 
               FieldName, 
               OldValue, 
               NewValue, 
               UpdateDate, 
               UserName)
select ''' + @Type + ''',''' 
       + @TableName + ''',' + @PKSelect
       + ',''' + @fieldname + ''''
       + ',convert(varchar(1000),d.' + @fieldname + ')'
       + ',convert(varchar(1000),i.' + @fieldname + ')'
       + ',''' + @UpdateDate + ''''
       + ',''' + @UserName + ''''
       + ' from #ins i full outer join #del d'
       + @PKCols
       + ' where i.' + @fieldname + ' <> d.' + @fieldname 
       + ' or (i.' + @fieldname + ' is null and  d.'
                                + @fieldname
                                + ' is not null)' 
       + ' or (i.' + @fieldname + ' is not null and  d.' 
                                + @fieldname
                                + ' is null)' 
               EXEC (@sql)
       END
END
GO
PRINT N'Creating [Test].[Tr_DB_Audit_GuiTestSteps]...';


GO
create TRIGGER [Test].[Tr_DB_Audit_GuiTestSteps] ON [Test].[GuiTestSteps] FOR INSERT, UPDATE, DELETE AS

DECLARE @bit INT ,
       @field INT ,
       @maxfield INT ,
       @char INT ,
       @fieldname VARCHAR(128) ,
       @TableName VARCHAR(128) ,
       @PKCols VARCHAR(1000) ,
       @sql VARCHAR(2000), 
       @UpdateDate VARCHAR(21) ,
       @UserName VARCHAR(128) ,
       @Type CHAR(1) ,
       @PKSelect VARCHAR(1000)


--You will need to change @TableName to match the table to be [dbo].[Audit_GuiMap]ed
SELECT @TableName = 'GuiTestSteps'

-- date and user
SELECT         @UserName = SYSTEM_USER ,
       @UpdateDate = CONVERT(VARCHAR(8), GETDATE(), 112) 
               + ' ' + CONVERT(VARCHAR(12), GETDATE(), 114)

-- Action
IF EXISTS (SELECT * FROM inserted)
       IF EXISTS (SELECT * FROM deleted)
               SELECT @Type = 'U'
       ELSE
               SELECT @Type = 'I'
ELSE
       SELECT @Type = 'D'

-- get list of columns
SELECT * INTO #ins FROM inserted
SELECT * INTO #del FROM deleted

-- Get primary key columns for full outer join
SELECT @PKCols = COALESCE(@PKCols + ' and', ' on') 
               + ' i.' + c.COLUMN_NAME + ' = d.' + c.COLUMN_NAME
       FROM    INFORMATION_SCHEMA.TABLE_CONSTRAINTS PK ,

              INFORMATION_SCHEMA.KEY_COLUMN_USAGE c
       WHERE   PK.TABLE_NAME = @TableName
       AND     CONSTRAINT_TYPE = 'PRIMARY KEY'
       AND     c.TABLE_NAME = PK.TABLE_NAME
       AND     c.CONSTRAINT_NAME = PK.CONSTRAINT_NAME

-- Get primary key select for insert
SELECT @PKSelect = COALESCE(@PKSelect+'+','') 
       + '''<' + COLUMN_NAME 
       + '=''+convert(varchar(100),
coalesce(i.' + COLUMN_NAME +',d.' + COLUMN_NAME + '))+''>''' 
       FROM    INFORMATION_SCHEMA.TABLE_CONSTRAINTS PK ,
               INFORMATION_SCHEMA.KEY_COLUMN_USAGE c
       WHERE   PK.TABLE_NAME = @TableName
       AND     CONSTRAINT_TYPE = 'PRIMARY KEY'
       AND     c.TABLE_NAME = PK.TABLE_NAME
       AND     c.CONSTRAINT_NAME = PK.CONSTRAINT_NAME

IF @PKCols IS NULL
BEGIN
       RAISERROR('no PK on table %s', 16, -1, @TableName)
       RETURN
END

SELECT         @field = 0, 
       @maxfield = MAX(ORDINAL_POSITION) 
       FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TableName
WHILE @field < @maxfield
BEGIN
       SELECT @field = MIN(ORDINAL_POSITION) 
               FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = @TableName 
               AND ORDINAL_POSITION > @field
       SELECT @bit = (@field - 1 )% 8 + 1
       SELECT @bit = POWER(2,@bit - 1)
       SELECT @char = ((@field - 1) / 8) + 1
       IF SUBSTRING(COLUMNS_UPDATED(),@char, 1) & @bit > 0
                                       OR @Type IN ('I','D')
       BEGIN
               SELECT @fieldname = COLUMN_NAME 
                       FROM INFORMATION_SCHEMA.COLUMNS 
                       WHERE TABLE_NAME = @TableName 
                       AND ORDINAL_POSITION = @field
               SELECT @sql = '
insert  dbo.Audit_GuiTestSteps (    Type, 
               TableName, 
               PK, 
               FieldName, 
               OldValue, 
               NewValue, 
               UpdateDate, 
               UserName)
select ''' + @Type + ''',''' 
       + @TableName + ''',' + @PKSelect
       + ',''' + @fieldname + ''''
       + ',convert(varchar(1000),d.' + @fieldname + ')'
       + ',convert(varchar(1000),i.' + @fieldname + ')'
       + ',''' + @UpdateDate + ''''
       + ',''' + @UserName + ''''
       + ' from #ins i full outer join #del d'
       + @PKCols
       + ' where i.' + @fieldname + ' <> d.' + @fieldname 
       + ' or (i.' + @fieldname + ' is null and  d.'
                                + @fieldname
                                + ' is not null)' 
       + ' or (i.' + @fieldname + ' is not null and  d.' 
                                + @fieldname
                                + ' is null)' 
               EXEC (@sql)
       END
END
GO
PRINT N'Creating Permission...';


GO
GRANT SELECT
    ON OBJECT::[cdc].[fn_cdc_get_net_changes_Test_GuiMap] TO [1]
    AS [cdc];


GO
PRINT N'Update complete.';


GO
