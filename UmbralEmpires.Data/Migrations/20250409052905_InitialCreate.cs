using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UmbralEmpires.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Astros",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Coordinates_Galaxy = table.Column<string>(type: "TEXT", nullable: false),
                    Coordinates_Region = table.Column<int>(type: "INTEGER", nullable: false),
                    Coordinates_System = table.Column<int>(type: "INTEGER", nullable: false),
                    Coordinates_Orbit = table.Column<int>(type: "INTEGER", nullable: false),
                    Terrain = table.Column<string>(type: "TEXT", nullable: false),
                    IsPlanet = table.Column<bool>(type: "INTEGER", nullable: false),
                    MetalPotential = table.Column<int>(type: "INTEGER", nullable: false),
                    GasPotential = table.Column<int>(type: "INTEGER", nullable: false),
                    CrystalsPotential = table.Column<int>(type: "INTEGER", nullable: false),
                    SolarPotential = table.Column<int>(type: "INTEGER", nullable: false),
                    BaseFertility = table.Column<int>(type: "INTEGER", nullable: false),
                    BaseArea = table.Column<int>(type: "INTEGER", nullable: false),
                    BaseId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Astros", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Bases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AstroId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PlayerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Structures = table.Column<string>(type: "TEXT", nullable: false),
                    ConstructionQueue = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bases_Astros_AstroId",
                        column: x => x.AstroId,
                        principalTable: "Astros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Astros_BaseId",
                table: "Astros",
                column: "BaseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bases_AstroId",
                table: "Bases",
                column: "AstroId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bases_PlayerId",
                table: "Bases",
                column: "PlayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bases");

            migrationBuilder.DropTable(
                name: "Astros");
        }
    }
}
