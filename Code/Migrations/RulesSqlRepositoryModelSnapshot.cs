﻿// <auto-generated />
using System;
using IL.RulesBasedOutputCache.Persistence.Rules;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace IL.RulesBasedOutputCache.Migrations
{
    [DbContext(typeof(SqlRulesRepository))]
    partial class RulesSqlRepositoryModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("IL.RulesBasedOutputCache.Models.CachingRule", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Priority")
                        .HasColumnType("int");

                    b.Property<TimeSpan?>("ResponseExpirationTimeSpan")
                        .HasColumnType("time");

                    b.Property<int>("RuleAction")
                        .HasColumnType("int");

                    b.Property<string>("RuleTemplate")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("RuleType")
                        .HasColumnType("int");

                    b.Property<bool>("VaryByCulture")
                        .HasColumnType("bit");

                    b.Property<bool>("VaryByHost")
                        .HasColumnType("bit");

                    b.Property<bool>("VaryByQueryString")
                        .HasColumnType("bit");

                    b.Property<bool>("VaryByUser")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.ToTable("CachingRules");
                });
#pragma warning restore 612, 618
        }
    }
}
