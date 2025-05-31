using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QSM.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class ServerInstance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Servers",
                columns: table => new
                {
                    Id = table.Column<ushort>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Running = table.Column<bool>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    MinecraftVersion = table.Column<string>(type: "TEXT", maxLength: 127, nullable: true),
                    ServerVersion = table.Column<string>(type: "TEXT", maxLength: 127, nullable: true),
                    ServerPath = table.Column<string>(type: "TEXT", maxLength: 4096, nullable: true),
                    Software = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servers", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Servers");
        }
    }
}
