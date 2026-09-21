using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TEAprendo.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIdentityYTraducirTablas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                table: "AspNetRoleClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_Classrooms_Sites_SiteId",
                table: "Classrooms");

            migrationBuilder.DropForeignKey(
                name: "FK_Sites_Institutions_InstitutionId",
                table: "Sites");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Sites",
                table: "Sites");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Institutions",
                table: "Institutions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Classrooms",
                table: "Classrooms");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserTokens",
                table: "AspNetUserTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUsers",
                table: "AspNetUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserRoles",
                table: "AspNetUserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserLogins",
                table: "AspNetUserLogins");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserClaims",
                table: "AspNetUserClaims");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetRoles",
                table: "AspNetRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetRoleClaims",
                table: "AspNetRoleClaims");

            migrationBuilder.RenameTable(
                name: "Sites",
                newName: "sedes");

            migrationBuilder.RenameTable(
                name: "Institutions",
                newName: "instituciones");

            migrationBuilder.RenameTable(
                name: "Classrooms",
                newName: "aulas");

            migrationBuilder.RenameTable(
                name: "AspNetUserTokens",
                newName: "usuarios_tokens");

            migrationBuilder.RenameTable(
                name: "AspNetUsers",
                newName: "usuarios");

            migrationBuilder.RenameTable(
                name: "AspNetUserRoles",
                newName: "usuarios_roles");

            migrationBuilder.RenameTable(
                name: "AspNetUserLogins",
                newName: "usuarios_accesos");

            migrationBuilder.RenameTable(
                name: "AspNetUserClaims",
                newName: "usuarios_atributos");

            migrationBuilder.RenameTable(
                name: "AspNetRoles",
                newName: "roles");

            migrationBuilder.RenameTable(
                name: "AspNetRoleClaims",
                newName: "roles_atributos");

            migrationBuilder.RenameIndex(
                name: "IX_Sites_InstitutionId",
                table: "sedes",
                newName: "IX_sedes_InstitutionId");

            migrationBuilder.RenameIndex(
                name: "IX_Classrooms_SiteId",
                table: "aulas",
                newName: "IX_aulas_SiteId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "usuarios_roles",
                newName: "IX_usuarios_roles_RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "usuarios_accesos",
                newName: "IX_usuarios_accesos_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "usuarios_atributos",
                newName: "IX_usuarios_atributos_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "roles_atributos",
                newName: "IX_roles_atributos_RoleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_sedes",
                table: "sedes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_instituciones",
                table: "instituciones",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_aulas",
                table: "aulas",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_usuarios_tokens",
                table: "usuarios_tokens",
                columns: new[] { "UserId", "LoginProvider", "Name" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_usuarios",
                table: "usuarios",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_usuarios_roles",
                table: "usuarios_roles",
                columns: new[] { "UserId", "RoleId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_usuarios_accesos",
                table: "usuarios_accesos",
                columns: new[] { "LoginProvider", "ProviderKey" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_usuarios_atributos",
                table: "usuarios_atributos",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_roles",
                table: "roles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_roles_atributos",
                table: "roles_atributos",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_aulas_sedes_SiteId",
                table: "aulas",
                column: "SiteId",
                principalTable: "sedes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_roles_atributos_roles_RoleId",
                table: "roles_atributos",
                column: "RoleId",
                principalTable: "roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_sedes_instituciones_InstitutionId",
                table: "sedes",
                column: "InstitutionId",
                principalTable: "instituciones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_usuarios_accesos_usuarios_UserId",
                table: "usuarios_accesos",
                column: "UserId",
                principalTable: "usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_usuarios_atributos_usuarios_UserId",
                table: "usuarios_atributos",
                column: "UserId",
                principalTable: "usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_usuarios_roles_roles_RoleId",
                table: "usuarios_roles",
                column: "RoleId",
                principalTable: "roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_usuarios_roles_usuarios_UserId",
                table: "usuarios_roles",
                column: "UserId",
                principalTable: "usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_usuarios_tokens_usuarios_UserId",
                table: "usuarios_tokens",
                column: "UserId",
                principalTable: "usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_aulas_sedes_SiteId",
                table: "aulas");

            migrationBuilder.DropForeignKey(
                name: "FK_roles_atributos_roles_RoleId",
                table: "roles_atributos");

            migrationBuilder.DropForeignKey(
                name: "FK_sedes_instituciones_InstitutionId",
                table: "sedes");

            migrationBuilder.DropForeignKey(
                name: "FK_usuarios_accesos_usuarios_UserId",
                table: "usuarios_accesos");

            migrationBuilder.DropForeignKey(
                name: "FK_usuarios_atributos_usuarios_UserId",
                table: "usuarios_atributos");

            migrationBuilder.DropForeignKey(
                name: "FK_usuarios_roles_roles_RoleId",
                table: "usuarios_roles");

            migrationBuilder.DropForeignKey(
                name: "FK_usuarios_roles_usuarios_UserId",
                table: "usuarios_roles");

            migrationBuilder.DropForeignKey(
                name: "FK_usuarios_tokens_usuarios_UserId",
                table: "usuarios_tokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_usuarios_tokens",
                table: "usuarios_tokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_usuarios_roles",
                table: "usuarios_roles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_usuarios_atributos",
                table: "usuarios_atributos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_usuarios_accesos",
                table: "usuarios_accesos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_usuarios",
                table: "usuarios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_sedes",
                table: "sedes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_roles_atributos",
                table: "roles_atributos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_roles",
                table: "roles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_instituciones",
                table: "instituciones");

            migrationBuilder.DropPrimaryKey(
                name: "PK_aulas",
                table: "aulas");

            migrationBuilder.RenameTable(
                name: "usuarios_tokens",
                newName: "AspNetUserTokens");

            migrationBuilder.RenameTable(
                name: "usuarios_roles",
                newName: "AspNetUserRoles");

            migrationBuilder.RenameTable(
                name: "usuarios_atributos",
                newName: "AspNetUserClaims");

            migrationBuilder.RenameTable(
                name: "usuarios_accesos",
                newName: "AspNetUserLogins");

            migrationBuilder.RenameTable(
                name: "usuarios",
                newName: "AspNetUsers");

            migrationBuilder.RenameTable(
                name: "sedes",
                newName: "Sites");

            migrationBuilder.RenameTable(
                name: "roles_atributos",
                newName: "AspNetRoleClaims");

            migrationBuilder.RenameTable(
                name: "roles",
                newName: "AspNetRoles");

            migrationBuilder.RenameTable(
                name: "instituciones",
                newName: "Institutions");

            migrationBuilder.RenameTable(
                name: "aulas",
                newName: "Classrooms");

            migrationBuilder.RenameIndex(
                name: "IX_usuarios_roles_RoleId",
                table: "AspNetUserRoles",
                newName: "IX_AspNetUserRoles_RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_usuarios_atributos_UserId",
                table: "AspNetUserClaims",
                newName: "IX_AspNetUserClaims_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_usuarios_accesos_UserId",
                table: "AspNetUserLogins",
                newName: "IX_AspNetUserLogins_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_sedes_InstitutionId",
                table: "Sites",
                newName: "IX_Sites_InstitutionId");

            migrationBuilder.RenameIndex(
                name: "IX_roles_atributos_RoleId",
                table: "AspNetRoleClaims",
                newName: "IX_AspNetRoleClaims_RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_aulas_SiteId",
                table: "Classrooms",
                newName: "IX_Classrooms_SiteId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserTokens",
                table: "AspNetUserTokens",
                columns: new[] { "UserId", "LoginProvider", "Name" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserRoles",
                table: "AspNetUserRoles",
                columns: new[] { "UserId", "RoleId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserClaims",
                table: "AspNetUserClaims",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserLogins",
                table: "AspNetUserLogins",
                columns: new[] { "LoginProvider", "ProviderKey" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUsers",
                table: "AspNetUsers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Sites",
                table: "Sites",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetRoleClaims",
                table: "AspNetRoleClaims",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetRoles",
                table: "AspNetRoles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Institutions",
                table: "Institutions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Classrooms",
                table: "Classrooms",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Classrooms_Sites_SiteId",
                table: "Classrooms",
                column: "SiteId",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Sites_Institutions_InstitutionId",
                table: "Sites",
                column: "InstitutionId",
                principalTable: "Institutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
