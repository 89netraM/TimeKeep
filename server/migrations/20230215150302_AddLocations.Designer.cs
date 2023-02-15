﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TimeKeep.Models;

#nullable disable

namespace TimeKeep.migrations
{
    [DbContext(typeof(TimeKeepContext))]
    [Migration("20230215150302_AddLocations")]
    partial class AddLocations
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("CategoryEntry", b =>
                {
                    b.Property<string>("CategoriesName")
                        .HasColumnType("text");

                    b.Property<Guid>("EntriesId")
                        .HasColumnType("uuid");

                    b.HasKey("CategoriesName", "EntriesId");

                    b.HasIndex("EntriesId");

                    b.ToTable("CategoryEntry");
                });

            modelBuilder.Entity("CategoryProject", b =>
                {
                    b.Property<string>("CategoriesName")
                        .HasColumnType("text");

                    b.Property<string>("ProjectsName")
                        .HasColumnType("text");

                    b.HasKey("CategoriesName", "ProjectsName");

                    b.HasIndex("ProjectsName");

                    b.ToTable("CategoryProject");
                });

            modelBuilder.Entity("TimeKeep.Models.Category", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.HasKey("Name");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("TimeKeep.Models.Entry", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime?>("End")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("LocationId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("Start")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("LocationId");

                    b.ToTable("Entries");
                });

            modelBuilder.Entity("TimeKeep.Models.Location", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("Name");

                    b.ToTable("Locations");
                });

            modelBuilder.Entity("TimeKeep.Models.Project", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.HasKey("Name");

                    b.ToTable("Projects");
                });

            modelBuilder.Entity("CategoryEntry", b =>
                {
                    b.HasOne("TimeKeep.Models.Category", null)
                        .WithMany()
                        .HasForeignKey("CategoriesName")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TimeKeep.Models.Entry", null)
                        .WithMany()
                        .HasForeignKey("EntriesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("CategoryProject", b =>
                {
                    b.HasOne("TimeKeep.Models.Category", null)
                        .WithMany()
                        .HasForeignKey("CategoriesName")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TimeKeep.Models.Project", null)
                        .WithMany()
                        .HasForeignKey("ProjectsName")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("TimeKeep.Models.Entry", b =>
                {
                    b.HasOne("TimeKeep.Models.Location", "Location")
                        .WithMany()
                        .HasForeignKey("LocationId");

                    b.Navigation("Location");
                });
#pragma warning restore 612, 618
        }
    }
}
