﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using dtr_nne.Infrastructure.Context;

#nullable disable

namespace dtr_nne.Infrastructure.Migrations
{
    [DbContext(typeof(NneDbContext))]
    partial class NneDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.0");

            modelBuilder.Entity("dtr_nne.Domain.Entities.ArticleContent", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Body")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasAnnotation("Relational:JsonPropertyName", "body");

                    b.PrimitiveCollection<string>("Copyright")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasAnnotation("Relational:JsonPropertyName", "copyright");

                    b.Property<int?>("EditedArticleId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("HeadlineId")
                        .HasColumnType("INTEGER");

                    b.PrimitiveCollection<string>("Images")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasAnnotation("Relational:JsonPropertyName", "images");

                    b.Property<string>("Source")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasAnnotation("Relational:JsonPropertyName", "source");

                    b.HasKey("Id");

                    b.HasIndex("EditedArticleId");

                    b.HasIndex("HeadlineId");

                    b.ToTable("ArticleContent");
                });

            modelBuilder.Entity("dtr_nne.Domain.Entities.EditedArticle", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("EditedBody")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("EditedBodyRunId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Header")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("HeaderRunId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Subheader")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("SubheaderRunId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("TranslatedBody")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("TranslatedBodyRunId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("EditedArticle");
                });

            modelBuilder.Entity("dtr_nne.Domain.Entities.ExternalService", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ApiKey")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<bool>("InUse")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ServiceName")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("TEXT");

                    b.Property<int>("Type")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("ExternalServices");
                });

            modelBuilder.Entity("dtr_nne.Domain.Entities.Headline", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("OriginalHeadline")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasAnnotation("Relational:JsonPropertyName", "header");

                    b.Property<string>("TranslatedHeadline")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Headline");
                });

            modelBuilder.Entity("dtr_nne.Domain.Entities.NewsArticle", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ArticleContentId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("EditedArticleId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Error")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("NewsOutletId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("ParseTime")
                        .HasColumnType("TEXT");

                    b.PrimitiveCollection<string>("Themes")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Uri")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ArticleContentId");

                    b.HasIndex("EditedArticleId");

                    b.HasIndex("NewsOutletId");

                    b.ToTable("NewsArticles");
                });

            modelBuilder.Entity("dtr_nne.Domain.Entities.NewsOutlet", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("AlwaysJs")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("InUse")
                        .HasColumnType("INTEGER");

                    b.Property<string>("MainPagePassword")
                        .IsRequired()
                        .HasMaxLength(300)
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("NewsPassword")
                        .IsRequired()
                        .HasMaxLength(300)
                        .HasColumnType("TEXT");

                    b.PrimitiveCollection<string>("Themes")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Website")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("NewsOutlets");
                });

            modelBuilder.Entity("dtr_nne.Domain.Entities.OpenAiAssistant", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("AssistantId")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("OpenAiAssistants");
                });

            modelBuilder.Entity("dtr_nne.Domain.Entities.ArticleContent", b =>
                {
                    b.HasOne("dtr_nne.Domain.Entities.EditedArticle", "EditedArticle")
                        .WithMany()
                        .HasForeignKey("EditedArticleId");

                    b.HasOne("dtr_nne.Domain.Entities.Headline", "Headline")
                        .WithMany()
                        .HasForeignKey("HeadlineId");

                    b.Navigation("EditedArticle");

                    b.Navigation("Headline");
                });

            modelBuilder.Entity("dtr_nne.Domain.Entities.NewsArticle", b =>
                {
                    b.HasOne("dtr_nne.Domain.Entities.ArticleContent", "ArticleContent")
                        .WithMany()
                        .HasForeignKey("ArticleContentId");

                    b.HasOne("dtr_nne.Domain.Entities.EditedArticle", "EditedArticle")
                        .WithMany()
                        .HasForeignKey("EditedArticleId");

                    b.HasOne("dtr_nne.Domain.Entities.NewsOutlet", "NewsOutlet")
                        .WithMany("Articles")
                        .HasForeignKey("NewsOutletId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ArticleContent");

                    b.Navigation("EditedArticle");

                    b.Navigation("NewsOutlet");
                });

            modelBuilder.Entity("dtr_nne.Domain.Entities.NewsOutlet", b =>
                {
                    b.Navigation("Articles");
                });
#pragma warning restore 612, 618
        }
    }
}
