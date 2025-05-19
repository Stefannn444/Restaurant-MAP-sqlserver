using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantAppSQLSERVER.Migrations
{
    /// <inheritdoc />
    public partial class SubtotalOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DishAllergens_Allergens_AllergenId",
                table: "DishAllergens");

            migrationBuilder.DropForeignKey(
                name: "FK_DishAllergens_Dishes_DishId",
                table: "DishAllergens");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Users_UserId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_OrderCode",
                table: "Orders");

            migrationBuilder.AlterColumn<string>(
                name: "OrderCode",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<decimal>(
                name: "Subtotal",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "DisplayMenuItem",
                columns: table => new
                {
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ItemPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ItemPhotoPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ItemType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuantityDisplay = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AllergensString = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MenuItemComponentsString = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    CategoryName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Email", "Nr_tel", "Nume", "Prenume" },
                values: new object[] { "client@exemplu.com", "0700000000", "Client", "Exemplu" });

            /*migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Adresa", "Email", "Nr_tel", "Nume", "Parola", "Prenume", "Rol" },
                values: new object[] { 2, "Sediul Restaurantului", "angajat@exemplu.com", "0711111111", "Angajat", "parolaangajat", "Restaurant", 1 });
*/
            migrationBuilder.AddForeignKey(
                name: "FK_DishAllergens_Allergens_AllergenId",
                table: "DishAllergens",
                column: "AllergenId",
                principalTable: "Allergens",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DishAllergens_Dishes_DishId",
                table: "DishAllergens",
                column: "DishId",
                principalTable: "Dishes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Users_UserId",
                table: "Orders",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DishAllergens_Allergens_AllergenId",
                table: "DishAllergens");

            migrationBuilder.DropForeignKey(
                name: "FK_DishAllergens_Dishes_DishId",
                table: "DishAllergens");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Users_UserId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "DisplayMenuItem");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DropColumn(
                name: "Subtotal",
                table: "Orders");

            migrationBuilder.AlterColumn<string>(
                name: "OrderCode",
                table: "Orders",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Email", "Nr_tel", "Nume", "Prenume" },
                values: new object[] { "ion.popescu@example.com", "0712345678", "Ion", "Popescu" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderCode",
                table: "Orders",
                column: "OrderCode",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DishAllergens_Allergens_AllergenId",
                table: "DishAllergens",
                column: "AllergenId",
                principalTable: "Allergens",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DishAllergens_Dishes_DishId",
                table: "DishAllergens",
                column: "DishId",
                principalTable: "Dishes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Users_UserId",
                table: "Orders",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
