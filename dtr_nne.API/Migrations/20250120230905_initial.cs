using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dtr_nne.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EditedArticle",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Header = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    HeaderRunId = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Subheader = table.Column<string>(type: "TEXT", maxLength: 10000, nullable: false),
                    SubheaderRunId = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    EditedBody = table.Column<string>(type: "TEXT", maxLength: 30000, nullable: false),
                    EditedBodyRunId = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    TranslatedBody = table.Column<string>(type: "TEXT", maxLength: 30000, nullable: false),
                    TranslatedBodyRunId = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EditedArticle", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExternalServices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ServiceName = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    InUse = table.Column<bool>(type: "INTEGER", nullable: false),
                    ApiKey = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalServices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Headline",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OriginalHeadline = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    TranslatedHeadline = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Headline", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NewsOutlets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    InUse = table.Column<bool>(type: "INTEGER", nullable: false),
                    AlwaysJs = table.Column<bool>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Website = table.Column<string>(type: "TEXT", nullable: false),
                    MainPagePassword = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    NewsPassword = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    Themes = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsOutlets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OpenAiAssistants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Role = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    AssistantId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenAiAssistants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArticleContent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Body = table.Column<string>(type: "TEXT", maxLength: 30000, nullable: false),
                    Images = table.Column<string>(type: "TEXT", nullable: false),
                    Copyright = table.Column<string>(type: "TEXT", nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    HeadlineId = table.Column<int>(type: "INTEGER", nullable: false),
                    EditedArticleId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleContent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArticleContent_EditedArticle_EditedArticleId",
                        column: x => x.EditedArticleId,
                        principalTable: "EditedArticle",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ArticleContent_Headline_HeadlineId",
                        column: x => x.HeadlineId,
                        principalTable: "Headline",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NewsArticles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Themes = table.Column<string>(type: "TEXT", nullable: false),
                    Uri = table.Column<string>(type: "TEXT", nullable: true),
                    Error = table.Column<string>(type: "TEXT", maxLength: 10000, nullable: false),
                    ParseTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NewsOutletId = table.Column<int>(type: "INTEGER", nullable: false),
                    ArticleContentId = table.Column<int>(type: "INTEGER", nullable: true),
                    EditedArticleId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsArticles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NewsArticles_ArticleContent_ArticleContentId",
                        column: x => x.ArticleContentId,
                        principalTable: "ArticleContent",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_NewsArticles_EditedArticle_EditedArticleId",
                        column: x => x.EditedArticleId,
                        principalTable: "EditedArticle",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_NewsArticles_NewsOutlets_NewsOutletId",
                        column: x => x.NewsOutletId,
                        principalTable: "NewsOutlets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArticleContent_EditedArticleId",
                table: "ArticleContent",
                column: "EditedArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleContent_HeadlineId",
                table: "ArticleContent",
                column: "HeadlineId");

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticles_ArticleContentId",
                table: "NewsArticles",
                column: "ArticleContentId");

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticles_EditedArticleId",
                table: "NewsArticles",
                column: "EditedArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticles_NewsOutletId",
                table: "NewsArticles",
                column: "NewsOutletId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExternalServices");

            migrationBuilder.DropTable(
                name: "NewsArticles");

            migrationBuilder.DropTable(
                name: "OpenAiAssistants");

            migrationBuilder.DropTable(
                name: "ArticleContent");

            migrationBuilder.DropTable(
                name: "NewsOutlets");

            migrationBuilder.DropTable(
                name: "EditedArticle");

            migrationBuilder.DropTable(
                name: "Headline");
        }
    }
}
