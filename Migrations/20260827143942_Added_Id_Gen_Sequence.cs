using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalWalletDemo.Migrations
{
    /// <inheritdoc />
    public partial class Added_Id_Gen_Sequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "TransactionIdSequence",
                startValue: 1001L);

            migrationBuilder.CreateSequence(
                name: "UserIdSequence",
                startValue: 1001L);

            migrationBuilder.CreateSequence(
                name: "WalletIdSequence",
                startValue: 1001L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropSequence(
                name: "TransactionIdSequence");

            migrationBuilder.DropSequence(
                name: "UserIdSequence");

            migrationBuilder.DropSequence(
                name: "WalletIdSequence");
        }
    }
}
