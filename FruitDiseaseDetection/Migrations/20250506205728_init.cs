using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FruitDiseaseDetection.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FruitVegetableDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    ReleaseDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FruitVegetableId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FruitVegetableDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", nullable: true),
                    Password = table.Column<string>(type: "TEXT", nullable: true),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    Role = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Diseases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    CommonName = table.Column<string>(type: "TEXT", nullable: true),
                    DiseaseDetailsId = table.Column<int>(type: "INTEGER", nullable: true),
                    TreatmentSuggestion = table.Column<string>(type: "TEXT", nullable: true),
                    Symptoms = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Diseases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Diseases_FruitVegetableDetails_DiseaseDetailsId",
                        column: x => x.DiseaseDetailsId,
                        principalTable: "FruitVegetableDetails",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Fruits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Species = table.Column<string>(type: "TEXT", nullable: true),
                    FruitDetailsId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fruits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fruits_FruitVegetableDetails_FruitDetailsId",
                        column: x => x.FruitDetailsId,
                        principalTable: "FruitVegetableDetails",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ImageResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ImageId = table.Column<int>(type: "INTEGER", nullable: true),
                    DetectedDiseaseId = table.Column<int>(type: "INTEGER", nullable: true),
                    ModelLabel = table.Column<string>(type: "TEXT", nullable: true),
                    Confidence = table.Column<float>(type: "REAL", nullable: true),
                    AnalysisDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImageResults_Diseases_DetectedDiseaseId",
                        column: x => x.DetectedDiseaseId,
                        principalTable: "Diseases",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DiseaseFruit",
                columns: table => new
                {
                    DiseasesId = table.Column<int>(type: "INTEGER", nullable: false),
                    FruitsVegetablesId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiseaseFruit", x => new { x.DiseasesId, x.FruitsVegetablesId });
                    table.ForeignKey(
                        name: "FK_DiseaseFruit_Diseases_DiseasesId",
                        column: x => x.DiseasesId,
                        principalTable: "Diseases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiseaseFruit_Fruits_FruitsVegetablesId",
                        column: x => x.FruitsVegetablesId,
                        principalTable: "Fruits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UploadedImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: true),
                    FruitVegetableId = table.Column<int>(type: "INTEGER", nullable: true),
                    FileName = table.Column<string>(type: "TEXT", nullable: true),
                    ImagePath = table.Column<string>(type: "TEXT", nullable: true),
                    ImageData = table.Column<byte[]>(type: "BLOB", nullable: true),
                    UploadDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ImageResultId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadedImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UploadedImages_Fruits_FruitVegetableId",
                        column: x => x.FruitVegetableId,
                        principalTable: "Fruits",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UploadedImages_ImageResults_ImageResultId",
                        column: x => x.ImageResultId,
                        principalTable: "ImageResults",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UploadedImages_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Fruits",
                columns: new[] { "Id", "FruitDetailsId", "Name", "Species" },
                values: new object[,]
                {
                    { 1, null, "Apple", "Gala" },
                    { 2, null, "Grape", "Vitis" },
                    { 3, null, "Peach", "Prunus" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiseaseFruit_FruitsVegetablesId",
                table: "DiseaseFruit",
                column: "FruitsVegetablesId");

            migrationBuilder.CreateIndex(
                name: "IX_Diseases_DiseaseDetailsId",
                table: "Diseases",
                column: "DiseaseDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_Fruits_FruitDetailsId",
                table: "Fruits",
                column: "FruitDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_ImageResults_DetectedDiseaseId",
                table: "ImageResults",
                column: "DetectedDiseaseId");

            migrationBuilder.CreateIndex(
                name: "IX_UploadedImages_FruitVegetableId",
                table: "UploadedImages",
                column: "FruitVegetableId");

            migrationBuilder.CreateIndex(
                name: "IX_UploadedImages_ImageResultId",
                table: "UploadedImages",
                column: "ImageResultId");

            migrationBuilder.CreateIndex(
                name: "IX_UploadedImages_UserId",
                table: "UploadedImages",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiseaseFruit");

            migrationBuilder.DropTable(
                name: "UploadedImages");

            migrationBuilder.DropTable(
                name: "Fruits");

            migrationBuilder.DropTable(
                name: "ImageResults");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Diseases");

            migrationBuilder.DropTable(
                name: "FruitVegetableDetails");
        }
    }
}
