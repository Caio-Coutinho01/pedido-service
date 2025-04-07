﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Pedido.Infrastructure.Persistence.Context;

#nullable disable

namespace Pedido.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(PedidoDbContext))]
    [Migration("20250407042617_AdicionaTentativasEnvio")]
    partial class AdicionaTentativasEnvio
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Pedido.Domain.Entities.PedidoEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("ClienteId")
                        .HasColumnType("int");

                    b.Property<DateTime>("DataCriacao")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DataEnvio")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("Imposto")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("JustificativaCancelamento")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("PedidoId")
                        .HasColumnType("int");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<int>("TentativasEnvio")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Pedidos");
                });

            modelBuilder.Entity("Pedido.Domain.Entities.PedidoItemEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("PedidoEntityId")
                        .HasColumnType("int");

                    b.Property<int>("ProdutoId")
                        .HasColumnType("int");

                    b.Property<int>("Quantidade")
                        .HasColumnType("int");

                    b.Property<decimal>("Valor")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("Id");

                    b.HasIndex("PedidoEntityId");

                    b.ToTable("Itens");
                });

            modelBuilder.Entity("Pedido.Domain.Entities.PedidoItemEntity", b =>
                {
                    b.HasOne("Pedido.Domain.Entities.PedidoEntity", "Pedido")
                        .WithMany("Itens")
                        .HasForeignKey("PedidoEntityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Pedido");
                });

            modelBuilder.Entity("Pedido.Domain.Entities.PedidoEntity", b =>
                {
                    b.Navigation("Itens");
                });
#pragma warning restore 612, 618
        }
    }
}
