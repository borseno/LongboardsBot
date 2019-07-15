using System.IO;

namespace LongBoardsBot.Models
{
    public static class FileExtensions
    {
        public static string NameWithoutExt(this FileInfo file)
            => file.Name.Remove(file.Name.Length - file.Extension.Length, file.Extension.Length);
    }
}
