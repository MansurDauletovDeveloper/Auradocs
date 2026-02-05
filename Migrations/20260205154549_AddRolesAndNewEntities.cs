using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocumentFlow.Migrations
{
    /// <inheritdoc />
    public partial class AddRolesAndNewEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedAt",
                table: "Documents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConfidentialityLevel",
                table: "Documents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "LegalReviewCompleted",
                table: "Documents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Documents",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresLegalReview",
                table: "Documents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RetentionPeriodMonths",
                table: "Documents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledDeletionDate",
                table: "Documents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AccessExpiresAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CanDownload",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanExport",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanPrint",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsExternalUser",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManagerId",
                table: "AspNetUsers",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApproverType",
                table: "ApprovalRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "CanBlock",
                table: "ApprovalRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DelegatedAt",
                table: "ApprovalRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DelegatedToId",
                table: "ApprovalRequests",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DelegationReason",
                table: "ApprovalRequests",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRequired",
                table: "ApprovalRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ReviewComment",
                table: "ApprovalRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SuggestedChanges",
                table: "ApprovalRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DocumentAccesses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AccessLevel = table.Column<int>(type: "int", nullable: false),
                    DocumentRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CanDownload = table.Column<bool>(type: "bit", nullable: false),
                    CanPrint = table.Column<bool>(type: "bit", nullable: false),
                    CanExport = table.Column<bool>(type: "bit", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GrantedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    GrantedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentAccesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentAccesses_AspNetUsers_GrantedById",
                        column: x => x.GrantedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentAccesses_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentAccesses_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentBlocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentId = table.Column<int>(type: "int", nullable: false),
                    BlockedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    BlockType = table.Column<int>(type: "int", nullable: false),
                    BlockedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UnblockedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UnblockedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    UnblockComment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentBlocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentBlocks_AspNetUsers_BlockedById",
                        column: x => x.BlockedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentBlocks_AspNetUsers_UnblockedById",
                        column: x => x.UnblockedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentBlocks_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserDelegations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ToUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DelegationType = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDelegations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDelegations_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserDelegations_AspNetUsers_FromUserId",
                        column: x => x.FromUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserDelegations_AspNetUsers_ToUserId",
                        column: x => x.ToUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ConfidentialityLevel",
                table: "Documents",
                column: "ConfidentialityLevel");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_OwnerId",
                table: "Documents",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Department",
                table: "AspNetUsers",
                column: "Department");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_IsExternalUser",
                table: "AspNetUsers",
                column: "IsExternalUser");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ManagerId",
                table: "AspNetUsers",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalRequests_ApproverType",
                table: "ApprovalRequests",
                column: "ApproverType");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalRequests_DelegatedToId",
                table: "ApprovalRequests",
                column: "DelegatedToId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentAccesses_DocumentId_UserId",
                table: "DocumentAccesses",
                columns: new[] { "DocumentId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentAccesses_ExpiresAt",
                table: "DocumentAccesses",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentAccesses_GrantedById",
                table: "DocumentAccesses",
                column: "GrantedById");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentAccesses_IsActive",
                table: "DocumentAccesses",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentAccesses_UserId",
                table: "DocumentAccesses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentBlocks_BlockedById",
                table: "DocumentBlocks",
                column: "BlockedById");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentBlocks_BlockType",
                table: "DocumentBlocks",
                column: "BlockType");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentBlocks_DocumentId",
                table: "DocumentBlocks",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentBlocks_IsActive",
                table: "DocumentBlocks",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentBlocks_UnblockedById",
                table: "DocumentBlocks",
                column: "UnblockedById");

            migrationBuilder.CreateIndex(
                name: "IX_UserDelegations_CreatedById",
                table: "UserDelegations",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_UserDelegations_FromUserId",
                table: "UserDelegations",
                column: "FromUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDelegations_IsActive",
                table: "UserDelegations",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_UserDelegations_StartDate_EndDate",
                table: "UserDelegations",
                columns: new[] { "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_UserDelegations_ToUserId",
                table: "UserDelegations",
                column: "ToUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalRequests_AspNetUsers_DelegatedToId",
                table: "ApprovalRequests",
                column: "DelegatedToId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_ManagerId",
                table: "AspNetUsers",
                column: "ManagerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_AspNetUsers_OwnerId",
                table: "Documents",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalRequests_AspNetUsers_DelegatedToId",
                table: "ApprovalRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_ManagerId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_AspNetUsers_OwnerId",
                table: "Documents");

            migrationBuilder.DropTable(
                name: "DocumentAccesses");

            migrationBuilder.DropTable(
                name: "DocumentBlocks");

            migrationBuilder.DropTable(
                name: "UserDelegations");

            migrationBuilder.DropIndex(
                name: "IX_Documents_ConfidentialityLevel",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_OwnerId",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_Department",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_IsExternalUser",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ManagerId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_ApprovalRequests_ApproverType",
                table: "ApprovalRequests");

            migrationBuilder.DropIndex(
                name: "IX_ApprovalRequests_DelegatedToId",
                table: "ApprovalRequests");

            migrationBuilder.DropColumn(
                name: "ArchivedAt",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "ConfidentialityLevel",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "LegalReviewCompleted",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "RequiresLegalReview",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "RetentionPeriodMonths",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "ScheduledDeletionDate",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "AccessExpiresAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CanDownload",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CanExport",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CanPrint",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsExternalUser",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastLoginAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ApproverType",
                table: "ApprovalRequests");

            migrationBuilder.DropColumn(
                name: "CanBlock",
                table: "ApprovalRequests");

            migrationBuilder.DropColumn(
                name: "DelegatedAt",
                table: "ApprovalRequests");

            migrationBuilder.DropColumn(
                name: "DelegatedToId",
                table: "ApprovalRequests");

            migrationBuilder.DropColumn(
                name: "DelegationReason",
                table: "ApprovalRequests");

            migrationBuilder.DropColumn(
                name: "IsRequired",
                table: "ApprovalRequests");

            migrationBuilder.DropColumn(
                name: "ReviewComment",
                table: "ApprovalRequests");

            migrationBuilder.DropColumn(
                name: "SuggestedChanges",
                table: "ApprovalRequests");
        }
    }
}
