using System.IO;

namespace TelegramBot_SampleFunctionalities
{
    public static class FileExtensions
    {
        public static string NameWithoutExt(this FileInfo file)
            => file.Name.Remove(file.Name.Length - file.Extension.Length, file.Extension.Length);
    }
}
