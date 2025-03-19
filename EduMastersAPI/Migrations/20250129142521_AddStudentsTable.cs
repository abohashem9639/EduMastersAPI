using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduMastersAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(nullable: false),
                    LastName = table.Column<string>(nullable: false),
                    FatherName = table.Column<string>(nullable: true),
                    MotherName = table.Column<string>(nullable: true),
                    Gender = table.Column<string>(nullable: false),
                    Nationality = table.Column<string>(nullable: false),
                    DateOfBirth = table.Column<DateTime>(nullable: false),
                    PhoneNumber = table.Column<string>(nullable: false),
                    ResidenceCountry = table.Column<string>(nullable: false),
                    Address = table.Column<string>(nullable: true),
                    PassportNumber = table.Column<string>(nullable: true),
                    GPA = table.Column<decimal>(nullable: false),
                    GraduationSchool = table.Column<string>(nullable: true),
                    GraduationCountry = table.Column<string>(nullable: true),
                    ProfileImage = table.Column<string>(nullable: true),
                    PassportImage = table.Column<string>(nullable: true),
                    HighSchoolDiploma = table.Column<string>(nullable: true),
                    Transcript = table.Column<string>(nullable: true),
                    LanguageCertificate = table.Column<string>(nullable: true),
                    AdditionalFile1 = table.Column<string>(nullable: true),
                    AdditionalFile2 = table.Column<string>(nullable: true),
                    AdditionalFile3 = table.Column<string>(nullable: true),
                    CreatedByUserId = table.Column<int>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Students_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
