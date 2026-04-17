using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace Backtrack.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CategoryCTIRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_post_matches_found_post_id",
                table: "post_matches");

            migrationBuilder.DropForeignKey(
                name: "fk_post_matches_lost_post_id",
                table: "post_matches");

            migrationBuilder.DropIndex(
                name: "ix_posts_item_search",
                table: "posts");

            migrationBuilder.DropIndex(
                name: "ix_post_matches_lost_post_id",
                table: "post_matches");

            migrationBuilder.DropIndex(
                name: "ux_post_matches_found_lost_active",
                table: "post_matches");

            migrationBuilder.DropColumn(
                name: "item_search",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "item_additional_details",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "item_brand",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "item_color",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "item_condition",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "item_distinctive_marks",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "item_material",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "item_name",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "item_size",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "assessment_summary",
                table: "post_matches");

            migrationBuilder.DropColumn(
                name: "distance_meters",
                table: "post_matches");

            migrationBuilder.DropColumn(
                name: "is_assessed",
                table: "post_matches");

            migrationBuilder.DropColumn(
                name: "match_score",
                table: "post_matches");

            migrationBuilder.DropColumn(
                name: "matching_level",
                table: "post_matches");

            migrationBuilder.RenameColumn(
                name: "item_category",
                table: "posts",
                newName: "category");

            migrationBuilder.RenameColumn(
                name: "time_gap_days",
                table: "post_matches",
                newName: "score");

            migrationBuilder.RenameColumn(
                name: "lost_post_id",
                table: "post_matches",
                newName: "source_post_id");

            migrationBuilder.RenameColumn(
                name: "found_post_id",
                table: "post_matches",
                newName: "candidate_post_id");

            migrationBuilder.RenameIndex(
                name: "ix_post_matches_found_post_id",
                table: "post_matches",
                newName: "IX_post_matches_candidate_post_id");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "posts",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Active",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "Active");

            migrationBuilder.AlterColumn<string>(
                name: "post_type",
                table: "posts",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "post_matching_status",
                table: "posts",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "embedding_status",
                table: "posts",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "subcategory_id",
                table: "posts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "match_reason",
                table: "post_matches",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "post_matches",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.CreateTable(
                name: "post_card_details",
                columns: table => new
                {
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    card_number_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    card_number_masked = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    holder_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    holder_name_normalized = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    date_of_birth = table.Column<DateOnly>(type: "date", nullable: true),
                    issue_date = table.Column<DateOnly>(type: "date", nullable: true),
                    expiry_date = table.Column<DateOnly>(type: "date", nullable: true),
                    issuing_authority = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    ocr_text = table.Column<string>(type: "text", nullable: true),
                    ai_description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_post_card_details", x => x.post_id);
                    table.ForeignKey(
                        name: "fk_post_card_details_post_id",
                        column: x => x.post_id,
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "post_electronic_details",
                columns: table => new
                {
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    brand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    color = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    has_case = table.Column<bool>(type: "boolean", nullable: true),
                    case_description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    screen_condition = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    lock_screen_description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    distinguishing_features = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ai_description = table.Column<string>(type: "text", nullable: true),
                    additional_details = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_post_electronic_details", x => x.post_id);
                    table.ForeignKey(
                        name: "fk_post_electronic_details_post_id",
                        column: x => x.post_id,
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "post_other_details",
                columns: table => new
                {
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_identifier = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    primary_color = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ai_description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_post_other_details", x => x.post_id);
                    table.ForeignKey(
                        name: "fk_post_other_details_post_id",
                        column: x => x.post_id,
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "post_personal_belonging_details",
                columns: table => new
                {
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    color = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    brand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    material = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    size = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    condition = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    distinctive_marks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ai_description = table.Column<string>(type: "text", nullable: true),
                    additional_details = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_post_personal_belonging_details", x => x.post_id);
                    table.ForeignKey(
                        name: "fk_post_personal_belonging_details_post_id",
                        column: x => x.post_id,
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "subcategories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subcategories", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_posts_match_filter",
                table: "posts",
                columns: new[] { "category", "post_type", "status", "event_time" });

            migrationBuilder.CreateIndex(
                name: "ix_posts_subcategory_id",
                table: "posts",
                column: "subcategory_id");

            migrationBuilder.CreateIndex(
                name: "ix_post_matches_by_source",
                table: "post_matches",
                columns: new[] { "source_post_id", "status", "score" });

            migrationBuilder.CreateIndex(
                name: "ix_post_matches_source_candidate",
                table: "post_matches",
                columns: new[] { "source_post_id", "candidate_post_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_post_card_details_card_number_hash",
                table: "post_card_details",
                column: "card_number_hash",
                filter: "card_number_hash IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_post_card_details_dob",
                table: "post_card_details",
                column: "date_of_birth",
                filter: "date_of_birth IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_post_electronic_details_brand_model",
                table: "post_electronic_details",
                columns: new[] { "brand", "model" });

            migrationBuilder.CreateIndex(
                name: "ix_subcategories_category",
                table: "subcategories",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "ix_subcategories_category_code",
                table: "subcategories",
                columns: new[] { "category", "code" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_post_matches_candidate_post_id",
                table: "post_matches",
                column: "candidate_post_id",
                principalTable: "posts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_post_matches_source_post_id",
                table: "post_matches",
                column: "source_post_id",
                principalTable: "posts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_posts_subcategory_id_subcategories_id",
                table: "posts",
                column: "subcategory_id",
                principalTable: "subcategories",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            // pg_trgm for fuzzy card holder-name search
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");
            migrationBuilder.Sql(@"
                CREATE INDEX ix_post_card_details_holder_name_trgm
                ON post_card_details USING gin (holder_name_normalized gin_trgm_ops)
                WHERE holder_name_normalized IS NOT NULL;");

            // ── Subcategory seed data ────────────────────────────────────────
            migrationBuilder.Sql(@"
                INSERT INTO subcategories (id, category, code, name, display_order, is_active, created_at) VALUES
                -- Electronics (10 rows)
                (gen_random_uuid(), 'Electronics', 'phone',          'Phone',          1,  true, NOW()),
                (gen_random_uuid(), 'Electronics', 'laptop',         'Laptop',         2,  true, NOW()),
                (gen_random_uuid(), 'Electronics', 'smartwatch',     'Smartwatch',     3,  true, NOW()),
                (gen_random_uuid(), 'Electronics', 'charger_adapter','Charger Adapter',4,  true, NOW()),
                (gen_random_uuid(), 'Electronics', 'mouse',          'Mouse',          5,  true, NOW()),
                (gen_random_uuid(), 'Electronics', 'keyboard',       'Keyboard',       6,  true, NOW()),
                (gen_random_uuid(), 'Electronics', 'powerbank',      'Powerbank',      7,  true, NOW()),
                (gen_random_uuid(), 'Electronics', 'power_outlet',   'Power Outlet',   8,  true, NOW()),
                (gen_random_uuid(), 'Electronics', 'headphone',      'Headphone',      9,  true, NOW()),
                (gen_random_uuid(), 'Electronics', 'earphone',       'Earphone',       10, true, NOW()),
                -- Cards (7 rows)
                (gen_random_uuid(), 'Cards', 'identification_card', 'Identification Card', 1, true, NOW()),
                (gen_random_uuid(), 'Cards', 'passport',            'Passport',            2, true, NOW()),
                (gen_random_uuid(), 'Cards', 'driver_license',      'Driver License',      3, true, NOW()),
                (gen_random_uuid(), 'Cards', 'personal_card',       'Personal Card',       4, true, NOW()),
                (gen_random_uuid(), 'Cards', 'bank_card',           'Bank Card',           5, true, NOW()),
                (gen_random_uuid(), 'Cards', 'student_card',        'Student Card',        6, true, NOW()),
                (gen_random_uuid(), 'Cards', 'company_card',        'Company Card',        7, true, NOW()),
                -- PersonalBelongings (6 rows)
                (gen_random_uuid(), 'PersonalBelongings', 'wallets',   'Wallets',   1, true, NOW()),
                (gen_random_uuid(), 'PersonalBelongings', 'keys',      'Keys',      2, true, NOW()),
                (gen_random_uuid(), 'PersonalBelongings', 'suitcases', 'Suitcases', 3, true, NOW()),
                (gen_random_uuid(), 'PersonalBelongings', 'backpack',  'Backpack',  4, true, NOW()),
                (gen_random_uuid(), 'PersonalBelongings', 'clothings', 'Clothings', 5, true, NOW()),
                (gen_random_uuid(), 'PersonalBelongings', 'jewelry',   'Jewelry',   6, true, NOW());
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS ix_post_card_details_holder_name_trgm;");

            migrationBuilder.DropForeignKey(
                name: "fk_post_matches_candidate_post_id",
                table: "post_matches");

            migrationBuilder.DropForeignKey(
                name: "fk_post_matches_source_post_id",
                table: "post_matches");

            migrationBuilder.DropForeignKey(
                name: "fk_posts_subcategory_id_subcategories_id",
                table: "posts");

            migrationBuilder.DropTable(
                name: "post_card_details");

            migrationBuilder.DropTable(
                name: "post_electronic_details");

            migrationBuilder.DropTable(
                name: "post_other_details");

            migrationBuilder.DropTable(
                name: "post_personal_belonging_details");

            migrationBuilder.DropTable(
                name: "subcategories");

            migrationBuilder.DropIndex(
                name: "ix_posts_match_filter",
                table: "posts");

            migrationBuilder.DropIndex(
                name: "ix_posts_subcategory_id",
                table: "posts");

            migrationBuilder.DropIndex(
                name: "ix_post_matches_by_source",
                table: "post_matches");

            migrationBuilder.DropIndex(
                name: "ix_post_matches_source_candidate",
                table: "post_matches");

            migrationBuilder.DropColumn(
                name: "subcategory_id",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "match_reason",
                table: "post_matches");

            migrationBuilder.DropColumn(
                name: "status",
                table: "post_matches");

            migrationBuilder.RenameColumn(
                name: "category",
                table: "posts",
                newName: "item_category");

            migrationBuilder.RenameColumn(
                name: "source_post_id",
                table: "post_matches",
                newName: "lost_post_id");

            migrationBuilder.RenameColumn(
                name: "score",
                table: "post_matches",
                newName: "time_gap_days");

            migrationBuilder.RenameColumn(
                name: "candidate_post_id",
                table: "post_matches",
                newName: "found_post_id");

            migrationBuilder.RenameIndex(
                name: "IX_post_matches_candidate_post_id",
                table: "post_matches",
                newName: "ix_post_matches_found_post_id");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "posts",
                type: "text",
                nullable: false,
                defaultValue: "Active",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Active");

            migrationBuilder.AlterColumn<string>(
                name: "post_type",
                table: "posts",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "post_matching_status",
                table: "posts",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "embedding_status",
                table: "posts",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<string>(
                name: "item_additional_details",
                table: "posts",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "item_brand",
                table: "posts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "item_color",
                table: "posts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "item_condition",
                table: "posts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "item_distinctive_marks",
                table: "posts",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "item_material",
                table: "posts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "item_name",
                table: "posts",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "item_size",
                table: "posts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "assessment_summary",
                table: "post_matches",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<float>(
                name: "distance_meters",
                table: "post_matches",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<bool>(
                name: "is_assessed",
                table: "post_matches",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<float>(
                name: "match_score",
                table: "post_matches",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<string>(
                name: "matching_level",
                table: "post_matches",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "item_search",
                table: "posts",
                type: "tsvector",
                nullable: true,
                computedColumnSql: "setweight(to_tsvector('english', coalesce(item_name, '')),               'A') || setweight(to_tsvector('english', coalesce(item_category, '') || ' ' || coalesce(item_brand, '')), 'B') || setweight(to_tsvector('english', coalesce(item_color, '')     || ' ' || coalesce(item_condition, '') || ' ' || coalesce(item_material, '') || ' ' || coalesce(item_size, '') || ' ' || coalesce(item_distinctive_marks, '')), 'C') || setweight(to_tsvector('english', coalesce(item_additional_details, '')), 'D')",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "ix_posts_item_search",
                table: "posts",
                column: "item_search")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "ix_post_matches_lost_post_id",
                table: "post_matches",
                column: "lost_post_id");

            migrationBuilder.CreateIndex(
                name: "ux_post_matches_found_lost_active",
                table: "post_matches",
                columns: new[] { "found_post_id", "lost_post_id" },
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.AddForeignKey(
                name: "fk_post_matches_found_post_id",
                table: "post_matches",
                column: "found_post_id",
                principalTable: "posts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_post_matches_lost_post_id",
                table: "post_matches",
                column: "lost_post_id",
                principalTable: "posts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
