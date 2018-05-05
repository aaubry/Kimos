// Copyright (C) 2018 Antoine Aubry
// 
// This file is part of Kimos.
// 
// Kimos is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Kimos is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Kimos.  If not, see <http://www.gnu.org/licenses/>.
// 

using Microsoft.EntityFrameworkCore.Migrations;

namespace Kimos.Tests.SqlServer.Migrations
{
    public partial class AddIndexOnName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "TestEntities",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TestEntities_Name",
                table: "TestEntities",
                column: "Name");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TestEntities_Name",
                table: "TestEntities");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "TestEntities",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
