using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostgresRangeTypes.Migrations
{
    /// <inheritdoc />
    public partial class ExcludeConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"Alter Table ""Meetings""
                                   Add Constraint ""NoOverlap""
                                   EXCLUDE USING gist (""RoomId"" WITH =, ""Time"" WITH &&)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"ALTER TABLE ""Meetings""
                                   DROP CONSTRAINT ""NoOverlap"";");
        }
    }
}