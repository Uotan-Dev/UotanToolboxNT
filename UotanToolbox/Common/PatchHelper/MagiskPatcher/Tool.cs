using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MagiskPatcher
{
    internal static class Tool
    {
        public static string CmdOutput = "";

        /// <summary>
        /// 检查文件中是否存在指定的十六进制序列
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="hexPattern">要查找的十六进制字符串（可以包含空格）</param>
        /// <param name="caseSensitive">是否区分大小写</param>
        /// <returns>是否存在匹配的序列</returns>
        public static bool ContainsHexPattern(string filePath, string hexPattern, bool caseSensitive = false)
        {
            byte[] patternBytes = HexStringToByteArray(hexPattern);

            byte[] fileBytes = File.ReadAllBytes(filePath);

            return ContainsByteSequence(fileBytes, patternBytes);
        }
        /// <summary>
        /// 将十六进制字符串转换为字节数组
        /// </summary>
        private static byte[] HexStringToByteArray(string hex)
        {
            int length = hex.Length;
            byte[] bytes = new byte[length / 2];

            for (int i = 0; i < length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }

            return bytes;
        }
        /// <summary>
        /// 在字节数组中搜索指定的字节序列
        /// </summary>
        private static bool ContainsByteSequence(byte[] source, byte[] pattern)
        {
            if (pattern.Length == 0) return true;
            if (source.Length < pattern.Length) return false;

            for (int i = 0; i <= source.Length - pattern.Length; i++)
            {
                bool found = true;
                for (int j = 0; j < pattern.Length; j++)
                {
                    if (source[i + j] != pattern[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found) return true;
            }

            return false;
        }


        public static string GetStrFromText(string allText, string strInTargetLine, char delim, int token)
        {
            if (string.IsNullOrEmpty(allText))
            {
                MagiskPatcherCore.Logger?.Warn("输入字符串不能为空");
                return "";
            }

            var targetLine = allText.Split(["\r\n", "\r", "\n"], StringSplitOptions.None)
                               .FirstOrDefault(line => line.Contains(strInTargetLine));

            if (targetLine == null)
            {
                MagiskPatcherCore.Logger?.Warn($"未找到包含'{strInTargetLine}'的行");
                return "";
            }
            var segments = targetLine.Split(delim);

            if (segments.Length < token)
            {
                MagiskPatcherCore.Logger?.Warn($"分割后的段数不足{token}段，实际只有{segments.Length}段");
                return "";
            }

            return segments[token - 1]; // 数组是0-based索引，所以第666个元素索引是665
        }


        public static string GenerateRandomString(string characterPool, int length)
        {
            if (string.IsNullOrEmpty(characterPool))
            {
                throw new ArgumentException("字符池不能为空", nameof(characterPool));
            }

            if (length <= 0)
            {
                throw new ArgumentException("长度必须大于0", nameof(length));
            }

            using (var rng = RandomNumberGenerator.Create())
            {
                var chars = characterPool.ToCharArray();
                var bytes = new byte[length];
                rng.GetBytes(bytes);
                var result = new char[length];
                for (int i = 0; i < length; i++)
                {
                    result[i] = chars[bytes[i] % chars.Length];
                }

                return new string(result);
            }
        }


        /// <summary>
        /// 向指定文件写入文本
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="text">要写入的内容</param>
        /// <param name="append">true表示追加模式，false表示覆盖模式</param>
        /// <param name="useCrLf">true表示使用CRLF换行符(\r\n)，false表示使用LF换行符(\n)</param>
        /// <param name="encoding">编码格式，默认为UTF-8</param>
        /// <returns>是否写入成功</returns>
        public static bool WriteToFile(string filePath, string text, bool append, bool useCrLf = true, Encoding? encoding = null)
        {
            try
            {
                // 如果编码未指定，使用UTF-8
                encoding = encoding ?? new UTF8Encoding(false);

                // 根据参数处理换行符
                string processedContent = useCrLf
                    ? text.Replace("\n", "\r\n")  // 确保所有换行都是CRLF
                    : text.Replace("\r\n", "\n"); // 确保所有换行都是LF

                // 如果是追加模式且文件已存在，则追加内容
                if (append && File.Exists(filePath))
                {
                    File.AppendAllText(filePath, processedContent, encoding);
                }
                else
                {
                    // 覆盖模式或文件不存在，直接写入
                    File.WriteAllText(filePath, processedContent, encoding);
                }

                return true;
            }
            catch (Exception ex)
            {
                // 在实际应用中，记录该错误到 host 提供的 logger
                MagiskPatcherCore.Logger?.Error($"写入文件时出错: {ex.Message}");
                return false;
            }
        }


        /// <summary>
        /// 从字符串中获取指定行号的内容
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <param name="lineNumber">要获取的行号(从1开始)</param>
        /// <returns>指定行的内容，如果行号无效则返回null</returns>
        public static string? GetLineFromString(string input, int lineNumber)
        {
            // 检查参数有效性
            if (string.IsNullOrEmpty(input) || lineNumber < 1)
            {
                return null;
            }

            using (var reader = new StringReader(input))
            {
                string? line;
                int currentLine = 1;

                while ((line = reader.ReadLine()) != null)
                {
                    if (currentLine == lineNumber)
                    {
                        return line;
                    }
                    currentLine++;
                }
            }

            // 如果行号超出范围，返回null
            return null;
        }


        /// <summary>
        /// 移除字符串中的所有空行（包括仅包含空白字符的行）
        /// </summary>
        /// <param name="input">原始字符串</param>
        /// <param name="preserveLineEndings">是否保留原始换行符风格（true则使用原始换行符，false则使用当前环境的换行符）</param>
        /// <returns>移除空行后的字符串</returns>
        public static string RemoveEmptyLines(this string? input, bool preserveLineEndings = false)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            // 分割字符串，考虑两种换行符
            var lines = input.Split(["\r\n", "\n"], StringSplitOptions.None);

            // 过滤掉空行或仅包含空白字符的行
            var nonEmptyLines = lines.Where(line => !string.IsNullOrWhiteSpace(line));

            // 决定使用哪种换行符
            string newLine = preserveLineEndings
                ? input.Contains("\r\n") ? "\r\n" : "\n"
                : Environment.NewLine;

            // 重新组合非空行
            return string.Join(newLine, nonEmptyLines);
        }


        public static int RunCommand(string workingDirectory, string filePath, string arguments, Dictionary<string, string>? environmentVars = null)
        {
            string standardOutput;
            string standardError;
            if (workingDirectory == "" || workingDirectory == null)
            {
                workingDirectory = Environment.CurrentDirectory;
            }

            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = filePath,
                    Arguments = arguments,
                    WorkingDirectory = workingDirectory ?? Environment.CurrentDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            if (environmentVars != null)
            {
                foreach (var pair in environmentVars)
                {
                    process.StartInfo.EnvironmentVariables[pair.Key] = pair.Value;
                    // host may log env changes if it wants
                }
            }

            process.OutputDataReceived += (sender, args) =>
            {
                if (args.Data != null)
                    outputBuilder.AppendLine(args.Data);
            };

            process.ErrorDataReceived += (sender, args) =>
            {
                if (args.Data != null)
                    errorBuilder.AppendLine(args.Data);
            };

            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                standardOutput = outputBuilder.ToString().TrimEnd();
                standardError = errorBuilder.ToString().TrimEnd();

                // Pass command outputs to host logger (debug) instead of Console
                CmdOutput = standardOutput + "\r\n" + standardError;
                MagiskPatcherCore.Logger?.Debug(CmdOutput);

                return process.ExitCode;
            }
            finally
            {
                process.Close();
            }
        }

        public static string CalculateFileMD5(string filePath)
        {
            try
            {
                // 创建MD5实例
                using (var md5 = MD5.Create())
                {
                    // 打开文件流
                    using (var stream = File.OpenRead(filePath))
                    {
                        // 计算哈希值
                        byte[] hashBytes = md5.ComputeHash(stream);

                        // 将字节数组转换为十六进制字符串
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < hashBytes.Length; i++)
                        {
                            sb.Append(hashBytes[i].ToString("x2"));
                        }

                        return sb.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                return $"计算MD5时出错: {ex.Message}";
            }
        }





    }
}
