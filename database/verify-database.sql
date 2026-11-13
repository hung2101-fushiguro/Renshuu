SET NOCOUNT ON;
SELECT DB_NAME() AS DatabaseName;
IF OBJECT_ID(N'dbo.__EFMigrationsHistory', N'U') IS NULL
    THROW 51000, 'Missing EF migration history.', 1;

IF EXISTS (
    SELECT MigrationId FROM (VALUES
        (N'20261019064138_InitialCreate'),
        (N'20261023051104_AddReadingPassages'),
        (N'20261113012516_AddIdentityAccounts'),
        (N'20261113020454_AddPracticeMode'),
        (N'20261113055114_AddQuestionClassification'),
        (N'20261113110547_AddLearnerFeatures')
    ) expected(MigrationId)
    EXCEPT SELECT MigrationId FROM dbo.__EFMigrationsHistory
)
    THROW 51001, 'Required migrations are missing.', 1;

IF COL_LENGTH(N'dbo.Questions', N'Chapter') IS NULL
    OR COL_LENGTH(N'dbo.Questions', N'Topic') IS NULL
    OR COL_LENGTH(N'dbo.Questions', N'Difficulty') IS NULL
    OR OBJECT_ID(N'dbo.LearnerProfiles', N'U') IS NULL
    OR OBJECT_ID(N'dbo.SavedQuestions', N'U') IS NULL
    THROW 51002, 'Required classification or learner schema is missing.', 1;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.SavedQuestions')
    AND name = N'IX_SavedQuestions_UserId_QuestionId' AND is_unique = 1)
    THROW 51003, 'Saved-question uniqueness index is missing.', 1;

SELECT MigrationId FROM dbo.__EFMigrationsHistory ORDER BY MigrationId;
SELECT N'Questions' AS Entity, COUNT_BIG(*) AS [RowCount] FROM dbo.Questions
UNION ALL SELECT N'Users', COUNT_BIG(*) FROM dbo.AspNetUsers
UNION ALL SELECT N'Attempts', COUNT_BIG(*) FROM dbo.Attempts
UNION ALL SELECT N'LearnerProfiles', COUNT_BIG(*) FROM dbo.LearnerProfiles
UNION ALL SELECT N'SavedQuestions', COUNT_BIG(*) FROM dbo.SavedQuestions;
PRINT 'Database verification passed.';
