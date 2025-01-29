using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderBook.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDecimalPrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "OrderBookItems");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "OrderBookItems");

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "OrderBookItems",
                type: "decimal(18,10)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "OrderBookItems",
                type: "decimal(18,10)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Amount",
                table: "OrderBookItems");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "OrderBookItems");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "OrderBookItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Value",
                table: "OrderBookItems",
                type: "decimal(18,2)",
                nullable: true);
        }
    }
}
