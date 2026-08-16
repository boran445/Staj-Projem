using System;
using System.Configuration;
using System.Data.SqlClient;

namespace DevExtremeMvcApp1.Services
{
    public static class DatabaseSchemaService
    {
        public static void EnsureLatestSchema()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            EnsureDatabaseExists(connectionString);

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                Execute(connection, @"
IF OBJECT_ID(N'[dbo].[CalculationResults]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[CalculationResults] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [AppUserId] INT NULL,
        [ShapeType] NVARCHAR(MAX) NOT NULL,
        [CreatedByUserName] NVARCHAR(MAX) NULL,
        [Param1] FLOAT NOT NULL,
        [Param2] FLOAT NULL,
        [Area] FLOAT NULL,
        [Volume] FLOAT NULL,
        [CreatedDate] DATETIME NOT NULL CONSTRAINT [DF_CalculationResults_CreatedDate] DEFAULT (GETDATE()),
        [CalculationDate] DATETIME NOT NULL CONSTRAINT [DF_CalculationResults_CalculationDate] DEFAULT (GETDATE()),
        CONSTRAINT [PK_dbo.CalculationResults] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END");

                Execute(connection, @"
IF OBJECT_ID(N'[dbo].[AppUsers]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AppUsers] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [UserName] NVARCHAR(100) NOT NULL,
        [PasswordHash] NVARCHAR(MAX) NOT NULL,
        [PasswordSalt] NVARCHAR(MAX) NOT NULL,
        [CreatedDate] DATETIME NOT NULL CONSTRAINT [DF_AppUsers_CreatedDate] DEFAULT (GETDATE()),
        CONSTRAINT [PK_dbo.AppUsers] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END");

                Execute(connection, @"
IF OBJECT_ID(N'[dbo].[AppUsers]', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.AppUsers', N'UserName') IS NULL
    ALTER TABLE [dbo].[AppUsers] ADD [UserName] NVARCHAR(100) NOT NULL CONSTRAINT [DF_AppUsers_UserName] DEFAULT ('');

IF OBJECT_ID(N'[dbo].[AppUsers]', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.AppUsers', N'PasswordHash') IS NULL
    ALTER TABLE [dbo].[AppUsers] ADD [PasswordHash] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_AppUsers_PasswordHash] DEFAULT ('');

IF OBJECT_ID(N'[dbo].[AppUsers]', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.AppUsers', N'PasswordSalt') IS NULL
    ALTER TABLE [dbo].[AppUsers] ADD [PasswordSalt] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_AppUsers_PasswordSalt] DEFAULT ('');

IF OBJECT_ID(N'[dbo].[AppUsers]', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.AppUsers', N'CreatedDate') IS NULL
    ALTER TABLE [dbo].[AppUsers] ADD [CreatedDate] DATETIME NOT NULL CONSTRAINT [DF_AppUsers_CreatedDate] DEFAULT (GETDATE()) WITH VALUES;");

                Execute(connection, @"
IF OBJECT_ID(N'[dbo].[AppUsers]', N'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AppUsers_UserName' AND object_id = OBJECT_ID(N'[dbo].[AppUsers]'))
    CREATE UNIQUE INDEX [IX_AppUsers_UserName] ON [dbo].[AppUsers]([UserName]);");

                Execute(connection, @"
IF OBJECT_ID(N'[dbo].[CalculationResults]', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.CalculationResults', N'AppUserId') IS NULL
    ALTER TABLE [dbo].[CalculationResults] ADD [AppUserId] INT NULL;

IF OBJECT_ID(N'[dbo].[CalculationResults]', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.CalculationResults', N'CreatedByUserName') IS NULL
    ALTER TABLE [dbo].[CalculationResults] ADD [CreatedByUserName] NVARCHAR(MAX) NULL;

IF OBJECT_ID(N'[dbo].[CalculationResults]', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.CalculationResults', N'Param2') IS NULL
    ALTER TABLE [dbo].[CalculationResults] ADD [Param2] FLOAT NULL;

IF OBJECT_ID(N'[dbo].[CalculationResults]', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.CalculationResults', N'CreatedDate') IS NULL
    ALTER TABLE [dbo].[CalculationResults] ADD [CreatedDate] DATETIME NOT NULL CONSTRAINT [DF_CalculationResults_CreatedDate] DEFAULT (GETDATE()) WITH VALUES;

IF OBJECT_ID(N'[dbo].[CalculationResults]', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.CalculationResults', N'CalculationDate') IS NULL
    ALTER TABLE [dbo].[CalculationResults] ADD [CalculationDate] DATETIME NOT NULL CONSTRAINT [DF_CalculationResults_CalculationDate] DEFAULT (GETDATE()) WITH VALUES;

IF OBJECT_ID(N'[dbo].[CalculationResults]', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.CalculationResults', N'Area') IS NOT NULL
    ALTER TABLE [dbo].[CalculationResults] ALTER COLUMN [Area] FLOAT NULL;

IF OBJECT_ID(N'[dbo].[CalculationResults]', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.CalculationResults', N'Volume') IS NOT NULL
    ALTER TABLE [dbo].[CalculationResults] ALTER COLUMN [Volume] FLOAT NULL;");

                Execute(connection, @"
IF OBJECT_ID(N'[dbo].[CalculationResults]', N'U') IS NOT NULL
AND OBJECT_ID(N'[dbo].[AppUsers]', N'U') IS NOT NULL
AND COL_LENGTH(N'dbo.CalculationResults', N'AppUserId') IS NOT NULL
AND COL_LENGTH(N'dbo.CalculationResults', N'CreatedByUserName') IS NOT NULL
BEGIN
    UPDATE result
    SET result.[AppUserId] = appUser.[Id]
    FROM [dbo].[CalculationResults] result
    INNER JOIN [dbo].[AppUsers] appUser ON result.[CreatedByUserName] = appUser.[UserName]
    WHERE result.[AppUserId] IS NULL;
END");

                Execute(connection, @"
IF OBJECT_ID(N'[dbo].[CalculationResults]', N'U') IS NOT NULL
AND COL_LENGTH(N'dbo.CalculationResults', N'AppUserId') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CalculationResults_AppUserId' AND object_id = OBJECT_ID(N'[dbo].[CalculationResults]'))
    CREATE INDEX [IX_CalculationResults_AppUserId] ON [dbo].[CalculationResults]([AppUserId]);");

                Execute(connection, @"
IF OBJECT_ID(N'[dbo].[CalculationResults]', N'U') IS NOT NULL
AND OBJECT_ID(N'[dbo].[AppUsers]', N'U') IS NOT NULL
AND COL_LENGTH(N'dbo.CalculationResults', N'AppUserId') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_dbo.CalculationResults_dbo.AppUsers_AppUserId')
AND NOT EXISTS (
    SELECT 1
    FROM [dbo].[CalculationResults] result
    WHERE result.[AppUserId] IS NOT NULL
    AND NOT EXISTS (SELECT 1 FROM [dbo].[AppUsers] appUser WHERE appUser.[Id] = result.[AppUserId])
)
BEGIN
    ALTER TABLE [dbo].[CalculationResults]
    ADD CONSTRAINT [FK_dbo.CalculationResults_dbo.AppUsers_AppUserId]
    FOREIGN KEY ([AppUserId]) REFERENCES [dbo].[AppUsers]([Id]);
END");
            }
        }

        private static void Execute(SqlConnection connection, string sql)
        {
            using (var command = new SqlCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        private static void EnsureDatabaseExists(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            string databaseName = builder.InitialCatalog;

            if (string.IsNullOrWhiteSpace(databaseName))
            {
                return;
            }

            builder.InitialCatalog = "master";

            using (var connection = new SqlConnection(builder.ConnectionString))
            {
                connection.Open();

                using (var command = new SqlCommand("SELECT DB_ID(@databaseName)", connection))
                {
                    command.Parameters.AddWithValue("@databaseName", databaseName);
                    object result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        return;
                    }
                }

                Execute(connection, "CREATE DATABASE " + QuoteName(databaseName));
            }
        }

        private static string QuoteName(string value)
        {
            return "[" + value.Replace("]", "]]") + "]";
        }
    }
}
