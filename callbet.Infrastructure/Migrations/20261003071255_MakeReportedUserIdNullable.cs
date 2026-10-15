using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace callbet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeReportedUserIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"Resumes\" SET \"Summary\" = LEFT(\"Summary\", 500) WHERE \"Summary\" IS NOT NULL AND LENGTH(\"Summary\") > 500;");
            migrationBuilder.Sql("UPDATE \"Reports\" SET \"Details\" = LEFT(\"Details\", 2000) WHERE \"Details\" IS NOT NULL AND LENGTH(\"Details\") > 2000;");
            migrationBuilder.Sql("UPDATE \"ProfessionalProfiles\" SET \"Headline\" = LEFT(\"Headline\", 70) WHERE \"Headline\" IS NOT NULL AND LENGTH(\"Headline\") > 70;");
            migrationBuilder.Sql("UPDATE \"ProfessionalProfiles\" SET \"Bio\" = LEFT(\"Bio\", 200) WHERE \"Bio\" IS NOT NULL AND LENGTH(\"Bio\") > 200;");
            migrationBuilder.Sql("UPDATE \"PortfolioItems\" SET \"Title\" = LEFT(\"Title\", 70) WHERE \"Title\" IS NOT NULL AND LENGTH(\"Title\") > 70;");
            migrationBuilder.Sql("UPDATE \"PortfolioItems\" SET \"Description\" = LEFT(\"Description\", 500) WHERE \"Description\" IS NOT NULL AND LENGTH(\"Description\") > 500;");
            migrationBuilder.Sql("UPDATE \"Certificates\" SET \"Title\" = LEFT(\"Title\", 70) WHERE \"Title\" IS NOT NULL AND LENGTH(\"Title\") > 70;");
            migrationBuilder.Sql("UPDATE \"Certificates\" SET \"Organization\" = LEFT(\"Organization\", 70) WHERE \"Organization\" IS NOT NULL AND LENGTH(\"Organization\") > 70;");

            migrationBuilder.AlterColumn<string>(
                name: "Summary",
                table: "Resumes",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ReportedUserId",
                table: "Reports",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "Details",
                table: "Reports",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ServiceRadiusKm",
                table: "ProfessionalProfiles",
                type: "numeric(5,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Headline",
                table: "ProfessionalProfiles",
                type: "character varying(70)",
                maxLength: 70,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Bio",
                table: "ProfessionalProfiles",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "PortfolioItems",
                type: "character varying(70)",
                maxLength: 70,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "PortfolioItems",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Certificates",
                type: "character varying(70)",
                maxLength: 70,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Organization",
                table: "Certificates",
                type: "character varying(70)",
                maxLength: 70,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AlterColumn<string>(
                name: "Summary",
                table: "Resumes",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ReportedUserId",
                table: "Reports",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Details",
                table: "Reports",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ServiceRadiusKm",
                table: "ProfessionalProfiles",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)");

            migrationBuilder.AlterColumn<string>(
                name: "Headline",
                table: "ProfessionalProfiles",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(70)",
                oldMaxLength: 70,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Bio",
                table: "ProfessionalProfiles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "PortfolioItems",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(70)",
                oldMaxLength: 70);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "PortfolioItems",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Certificates",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(70)",
                oldMaxLength: 70);

            migrationBuilder.AlterColumn<string>(
                name: "Organization",
                table: "Certificates",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(70)",
                oldMaxLength: 70);
        }
    }
}
