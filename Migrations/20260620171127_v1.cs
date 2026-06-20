using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medicare.Migrations
{
    /// <inheritdoc />
    public partial class v1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEMXQDYgEPH9uCgxgnrQdyLcObiljD6zoDQmXRdbSaMDo5VZnflZT8+ZVZm/9msRxOQ==");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAELlliuXQE5iCpDzdIKQLosBC7Ytbpxz49/kGz54N9k7NdepXOs3eyk/nBV0MiDRAqw==");
        }
    }
}
