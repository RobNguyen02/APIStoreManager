﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIStoreManager.Migrations
{
    /// <inheritdoc />
    public partial class AddShopNameUniqueConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Shops_Name",
                table: "Shops",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Shops_Name",
                table: "Shops");
        }
    }
}
