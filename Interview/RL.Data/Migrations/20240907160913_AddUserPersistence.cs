using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RL.Data.Migrations
{
    public partial class AddUserPersistence : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlanProcedureUser",
                columns: table => new
                {
                    UsersUserId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProceduresPlanId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProceduresProcedureId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanProcedureUser", x => new { x.UsersUserId, x.ProceduresPlanId, x.ProceduresProcedureId });
                    table.ForeignKey(
                        name: "FK_PlanProcedureUser_PlanProcedures_ProceduresPlanId_ProceduresProcedureId",
                        columns: x => new { x.ProceduresPlanId, x.ProceduresProcedureId },
                        principalTable: "PlanProcedures",
                        principalColumns: new[] { "PlanId", "ProcedureId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlanProcedureUser_Users_UsersUserId",
                        column: x => x.UsersUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlanProcedureUser_ProceduresPlanId_ProceduresProcedureId",
                table: "PlanProcedureUser",
                columns: new[] { "ProceduresPlanId", "ProceduresProcedureId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlanProcedureUser");
        }
    }
}
