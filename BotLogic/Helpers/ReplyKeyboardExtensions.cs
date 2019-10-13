using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace LongBoardsBot.Helpers
{
    public static class ReplyKeyboardExtensions
    {
        public static ReplyKeyboardMarkup Append(this ReplyKeyboardMarkup markup, IEnumerable<KeyboardButton> buttonsToAppend)
        {
            var buttonsList = markup.Keyboard.Select(i => i.ToList()).ToList();

            buttonsList.Last().AddRange(buttonsToAppend);

            var newMarkup = new ReplyKeyboardMarkup
            {
                Keyboard = buttonsList,
                OneTimeKeyboard = markup.OneTimeKeyboard,
                ResizeKeyboard = markup.ResizeKeyboard,
                Selective = markup.Selective
            };

            return newMarkup;
        }

        public static ReplyKeyboardMarkup Append(this ReplyKeyboardMarkup markup, params KeyboardButton[] buttonsToAppend)
        {
            var buttonsList = markup.Keyboard.Select(i => i.ToList()).ToList();

            buttonsList.Last().AddRange(buttonsToAppend);

            var newMarkup = new ReplyKeyboardMarkup
            {
                Keyboard = buttonsList,
                OneTimeKeyboard = markup.OneTimeKeyboard,
                ResizeKeyboard = markup.ResizeKeyboard,
                Selective = markup.Selective
            };

            return newMarkup;
        }
    }
}
