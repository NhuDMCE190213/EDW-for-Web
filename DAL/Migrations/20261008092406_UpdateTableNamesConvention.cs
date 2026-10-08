using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTableNamesConvention : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_cart_item_product_variant_product_variant_id",
                table: "cart_item");

            migrationBuilder.DropForeignKey(
                name: "fk_order_customer_customer_id",
                table: "order");

            migrationBuilder.DropForeignKey(
                name: "fk_order_item_order_order_id",
                table: "order_item");

            migrationBuilder.DropForeignKey(
                name: "fk_order_item_product_variant_product_variant_id",
                table: "order_item");

            migrationBuilder.DropForeignKey(
                name: "fk_product_review_customer_customer_id",
                table: "product_review");

            migrationBuilder.DropForeignKey(
                name: "fk_product_review_product_product_id",
                table: "product_review");

            migrationBuilder.DropPrimaryKey(
                name: "pk_product_review",
                table: "product_review");

            migrationBuilder.DropPrimaryKey(
                name: "pk_order_item",
                table: "order_item");

            migrationBuilder.DropPrimaryKey(
                name: "pk_order",
                table: "order");

            migrationBuilder.DropPrimaryKey(
                name: "pk_customer",
                table: "customer");

            migrationBuilder.DropPrimaryKey(
                name: "pk_cart_item",
                table: "cart_item");

            migrationBuilder.RenameTable(
                name: "product_review",
                newName: "product_reviews");

            migrationBuilder.RenameTable(
                name: "order_item",
                newName: "order_items");

            migrationBuilder.RenameTable(
                name: "order",
                newName: "orders");

            migrationBuilder.RenameTable(
                name: "customer",
                newName: "customers");

            migrationBuilder.RenameTable(
                name: "cart_item",
                newName: "cart_items");

            migrationBuilder.RenameIndex(
                name: "IX_Product_ProductName",
                table: "products",
                newName: "ix_products_product_name");

            migrationBuilder.RenameIndex(
                name: "IX_ProductVariant_Sku",
                table: "product_variants",
                newName: "ix_product_variants_sku");

            migrationBuilder.RenameIndex(
                name: "IX_ProductVariant_Price",
                table: "product_variants",
                newName: "ix_product_variants_price");

            migrationBuilder.RenameIndex(
                name: "ix_product_review_product_id",
                table: "product_reviews",
                newName: "ix_product_reviews_product_id");

            migrationBuilder.RenameIndex(
                name: "ix_product_review_customer_id",
                table: "product_reviews",
                newName: "ix_product_reviews_customer_id");

            migrationBuilder.RenameIndex(
                name: "ix_customer_email",
                table: "customers",
                newName: "ix_customers_email");

            migrationBuilder.RenameIndex(
                name: "ix_cart_item_product_variant_id",
                table: "cart_items",
                newName: "ix_cart_items_product_variant_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_product_reviews",
                table: "product_reviews",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_order_items",
                table: "order_items",
                column: "order_item_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_orders",
                table: "orders",
                column: "order_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_customers",
                table: "customers",
                column: "customer_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_cart_items",
                table: "cart_items",
                column: "cart_item_id");

            migrationBuilder.AddForeignKey(
                name: "fk_cart_items_product_variant_product_variant_id",
                table: "cart_items",
                column: "product_variant_id",
                principalTable: "product_variants",
                principalColumn: "product_variant_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_order_items_orders_order_id",
                table: "order_items",
                column: "order_id",
                principalTable: "orders",
                principalColumn: "order_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_order_items_product_variant_product_variant_id",
                table: "order_items",
                column: "product_variant_id",
                principalTable: "product_variants",
                principalColumn: "product_variant_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_orders_customers_customer_id",
                table: "orders",
                column: "customer_id",
                principalTable: "customers",
                principalColumn: "customer_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_product_reviews_customers_customer_id",
                table: "product_reviews",
                column: "customer_id",
                principalTable: "customers",
                principalColumn: "customer_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_product_reviews_products_product_id",
                table: "product_reviews",
                column: "product_id",
                principalTable: "products",
                principalColumn: "product_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_cart_items_product_variant_product_variant_id",
                table: "cart_items");

            migrationBuilder.DropForeignKey(
                name: "fk_order_items_orders_order_id",
                table: "order_items");

            migrationBuilder.DropForeignKey(
                name: "fk_order_items_product_variant_product_variant_id",
                table: "order_items");

            migrationBuilder.DropForeignKey(
                name: "fk_orders_customers_customer_id",
                table: "orders");

            migrationBuilder.DropForeignKey(
                name: "fk_product_reviews_customers_customer_id",
                table: "product_reviews");

            migrationBuilder.DropForeignKey(
                name: "fk_product_reviews_products_product_id",
                table: "product_reviews");

            migrationBuilder.DropPrimaryKey(
                name: "pk_product_reviews",
                table: "product_reviews");

            migrationBuilder.DropPrimaryKey(
                name: "pk_orders",
                table: "orders");

            migrationBuilder.DropPrimaryKey(
                name: "pk_order_items",
                table: "order_items");

            migrationBuilder.DropPrimaryKey(
                name: "pk_customers",
                table: "customers");

            migrationBuilder.DropPrimaryKey(
                name: "pk_cart_items",
                table: "cart_items");

            migrationBuilder.RenameTable(
                name: "product_reviews",
                newName: "product_review");

            migrationBuilder.RenameTable(
                name: "orders",
                newName: "order");

            migrationBuilder.RenameTable(
                name: "order_items",
                newName: "order_item");

            migrationBuilder.RenameTable(
                name: "customers",
                newName: "customer");

            migrationBuilder.RenameTable(
                name: "cart_items",
                newName: "cart_item");

            migrationBuilder.RenameIndex(
                name: "ix_products_product_name",
                table: "products",
                newName: "IX_Product_ProductName");

            migrationBuilder.RenameIndex(
                name: "ix_product_variants_sku",
                table: "product_variants",
                newName: "IX_ProductVariant_Sku");

            migrationBuilder.RenameIndex(
                name: "ix_product_variants_price",
                table: "product_variants",
                newName: "IX_ProductVariant_Price");

            migrationBuilder.RenameIndex(
                name: "ix_product_reviews_product_id",
                table: "product_review",
                newName: "ix_product_review_product_id");

            migrationBuilder.RenameIndex(
                name: "ix_product_reviews_customer_id",
                table: "product_review",
                newName: "ix_product_review_customer_id");

            migrationBuilder.RenameIndex(
                name: "ix_customers_email",
                table: "customer",
                newName: "ix_customer_email");

            migrationBuilder.RenameIndex(
                name: "ix_cart_items_product_variant_id",
                table: "cart_item",
                newName: "ix_cart_item_product_variant_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_product_review",
                table: "product_review",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_order",
                table: "order",
                column: "order_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_order_item",
                table: "order_item",
                column: "order_item_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_customer",
                table: "customer",
                column: "customer_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_cart_item",
                table: "cart_item",
                column: "cart_item_id");

            migrationBuilder.AddForeignKey(
                name: "fk_cart_item_product_variant_product_variant_id",
                table: "cart_item",
                column: "product_variant_id",
                principalTable: "product_variants",
                principalColumn: "product_variant_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_order_customer_customer_id",
                table: "order",
                column: "customer_id",
                principalTable: "customer",
                principalColumn: "customer_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_order_item_order_order_id",
                table: "order_item",
                column: "order_id",
                principalTable: "order",
                principalColumn: "order_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_order_item_product_variant_product_variant_id",
                table: "order_item",
                column: "product_variant_id",
                principalTable: "product_variants",
                principalColumn: "product_variant_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_product_review_customer_customer_id",
                table: "product_review",
                column: "customer_id",
                principalTable: "customer",
                principalColumn: "customer_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_product_review_product_product_id",
                table: "product_review",
                column: "product_id",
                principalTable: "products",
                principalColumn: "product_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
