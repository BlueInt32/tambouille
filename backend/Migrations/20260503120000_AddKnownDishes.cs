using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tambouille.Migrations
{
    /// <inheritdoc />
    public partial class AddKnownDishes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KnownDishes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnownDishes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KnownDishes_Name",
                table: "KnownDishes",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KnownDishes");
        }
    }
}
