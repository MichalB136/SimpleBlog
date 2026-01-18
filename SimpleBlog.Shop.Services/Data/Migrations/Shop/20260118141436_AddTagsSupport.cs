using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleBlog.Shop.Services.Data.Migrations.Shop
{
    /// <inheritdoc />
    public partial class AddTagsSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Note: Tags table is shared between BlogDbContext and ShopDbContext.
            // It is created by BlogDbContext migration (20260118141436_AddTagsSupport).
            // This migration only creates the ProductTags junction table.
            // Ensure BlogDbContext migrations run first to create Tags table.
            
            migrationBuilder.CreateTable(
                name: "ProductTags",
                columns: table => new
                {
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductTags", x => new { x.ProductId, x.TagId });
                    table.ForeignKey(
                        name: "FK_ProductTags_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductTags_TagId",
                table: "ProductTags",
                column: "TagId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductTags");
            
            // Note: Tags table is NOT dropped here as it's managed by BlogDbContext
            // and may still be in use by PostTags
        }
    }
}
