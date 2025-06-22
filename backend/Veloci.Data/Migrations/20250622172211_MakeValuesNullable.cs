using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Veloci.Data.Migrations
{
    /// <inheritdoc />
    public partial class MakeValuesNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<ulong>(
                name: "ULongValue",
                table: "CompetitionVariables",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "StringValue",
                table: "CompetitionVariables",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<int>(
                name: "IntValue",
                table: "CompetitionVariables",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<double>(
                name: "DoubleValue",
                table: "CompetitionVariables",
                type: "REAL",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "REAL");

            migrationBuilder.AlterColumn<bool>(
                name: "BoolValue",
                table: "CompetitionVariables",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "INTEGER");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<ulong>(
                name: "ULongValue",
                table: "CompetitionVariables",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0ul,
                oldClrType: typeof(ulong),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StringValue",
                table: "CompetitionVariables",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "IntValue",
                table: "CompetitionVariables",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "DoubleValue",
                table: "CompetitionVariables",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "REAL",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "BoolValue",
                table: "CompetitionVariables",
                type: "INTEGER",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldNullable: true);
        }
    }
}
