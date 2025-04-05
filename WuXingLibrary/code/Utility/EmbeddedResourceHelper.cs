using System.IO;
using System.Reflection;

namespace WuXingLibrary.code.Utility
{
    public static class EmbeddedResourceHelper
    {
        /// <summary>
        /// 获取嵌入的资源流。
        /// </summary>
        /// <param name="resourceName">资源文件名（例如 "programmer.bin"）。</param>
        /// <returns>资源的 Stream 对象。</returns>
        public static Stream GetEmbeddedResourceStream(string resourceName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string fullResourceName = $"{resourceName}";

            Stream resourceStream = assembly.GetManifestResourceStream(fullResourceName) ?? throw new FileNotFoundException($"资源 '{fullResourceName}' 未找到。");
            return resourceStream;
        }
    }
}
