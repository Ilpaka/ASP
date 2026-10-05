using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TaskFlowApi.Data;

#nullable disable

namespace TaskFlowApi.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261005000000_Init")]
public sealed class Init : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(name: "Projects", columns: table => new
        {
            Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
            Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
            Description = table.Column<string>(type: "TEXT", nullable: true),
            CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
        }, constraints: table => table.PrimaryKey("PK_Projects", x => x.Id));
        migrationBuilder.CreateTable(name: "Users", columns: table => new
        {
            Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
            Username = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
            Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
            PasswordHash = table.Column<string>(type: "TEXT", nullable: false)
        }, constraints: table => table.PrimaryKey("PK_Users", x => x.Id));
        migrationBuilder.CreateTable(name: "IdempotencyRecords", columns: table => new
        {
            Key = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
            RequestBodyHash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
            ResponseBody = table.Column<string>(type: "TEXT", nullable: false),
            StatusCode = table.Column<int>(type: "INTEGER", nullable: false),
            Location = table.Column<string>(type: "TEXT", nullable: true),
            CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
        }, constraints: table => table.PrimaryKey("PK_IdempotencyRecords", x => x.Key));
        migrationBuilder.CreateTable(name: "Tasks", columns: table => new
        {
            Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
            ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
            Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
            Description = table.Column<string>(type: "TEXT", nullable: true),
            Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
            Priority = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
            AssignedToId = table.Column<int>(type: "INTEGER", nullable: true),
            DueDate = table.Column<DateTime>(type: "TEXT", nullable: true),
            CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_Tasks", x => x.Id);
            table.ForeignKey("FK_Tasks_Projects_ProjectId", x => x.ProjectId, "Projects", "Id", onDelete: ReferentialAction.Cascade);
            table.ForeignKey("FK_Tasks_Users_AssignedToId", x => x.AssignedToId, "Users", "Id", onDelete: ReferentialAction.SetNull);
        });
        migrationBuilder.CreateTable(name: "Comments", columns: table => new
        {
            Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
            TaskItemId = table.Column<int>(type: "INTEGER", nullable: false),
            AuthorId = table.Column<int>(type: "INTEGER", nullable: false),
            Content = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
            CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_Comments", x => x.Id);
            table.ForeignKey("FK_Comments_Tasks_TaskItemId", x => x.TaskItemId, "Tasks", "Id", onDelete: ReferentialAction.Cascade);
            table.ForeignKey("FK_Comments_Users_AuthorId", x => x.AuthorId, "Users", "Id", onDelete: ReferentialAction.Restrict);
        });
        migrationBuilder.CreateIndex("IX_Users_Username", "Users", "Username", unique: true);
        migrationBuilder.CreateIndex("IX_Tasks_ProjectId", "Tasks", "ProjectId");
        migrationBuilder.CreateIndex("IX_Tasks_AssignedToId", "Tasks", "AssignedToId");
        migrationBuilder.CreateIndex("IX_Tasks_Status", "Tasks", "Status");
        migrationBuilder.CreateIndex("IX_Comments_TaskItemId", "Comments", "TaskItemId");
        migrationBuilder.CreateIndex("IX_Comments_AuthorId", "Comments", "AuthorId");
        migrationBuilder.Sql("INSERT INTO Projects (Id, Name, Description, CreatedAt) VALUES (1, 'Демо-проект', 'Первый проект TaskFlow', '2026-01-01 00:00:00');");
        migrationBuilder.Sql("INSERT INTO Users (Id, Username, Email, PasswordHash) VALUES (1, 'demo', 'demo@example.local', 'demo-only-no-login');");
        migrationBuilder.Sql("INSERT INTO Tasks (Id, ProjectId, Title, Status, Priority, CreatedAt) VALUES (1, 1, 'Подготовить техническое задание', 'ToDo', 'Medium', '2026-01-01 00:00:00');");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("Comments");
        migrationBuilder.DropTable("IdempotencyRecords");
        migrationBuilder.DropTable("Tasks");
        migrationBuilder.DropTable("Projects");
        migrationBuilder.DropTable("Users");
    }
}
