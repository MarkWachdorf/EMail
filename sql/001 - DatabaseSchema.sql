

-- Email Microservice Database Schema
-- Following API Standards with audit fields and error logging

-- =============================================
-- Email Messages Table (Main logging table)
-- =============================================
CREATE TABLE [dbo].[EmailMessages] (
    [Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [CompanyCode] NVARCHAR(50) NOT NULL,
    [ApplicationCode] NVARCHAR(50) NOT NULL,
    [FromAddress] NVARCHAR(255) NOT NULL,
    [ToAddresses] NVARCHAR(MAX) NOT NULL,
    [CcAddresses] NVARCHAR(MAX) NULL,
    [BccAddresses] NVARCHAR(MAX) NULL,
    [Subject] NVARCHAR(500) NOT NULL,
    [Body] NVARCHAR(MAX) NOT NULL,
    [Header] NVARCHAR(MAX) NULL,
    [Footer] NVARCHAR(MAX) NULL,
    [MessageSeparator] NVARCHAR(100) NULL,
    [ImportanceFlag] NVARCHAR(20) NOT NULL DEFAULT 'Normal',
    [HtmlFlag] BIT NOT NULL DEFAULT 0,
    [SplitFlag] BIT NOT NULL DEFAULT 0,
    [Status] NVARCHAR(20) NOT NULL DEFAULT 'Pending', -- Pending, Sent, Failed, Cached
    [ErrorMessage] NVARCHAR(MAX) NULL,
    [RetryCount] INT NOT NULL DEFAULT 0,
    [MaxRetries] INT NOT NULL DEFAULT 3,
    [SentAt] DATETIME2 NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [RowVersion] TIMESTAMP NOT NULL,
    
    -- Audit fields (API Standards requirement)
    [CreatedBy] NVARCHAR(100) NULL,
    [UpdatedBy] NVARCHAR(100) NULL,
    [CreatedFrom] NVARCHAR(100) NULL, -- IP address or service name
    [UpdatedFrom] NVARCHAR(100) NULL
);

-- =============================================
-- Email Cache Table (For SendmailCached functionality)
-- =============================================
CREATE TABLE [dbo].[EmailCache] (
    [Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [CacheKey] NVARCHAR(500) NOT NULL UNIQUE, -- Hash of To+Cc+Bcc+Subject
    [CompanyCode] NVARCHAR(50) NOT NULL,
    [ApplicationCode] NVARCHAR(50) NOT NULL,
    [FromAddress] NVARCHAR(255) NOT NULL,
    [ToAddresses] NVARCHAR(MAX) NOT NULL,
    [CcAddresses] NVARCHAR(MAX) NULL,
    [BccAddresses] NVARCHAR(MAX) NULL,
    [Subject] NVARCHAR(500) NOT NULL,
    [Header] NVARCHAR(MAX) NULL,
    [Footer] NVARCHAR(MAX) NULL,
    [MessageSeparator] NVARCHAR(100) NULL,
    [ImportanceFlag] NVARCHAR(20) NOT NULL DEFAULT 'Normal',
    [HtmlFlag] BIT NOT NULL DEFAULT 0,
    [ConsolidatedBody] NVARCHAR(MAX) NOT NULL,
    [MessageCount] INT NOT NULL DEFAULT 1,
    [ExpiresAt] DATETIME2 NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [RowVersion] TIMESTAMP NOT NULL,
    
    -- Audit fields
    [CreatedBy] NVARCHAR(100) NULL,
    [UpdatedBy] NVARCHAR(100) NULL
);

-- =============================================
-- Email History Table (API Standards requirement)
-- =============================================
CREATE TABLE [dbo].[EmailHistory] (
    [Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [EmailMessageId] BIGINT NOT NULL,
    [Action] NVARCHAR(50) NOT NULL, -- Created, Sent, Failed, Retried, Cached, Deleted
    [Status] NVARCHAR(20) NOT NULL,
    [ErrorMessage] NVARCHAR(MAX) NULL,
    [RetryCount] INT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    -- Audit fields (API Standards: who, when, how, what)
    [Who] NVARCHAR(100) NULL, -- User or service that performed the action
    [SentWhen] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [How] NVARCHAR(100) NULL, -- Method or endpoint used
    [What] NVARCHAR(MAX) NULL, -- Description of what was done
    
    CONSTRAINT [FK_EmailHistory_EmailMessages] FOREIGN KEY ([EmailMessageId]) 
        REFERENCES [dbo].[EmailMessages]([Id])
);

-- =============================================
-- Error Logs Table (API Standards requirement)
-- =============================================
CREATE TABLE [dbo].[log_errors] (
    [Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Timestamp] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [Level] NVARCHAR(20) NOT NULL, -- Error, Warning, Info, Debug
    [Message] NVARCHAR(MAX) NOT NULL,
    [Exception] NVARCHAR(MAX) NULL,
    [StackTrace] NVARCHAR(MAX) NULL,
    [RequestId] NVARCHAR(100) NULL,
    [CorrelationId] NVARCHAR(100) NULL,
    [UserId] NVARCHAR(100) NULL,
    [HttpMethod] NVARCHAR(10) NULL,
    [HttpPath] NVARCHAR(500) NULL,
    [StatusCode] INT NULL,
    [ExecutionTime] INT NULL, -- milliseconds
    [CompanyCode] NVARCHAR(50) NULL,
    [ApplicationCode] NVARCHAR(50) NULL,
    [EmailMessageId] BIGINT NULL,
    
    CONSTRAINT [FK_log_errors_EmailMessages] FOREIGN KEY ([EmailMessageId]) 
        REFERENCES [dbo].[EmailMessages]([Id])
);

-- =============================================
-- Email Templates Table (Optional - for future use)
-- =============================================
CREATE TABLE [dbo].[EmailTemplates] (
    [Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [CompanyCode] NVARCHAR(50) NOT NULL,
    [ApplicationCode] NVARCHAR(50) NOT NULL,
    [TemplateName] NVARCHAR(100) NOT NULL,
    [Subject] NVARCHAR(500) NOT NULL,
    [Body] NVARCHAR(MAX) NOT NULL,
    [Header] NVARCHAR(MAX) NULL,
    [Footer] NVARCHAR(MAX) NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [RowVersion] TIMESTAMP NOT NULL,
    
    -- Audit fields
    [CreatedBy] NVARCHAR(100) NULL,
    [UpdatedBy] NVARCHAR(100) NULL,
    
    CONSTRAINT [UQ_EmailTemplates_Company_App_Name] UNIQUE ([CompanyCode], [ApplicationCode], [TemplateName])
);

-- =============================================
-- Indexes for Performance
-- =============================================

-- EmailMessages indexes
CREATE INDEX [IX_EmailMessages_CompanyCode] ON [dbo].[EmailMessages]([CompanyCode]);
CREATE INDEX [IX_EmailMessages_ApplicationCode] ON [dbo].[EmailMessages]([ApplicationCode]);
CREATE INDEX [IX_EmailMessages_Status] ON [dbo].[EmailMessages]([Status]);
CREATE INDEX [IX_EmailMessages_CreatedAt] ON [dbo].[EmailMessages]([CreatedAt]);
CREATE INDEX [IX_EmailMessages_SentAt] ON [dbo].[EmailMessages]([SentAt]);
CREATE INDEX [IX_EmailMessages_Company_App_Status] ON [dbo].[EmailMessages]([CompanyCode], [ApplicationCode], [Status]);

-- EmailCache indexes
CREATE INDEX [IX_EmailCache_ExpiresAt] ON [dbo].[EmailCache]([ExpiresAt]);
CREATE INDEX [IX_EmailCache_Company_App] ON [dbo].[EmailCache]([CompanyCode], [ApplicationCode]);

-- EmailHistory indexes
CREATE INDEX [IX_EmailHistory_EmailMessageId] ON [dbo].[EmailHistory]([EmailMessageId]);
CREATE INDEX [IX_EmailHistory_CreatedAt] ON [dbo].[EmailHistory]([CreatedAt]);
CREATE INDEX [IX_EmailHistory_Action] ON [dbo].[EmailHistory]([Action]);

-- Error logs indexes
CREATE INDEX [IX_log_errors_Timestamp] ON [dbo].[log_errors]([Timestamp]);
CREATE INDEX [IX_log_errors_Level] ON [dbo].[log_errors]([Level]);
CREATE INDEX [IX_log_errors_CompanyCode] ON [dbo].[log_errors]([CompanyCode]);
CREATE INDEX [IX_log_errors_RequestId] ON [dbo].[log_errors]([RequestId]);
go
-- =============================================
-- Triggers for History Tracking (API Standards)
-- =============================================

-- Trigger to automatically log changes to EmailMessages
CREATE TRIGGER [TR_EmailMessages_History]
ON [dbo].[EmailMessages]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Log INSERT operations
    IF EXISTS (SELECT * FROM INSERTED) AND NOT EXISTS (SELECT * FROM DELETED)
    BEGIN
        INSERT INTO [dbo].[EmailHistory] ([EmailMessageId], [Action], [Status], [Who], [How], [What])
        SELECT 
            i.[Id],
            'Created',
            i.[Status],
            i.[CreatedBy],
            'API',
            'Email message created'
        FROM INSERTED i;
    END
    
    -- Log UPDATE operations
    IF EXISTS (SELECT * FROM INSERTED) AND EXISTS (SELECT * FROM DELETED)
    BEGIN
        INSERT INTO [dbo].[EmailHistory] ([EmailMessageId], [Action], [Status], [Who], [How], [What])
        SELECT 
            i.[Id],
            'Updated',
            i.[Status],
            i.[UpdatedBy],
            'API',
            'Email message updated'
        FROM INSERTED i
        INNER JOIN DELETED d ON i.[Id] = d.[Id]
        WHERE i.[Status] != d.[Status] OR i.[RetryCount] != d.[RetryCount];
    END
    
    -- Log DELETE operations (soft delete)
    IF EXISTS (SELECT * FROM DELETED) AND NOT EXISTS (SELECT * FROM INSERTED)
    BEGIN
        INSERT INTO [dbo].[EmailHistory] ([EmailMessageId], [Action], [Status], [Who], [How], [What])
        SELECT 
            d.[Id],
            'Deleted',
            'Deleted',
            d.[UpdatedBy],
            'API',
            'Email message soft deleted'
        FROM DELETED d;
    END
END;

-- =============================================
-- Stored Procedures for Common Operations
-- =============================================

go

-- Get unsent messages for a company/application
CREATE PROCEDURE [dbo].[sp_GetUnsentMessages]
    @CompanyCode NVARCHAR(50),
    @ApplicationCode NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [Id],
        [CompanyCode],
        [ApplicationCode],
        [FromAddress],
        [ToAddresses],
        [CcAddresses],
        [BccAddresses],
        [Subject],
        [Body],
        [Status],
        [ErrorMessage],
        [RetryCount],
        [MaxRetries],
        [CreatedAt]
    FROM [dbo].[EmailMessages]
    WHERE [CompanyCode] = @CompanyCode
        AND (@ApplicationCode IS NULL OR [ApplicationCode] LIKE @ApplicationCode)
        AND [Status] IN ('Failed', 'Pending')
        AND [IsDeleted] = 0
        AND [RetryCount] < [MaxRetries]
    ORDER BY [CreatedAt] ASC;
END;

go
-- Clean up expired cache entries
CREATE PROCEDURE [dbo].[sp_CleanupExpiredCache]
AS
BEGIN
    SET NOCOUNT ON;
    
    DELETE FROM [dbo].[EmailCache]
    WHERE [ExpiresAt] < GETUTCDATE();
END;

-- =============================================
-- Views for Common Queries
-- =============================================
go

-- View for email statistics
CREATE VIEW [dbo].[vw_EmailStatistics] AS
SELECT 
    [CompanyCode],
    [ApplicationCode],
    [Status],
    COUNT(*) as [Count],
    MIN([CreatedAt]) as [FirstEmail],
    MAX([CreatedAt]) as [LastEmail]
FROM [dbo].[EmailMessages]
WHERE [IsDeleted] = 0
GROUP BY [CompanyCode], [ApplicationCode], [Status];

go
-- View for failed emails with retry info
CREATE VIEW [dbo].[vw_FailedEmails] AS
SELECT 
    [Id],
    [CompanyCode],
    [ApplicationCode],
    [FromAddress],
    [ToAddresses],
    [Subject],
    [Status],
    [ErrorMessage],
    [RetryCount],
    [MaxRetries],
    [CreatedAt],
    [UpdatedAt]
FROM [dbo].[EmailMessages]
WHERE [Status] = 'Failed'
    AND [IsDeleted] = 0
    AND [RetryCount] < [MaxRetries];
go
