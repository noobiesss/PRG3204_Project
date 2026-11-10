-- Campus Event Management System
-- Schema version: 2.1 | Release date: 2026-11-04
-- Plain SQL Server schema for the Web Forms project.
-- Create and select an empty target database before running this script.
-- One-time installation: this script does not skip existing tables.
-- No application data or EF migration history is included.

SET XACT_ABORT ON;
GO

BEGIN TRY
BEGIN TRANSACTION;
CREATE TABLE [Categories] (
    [CategoryId] int NOT NULL IDENTITY,
    [CategoryName] nvarchar(100) NOT NULL,
    [Description] nvarchar(255) NULL,
    CONSTRAINT [PK_Categories] PRIMARY KEY ([CategoryId])
);

CREATE TABLE [Users] (
    [UserId] int NOT NULL IDENTITY,
    [MatricOrStaffNo] nvarchar(20) NOT NULL,
    [FullName] nvarchar(100) NOT NULL,
    [SchoolEmail] nvarchar(100) NOT NULL,
    [PersonalEmail] nvarchar(100) NULL,
    [PasswordHash] nvarchar(255) NOT NULL,
    [Role] nvarchar(20) NOT NULL DEFAULT N'Student',
    [Gender] nvarchar(10) NULL,
    [DateOfBirth] date NULL,
    [PhoneNumber] nvarchar(20) NULL,
    [Faculty] nvarchar(100) NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [CreatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_Users] PRIMARY KEY ([UserId]),
    CONSTRAINT [CK_Users_Gender] CHECK ([Gender] IS NULL OR [Gender] IN ('Male', 'Female')),
    CONSTRAINT [CK_Users_Role] CHECK ([Role] IN ('Student', 'Staff', 'Admin')),
    CONSTRAINT [CK_Users_SchoolEmail] CHECK ([SchoolEmail] LIKE '%.edu.my')
);

CREATE TABLE [Events] (
    [EventId] int NOT NULL IDENTITY,
    [CategoryId] int NOT NULL,
    [OrganizerId] int NOT NULL,
    [Title] nvarchar(200) NOT NULL,
    [Description] nvarchar(max) NULL,
    [Location] nvarchar(200) NOT NULL,
    [StartDateTime] datetime2 NOT NULL,
    [EndDateTime] datetime2 NOT NULL,
    [Capacity] int NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Events] PRIMARY KEY ([EventId]),
    CONSTRAINT [CK_Events_Capacity] CHECK ([Capacity] > 0),
    CONSTRAINT [CK_Events_Dates] CHECK ([EndDateTime] > [StartDateTime]),
    CONSTRAINT [CK_Events_Status] CHECK ([Status] IN ('Upcoming', 'Ongoing', 'Completed', 'Cancelled')),
    CONSTRAINT [FK_Events_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([CategoryId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Events_Users_OrganizerId] FOREIGN KEY ([OrganizerId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);

CREATE TABLE [Enquiries] (
    [EnquiryId] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [EventId] int NULL,
    [OrganizerId] int NOT NULL,
    [Subject] nvarchar(150) NOT NULL,
    [Message] nvarchar(1000) NOT NULL,
    [Status] nvarchar(20) NOT NULL DEFAULT N'Pending',
    [OrganizerReply] nvarchar(1000) NULL,
    [SubmittedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [RepliedAt] datetime2 NULL,
    CONSTRAINT [PK_Enquiries] PRIMARY KEY ([EnquiryId]),
    CONSTRAINT [CK_Enquiries_Status] CHECK ([Status] IN ('Pending', 'Responded', 'Closed')),
    CONSTRAINT [FK_Enquiries_Events_EventId] FOREIGN KEY ([EventId]) REFERENCES [Events] ([EventId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Enquiries_Users_OrganizerId] FOREIGN KEY ([OrganizerId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Enquiries_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);

CREATE TABLE [EventResources] (
    [ResourceId] int NOT NULL IDENTITY,
    [EventId] int NOT NULL,
    [FileName] nvarchar(255) NOT NULL,
    [FilePath] nvarchar(500) NOT NULL,
    [FileType] nvarchar(50) NOT NULL,
    [UploadedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_EventResources] PRIMARY KEY ([ResourceId]),
    CONSTRAINT [FK_EventResources_Events_EventId] FOREIGN KEY ([EventId]) REFERENCES [Events] ([EventId]) ON DELETE NO ACTION
);

CREATE TABLE [Notifications] (
    [NotificationId] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [EventId] int NULL,
    [Subject] nvarchar(150) NOT NULL,
    [Message] nvarchar(1000) NOT NULL,
    [Channel] nvarchar(10) NOT NULL DEFAULT N'Email',
    [Status] nvarchar(10) NOT NULL DEFAULT N'Pending',
    [CreatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [SentAt] datetime2 NULL,
    CONSTRAINT [PK_Notifications] PRIMARY KEY ([NotificationId]),
    CONSTRAINT [CK_Notifications_Channel] CHECK ([Channel] IN ('Email', 'SMS')),
    CONSTRAINT [CK_Notifications_Status] CHECK ([Status] IN ('Pending', 'Sent', 'Failed')),
    CONSTRAINT [FK_Notifications_Events_EventId] FOREIGN KEY ([EventId]) REFERENCES [Events] ([EventId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Notifications_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);

CREATE TABLE [Registrations] (
    [RegistrationId] int NOT NULL IDENTITY,
    [EventId] int NOT NULL,
    [UserId] int NOT NULL,
    [Status] nvarchar(20) NOT NULL DEFAULT N'Registered',
    [RegisteredAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] datetime2 NULL,
    [CancelledAt] datetime2 NULL,
    [IsAttended] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_Registrations] PRIMARY KEY ([RegistrationId]),
    CONSTRAINT [CK_Registrations_Attendance] CHECK ([IsAttended] = 0 OR [Status] = 'Registered'),
    CONSTRAINT [CK_Registrations_CancelledAt] CHECK (([Status] = 'Registered' AND [CancelledAt] IS NULL) OR ([Status] = 'Cancelled' AND [CancelledAt] IS NOT NULL)),
    CONSTRAINT [CK_Registrations_Status] CHECK ([Status] IN ('Registered', 'Cancelled')),
    CONSTRAINT [FK_Registrations_Events_EventId] FOREIGN KEY ([EventId]) REFERENCES [Events] ([EventId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Registrations_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);

CREATE TABLE [Feedback] (
    [FeedbackId] int NOT NULL IDENTITY,
    [RegistrationId] int NOT NULL,
    [Rating] int NOT NULL,
    [Comments] nvarchar(1000) NULL,
    [WouldRecommend] bit NULL,
    [SubmittedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_Feedback] PRIMARY KEY ([FeedbackId]),
    CONSTRAINT [CK_Feedback_Rating] CHECK ([Rating] BETWEEN 1 AND 5),
    CONSTRAINT [FK_Feedback_Registrations_RegistrationId] FOREIGN KEY ([RegistrationId]) REFERENCES [Registrations] ([RegistrationId]) ON DELETE NO ACTION
);

CREATE TABLE [RegistrationLogs] (
    [LogId] int NOT NULL IDENTITY,
    [RegistrationId] int NOT NULL,
    [UserId] int NOT NULL,
    [Action] nvarchar(20) NOT NULL,
    [OldStatus] nvarchar(20) NULL,
    [NewStatus] nvarchar(20) NOT NULL,
    [OldEventId] int NULL,
    [NewEventId] int NULL,
    [ChangeReason] nvarchar(500) NULL,
    [ChangedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_RegistrationLogs] PRIMARY KEY ([LogId]),
    CONSTRAINT [CK_RegistrationLogs_Action] CHECK ([Action] IN ('Registered', 'Cancelled', 'Reregistered', 'Updated')),
    CONSTRAINT [CK_RegistrationLogs_NewStatus] CHECK ([NewStatus] IN ('Registered', 'Cancelled')),
    CONSTRAINT [CK_RegistrationLogs_OldStatus] CHECK ([OldStatus] IS NULL OR [OldStatus] IN ('Registered', 'Cancelled')),
    CONSTRAINT [FK_RegistrationLogs_Registrations_RegistrationId] FOREIGN KEY ([RegistrationId]) REFERENCES [Registrations] ([RegistrationId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_RegistrationLogs_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);

-- Historical event IDs must reference existing events.
ALTER TABLE [RegistrationLogs] ADD
    CONSTRAINT [FK_RegistrationLogs_Events_OldEventId]
        FOREIGN KEY ([OldEventId]) REFERENCES [Events] ([EventId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_RegistrationLogs_Events_NewEventId]
        FOREIGN KEY ([NewEventId]) REFERENCES [Events] ([EventId]) ON DELETE NO ACTION;

CREATE INDEX [IX_Events_StartDateTime] ON [Events] ([StartDateTime]);

CREATE UNIQUE INDEX [IX_Categories_CategoryName] ON [Categories] ([CategoryName]);

CREATE INDEX [IX_Enquiries_EventId] ON [Enquiries] ([EventId]);

CREATE INDEX [IX_Enquiries_OrganizerId] ON [Enquiries] ([OrganizerId]);

CREATE INDEX [IX_Enquiries_UserId] ON [Enquiries] ([UserId]);

CREATE INDEX [IX_EventResources_EventId] ON [EventResources] ([EventId]);

CREATE INDEX [IX_Events_CategoryId] ON [Events] ([CategoryId]);

CREATE INDEX [IX_Events_OrganizerId] ON [Events] ([OrganizerId]);

CREATE UNIQUE INDEX [IX_Feedback_RegistrationId] ON [Feedback] ([RegistrationId]);

CREATE INDEX [IX_Notifications_EventId] ON [Notifications] ([EventId]);

CREATE INDEX [IX_Notifications_UserId] ON [Notifications] ([UserId]);

CREATE INDEX [IX_RegistrationLogs_RegistrationId] ON [RegistrationLogs] ([RegistrationId]);

CREATE INDEX [IX_RegistrationLogs_UserId] ON [RegistrationLogs] ([UserId]);

CREATE INDEX [IX_Registrations_EventId_Status] ON [Registrations] ([EventId], [Status]);

CREATE UNIQUE INDEX [IX_Registrations_EventId_UserId] ON [Registrations] ([EventId], [UserId]);

CREATE INDEX [IX_Registrations_UserId] ON [Registrations] ([UserId]);

CREATE UNIQUE INDEX [IX_Users_MatricOrStaffNo] ON [Users] ([MatricOrStaffNo]);

CREATE UNIQUE INDEX [IX_Users_SchoolEmail] ON [Users] ([SchoolEmail]);

COMMIT;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
