using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Kimos.Tests.PostgreSql.Migrations
{
    public partial class MakeIndexOnNameUnique : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TestEntities_Name",
                table: "TestEntities");

            migrationBuilder.CreateIndex(
                name: "IX_TestEntities_Name",
                table: "TestEntities",
                column: "Name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TestEntities_Name",
                table: "TestEntities");

            migrationBuilder.CreateIndex(
                name: "IX_TestEntities_Name",
                table: "TestEntities",
                column: "Name");
        }
    }
}
