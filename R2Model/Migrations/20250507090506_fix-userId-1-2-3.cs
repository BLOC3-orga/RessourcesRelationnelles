using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace R2Model.Migrations
{
    /// <inheritdoc />
    public partial class fixuserId123 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ressources_AspNetUsers_UserId",
                table: "Ressources");

            migrationBuilder.DropForeignKey(
                name: "FK_Ressources_AspNetUsers_UserId1",
                table: "Ressources");

            migrationBuilder.DropForeignKey(
                name: "FK_Ressources_AspNetUsers_UserId2",
                table: "Ressources");

            migrationBuilder.DropForeignKey(
                name: "FK_Ressources_AspNetUsers_UserId3",
                table: "Ressources");

            migrationBuilder.DropIndex(
                name: "IX_Ressources_UserId",
                table: "Ressources");

            migrationBuilder.DropIndex(
                name: "IX_Ressources_UserId1",
                table: "Ressources");

            migrationBuilder.DropIndex(
                name: "IX_Ressources_UserId2",
                table: "Ressources");

            migrationBuilder.DropIndex(
                name: "IX_Ressources_UserId3",
                table: "Ressources");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Ressources");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Ressources");

            migrationBuilder.DropColumn(
                name: "UserId2",
                table: "Ressources");

            migrationBuilder.DropColumn(
                name: "UserId3",
                table: "Ressources");

            migrationBuilder.CreateTable(
                name: "UserCreatedResources",
                columns: table => new
                {
                    ResourceId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCreatedResources", x => new { x.ResourceId, x.UserId });
                    table.ForeignKey(
                        name: "FK_UserCreatedResources_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCreatedResources_Ressources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Ressources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserDraftResources",
                columns: table => new
                {
                    ResourceId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDraftResources", x => new { x.ResourceId, x.UserId });
                    table.ForeignKey(
                        name: "FK_UserDraftResources_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserDraftResources_Ressources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Ressources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserExploitedResources",
                columns: table => new
                {
                    ResourceId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserExploitedResources", x => new { x.ResourceId, x.UserId });
                    table.ForeignKey(
                        name: "FK_UserExploitedResources_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserExploitedResources_Ressources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Ressources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserFavoriteResources",
                columns: table => new
                {
                    ResourceId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFavoriteResources", x => new { x.ResourceId, x.UserId });
                    table.ForeignKey(
                        name: "FK_UserFavoriteResources_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserFavoriteResources_Ressources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Ressources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserCreatedResources_UserId",
                table: "UserCreatedResources",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDraftResources_UserId",
                table: "UserDraftResources",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserExploitedResources_UserId",
                table: "UserExploitedResources",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFavoriteResources_UserId",
                table: "UserFavoriteResources",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserCreatedResources");

            migrationBuilder.DropTable(
                name: "UserDraftResources");

            migrationBuilder.DropTable(
                name: "UserExploitedResources");

            migrationBuilder.DropTable(
                name: "UserFavoriteResources");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Ressources",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "Ressources",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId2",
                table: "Ressources",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId3",
                table: "Ressources",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ressources_UserId",
                table: "Ressources",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Ressources_UserId1",
                table: "Ressources",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_Ressources_UserId2",
                table: "Ressources",
                column: "UserId2");

            migrationBuilder.CreateIndex(
                name: "IX_Ressources_UserId3",
                table: "Ressources",
                column: "UserId3");

            migrationBuilder.AddForeignKey(
                name: "FK_Ressources_AspNetUsers_UserId",
                table: "Ressources",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Ressources_AspNetUsers_UserId1",
                table: "Ressources",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Ressources_AspNetUsers_UserId2",
                table: "Ressources",
                column: "UserId2",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Ressources_AspNetUsers_UserId3",
                table: "Ressources",
                column: "UserId3",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
