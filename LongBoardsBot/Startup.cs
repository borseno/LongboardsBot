using LongBoardsBot.Models;
using LongBoardsBot.Models.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using static LongBoardsBot.Models.Constants;

namespace LongBoardsBot
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString("LongBoardist");
            
            var botSettings = Configuration.GetSection("BotSettings").Get<BotSettings>();
            
            TelegramBotClient bot = GetNewBot(botSettings.ApiKey, botSettings.WebHookUrl);

            services.AddSingleton(bot);
            services.AddScoped<StageHandler>();
            services.AddScoped<CallbackHandler>();
            services.AddDbContext<LongboardistDBContext>(o => o.UseSqlServer(connectionString));
        }

        private static TelegramBotClient GetNewBot(string key, string url)
        {
            var bot = new TelegramBotClient(key);
            bot.SetWebhookAsync(url).GetAwaiter().GetResult();
            return bot;
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {           
                app.UseHsts();
            }

            app.UseHttpsRedirection();
        }
    }
}
