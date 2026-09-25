using Microsoft.EntityFrameworkCore.Migrations;

namespace CouponHub.Infrastructure.Migrations;

public partial class AccountsAndPersonalization : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // PostgreSQL migration is transactional. Case-colliding legacy codes abort without
        // deleting user data; resolve the duplicates before retrying the migration.
        migrationBuilder.Sql("""
            CREATE TABLE accounts (
                "Id" uuid PRIMARY KEY,
                "Email" varchar(254) NOT NULL,
                "PasswordHash" varchar(500) NOT NULL,
                "IsAdmin" boolean NOT NULL,
                "Categories" text[] NOT NULL
            );
            CREATE UNIQUE INDEX "IX_accounts_Email" ON accounts ("Email");
            CREATE TABLE sessions (
                "TokenHash" varchar(64) PRIMARY KEY,
                "AccountId" uuid NOT NULL REFERENCES accounts("Id") ON DELETE CASCADE,
                "ExpiresAt" timestamptz NOT NULL
            );
            CREATE INDEX "IX_sessions_AccountId" ON sessions ("AccountId");
            CREATE INDEX "IX_sessions_ExpiresAt" ON sessions ("ExpiresAt");
            ALTER TABLE coupons ADD COLUMN owner_id uuid NULL;
            ALTER TABLE coupons ADD CONSTRAINT "FK_coupons_accounts_owner_id"
                FOREIGN KEY (owner_id) REFERENCES accounts("Id") ON DELETE CASCADE;
            UPDATE coupons SET coupon_code = upper(trim(coupon_code));
            DROP INDEX "IX_coupons_brand_id_coupon_code";
            CREATE UNIQUE INDEX "IX_coupons_brand_id_coupon_code" ON coupons (brand_id, coupon_code) WHERE owner_id IS NULL;
            CREATE UNIQUE INDEX "IX_coupons_owner_id_brand_id_coupon_code" ON coupons (owner_id, brand_id, coupon_code) WHERE owner_id IS NOT NULL;
            CREATE TABLE saved_coupons (
                "AccountId" uuid NOT NULL REFERENCES accounts("Id") ON DELETE CASCADE,
                "CouponId" uuid NOT NULL REFERENCES coupons(id) ON DELETE CASCADE,
                "SavedAt" timestamptz NOT NULL,
                PRIMARY KEY ("AccountId", "CouponId")
            );
            CREATE INDEX "IX_saved_coupons_CouponId" ON saved_coupons ("CouponId");
            CREATE TABLE coupon_events (
                "Id" uuid PRIMARY KEY,
                "AccountId" uuid NOT NULL REFERENCES accounts("Id") ON DELETE CASCADE,
                "CouponId" uuid NOT NULL REFERENCES coupons(id) ON DELETE CASCADE,
                "Kind" varchar(20) NOT NULL,
                "CreatedAt" timestamptz NOT NULL
            );
            CREATE INDEX "IX_coupon_events_AccountId_CreatedAt" ON coupon_events ("AccountId", "CreatedAt");
            CREATE INDEX "IX_coupon_events_CouponId" ON coupon_events ("CouponId");
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Do not merge private coupons into the old public-only schema during rollback.
        migrationBuilder.Sql("""
            DO $$ BEGIN
                IF EXISTS (SELECT 1 FROM coupons WHERE owner_id IS NOT NULL) THEN
                    RAISE EXCEPTION 'Export/remove private coupons before rolling back';
                END IF;
            END $$;
            DROP TABLE coupon_events;
            DROP TABLE saved_coupons;
            DROP TABLE sessions;
            DROP INDEX "IX_coupons_owner_id_brand_id_coupon_code";
            DROP INDEX "IX_coupons_brand_id_coupon_code";
            ALTER TABLE coupons DROP COLUMN owner_id;
            CREATE UNIQUE INDEX "IX_coupons_brand_id_coupon_code" ON coupons (brand_id, coupon_code);
            DROP TABLE accounts;
            """);
    }
}
