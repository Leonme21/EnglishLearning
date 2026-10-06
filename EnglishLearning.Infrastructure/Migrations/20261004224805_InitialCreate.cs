using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EnglishLearning.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EvaluacionesDiagnosticas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NivelAsignado = table.Column<string>(type: "text", nullable: false),
                    PuntajeTotal = table.Column<int>(type: "integer", nullable: false),
                    PuntajesPorHabilidadJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluacionesDiagnosticas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lecciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    NivelRequerido = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lecciones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PreguntasDiagnosticas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Habilidad = table.Column<string>(type: "text", nullable: false),
                    Contenido = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    OpcionesJson = table.Column<string>(type: "jsonb", nullable: false),
                    RespuestaCorrecta = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Nivel = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreguntasDiagnosticas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RespuestasUsuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    EjercicioId = table.Column<int>(type: "integer", nullable: false),
                    RespuestaTexto = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    EsCorrecta = table.Column<bool>(type: "boolean", nullable: false),
                    FechaRealizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RespuestasUsuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Correo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    NivelActual = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ejercicios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LeccionId = table.Column<int>(type: "integer", nullable: false),
                    Contenido = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    RespuestaCorrecta = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ejercicios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ejercicios_Lecciones_LeccionId",
                        column: x => x.LeccionId,
                        principalTable: "Lecciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProgresosNiveles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    Nivel = table.Column<string>(type: "text", nullable: false),
                    PorcentajeCompletado = table.Column<double>(type: "double precision", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgresosNiveles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgresosNiveles_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ejercicios_LeccionId",
                table: "Ejercicios",
                column: "LeccionId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluacionesDiagnosticas_Fecha",
                table: "EvaluacionesDiagnosticas",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluacionesDiagnosticas_UsuarioId",
                table: "EvaluacionesDiagnosticas",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Lecciones_NivelRequerido",
                table: "Lecciones",
                column: "NivelRequerido");

            migrationBuilder.CreateIndex(
                name: "IX_PreguntasDiagnosticas_Habilidad",
                table: "PreguntasDiagnosticas",
                column: "Habilidad");

            migrationBuilder.CreateIndex(
                name: "IX_PreguntasDiagnosticas_Nivel",
                table: "PreguntasDiagnosticas",
                column: "Nivel");

            migrationBuilder.CreateIndex(
                name: "IX_ProgresosNiveles_UsuarioId",
                table: "ProgresosNiveles",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgresosNiveles_UsuarioId_Nivel",
                table: "ProgresosNiveles",
                columns: new[] { "UsuarioId", "Nivel" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RespuestasUsuarios_EjercicioId",
                table: "RespuestasUsuarios",
                column: "EjercicioId");

            migrationBuilder.CreateIndex(
                name: "IX_RespuestasUsuarios_FechaRealizacion",
                table: "RespuestasUsuarios",
                column: "FechaRealizacion");

            migrationBuilder.CreateIndex(
                name: "IX_RespuestasUsuarios_UsuarioId",
                table: "RespuestasUsuarios",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Correo",
                table: "Usuarios",
                column: "Correo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ejercicios");

            migrationBuilder.DropTable(
                name: "EvaluacionesDiagnosticas");

            migrationBuilder.DropTable(
                name: "PreguntasDiagnosticas");

            migrationBuilder.DropTable(
                name: "ProgresosNiveles");

            migrationBuilder.DropTable(
                name: "RespuestasUsuarios");

            migrationBuilder.DropTable(
                name: "Lecciones");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
