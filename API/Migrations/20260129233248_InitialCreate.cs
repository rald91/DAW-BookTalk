using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccessLog",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessLog", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Autor",
                columns: table => new
                {
                    id_autor = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Autor__C252147DA63CC2B9", x => x.id_autor);
                });

            migrationBuilder.CreateTable(
                name: "Editora",
                columns: table => new
                {
                    id_editora = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Editora__C252147DA63CC2B9", x => x.id_editora);
                });

            migrationBuilder.CreateTable(
                name: "Genero",
                columns: table => new
                {
                    id_genero = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Genero__C252147DA63CC2B9", x => x.id_genero);
                });

            migrationBuilder.CreateTable(
                name: "Utilizador",
                columns: table => new
                {
                    id_utilizador = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    tipo_utilizador = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Utilizad__71C536835F89D131", x => x.id_utilizador);
                });

            migrationBuilder.CreateTable(
                name: "Livro",
                columns: table => new
                {
                    id_livro = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    titulo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    isbn = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    idioma = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ano_publicacao = table.Column<DateOnly>(type: "date", nullable: true),
                    sinopse = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "ativo"),
                    capa_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    id_editora = table.Column<int>(type: "int", nullable: true),
                    clicks = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Livro__C252147DA63CC2B9", x => x.id_livro);
                    table.ForeignKey(
                        name: "FK_Livro_Editora",
                        column: x => x.id_editora,
                        principalTable: "Editora",
                        principalColumn: "id_editora");
                });

            migrationBuilder.CreateTable(
                name: "Reserva",
                columns: table => new
                {
                    id_reserva = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_utilizador = table.Column<int>(type: "int", nullable: false),
                    data_inicio = table.Column<DateOnly>(type: "date", nullable: false),
                    data_fim = table.Column<DateOnly>(type: "date", nullable: true),
                    Confirmada = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Reserva__423CBE5D3753DD0B", x => x.id_reserva);
                    table.ForeignKey(
                        name: "FK_Reserva_Utilizador",
                        column: x => x.id_utilizador,
                        principalTable: "Utilizador",
                        principalColumn: "id_utilizador",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LivroAutor",
                columns: table => new
                {
                    id_livro = table.Column<int>(type: "int", nullable: false),
                    id_autor = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LivroAutor", x => new { x.id_livro, x.id_autor });
                    table.ForeignKey(
                        name: "FK_LivroAutor_Autor",
                        column: x => x.id_autor,
                        principalTable: "Autor",
                        principalColumn: "id_autor",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LivroAutor_Livro",
                        column: x => x.id_livro,
                        principalTable: "Livro",
                        principalColumn: "id_livro",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LivroGenero",
                columns: table => new
                {
                    id_livro = table.Column<int>(type: "int", nullable: false),
                    id_genero = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LivroGenero", x => new { x.id_livro, x.id_genero });
                    table.ForeignKey(
                        name: "FK_LivroGenero_Genero",
                        column: x => x.id_genero,
                        principalTable: "Genero",
                        principalColumn: "id_genero",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LivroGenero_Livro",
                        column: x => x.id_livro,
                        principalTable: "Livro",
                        principalColumn: "id_livro",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Review",
                columns: table => new
                {
                    id_review = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_utilizador = table.Column<int>(type: "int", nullable: false),
                    id_livro = table.Column<int>(type: "int", nullable: false),
                    texto_review = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    rating = table.Column<byte>(type: "tinyint", nullable: true),
                    status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "pendente"),
                    data_submissao = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "(CONVERT([date],getdate()))"),
                    data_aprovacao = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Review__2F79F8C7613DA0E9", x => x.id_review);
                    table.ForeignKey(
                        name: "FK_Review_Livro",
                        column: x => x.id_livro,
                        principalTable: "Livro",
                        principalColumn: "id_livro",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Review_Utilizador",
                        column: x => x.id_utilizador,
                        principalTable: "Utilizador",
                        principalColumn: "id_utilizador",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReservaLivro",
                columns: table => new
                {
                    id_reserva = table.Column<int>(type: "int", nullable: false),
                    id_livro = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservaLivro", x => new { x.id_reserva, x.id_livro });
                    table.ForeignKey(
                        name: "FK_ReservaLivro_Livro",
                        column: x => x.id_livro,
                        principalTable: "Livro",
                        principalColumn: "id_livro");
                    table.ForeignKey(
                        name: "FK_ReservaLivro_Reserva",
                        column: x => x.id_reserva,
                        principalTable: "Reserva",
                        principalColumn: "id_reserva",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Livro_id_editora",
                table: "Livro",
                column: "id_editora");

            migrationBuilder.CreateIndex(
                name: "IX_LivroAutor_id_autor",
                table: "LivroAutor",
                column: "id_autor");

            migrationBuilder.CreateIndex(
                name: "IX_LivroGenero_id_genero",
                table: "LivroGenero",
                column: "id_genero");

            migrationBuilder.CreateIndex(
                name: "IX_Reserva_id_utilizador",
                table: "Reserva",
                column: "id_utilizador");

            migrationBuilder.CreateIndex(
                name: "IX_ReservaLivro_id_livro",
                table: "ReservaLivro",
                column: "id_livro");

            migrationBuilder.CreateIndex(
                name: "IX_Review_id_livro",
                table: "Review",
                column: "id_livro");

            migrationBuilder.CreateIndex(
                name: "UQ_Review_User_Livro",
                table: "Review",
                columns: new[] { "id_utilizador", "id_livro" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Utilizad__AB6E616464D5E079",
                table: "Utilizador",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessLog");

            migrationBuilder.DropTable(
                name: "LivroAutor");

            migrationBuilder.DropTable(
                name: "LivroGenero");

            migrationBuilder.DropTable(
                name: "ReservaLivro");

            migrationBuilder.DropTable(
                name: "Review");

            migrationBuilder.DropTable(
                name: "Autor");

            migrationBuilder.DropTable(
                name: "Genero");

            migrationBuilder.DropTable(
                name: "Reserva");

            migrationBuilder.DropTable(
                name: "Livro");

            migrationBuilder.DropTable(
                name: "Utilizador");

            migrationBuilder.DropTable(
                name: "Editora");
        }
    }
}
