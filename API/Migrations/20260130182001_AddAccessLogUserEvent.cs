using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class AddAccessLogUserEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop EventType only if it exists
            migrationBuilder.Sql(@"
IF EXISTS(SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[AccessLog]') AND name = N'EventType')
BEGIN
    DECLARE @var nvarchar(max);
    SELECT @var = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AccessLog]') AND [c].[name] = N'EventType');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [AccessLog] DROP CONSTRAINT ' + @var + ';');
    ALTER TABLE [AccessLog] DROP COLUMN [EventType];
END
");

            // Rename UserId to user_id if UserId exists and user_id does not
            migrationBuilder.Sql(@"
IF EXISTS(SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[AccessLog]') AND name = N'UserId')
    AND NOT EXISTS(SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[AccessLog]') AND name = N'user_id')
BEGIN
    EXEC('sp_rename ''AccessLog.UserId'', ''user_id'', ''COLUMN''');
END
");

            // Add user_id column if it doesn't exist (neither as user_id nor as UserId)
            migrationBuilder.Sql(@"
IF NOT EXISTS(SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[AccessLog]') AND name = N'user_id')
    AND NOT EXISTS(SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[AccessLog]') AND name = N'UserId')
BEGIN
    ALTER TABLE [AccessLog] ADD [user_id] INT NULL;
END
");

            // Add event column if not exists
            migrationBuilder.Sql(@"
IF NOT EXISTS(SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[AccessLog]') AND name = N'event')
BEGIN
    ALTER TABLE [AccessLog] ADD [event] NVARCHAR(50) NULL;
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Attempt safe rollback: drop event if exists, rename user_id back to UserId if exists, and add EventType if missing
            migrationBuilder.Sql(@"
IF EXISTS(SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[AccessLog]') AND name = N'event')
BEGIN
    ALTER TABLE [AccessLog] DROP COLUMN [event];
END
");

            migrationBuilder.Sql(@"
IF EXISTS(SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[AccessLog]') AND name = N'user_id')
    AND NOT EXISTS(SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[AccessLog]') AND name = N'UserId')
BEGIN
    EXEC('sp_rename ''AccessLog.user_id'', ''UserId'', ''COLUMN''');
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS(SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[AccessLog]') AND name = N'EventType')
BEGIN
    ALTER TABLE [AccessLog] ADD [EventType] NVARCHAR(MAX) NULL;
END
");
        }
    }
}
