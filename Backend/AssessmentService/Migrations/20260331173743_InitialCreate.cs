using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AssessmentService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssessmentResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssessmentId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Domain = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    MaxScore = table.Column<int>(type: "int", nullable: false),
                    Percentage = table.Column<double>(type: "float", nullable: false),
                    Grade = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsPremiumResult = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentResults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Assessments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Domain = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AttemptNumber = table.Column<int>(type: "int", nullable: false),
                    TimeLimitMinutes = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assessments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MCQQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Domain = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OptionA = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OptionB = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OptionC = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OptionD = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CorrectOption = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Subtopic = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Marks = table.Column<int>(type: "int", nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MCQQuestions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserAnswers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssessmentId = table.Column<int>(type: "int", nullable: false),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    SelectedOption = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAnswers", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "MCQQuestions",
                columns: new[] { "Id", "CorrectOption", "Domain", "Marks", "OptionA", "OptionB", "OptionC", "OptionD", "OrderIndex", "Subtopic", "Text" },
                values: new object[,]
                {
                    { 1, "A", "C#", 1, "Many forms", "Hiding data", "Code reuse", "Creating objects", 1, "OOP", "What is polymorphism in OOP?" },
                    { 2, "B", "C#", 1, "public", "private", "protected", "internal", 2, "OOP", "Which modifier restricts access to the containing class?" },
                    { 3, "A", "C#", 1, "async", "await", "Task", "Thread", 3, "Async", "What keyword is used for asynchronous methods?" },
                    { 4, "B", "C#", 1, "Thread.Sleep()", "await Task.Delay()", "Wait()", "Yield()", 4, "Async", "How to pause async method execution?" },
                    { 5, "C", "C#", 1, "ICollection", "IList", "IEnumerable", "IDictionary", 5, "Collections", "Which interface allows iterating over a collection?" },
                    { 6, "C", "C#", 1, "List represents fixed size", "Array represents dynamic size", "List is dynamic, Array is fixed", "No difference", 6, "Collections", "What is the key difference between Array and List?" },
                    { 7, "A", "C#", 1, "Language Integrated Query", "Language Internal Query", "List Integrated Query", "List Internal Query", 7, "LINQ", "What does LINQ stand for?" },
                    { 8, "B", "C#", 1, "Select()", "Where()", "OrderBy()", "GroupBy()", 8, "LINQ", "Which LINQ method filters elements?" },
                    { 9, "C", "C#", 1, "try", "catch", "finally", "throw", 9, "ExceptionHandling", "Which block must execute regardless of exceptions?" },
                    { 10, "C", "C#", 1, "SystemException", "ApplicationException", "Exception", "Error", 10, "ExceptionHandling", "What is the base class for all exceptions?" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssessmentResults");

            migrationBuilder.DropTable(
                name: "Assessments");

            migrationBuilder.DropTable(
                name: "MCQQuestions");

            migrationBuilder.DropTable(
                name: "UserAnswers");
        }
    }
}
