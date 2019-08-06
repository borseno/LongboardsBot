using LongBoardsBot.Helpers;
using LongBoardsBot.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static LongBoardsBot.Models.Constants;
using static System.String;

namespace LongBoardsBot.Models.TextsFunctions
{
    public static class FormattedTexts
    {
        public static async Task<string> GetFormattedFinalTextToAdminsAsync(BotUser instance, Purchase purchase = null)
        {
            if (purchase == null)
                purchase = instance.LatestPurchase;

            var textPatternToAdminsTask = Texts.GetFinalTextToAdminsAsync();

            var adressToDeliver = purchase.AdressToDeliver;
            var lbrds = Join(ElementsSeparator, purchase.Basket);
            var cost = Math.Round(purchase.Cost, 2);
            var adressInfo = adressToDeliver ?? NoDeliveryInfo;

            var textPatternToAdmins = await textPatternToAdminsTask;

            var textFormattedToAdminGroup = Format(
                textPatternToAdmins,
                $"[{instance.Name}](tg://user?id={instance.UserId})",
                lbrds,
                cost.ToString(),
                instance.Name,
                instance.Phone,
                adressInfo,
                purchase.Guid.ToStringHashTag()
                );

            return textFormattedToAdminGroup;
        }
    }
}
