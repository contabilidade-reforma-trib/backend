using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Praxis.Modules.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "assinaturas_assinatura",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizacaoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Situacao = table.Column<int>(type: "integer", nullable: false),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assinaturas_assinatura", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "identidade_organizacao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RazaoSocial = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Documento = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_identidade_organizacao", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "assinaturas_direito_de_uso",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssinaturaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Produto = table.Column<int>(type: "integer", nullable: false),
                    InicioEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FimEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assinaturas_direito_de_uso", x => x.Id);
                    table.ForeignKey(
                        name: "FK_assinaturas_direito_de_uso_assinaturas_assinatura_Assinatur~",
                        column: x => x.AssinaturaId,
                        principalTable: "assinaturas_assinatura",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "assinaturas_pagamento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssinaturaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Meio = table.Column<int>(type: "integer", nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Confirmado = table.Column<bool>(type: "boolean", nullable: false),
                    ConfirmadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assinaturas_pagamento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_assinaturas_pagamento_assinaturas_assinatura_AssinaturaId",
                        column: x => x.AssinaturaId,
                        principalTable: "assinaturas_assinatura",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "identidade_usuario",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizacaoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    Papel = table.Column<int>(type: "integer", nullable: false),
                    RegistroProfissional = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    Telefone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_identidade_usuario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_identidade_usuario_identidade_organizacao_OrganizacaoId",
                        column: x => x.OrganizacaoId,
                        principalTable: "identidade_organizacao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "identidade_perfil_de_uso",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    AreaDeAtuacao = table.Column<int>(type: "integer", nullable: false),
                    RegimePredominante = table.Column<int>(type: "integer", nullable: false),
                    Setores = table.Column<string[]>(type: "text[]", nullable: false),
                    DorAtual = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_identidade_perfil_de_uso", x => x.Id);
                    table.ForeignKey(
                        name: "FK_identidade_perfil_de_uso_identidade_usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "identidade_usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_assinaturas_assinatura_OrganizacaoId",
                table: "assinaturas_assinatura",
                column: "OrganizacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_assinaturas_direito_de_uso_AssinaturaId_Produto",
                table: "assinaturas_direito_de_uso",
                columns: new[] { "AssinaturaId", "Produto" });

            migrationBuilder.CreateIndex(
                name: "IX_assinaturas_pagamento_AssinaturaId",
                table: "assinaturas_pagamento",
                column: "AssinaturaId");

            migrationBuilder.CreateIndex(
                name: "IX_identidade_organizacao_Documento",
                table: "identidade_organizacao",
                column: "Documento",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_identidade_perfil_de_uso_UsuarioId",
                table: "identidade_perfil_de_uso",
                column: "UsuarioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_identidade_usuario_Email",
                table: "identidade_usuario",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_identidade_usuario_OrganizacaoId",
                table: "identidade_usuario",
                column: "OrganizacaoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "assinaturas_direito_de_uso");

            migrationBuilder.DropTable(
                name: "assinaturas_pagamento");

            migrationBuilder.DropTable(
                name: "identidade_perfil_de_uso");

            migrationBuilder.DropTable(
                name: "assinaturas_assinatura");

            migrationBuilder.DropTable(
                name: "identidade_usuario");

            migrationBuilder.DropTable(
                name: "identidade_organizacao");
        }
    }
}
