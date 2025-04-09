﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using UmbralEmpires.Data;

#nullable disable

namespace UmbralEmpires.Data.Migrations
{
    [DbContext(typeof(UmbralDbContext))]
    [Migration("20250409072322_AddBaseConstructionPausedFlag")]
    partial class AddBaseConstructionPausedFlag
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.4");

            modelBuilder.Entity("UmbralEmpires.Core.Gameplay.Base", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("AstroId")
                        .HasColumnType("TEXT");

                    b.Property<string>("ConstructionQueue")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<Guid>("PlayerId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Structures")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("AstroId")
                        .IsUnique();

                    b.HasIndex("PlayerId");

                    b.ToTable("Bases");
                });

            modelBuilder.Entity("UmbralEmpires.Core.Gameplay.Player", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<int>("Credits")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Players");
                });

            modelBuilder.Entity("UmbralEmpires.Core.World.Astro", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<int>("BaseArea")
                        .HasColumnType("INTEGER");

                    b.Property<int>("BaseFertility")
                        .HasColumnType("INTEGER");

                    b.Property<Guid?>("BaseId")
                        .HasColumnType("TEXT");

                    b.Property<int>("CrystalsPotential")
                        .HasColumnType("INTEGER");

                    b.Property<int>("GasPotential")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsPlanet")
                        .HasColumnType("INTEGER");

                    b.Property<int>("MetalPotential")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SolarPotential")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Terrain")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("BaseId")
                        .IsUnique();

                    b.ToTable("Astros");
                });

            modelBuilder.Entity("UmbralEmpires.Core.Gameplay.Base", b =>
                {
                    b.HasOne("UmbralEmpires.Core.World.Astro", null)
                        .WithOne()
                        .HasForeignKey("UmbralEmpires.Core.Gameplay.Base", "AstroId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("UmbralEmpires.Core.World.Astro", b =>
                {
                    b.OwnsOne("UmbralEmpires.Core.World.AstroCoordinates", "Coordinates", b1 =>
                        {
                            b1.Property<Guid>("AstroId")
                                .HasColumnType("TEXT");

                            b1.Property<string>("Galaxy")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.Property<int>("Orbit")
                                .HasColumnType("INTEGER");

                            b1.Property<int>("Region")
                                .HasColumnType("INTEGER");

                            b1.Property<int>("System")
                                .HasColumnType("INTEGER");

                            b1.HasKey("AstroId");

                            b1.ToTable("Astros");

                            b1.WithOwner()
                                .HasForeignKey("AstroId");
                        });

                    b.Navigation("Coordinates")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
