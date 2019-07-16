using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LongBoardsBot.Helpers
{
    public static class DirectoryExtensions
    {
        public static FileInfo[] GetInfoAsync(this DirectoryInfo startDirectory, string subDirectory)
    => startDirectory.GetDirectories().First(i => i.Name == subDirectory).GetFiles();
    }
}
