using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LongBoardsBot.Models
{
    public class LongboardistDBContext : DbContext
    {
        public LongboardistDBContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<BotUser> BotUsers { get; set; }
        public DbSet<LongBoard> Longboards { get; set; }

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
                .WithMany(bu => bu.BotUserLongBoards)
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
        }
    }
}
