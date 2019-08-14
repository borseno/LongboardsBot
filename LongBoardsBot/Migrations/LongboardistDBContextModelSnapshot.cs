﻿// <auto-generated />
using System;
using LongBoardsBot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LongBoardsBot.Migrations
{
    [DbContext(typeof(LongboardistDBContext))]
    partial class LongboardistDBContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("LongBoardsBot.Models.BotUserLongBoard", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("Amount")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(0);

                    b.Property<long>("BotUserId");

                    b.Property<int>("LongboardId");

                    b.Property<Guid?>("PurchaseGuid");

                    b.HasKey("Id");

                    b.HasIndex("BotUserId");

                    b.HasIndex("LongboardId");

                    b.HasIndex("PurchaseGuid");

                    b.ToTable("BotUserLongBoard");
                });

            modelBuilder.Entity("LongBoardsBot.Models.Entities.BotUser", b =>
                {
                    b.Property<long>("ChatId");

                    b.Property<Guid?>("CurrentPurchaseGuid");

                    b.Property<bool>("IsLivingInKharkiv");

                    b.Property<bool>("IsOneTimeStatistics");

                    b.Property<string>("Name");

                    b.Property<int?>("PendingId");

                    b.Property<string>("Phone");

                    b.Property<int>("Stage")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(0);

                    b.Property<int>("State")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(0);

                    b.Property<int>("StatisticsStage")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(0);

                    b.Property<int?>("UserId");

                    b.Property<string>("UserName");

                    b.HasKey("ChatId");

                    b.HasIndex("CurrentPurchaseGuid");

                    b.HasIndex("PendingId");

                    b.ToTable("BotUsers");
                });

            modelBuilder.Entity("LongBoardsBot.Models.Entities.ChatMessage", b =>
                {
                    b.Property<int>("MessageId");

                    b.Property<bool>("IgnoreDelete");

                    b.Property<long>("UserChatId");

                    b.HasKey("MessageId");

                    b.HasIndex("UserChatId");

                    b.ToTable("ChatMessage");
                });

            modelBuilder.Entity("LongBoardsBot.Models.Entities.Comment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<long>("AuthorChatId");

                    b.Property<string>("Data")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("AuthorChatId");

                    b.ToTable("Comments");
                });

            modelBuilder.Entity("LongBoardsBot.Models.Entities.LongBoard", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("Amount");

                    b.Property<decimal>("Price");

                    b.Property<string>("Style")
                        .IsRequired();

                    b.HasKey("Id");

                    b.ToTable("Longboards");
                });

            modelBuilder.Entity("LongBoardsBot.Models.Entities.Purchase", b =>
                {
                    b.Property<Guid>("Guid")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AdressToDeliver");

                    b.Property<long>("BotUserChatId");

                    b.Property<decimal>("Cost");

                    b.Property<bool>("Delivered");

                    b.HasKey("Guid");

                    b.HasIndex("BotUserChatId");

                    b.ToTable("Purchases");
                });

            modelBuilder.Entity("LongBoardsBot.Models.Entities.TestingInfo", b =>
                {
                    b.Property<long>("BotUserId");

                    b.Property<bool>("Occurred");

                    b.Property<DateTime>("VisitDateTime");

                    b.HasKey("BotUserId");

                    b.ToTable("TestingInfo");
                });

            modelBuilder.Entity("LongBoardsBot.Models.Entities.UserStatistics", b =>
                {
                    b.Property<long>("BotUserId");

                    b.Property<int>("Age");

                    b.Property<string>("Profession");

                    b.Property<int>("SocialStatus")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(0);

                    b.HasKey("BotUserId");

                    b.ToTable("UserStatistics");
                });

            modelBuilder.Entity("LongBoardsBot.Models.BotUserLongBoard", b =>
                {
                    b.HasOne("LongBoardsBot.Models.Entities.BotUser", "BotUser")
                        .WithMany()
                        .HasForeignKey("BotUserId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("LongBoardsBot.Models.Entities.LongBoard", "Longboard")
                        .WithMany("BotUserLongBoards")
                        .HasForeignKey("LongboardId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("LongBoardsBot.Models.Entities.Purchase")
                        .WithMany("Basket")
                        .HasForeignKey("PurchaseGuid");
                });

            modelBuilder.Entity("LongBoardsBot.Models.Entities.BotUser", b =>
                {
                    b.HasOne("LongBoardsBot.Models.Entities.Purchase", "CurrentPurchase")
                        .WithMany()
                        .HasForeignKey("CurrentPurchaseGuid");

                    b.HasOne("LongBoardsBot.Models.Entities.LongBoard", "Pending")
                        .WithMany()
                        .HasForeignKey("PendingId");
                });

            modelBuilder.Entity("LongBoardsBot.Models.Entities.ChatMessage", b =>
                {
                    b.HasOne("LongBoardsBot.Models.Entities.BotUser", "User")
                        .WithMany("History")
                        .HasForeignKey("UserChatId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("LongBoardsBot.Models.Entities.Comment", b =>
                {
                    b.HasOne("LongBoardsBot.Models.Entities.BotUser", "Author")
                        .WithMany("Comments")
                        .HasForeignKey("AuthorChatId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("LongBoardsBot.Models.Entities.Purchase", b =>
                {
                    b.HasOne("LongBoardsBot.Models.Entities.BotUser", "BotUser")
                        .WithMany("Purchases")
                        .HasForeignKey("BotUserChatId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("LongBoardsBot.Models.Entities.TestingInfo", b =>
                {
                    b.HasOne("LongBoardsBot.Models.Entities.BotUser", "BotUser")
                        .WithOne("TestingInfo")
                        .HasForeignKey("LongBoardsBot.Models.Entities.TestingInfo", "BotUserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("LongBoardsBot.Models.Entities.UserStatistics", b =>
                {
                    b.HasOne("LongBoardsBot.Models.Entities.BotUser", "BotUser")
                        .WithOne("StatisticsInfo")
                        .HasForeignKey("LongBoardsBot.Models.Entities.UserStatistics", "BotUserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
