using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InterviewService.Migrations
{
    /// <inheritdoc />
    public partial class @new : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CorrectAnswer", "QuestionType", "Text" },
                values: new object[] { "Language", "FillBlank", "Fill in the blank: LINQ stands for ______ Integrated Query." });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CorrectAnswer", "QuestionType", "Text" },
                values: new object[] { "Exception", "FillBlank", "Fill in the blank: The base class for most C# runtime errors is ______." });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CorrectAnswer", "QuestionType", "Text" },
                values: new object[] { null, "Subjective", "Explain deferred execution in LINQ." });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CorrectAnswer", "QuestionType", "Text" },
                values: new object[] { null, "Subjective", "How would you design custom exception handling in an API?" });
        }
    }
}
