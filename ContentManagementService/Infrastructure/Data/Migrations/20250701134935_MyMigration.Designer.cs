﻿// <auto-generated />
using ContentManagementService.Infrastructure.Data.DBContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace ContentManagementService.Migrations
{
    [DbContext(typeof(ContentManagementContext))]
    [Migration("20250701134935_MyMigration")]
    partial class MyMigration
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.13")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("BannerService.Domain.Entities.Filter", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID"));

                    b.Property<string>("Position")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ID");

                    b.ToTable("Filters");
                });

            modelBuilder.Entity("ContentManagementService.Domain.Entities.FilterDetail", b =>
                {
                    b.Property<int>("FilterID")
                        .HasColumnType("int");

                    b.Property<int>("ProductPropertyID")
                        .HasColumnType("int");

                    b.HasKey("FilterID", "ProductPropertyID");

                    b.ToTable("FilterDetails");
                });

            modelBuilder.Entity("ContentManagementService.Domain.Entities.FilterDetail", b =>
                {
                    b.HasOne("BannerService.Domain.Entities.Filter", "Filter")
                        .WithMany("FilterDetails")
                        .HasForeignKey("FilterID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Filter");
                });

            modelBuilder.Entity("BannerService.Domain.Entities.Filter", b =>
                {
                    b.Navigation("FilterDetails");
                });
#pragma warning restore 612, 618
        }
    }
}
