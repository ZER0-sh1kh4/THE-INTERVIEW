using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InterviewService.Migrations
{
    /// <inheritdoc />
    public partial class updated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CorrectAnswer", "OptionA", "OptionB", "OptionC", "OptionD", "QuestionType", "Text" },
                values: new object[] { "B", "static", "sealed", "abstract", "readonly", "MCQ", "What keyword prevents class inheritance in C#?" });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CorrectAnswer", "OptionA", "OptionB", "OptionC", "OptionD", "QuestionType", "Subtopic", "Text" },
                values: new object[] { "B", "List", "Dictionary", "Queue", "Stack", "MCQ", "Collections", "Which collection is best for key-value lookup?" });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CorrectAnswer", "OptionA", "OptionB", "OptionC", "OptionD", "QuestionType", "Subtopic", "Text" },
                values: new object[] { "B", "Thread", "Task", "Action", "Event", "MCQ", "Async", "What does async usually return in C#?" });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 4,
                column: "Text",
                value: "Explain deferred execution in LINQ.");

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 5,
                column: "Text",
                value: "How would you design custom exception handling in an API?");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CorrectAnswer", "OptionA", "OptionB", "OptionC", "OptionD", "QuestionType", "Text" },
                values: new object[] { null, null, null, null, null, "Subjective", "What is encapsulation?" });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CorrectAnswer", "OptionA", "OptionB", "OptionC", "OptionD", "QuestionType", "Subtopic", "Text" },
                values: new object[] { null, null, null, null, null, "Subjective", "Async", "Explain async/await in C#." });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CorrectAnswer", "OptionA", "OptionB", "OptionC", "OptionD", "QuestionType", "Subtopic", "Text" },
                values: new object[] { null, null, null, null, null, "Subjective", "Collections", "Difference between List and Dictionary?" });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 4,
                column: "Text",
                value: "What is deferred execution in LINQ?");

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 5,
                column: "Text",
                value: "How to handle custom exceptions?");
        }
    }
}
