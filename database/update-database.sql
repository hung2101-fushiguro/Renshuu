IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019064138_InitialCreate'
)
BEGIN
    CREATE TABLE [Subjects] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        CONSTRAINT [PK_Subjects] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019064138_InitialCreate'
)
BEGIN
    CREATE TABLE [Attempts] (
        [Id] int NOT NULL IDENTITY,
        [SubjectId] int NOT NULL,
        [StartedAt] datetime2 NOT NULL,
        [SubmittedAt] datetime2 NULL,
        [TimeLimitMinutes] int NULL,
        [TotalQuestions] int NOT NULL,
        [CorrectCount] int NOT NULL,
        CONSTRAINT [PK_Attempts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Attempts_Subjects_SubjectId] FOREIGN KEY ([SubjectId]) REFERENCES [Subjects] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019064138_InitialCreate'
)
BEGIN
    CREATE TABLE [Questions] (
        [Id] int NOT NULL IDENTITY,
        [SubjectId] int NOT NULL,
        [Content] nvarchar(2000) NOT NULL,
        [OptionA] nvarchar(500) NOT NULL,
        [OptionB] nvarchar(500) NOT NULL,
        [OptionC] nvarchar(500) NOT NULL,
        [OptionD] nvarchar(500) NOT NULL,
        [CorrectOption] nvarchar(1) NOT NULL,
        [Explanation] nvarchar(2000) NULL,
        CONSTRAINT [PK_Questions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Questions_Subjects_SubjectId] FOREIGN KEY ([SubjectId]) REFERENCES [Subjects] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019064138_InitialCreate'
)
BEGIN
    CREATE TABLE [AttemptAnswers] (
        [Id] int NOT NULL IDENTITY,
        [AttemptId] int NOT NULL,
        [QuestionId] int NOT NULL,
        [SelectedOption] nvarchar(1) NULL,
        [IsCorrect] bit NOT NULL,
        CONSTRAINT [PK_AttemptAnswers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AttemptAnswers_Attempts_AttemptId] FOREIGN KEY ([AttemptId]) REFERENCES [Attempts] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AttemptAnswers_Questions_QuestionId] FOREIGN KEY ([QuestionId]) REFERENCES [Questions] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019064138_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AttemptAnswers_AttemptId] ON [AttemptAnswers] ([AttemptId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019064138_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AttemptAnswers_QuestionId_Id] ON [AttemptAnswers] ([QuestionId], [Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019064138_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Attempts_SubjectId_StartedAt] ON [Attempts] ([SubjectId], [StartedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019064138_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Questions_SubjectId] ON [Questions] ([SubjectId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019064138_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261019064138_InitialCreate', N'8.0.32');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261023051104_AddReadingPassages'
)
BEGIN
    ALTER TABLE [Questions] ADD [OptionStyle] nvarchar(4) NOT NULL DEFAULT N'ABCD';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261023051104_AddReadingPassages'
)
BEGIN
    ALTER TABLE [Questions] ADD [PassageId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261023051104_AddReadingPassages'
)
BEGIN
    CREATE TABLE [ReadingPassages] (
        [Id] int NOT NULL IDENTITY,
        [SubjectId] int NOT NULL,
        [Title] nvarchar(300) NULL,
        [Content] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_ReadingPassages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ReadingPassages_Subjects_SubjectId] FOREIGN KEY ([SubjectId]) REFERENCES [Subjects] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261023051104_AddReadingPassages'
)
BEGIN
    CREATE INDEX [IX_Questions_PassageId] ON [Questions] ([PassageId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261023051104_AddReadingPassages'
)
BEGIN
    CREATE INDEX [IX_ReadingPassages_SubjectId] ON [ReadingPassages] ([SubjectId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261023051104_AddReadingPassages'
)
BEGIN
    ALTER TABLE [Questions] ADD CONSTRAINT [FK_Questions_ReadingPassages_PassageId] FOREIGN KEY ([PassageId]) REFERENCES [ReadingPassages] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261023051104_AddReadingPassages'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261023051104_AddReadingPassages', N'8.0.32');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110061027_AddSubjectFolders'
)
BEGIN
    ALTER TABLE [Subjects] ADD [FolderId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110061027_AddSubjectFolders'
)
BEGIN
    CREATE TABLE [SubjectFolders] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        CONSTRAINT [PK_SubjectFolders] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110061027_AddSubjectFolders'
)
BEGIN
    CREATE INDEX [IX_Subjects_FolderId] ON [Subjects] ([FolderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110061027_AddSubjectFolders'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SubjectFolders_Name] ON [SubjectFolders] ([Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110061027_AddSubjectFolders'
)
BEGIN
    ALTER TABLE [Subjects] ADD CONSTRAINT [FK_Subjects_SubjectFolders_FolderId] FOREIGN KEY ([FolderId]) REFERENCES [SubjectFolders] ([Id]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110061027_AddSubjectFolders'
)
BEGIN
    IF EXISTS (SELECT 1 FROM [Subjects] WHERE [Name] LIKE N'JLPT-N3%')
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM [SubjectFolders] WHERE [Name] = N'JLPT N3')
        BEGIN
            INSERT INTO [SubjectFolders] ([Name]) VALUES (N'JLPT N3');
        END;

        UPDATE [Subjects]
        SET [FolderId] = (SELECT [Id] FROM [SubjectFolders] WHERE [Name] = N'JLPT N3')
        WHERE [Name] LIKE N'JLPT-N3%';
    END;

    IF EXISTS (SELECT 1 FROM [Subjects] WHERE [Name] LIKE N'PRN222%')
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM [SubjectFolders] WHERE [Name] = N'PRN222')
        BEGIN
            INSERT INTO [SubjectFolders] ([Name]) VALUES (N'PRN222');
        END;

        UPDATE [Subjects]
        SET [FolderId] = (SELECT [Id] FROM [SubjectFolders] WHERE [Name] = N'PRN222')
        WHERE [Name] LIKE N'PRN222%';
    END;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110061027_AddSubjectFolders'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261110061027_AddSubjectFolders', N'8.0.32');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110092823_SaveAttemptDrafts'
)
BEGIN
    ALTER TABLE [Attempts] ADD [DraftSavedAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110092823_SaveAttemptDrafts'
)
BEGIN
    CREATE INDEX [IX_Attempts_SubmittedAt_Id] ON [Attempts] ([SubmittedAt], [Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110092823_SaveAttemptDrafts'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261110092823_SaveAttemptDrafts', N'8.0.32');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114025237_AddIdentityAccountsAndOwnership'
)
BEGIN
    DROP INDEX [IX_SubjectFolders_Name] ON [SubjectFolders];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114025237_AddIdentityAccountsAndOwnership'
)
BEGIN
    ALTER TABLE [Subjects] ADD [CreatedAt] datetime2 NOT NULL DEFAULT (GETDATE());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114025237_AddIdentityAccountsAndOwnership'
)
BEGIN
    ALTER TABLE [Subjects] ADD [OwnerId] nvarchar(450) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114025237_AddIdentityAccountsAndOwnership'
)
BEGIN
    ALTER TABLE [SubjectFolders] ADD [CreatedAt] datetime2 NOT NULL DEFAULT (GETDATE());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114025237_AddIdentityAccountsAndOwnership'
)
BEGIN
    ALTER TABLE [SubjectFolders] ADD [OwnerId] nvarchar(450) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114025237_AddIdentityAccountsAndOwnership'
)
BEGIN
    ALTER TABLE [Attempts] ADD [OwnerId] nvarchar(450) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114025237_AddIdentityAccountsAndOwnership'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114025237_AddIdentityAccountsAndOwnership'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [DisplayName] nvarchar(120) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114025237_AddIdentityAccountsAndOwnership'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114025237_AddIdentityAccountsAndOwnership'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114025237_AddIdentityAccountsAndOwnership'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114025237_AddIdentityAccountsAndOwnership'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114025237_AddIdentityAccountsAndOwnership'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114025237_AddIdentityAccountsAndOwnership'
)
BEGIN
    CREATE INDEX [IX_Subjects_OwnerId] ON [Subjects] ([OwnerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114025237_AddIdentityAccountsAndOwnership'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_SubjectFolders_OwnerId_Name] ON [SubjectFolders] ([OwnerId], [Name]) WHERE [OwnerId] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114025237_AddIdentityAccountsAndOwnership'
)
BEGIN
    CREATE INDEX [IX_Attempts_OwnerId] ON [Attempts] ([OwnerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114025237_AddIdentityAccountsAndOwnership'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114025237_AddIdentityAccountsAndOwnership'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114025237_AddIdentityAccountsAndOwnership'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114025237_AddIdentityAccountsAndOwnership'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114025237_AddIdentityAccountsAndOwnership'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114025237_AddIdentityAccountsAndOwnership'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114025237_AddIdentityAccountsAndOwnership'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114025237_AddIdentityAccountsAndOwnership'
)
BEGIN
    ALTER TABLE [Attempts] ADD CONSTRAINT [FK_Attempts_AspNetUsers_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114025237_AddIdentityAccountsAndOwnership'
)
BEGIN
    ALTER TABLE [SubjectFolders] ADD CONSTRAINT [FK_SubjectFolders_AspNetUsers_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114025237_AddIdentityAccountsAndOwnership'
)
BEGIN
    ALTER TABLE [Subjects] ADD CONSTRAINT [FK_Subjects_AspNetUsers_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114025237_AddIdentityAccountsAndOwnership'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261114025237_AddIdentityAccountsAndOwnership', N'8.0.32');
END;
GO

COMMIT;
GO

