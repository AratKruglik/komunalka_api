using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KomunalkaAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAddressTypeNamesToUkrainian : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update existing address type names to Ukrainian
            migrationBuilder.Sql(@"
                UPDATE address_types
                SET name = 'Квартира',
                    description = 'Багатоквартирний будинок у місті',
                    updated_at = NOW()
                WHERE name = 'Apartment';

                UPDATE address_types
                SET name = 'Приватний будинок',
                    description = 'Окрема садиба або дача',
                    updated_at = NOW()
                WHERE name = 'Private House';

                UPDATE address_types
                SET name = 'Офіс',
                    description = 'Комерційне або офісне приміщення',
                    updated_at = NOW()
                WHERE name = 'Office';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert address type names back to English
            migrationBuilder.Sql(@"
                UPDATE address_types
                SET name = 'Apartment',
                    description = 'Residential unit in a multi-unit building',
                    updated_at = NOW()
                WHERE name = 'Квартира';

                UPDATE address_types
                SET name = 'Private House',
                    description = 'Detached residential building',
                    updated_at = NOW()
                WHERE name = 'Приватний будинок';

                UPDATE address_types
                SET name = 'Office',
                    description = 'Commercial space for business operations',
                    updated_at = NOW()
                WHERE name = 'Офіс';
            ");
        }
    }
}
