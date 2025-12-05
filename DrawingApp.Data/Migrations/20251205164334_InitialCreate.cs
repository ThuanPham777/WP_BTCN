using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DrawingApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Profiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Theme = table.Column<int>(type: "INTEGER", nullable: false),
                    DefaultBoardWidth = table.Column<double>(type: "REAL", nullable: false),
                    DefaultBoardHeight = table.Column<double>(type: "REAL", nullable: false),
                    DefaultStrokeColor = table.Column<string>(type: "TEXT", nullable: false),
                    DefaultStrokeThickness = table.Column<double>(type: "REAL", nullable: false),
                    DefaultStrokeDash = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Templates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    ShapeType = table.Column<int>(type: "INTEGER", nullable: false),
                    StrokeColor = table.Column<string>(type: "TEXT", nullable: true),
                    FillColor = table.Column<string>(type: "TEXT", nullable: true),
                    Thickness = table.Column<double>(type: "REAL", nullable: false),
                    DashStyle = table.Column<int>(type: "INTEGER", nullable: false),
                    GeometryJson = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Templates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Boards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Width = table.Column<double>(type: "REAL", nullable: false),
                    Height = table.Column<double>(type: "REAL", nullable: false),
                    BackgroundColor = table.Column<string>(type: "TEXT", nullable: false),
                    ProfileId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Boards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Boards_Profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BoardShapes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BoardId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ShapeType = table.Column<int>(type: "INTEGER", nullable: false),
                    StrokeColor = table.Column<string>(type: "TEXT", nullable: false),
                    FillColor = table.Column<string>(type: "TEXT", nullable: true),
                    Thickness = table.Column<double>(type: "REAL", nullable: false),
                    DashStyle = table.Column<int>(type: "INTEGER", nullable: false),
                    GeometryJson = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardShapes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoardShapes_Boards_BoardId",
                        column: x => x.BoardId,
                        principalTable: "Boards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Boards_ProfileId",
                table: "Boards",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_BoardShapes_BoardId",
                table: "BoardShapes",
                column: "BoardId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BoardShapes");

            migrationBuilder.DropTable(
                name: "Templates");

            migrationBuilder.DropTable(
                name: "Boards");

            migrationBuilder.DropTable(
                name: "Profiles");
        }
    }
}
