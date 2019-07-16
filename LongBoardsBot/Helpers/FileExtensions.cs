using System.IO;
using System.Threading.Tasks;

namespace LongBoardsBot.Helpers
{
    public static class FileExtensions
    {
        public static string NameWithoutExt(this FileInfo file)
            => file.Name.Remove(file.Name.Length - file.Extension.Length, file.Extension.Length);

        public static async Task<string> ReadAllLinesAsync(string path)
        {
            string result;
            using (var reader = new StreamReader(path))
            {
                result = await reader.ReadToEndAsync();
            }
            return result;
        }
    }
}
