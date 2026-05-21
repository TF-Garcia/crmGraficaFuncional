using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrintFlowApi.Migrations
{
    /// <inheritdoc />
    public partial class CustomerSelfServiceSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowCustomerOrderCancellation",
                table: "SystemSettings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AllowCustomerOrderEdit",
                table: "SystemSettings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AllowCustomerQuoteEdit",
                table: "SystemSettings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AllowCustomerRefundRequest",
                table: "SystemSettings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("""
                UPDATE SystemSettings
                SET AllowCustomerQuoteEdit = 1,
                    AllowCustomerOrderCancellation = 1;
                """);

            migrationBuilder.Sql("""
                UPDATE Orders
                SET PaymentStatus = 2,
                    Status = 2
                WHERE PaymentMethod = 2
                  AND PaymentStatus = 1;
                """);

            migrationBuilder.Sql("""
                UPDATE Payments
                SET Status = 2,
                    PaidAt = COALESCE(PaidAt, UTC_TIMESTAMP()),
                    UpdatedAt = UTC_TIMESTAMP()
                WHERE Method = 2
                  AND Status = 1;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowCustomerOrderCancellation",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "AllowCustomerOrderEdit",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "AllowCustomerQuoteEdit",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "AllowCustomerRefundRequest",
                table: "SystemSettings");
        }
    }
}
