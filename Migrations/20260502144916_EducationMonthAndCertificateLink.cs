using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrmApp.Migrations
{
    /// <inheritdoc />
    public partial class EducationMonthAndCertificateLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EndMonth",
                table: "CandidateExperiences",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StartMonth",
                table: "CandidateExperiences",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EndMonth",
                table: "CandidateEducations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StartMonth",
                table: "CandidateEducations",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CertificateLink",
                table: "CandidateCertificates",
                type: "TEXT",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE CandidateExperiences
                SET StartMonth = CASE WHEN StartYear > 0 THEN printf('%04d-01', StartYear) ELSE '' END,
                    EndMonth = CASE WHEN EndYear IS NOT NULL AND EndYear > 0 THEN printf('%04d-01', EndYear) ELSE NULL END;
                """);

            migrationBuilder.Sql("""
                UPDATE CandidateEducations
                SET StartMonth = CASE WHEN StartYear > 0 THEN printf('%04d-01', StartYear) ELSE '' END,
                    EndMonth = CASE WHEN EndYear IS NOT NULL AND EndYear > 0 THEN printf('%04d-01', EndYear) ELSE NULL END;
                """);

            migrationBuilder.DropColumn(
                name: "EndYear",
                table: "CandidateExperiences");

            migrationBuilder.DropColumn(
                name: "StartYear",
                table: "CandidateExperiences");

            migrationBuilder.DropColumn(
                name: "EndYear",
                table: "CandidateEducations");

            migrationBuilder.DropColumn(
                name: "StartYear",
                table: "CandidateEducations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EndYear",
                table: "CandidateExperiences",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StartYear",
                table: "CandidateExperiences",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EndYear",
                table: "CandidateEducations",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StartYear",
                table: "CandidateEducations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("""
                UPDATE CandidateExperiences
                SET StartYear = CASE WHEN length(StartMonth) >= 4 THEN CAST(substr(StartMonth, 1, 4) AS INTEGER) ELSE 0 END,
                    EndYear = CASE WHEN EndMonth IS NOT NULL AND length(EndMonth) >= 4 THEN CAST(substr(EndMonth, 1, 4) AS INTEGER) ELSE NULL END;
                """);

            migrationBuilder.Sql("""
                UPDATE CandidateEducations
                SET StartYear = CASE WHEN length(StartMonth) >= 4 THEN CAST(substr(StartMonth, 1, 4) AS INTEGER) ELSE 0 END,
                    EndYear = CASE WHEN EndMonth IS NOT NULL AND length(EndMonth) >= 4 THEN CAST(substr(EndMonth, 1, 4) AS INTEGER) ELSE NULL END;
                """);

            migrationBuilder.DropColumn(
                name: "EndMonth",
                table: "CandidateExperiences");

            migrationBuilder.DropColumn(
                name: "StartMonth",
                table: "CandidateExperiences");

            migrationBuilder.DropColumn(
                name: "EndMonth",
                table: "CandidateEducations");

            migrationBuilder.DropColumn(
                name: "StartMonth",
                table: "CandidateEducations");

            migrationBuilder.DropColumn(
                name: "CertificateLink",
                table: "CandidateCertificates");
        }
    }
}
