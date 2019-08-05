using LongBoardsBot.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LongBoardsBot.Models
{
    public class LongboardistDBContext : DbContext
    {
        public LongboardistDBContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<BotUser> BotUsers { get; set; }
        public DbSet<LongBoard> Longboards { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder blder)
        {
            blder.Entity<BotUser>()
                .Property(bu => bu.Stage)
                .HasConversion<int>();

            blder.Entity<BotUser>()
                .Property(bu => bu.Stage)
                .HasDefaultValue(Stage.AskingName);

            blder.Entity<BotUserLongBoard>()
                .HasKey(bulb => new { bulb.BotUserId, bulb.LongboardId });

            blder.Entity<BotUserLongBoard>()
                .HasOne(bulb => bulb.Longboard)
                .WithMany(lb => lb.BotUserLongBoards)
                .HasForeignKey(bulb => bulb.LongboardId);

            blder.Entity<BotUserLongBoard>()
                .HasOne(bulb => bulb.BotUser)
                .WithMany(bu => bu.Basket)
                .HasForeignKey(bulb => bulb.BotUserId);

            blder.Entity<BotUser>()
                .Property(i => i.ChatId)
                .ValueGeneratedNever();

            blder.Entity<BotUser>()
                .HasKey(i => i.ChatId);

            blder.Entity<ChatMessage>()
                .HasKey(i => i.MessageId);

            blder.Entity<ChatMessage>()
                .Property(i => i.MessageId)
                .ValueGeneratedNever();

            blder.Entity<BotUserLongBoard>()
                .Property(i => i.Amount)
                .IsRequired(true);

            blder.Entity<BotUserLongBoard>()
                .Property(i => i.Amount)
                .HasDefaultValue(0);

            blder.Entity<Purchase>()
                .HasKey(i => i.Guid);

            blder.Entity<BotUser>()
                .HasOne(i => i.StatisticsInfo)
                .WithOne(i => i.BotUser)
                .HasForeignKey<UserStatistics>(i => i.BotUserId);

            blder.Entity<UserStatistics>()
                .HasKey(i => i.BotUserId);

            blder.Entity<BotUser>()
                .HasMany(i => i.Comments)
                .WithOne(i => i.Author);

            blder.Entity<Purchase>()
                .HasOne(i => i.BotUser)
                .WithMany(i => i.Purchases);
        }
    }
}
