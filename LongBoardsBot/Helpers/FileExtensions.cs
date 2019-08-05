using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace LongBoardsBot.Helpers
{
    public static class FileExtensions
    {
        public static string NameWithoutExt(this FileInfo file)
            => file.Name.Remove(file.Name.Length - file.Extension.Length, file.Extension.Length);
    }
}
