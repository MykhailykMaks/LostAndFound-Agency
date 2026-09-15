using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LostAndFoundAgency.Migrations
{
    public partial class StrengthenDatabaseModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE [Persons]
SET
    [FullName] = LEFT(COALESCE(NULLIF(LTRIM(RTRIM([FullName])), N''), CONCAT(N'Клієнт ', [PersonId])), 120),
    [Login] = LEFT(COALESCE(NULLIF(LTRIM(RTRIM([Login])), N''), CONCAT(N'client_', [PersonId])), 64),
    [PasswordHash] = LEFT(COALESCE(NULLIF(LTRIM(RTRIM([PasswordHash])), N''), N'change_me'), 256),
    [Phone] = LEFT([Phone], 32),
    [Email] = LEFT([Email], 255),
    [DocumentNumber] = LEFT([DocumentNumber], 64),
    [Address] = LEFT([Address], 250);

WITH duplicate_persons AS (
    SELECT [PersonId], ROW_NUMBER() OVER (PARTITION BY [Login] ORDER BY [PersonId]) AS [RowNumber]
    FROM [Persons]
)
UPDATE p
SET [Login] = LEFT(CONCAT(p.[Login], N'_', p.[PersonId]), 64)
FROM [Persons] p
INNER JOIN duplicate_persons d ON d.[PersonId] = p.[PersonId]
WHERE d.[RowNumber] > 1;

UPDATE [Employees]
SET
    [FullName] = LEFT(COALESCE(NULLIF(LTRIM(RTRIM([FullName])), N''), CONCAT(N'Співробітник ', [EmployeeId])), 120),
    [Login] = LEFT(COALESCE(NULLIF(LTRIM(RTRIM([Login])), N''), CONCAT(N'employee_', [EmployeeId])), 64),
    [PasswordHash] = LEFT(COALESCE(NULLIF(LTRIM(RTRIM([PasswordHash])), N''), N'change_me'), 256),
    [Position] = LEFT(COALESCE(NULLIF(LTRIM(RTRIM([Position])), N''), N'Співробітник'), 80),
    [Phone] = LEFT([Phone], 32);

WITH duplicate_employees AS (
    SELECT [EmployeeId], ROW_NUMBER() OVER (PARTITION BY [Login] ORDER BY [EmployeeId]) AS [RowNumber]
    FROM [Employees]
)
UPDATE e
SET [Login] = LEFT(CONCAT(e.[Login], N'_', e.[EmployeeId]), 64)
FROM [Employees] e
INNER JOIN duplicate_employees d ON d.[EmployeeId] = e.[EmployeeId]
WHERE d.[RowNumber] > 1;

UPDATE [Categories]
SET
    [CategoryName] = LEFT(COALESCE(NULLIF(LTRIM(RTRIM([CategoryName])), N''), CONCAT(N'Категорія ', [CategoryId])), 80),
    [Description] = LEFT([Description], 500);

WITH duplicate_categories AS (
    SELECT [CategoryId], ROW_NUMBER() OVER (PARTITION BY [CategoryName] ORDER BY [CategoryId]) AS [RowNumber]
    FROM [Categories]
)
UPDATE c
SET [CategoryName] = LEFT(CONCAT(c.[CategoryName], N' ', c.[CategoryId]), 80)
FROM [Categories] c
INNER JOIN duplicate_categories d ON d.[CategoryId] = c.[CategoryId]
WHERE d.[RowNumber] > 1;

UPDATE [Locations]
SET
    [LocationName] = LEFT(COALESCE(NULLIF(LTRIM(RTRIM([LocationName])), N''), CONCAT(N'Локація ', [LocationId])), 120),
    [Address] = LEFT([Address], 250),
    [Description] = LEFT([Description], 500);

WITH duplicate_locations AS (
    SELECT [LocationId], ROW_NUMBER() OVER (PARTITION BY [LocationName] ORDER BY [LocationId]) AS [RowNumber]
    FROM [Locations]
)
UPDATE l
SET [LocationName] = LEFT(CONCAT(l.[LocationName], N' ', l.[LocationId]), 120)
FROM [Locations] l
INNER JOIN duplicate_locations d ON d.[LocationId] = l.[LocationId]
WHERE d.[RowNumber] > 1;

UPDATE [LostRequests]
SET
    [ItemName] = LEFT(COALESCE(NULLIF(LTRIM(RTRIM([ItemName])), N''), CONCAT(N'Втрачена річ ', [LostRequestId])), 120),
    [Description] = LEFT([Description], 1000),
    [Color] = LEFT([Color], 50),
    [Brand] = LEFT([Brand], 80),
    [Status] = CASE
        WHEN [Status] IN (N'Очікує обробки', N'Закрито (Знайдено)', N'Відхилено') THEN [Status]
        ELSE N'Очікує обробки'
    END,
    [RequestDate] = CASE WHEN [RequestDate] < '19000101' THEN SYSUTCDATETIME() ELSE [RequestDate] END,
    [DateLost] = CASE WHEN [DateLost] < '19000101' THEN SYSUTCDATETIME() ELSE [DateLost] END;

UPDATE [FoundItems]
SET
    [ItemName] = LEFT(COALESCE(NULLIF(LTRIM(RTRIM([ItemName])), N''), CONCAT(N'Знайдена річ ', [FoundItemId])), 120),
    [Description] = LEFT([Description], 1000),
    [Color] = LEFT([Color], 50),
    [Brand] = LEFT([Brand], 80),
    [StorageLocation] = LEFT([StorageLocation], 160),
    [Status] = CASE
        WHEN [Status] IN (N'Очікує обробки', N'Закрито (Повернуто)', N'Відхилено') THEN [Status]
        ELSE N'Очікує обробки'
    END,
    [PhotoPath] = LEFT([PhotoPath], 500),
    [DateFound] = CASE WHEN [DateFound] < '19000101' THEN SYSUTCDATETIME() ELSE [DateFound] END;

UPDATE [Matches]
SET
    [MatchPercent] = CASE
        WHEN [MatchPercent] < 0 THEN 0
        WHEN [MatchPercent] > 100 THEN 100
        ELSE [MatchPercent]
    END,
    [MatchStatus] = CASE
        WHEN [MatchStatus] IN (N'Потенційний', N'Підтверджено', N'Відхилено') THEN [MatchStatus]
        ELSE N'Потенційний'
    END,
    [Comment] = LEFT([Comment], 500),
    [CreatedAt] = CASE WHEN [CreatedAt] < '19000101' THEN SYSUTCDATETIME() ELSE [CreatedAt] END;

WITH duplicate_matches AS (
    SELECT [MatchId], ROW_NUMBER() OVER (PARTITION BY [FoundItemId], [LostRequestId] ORDER BY [MatchId]) AS [RowNumber]
    FROM [Matches]
)
DELETE FROM duplicate_matches WHERE [RowNumber] > 1;

DELETE r
FROM [Returns] r
WHERE NOT EXISTS (SELECT 1 FROM [FoundItems] f WHERE f.[FoundItemId] = r.[FoundItemId])
   OR NOT EXISTS (SELECT 1 FROM [Persons] p WHERE p.[PersonId] = r.[PersonId])
   OR NOT EXISTS (SELECT 1 FROM [Employees] e WHERE e.[EmployeeId] = r.[EmployeeId]);

UPDATE [Returns]
SET
    [DocumentNumber] = LEFT([DocumentNumber], 64),
    [Comment] = LEFT([Comment], 500),
    [ReturnDate] = CASE WHEN [ReturnDate] < '19000101' THEN SYSUTCDATETIME() ELSE [ReturnDate] END;

WITH duplicate_returns AS (
    SELECT [ReturnId], ROW_NUMBER() OVER (PARTITION BY [FoundItemId] ORDER BY [ReturnId]) AS [RowNumber]
    FROM [Returns]
)
DELETE FROM duplicate_returns WHERE [RowNumber] > 1;
");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Persons",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Login",
                table: "Persons",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Persons",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Persons",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Persons",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DocumentNumber",
                table: "Persons",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Persons",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Employees",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Login",
                table: "Employees",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Employees",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Position",
                table: "Employees",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Employees",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CategoryName",
                table: "Categories",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Categories",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LocationName",
                table: "Locations",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Locations",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Locations",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ItemName",
                table: "LostRequests",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "LostRequests",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Color",
                table: "LostRequests",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Brand",
                table: "LostRequests",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "LostRequests",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "Очікує обробки",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "RequestDate",
                table: "LostRequests",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "ItemName",
                table: "FoundItems",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "FoundItems",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Color",
                table: "FoundItems",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Brand",
                table: "FoundItems",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StorageLocation",
                table: "FoundItems",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "FoundItems",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "Очікує обробки",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PhotoPath",
                table: "FoundItems",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MatchStatus",
                table: "Matches",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "Потенційний",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                table: "Matches",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Matches",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "DocumentNumber",
                table: "Returns",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                table: "Returns",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ReturnDate",
                table: "Returns",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.CreateIndex(name: "IX_Persons_Login", table: "Persons", column: "Login", unique: true);
            migrationBuilder.CreateIndex(name: "IX_Employees_Login", table: "Employees", column: "Login", unique: true);
            migrationBuilder.CreateIndex(name: "IX_Categories_CategoryName", table: "Categories", column: "CategoryName", unique: true);
            migrationBuilder.CreateIndex(name: "IX_Locations_LocationName", table: "Locations", column: "LocationName", unique: true);

            migrationBuilder.CreateIndex(name: "IX_LostRequests_Status", table: "LostRequests", column: "Status");
            migrationBuilder.CreateIndex(name: "IX_LostRequests_DateLost", table: "LostRequests", column: "DateLost");
            migrationBuilder.CreateIndex(name: "IX_LostRequests_CategoryId_Status", table: "LostRequests", columns: new[] { "CategoryId", "Status" });

            migrationBuilder.CreateIndex(name: "IX_FoundItems_Status", table: "FoundItems", column: "Status");
            migrationBuilder.CreateIndex(name: "IX_FoundItems_DateFound", table: "FoundItems", column: "DateFound");
            migrationBuilder.CreateIndex(name: "IX_FoundItems_CategoryId_Status", table: "FoundItems", columns: new[] { "CategoryId", "Status" });

            migrationBuilder.CreateIndex(name: "IX_Matches_FoundItemId_LostRequestId", table: "Matches", columns: new[] { "FoundItemId", "LostRequestId" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_Matches_MatchStatus", table: "Matches", column: "MatchStatus");

            migrationBuilder.CreateIndex(name: "IX_Returns_FoundItemId", table: "Returns", column: "FoundItemId", unique: true);
            migrationBuilder.CreateIndex(name: "IX_Returns_PersonId", table: "Returns", column: "PersonId");
            migrationBuilder.CreateIndex(name: "IX_Returns_EmployeeId", table: "Returns", column: "EmployeeId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_LostRequests_Status",
                table: "LostRequests",
                sql: "[Status] IN (N'Очікує обробки', N'Закрито (Знайдено)', N'Відхилено')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FoundItems_Status",
                table: "FoundItems",
                sql: "[Status] IN (N'Очікує обробки', N'Закрито (Повернуто)', N'Відхилено')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Matches_MatchPercent",
                table: "Matches",
                sql: "[MatchPercent] >= 0 AND [MatchPercent] <= 100");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Matches_Status",
                table: "Matches",
                sql: "[MatchStatus] IN (N'Потенційний', N'Підтверджено', N'Відхилено')");

            migrationBuilder.AddForeignKey(
                name: "FK_Returns_Employees_EmployeeId",
                table: "Returns",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Returns_FoundItems_FoundItemId",
                table: "Returns",
                column: "FoundItemId",
                principalTable: "FoundItems",
                principalColumn: "FoundItemId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Returns_Persons_PersonId",
                table: "Returns",
                column: "PersonId",
                principalTable: "Persons",
                principalColumn: "PersonId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Returns_Employees_EmployeeId", table: "Returns");
            migrationBuilder.DropForeignKey(name: "FK_Returns_FoundItems_FoundItemId", table: "Returns");
            migrationBuilder.DropForeignKey(name: "FK_Returns_Persons_PersonId", table: "Returns");

            migrationBuilder.DropCheckConstraint(name: "CK_LostRequests_Status", table: "LostRequests");
            migrationBuilder.DropCheckConstraint(name: "CK_FoundItems_Status", table: "FoundItems");
            migrationBuilder.DropCheckConstraint(name: "CK_Matches_MatchPercent", table: "Matches");
            migrationBuilder.DropCheckConstraint(name: "CK_Matches_Status", table: "Matches");

            migrationBuilder.DropIndex(name: "IX_Returns_EmployeeId", table: "Returns");
            migrationBuilder.DropIndex(name: "IX_Returns_FoundItemId", table: "Returns");
            migrationBuilder.DropIndex(name: "IX_Returns_PersonId", table: "Returns");
            migrationBuilder.DropIndex(name: "IX_Matches_FoundItemId_LostRequestId", table: "Matches");
            migrationBuilder.DropIndex(name: "IX_Matches_MatchStatus", table: "Matches");
            migrationBuilder.DropIndex(name: "IX_FoundItems_CategoryId_Status", table: "FoundItems");
            migrationBuilder.DropIndex(name: "IX_FoundItems_DateFound", table: "FoundItems");
            migrationBuilder.DropIndex(name: "IX_FoundItems_Status", table: "FoundItems");
            migrationBuilder.DropIndex(name: "IX_LostRequests_CategoryId_Status", table: "LostRequests");
            migrationBuilder.DropIndex(name: "IX_LostRequests_DateLost", table: "LostRequests");
            migrationBuilder.DropIndex(name: "IX_LostRequests_Status", table: "LostRequests");
            migrationBuilder.DropIndex(name: "IX_Locations_LocationName", table: "Locations");
            migrationBuilder.DropIndex(name: "IX_Categories_CategoryName", table: "Categories");
            migrationBuilder.DropIndex(name: "IX_Employees_Login", table: "Employees");
            migrationBuilder.DropIndex(name: "IX_Persons_Login", table: "Persons");

            migrationBuilder.AlterColumn<string>(name: "FullName", table: "Persons", type: "nvarchar(max)", nullable: true, oldClrType: typeof(string), oldType: "nvarchar(120)", oldMaxLength: 120);
            migrationBuilder.AlterColumn<string>(name: "Login", table: "Persons", type: "nvarchar(max)", nullable: true, oldClrType: typeof(string), oldType: "nvarchar(64)", oldMaxLength: 64);
            migrationBuilder.AlterColumn<string>(name: "PasswordHash", table: "Persons", type: "nvarchar(max)", nullable: true, oldClrType: typeof(string), oldType: "nvarchar(256)", oldMaxLength: 256);
            migrationBuilder.AlterColumn<string>(name: "Phone", table: "Persons", type: "nvarchar(max)", nullable: true, oldClrType: typeof(string), oldType: "nvarchar(32)", oldMaxLength: 32, oldNullable: true);
            migrationBuilder.AlterColumn<string>(name: "Email", table: "Persons", type: "nvarchar(max)", nullable: true, oldClrType: typeof(string), oldType: "nvarchar(255)", oldMaxLength: 255, oldNullable: true);
            migrationBuilder.AlterColumn<string>(name: "DocumentNumber", table: "Persons", type: "nvarchar(max)", nullable: true, oldClrType: typeof(string), oldType: "nvarchar(64)", oldMaxLength: 64, oldNullable: true);
            migrationBuilder.AlterColumn<string>(name: "Address", table: "Persons", type: "nvarchar(max)", nullable: true, oldClrType: typeof(string), oldType: "nvarchar(250)", oldMaxLength: 250, oldNullable: true);

            migrationBuilder.AlterColumn<string>(name: "FullName", table: "Employees", type: "nvarchar(max)", nullable: true, oldClrType: typeof(string), oldType: "nvarchar(120)", oldMaxLength: 120);
            migrationBuilder.AlterColumn<string>(name: "Login", table: "Employees", type: "nvarchar(max)", nullable: true, oldClrType: typeof(string), oldType: "nvarchar(64)", oldMaxLength: 64);
            migrationBuilder.AlterColumn<string>(name: "PasswordHash", table: "Employees", type: "nvarchar(max)", nullable: true, oldClrType: typeof(string), oldType: "nvarchar(256)", oldMaxLength: 256);
            migrationBuilder.AlterColumn<string>(name: "Position", table: "Employees", type: "nvarchar(max)", nullable: true, oldClrType: typeof(string), oldType: "nvarchar(80)", oldMaxLength: 80);
            migrationBuilder.AlterColumn<string>(name: "Phone", table: "Employees", type: "nvarchar(max)", nullable: true, oldClrType: typeof(string), oldType: "nvarchar(32)", oldMaxLength: 32, oldNullable: true);

            migrationBuilder.AlterColumn<string>(name: "CategoryName", table: "Categories", type: "nvarchar(max)", nullable: true, oldClrType: typeof(string), oldType: "nvarchar(80)", oldMaxLength: 80);
            migrationBuilder.AlterColumn<string>(name: "Description", table: "Categories", type: "nvarchar(max)", nullable: true, oldClrType: typeof(string), oldType: "nvarchar(500)", oldMaxLength: 500, oldNullable: true);

            migrationBuilder.AlterColumn<string>(name: "LocationName", table: "Locations", type: "nvarchar(max)", nullable: true, oldClrType: typeof(string), oldType: "nvarchar(120)", oldMaxLength: 120);
            migrationBuilder.AlterColumn<string>(name: "Address", table: "Locations", type: "nvarchar(max)", nullable: true, oldClrType: typeof(string), oldType: "nvarchar(250)", oldMaxLength: 250, oldNullable: true);
            migrationBuilder.AlterColumn<string>(name: "Description", table: "Locations", type: "nvarchar(max)", nullable: true, oldClrType: typeof(string), oldType: "nvarchar(500)", oldMaxLength: 500, oldNullable: true);

            migrationBuilder.AlterColumn<string>(name: "ItemName", table: "LostRequests", type: "nvarchar(max)", nullable: true, oldClrType: typeof(string), oldType: "nvarchar(120)", oldMaxLength: 120);
            migrationBuilder.AlterColumn<string>(name: "Description", table: "LostRequests", type: "nvarchar(max)", nullable: true, oldClrType: typeof(string), oldType: "nvarchar(1000)", oldMaxLength: 1000, oldNullable: true);
            migrationBuilder.AlterColumn<string>(name: "Color", table: "LostRequests", type: "nvarchar(max)", nullable: true, oldClrType: typeof(string), oldType: "nvarchar(50)", oldMaxLength: 50, oldNullable: true);
            migrationBuilder.AlterColumn<string>(name: "Brand", table: "LostRequests", type: "nvarchar(max)", nullable: true, oldClrType: typeof(string), oldType: "nvarchar(80)", oldMaxLength: 80, oldNullable: true);
            migrationBuilder.AlterColumn<string>(name: "Status", table: "LostRequests", type: "nvarchar(max)", nullable: true, oldClrType: typeof(string), oldType: "nvarchar(40)", oldMaxLength: 40);
            migrationBuilder.AlterColumn<DateTime>(name: "RequestDate", table: "LostRequests", type: "datetime2", nullable: false, oldClrType: typeof(DateTime), oldType: "datetime2", oldDefaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AlterColumn<string>(name: "ItemName", table: "FoundItems", type: "nvarchar(max)", nullable: true, oldClrType: typeof(string), oldType: "nvarchar(120)", oldMaxLength: 120);
            migrationBuilder.AlterColumn<string>(name: "Description", table: "FoundItems", type: "nvarchar(max)", nullable: true, oldClrType: typeof(string), oldType: "nvarchar(1000)", oldMaxLength: 1000, oldNullable: true);
            migrationBuilder.AlterColumn<string>(name: "Color", table: "FoundItems", type: "nvarchar(max)", nullable: true, oldClrType: typeof(string), oldType: "nvarchar(50)", oldMaxLength: 50, oldNullable: true);
            migrationBuilder.AlterColumn<string>(name: "Brand", table: "FoundItems", type: "nvarchar(max)", nullable: true, oldClrType: typeof(string), oldType: "nvarchar(80)", oldMaxLength: 80, oldNullable: true);
            migrationBuilder.AlterColumn<string>(name: "StorageLocation", table: "FoundItems", type: "nvarchar(max)", nullable: true, oldClrType: typeof(string), oldType: "nvarchar(160)", oldMaxLength: 160, oldNullable: true);
            migrationBuilder.AlterColumn<string>(name: "Status", table: "FoundItems", type: "nvarchar(max)", nullable: true, oldClrType: typeof(string), oldType: "nvarchar(40)", oldMaxLength: 40);
            migrationBuilder.AlterColumn<string>(name: "PhotoPath", table: "FoundItems", type: "nvarchar(max)", nullable: true, oldClrType: typeof(string), oldType: "nvarchar(500)", oldMaxLength: 500, oldNullable: true);

            migrationBuilder.AlterColumn<string>(name: "MatchStatus", table: "Matches", type: "nvarchar(max)", nullable: true, oldClrType: typeof(string), oldType: "nvarchar(40)", oldMaxLength: 40);
            migrationBuilder.AlterColumn<string>(name: "Comment", table: "Matches", type: "nvarchar(max)", nullable: true, oldClrType: typeof(string), oldType: "nvarchar(500)", oldMaxLength: 500, oldNullable: true);
            migrationBuilder.AlterColumn<DateTime>(name: "CreatedAt", table: "Matches", type: "datetime2", nullable: false, oldClrType: typeof(DateTime), oldType: "datetime2", oldDefaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AlterColumn<string>(name: "DocumentNumber", table: "Returns", type: "nvarchar(max)", nullable: true, oldClrType: typeof(string), oldType: "nvarchar(64)", oldMaxLength: 64, oldNullable: true);
            migrationBuilder.AlterColumn<string>(name: "Comment", table: "Returns", type: "nvarchar(max)", nullable: true, oldClrType: typeof(string), oldType: "nvarchar(500)", oldMaxLength: 500, oldNullable: true);
            migrationBuilder.AlterColumn<DateTime>(name: "ReturnDate", table: "Returns", type: "datetime2", nullable: false, oldClrType: typeof(DateTime), oldType: "datetime2", oldDefaultValueSql: "SYSUTCDATETIME()");
        }
    }
}
