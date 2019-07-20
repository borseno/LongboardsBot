using System.IO;
using System.Linq;

namespace LongBoardsBot.Helpers
{
    public static class DirectoryExtensions
    {
        public static FileInfo[] GetInfoAsync(this DirectoryInfo startDirectory, string subDirectory)
            => startDirectory.GetDirectories().First(i => i.Name == subDirectory).GetFiles();
    }
}
