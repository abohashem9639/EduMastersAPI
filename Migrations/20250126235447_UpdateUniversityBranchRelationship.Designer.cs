﻿// <auto-generated />
using System;
using EduMastersAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace EduMastersAPI.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20250126235447_UpdateUniversityBranchRelationship")]
    partial class UpdateUniversityBranchRelationship
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.12")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("EduMastersAPI.Models.University", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Country")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Universities");
                });

            modelBuilder.Entity("EduMastersAPI.Models.UniversityBranch", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<decimal>("AnnualFee")
                        .HasColumnType("decimal(65,30)");

                    b.Property<string>("BranchName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("DurationYears")
                        .HasColumnType("int");

                    b.Property<string>("Languages")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Levels")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("UniversityId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UniversityId");

                    b.ToTable("UniversityBranches");
                });

            modelBuilder.Entity("EduMastersAPI.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int?>("LinkedUserId")
                        .HasColumnType("int");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("ProfileImageUrl")
                        .HasColumnType("longtext");

                    b.Property<string>("UserType")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("EduMastersAPI.Models.UniversityBranch", b =>
                {
                    b.HasOne("EduMastersAPI.Models.University", "University")
                        .WithMany("Branches")
                        .HasForeignKey("UniversityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("University");
                });

            modelBuilder.Entity("EduMastersAPI.Models.University", b =>
                {
                    b.Navigation("Branches");
                });
#pragma warning restore 612, 618
        }
    }
}
