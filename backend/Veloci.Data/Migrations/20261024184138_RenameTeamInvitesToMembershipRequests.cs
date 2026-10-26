using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Veloci.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameTeamInvitesToMembershipRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TeamMembershipRequests",
                columns: table => new
                {
                    TeamId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    InvitedByUserId = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamMembershipRequests", x => new { x.TeamId, x.UserId });
                    table.ForeignKey(
                        name: "FK_TeamMembershipRequests_AspNetUsers_InvitedByUserId",
                        column: x => x.InvitedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamMembershipRequests_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamMembershipRequests_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeamMembershipRequests_InvitedByUserId",
                table: "TeamMembershipRequests",
                column: "InvitedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamMembershipRequests_UserId",
                table: "TeamMembershipRequests",
                column: "UserId");

            // Copy every row across before the old table goes: a self-invite (the old marker of a join request)
            // becomes a row without an inviter.
            migrationBuilder.Sql(
                "INSERT INTO TeamMembershipRequests (TeamId, UserId, InvitedByUserId, CreatedOn) " +
                "SELECT TeamId, UserId, CASE WHEN InvitedByUserId = UserId THEN NULL ELSE InvitedByUserId END, InvitedOn " +
                "FROM TeamInvites;");

            migrationBuilder.DropTable(
                name: "TeamInvites");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TeamInvites",
                columns: table => new
                {
                    TeamId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    InvitedByUserId = table.Column<string>(type: "TEXT", nullable: false),
                    InvitedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamInvites", x => new { x.TeamId, x.UserId });
                    table.ForeignKey(
                        name: "FK_TeamInvites_AspNetUsers_InvitedByUserId",
                        column: x => x.InvitedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamInvites_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamInvites_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeamInvites_InvitedByUserId",
                table: "TeamInvites",
                column: "InvitedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamInvites_UserId",
                table: "TeamInvites",
                column: "UserId");

            migrationBuilder.Sql(
                "INSERT INTO TeamInvites (TeamId, UserId, InvitedByUserId, InvitedOn) " +
                "SELECT TeamId, UserId, COALESCE(InvitedByUserId, UserId), CreatedOn " +
                "FROM TeamMembershipRequests;");

            migrationBuilder.DropTable(
                name: "TeamMembershipRequests");
        }
    }
}
