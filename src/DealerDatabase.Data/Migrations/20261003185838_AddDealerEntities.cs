using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DealerDatabase.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDealerEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompanyNumber",
                table: "Dealers",
                type: "TEXT",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyStatus",
                table: "Dealers",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "Dealers",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Domain",
                table: "Dealers",
                type: "TEXT",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FcaReferenceNumber",
                table: "Dealers",
                type: "TEXT",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FcaStatus",
                table: "Dealers",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "IcoExpiryDate",
                table: "Dealers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IcoRegistrationNumber",
                table: "Dealers",
                type: "TEXT",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "IncorporationDate",
                table: "Dealers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LineageTag",
                table: "Dealers",
                type: "TEXT",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedOn",
                table: "Dealers",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "SafExpiryDate",
                table: "Dealers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SafStatus",
                table: "Dealers",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VatNumber",
                table: "Dealers",
                type: "TEXT",
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DealerId = table.Column<int>(type: "INTEGER", nullable: false),
                    AddressType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    RawAddress = table.Column<string>(type: "TEXT", nullable: true),
                    AddressLine1 = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    AddressLine2 = table.Column<string>(type: "TEXT", nullable: true),
                    AddressLine3 = table.Column<string>(type: "TEXT", nullable: true),
                    City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    County = table.Column<string>(type: "TEXT", nullable: true),
                    Country = table.Column<string>(type: "TEXT", nullable: true),
                    Postcode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Source = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Addresses_Dealers_DealerId",
                        column: x => x.DealerId,
                        principalTable: "Dealers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Contacts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DealerId = table.Column<int>(type: "INTEGER", nullable: false),
                    ContactType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Value = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                    Source = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contacts_Dealers_DealerId",
                        column: x => x.DealerId,
                        principalTable: "Dealers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Directors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DealerId = table.Column<int>(type: "INTEGER", nullable: false),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    Role = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Source = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    AppointedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ResignedOn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Directors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Directors_Dealers_DealerId",
                        column: x => x.DealerId,
                        principalTable: "Dealers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DealerId = table.Column<int>(type: "INTEGER", nullable: false),
                    SourceName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    RawPayloadJson = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sources_Dealers_DealerId",
                        column: x => x.DealerId,
                        principalTable: "Dealers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TradingNames",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DealerId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradingNames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TradingNames_Dealers_DealerId",
                        column: x => x.DealerId,
                        principalTable: "Dealers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Dealers_CompanyNumber",
                table: "Dealers",
                column: "CompanyNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Dealers_FcaReferenceNumber",
                table: "Dealers",
                column: "FcaReferenceNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Dealers_VatNumber",
                table: "Dealers",
                column: "VatNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_DealerId",
                table: "Addresses",
                column: "DealerId");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_Postcode",
                table: "Addresses",
                column: "Postcode");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_DealerId",
                table: "Contacts",
                column: "DealerId");

            migrationBuilder.CreateIndex(
                name: "IX_Directors_DealerId",
                table: "Directors",
                column: "DealerId");

            migrationBuilder.CreateIndex(
                name: "IX_Sources_DealerId",
                table: "Sources",
                column: "DealerId");

            migrationBuilder.CreateIndex(
                name: "IX_TradingNames_DealerId",
                table: "TradingNames",
                column: "DealerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "Contacts");

            migrationBuilder.DropTable(
                name: "Directors");

            migrationBuilder.DropTable(
                name: "Sources");

            migrationBuilder.DropTable(
                name: "TradingNames");

            migrationBuilder.DropIndex(
                name: "IX_Dealers_CompanyNumber",
                table: "Dealers");

            migrationBuilder.DropIndex(
                name: "IX_Dealers_FcaReferenceNumber",
                table: "Dealers");

            migrationBuilder.DropIndex(
                name: "IX_Dealers_VatNumber",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "CompanyNumber",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "CompanyStatus",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "Domain",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "FcaReferenceNumber",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "FcaStatus",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "IcoExpiryDate",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "IcoRegistrationNumber",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "IncorporationDate",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "LineageTag",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "ModifiedOn",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "SafExpiryDate",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "SafStatus",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "VatNumber",
                table: "Dealers");
        }
    }
}
