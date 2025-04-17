using System;
using System.IO;
using System.Text;

namespace EDLLibrary.code.module
{
    public class PtAnalyzer
    {

        private static string outputLog = "";

        public static string Start(string Filepath, string Memorytype)
        {

            outputLog = "";

            string filepath = Filepath;
            Stream filestream = null;
            string memorytype = Memorytype;
            string warninfo = "";
            string wholefiledata_hex = "";

            if (filepath == "" && filestream == null)
            {
                AppendOutput("Error: Argument \"-f\" or file stream is required.");
            }
            if (filepath != "" && !File.Exists(filepath))
            {
                AppendOutput("Error: File not found.");
            }

            ulong sectorsize = 0;
            if (memorytype == "emmc")
                sectorsize = 512UL;
            else if (memorytype == "ufs")
                sectorsize = 4096UL;

            // 初始化各类变量
            int parnum = 0;
            string[] parname_hex = new string[128];
            string[] parname_ascii = new string[128];
            string[] parstartsec_hex = new string[128];
            ulong[] parstartsec_dec = new ulong[128];
            ulong[] parstartb_dec = new ulong[128];
            string[] parstartb_hex = new string[128];
            string[] parendsec_hex = new string[128];
            ulong[] parendsec_dec = new ulong[128];
            ulong[] parendb_dec = new ulong[128];
            string[] parendb_hex = new string[128];
            ulong[] parsizesec_dec = new ulong[128];
            ulong[] parsizeb_dec = new ulong[128];
            string[] parsizeb_hex = new string[128];
            ulong[] parsizekb_dec = new ulong[128];
            string[] partype = new string[128];
            string[] parguid = new string[128];
            ulong[] paroffsetsec_dec = new ulong[128];
            int npdnum = 0;
            int[] npdpreviousparnum = new int[128];
            int[] npdnextparnum = new int[128];
            string[] npdstartsec_hex = new string[128];
            ulong[] npdstartsec_dec = new ulong[128];
            ulong[] npdstartb_dec = new ulong[128];
            string[] npdstartb_hex = new string[128];
            string[] npdendsec_hex = new string[128];
            ulong[] npdendsec_dec = new ulong[128];
            ulong[] npdendb_dec = new ulong[128];
            string[] npdendb_hex = new string[128];
            ulong[] npdsizesec_dec = new ulong[128];
            string[] npdsizesec_hex = new string[128];
            ulong[] npdsizeb_dec = new ulong[128];
            string[] npdsizeb_hex = new string[128];
            ulong[] npdsizekb_dec = new ulong[128];

            // 文件大小检测
            ulong length = filepath != ""
                ? (ulong)new FileInfo(filepath).Length
                : filestream == null ? 0UL : (ulong)filestream.Length;

            if (length == 0UL)
            {
                AppendOutput("Error: File size is 0.");
            }

            // 读取文件流，并转换为十六进制字符串
            using (MemoryStream memoryStream = new())
            {
                if (filepath != "")
                {
                    using FileStream fileStream = new(filepath, FileMode.Open, FileAccess.Read);
                    byte[] buffer = new byte[51200];
                    int count;
                    while ((count = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                        memoryStream.Write(buffer, 0, count);
                }
                else
                {
                    filestream?.CopyTo(memoryStream);
                }
                wholefiledata_hex = BitConverter
                    .ToString(memoryStream.ToArray(), 0)
                    .Replace("-", string.Empty)
                    .ToLower();
            }

            // 如果sectorsize还是0，没有设置-m参数或内存类型不正确
            if (sectorsize == 0UL)
            {
                AppendOutput("Error: Argument \"-m\" is required.");
            }

            // 分析GPT
            if (ChkGPTHeaderSig(sectorsize, ref wholefiledata_hex)
                && GPTAnalyse(sectorsize * 2UL,
                              ref wholefiledata_hex,
                              ref warninfo,
                              ref parnum,
                              parname_hex,
                              parname_ascii,
                              parstartsec_hex,
                              parstartsec_dec,
                              parstartb_dec,
                              parstartb_hex,
                              parendsec_hex,
                              parendsec_dec,
                              parendb_dec,
                              parendb_hex,
                              parsizesec_dec,
                              parsizeb_dec,
                              parsizeb_hex,
                              parsizekb_dec,
                              partype,
                              parguid,
                              paroffsetsec_dec,
                              memorytype,
                              ref npdnum,
                              npdpreviousparnum,
                              npdnextparnum,
                              npdstartsec_hex,
                              npdstartsec_dec,
                              npdstartb_dec,
                              npdstartb_hex,
                              npdendsec_hex,
                              npdendsec_dec,
                              npdendb_dec,
                              npdendb_hex,
                              npdsizesec_dec,
                              npdsizesec_hex,
                              npdsizeb_dec,
                              npdsizeb_hex,
                              npdsizekb_dec,
                              sectorsize))
            {
                GPTPrint(ref warninfo, parnum, parname_ascii, parstartsec_dec, parsizesec_dec);
            }
            else
            {
                AppendOutput("Error: Failed to analyze according to type \"gptmain\".");
            }

            // 返回累积的输出日志
            return outputLog;

            // 将分区信息写入输出文本
            bool GPTPrint(ref string warnInformation,
                          int partitionCount,
                          string[] partitionNameAscii,
                          ulong[] partitionStartSecDec,
                          ulong[] partitionSizeSecDec)
            {
                if (!string.IsNullOrEmpty(warnInformation))
                    AppendOutput(warnInformation);

                for (int i = 1; i <= partitionCount; ++i)
                {
                    AppendOutput($"{partitionNameAscii[i]} {partitionStartSecDec[i]} {partitionSizeSecDec[i]}");
                }
                return true;
            }

            // 检查GPT头部签名
            bool ChkGPTHeaderSig(ulong startoffset_byte, ref string wholefiledataHex)
            {
                ulong startIndex = startoffset_byte * 2UL;
                // 长度检查，防止超出范围
                if ((int)startIndex + 16 <= wholefiledataHex.Length)
                {
                    if (wholefiledataHex.Substring((int)startIndex, 16) == "4546492050415254")
                        return true;
                }
                AppendOutput("Error: Failed to check GPT header signature.");
                return false;
            }

            // 分析GPT
            bool GPTAnalyse(
                ulong startoffset_byte,
                ref string wholefiledataHex,
                ref string warnInformation,
                ref int partitionCount,
                string[] parnameHex,
                string[] parnameAscii,
                string[] parstartsecHex,
                ulong[] parstartsecDec,
                ulong[] parstartbDec,
                string[] parstartbHex,
                string[] parendsecHex,
                ulong[] parendsecDec,
                ulong[] parendbDec,
                string[] parendbHex,
                ulong[] parsizesecDec,
                ulong[] parsizebDec,
                string[] parsizebHex,
                ulong[] parsizekbDec,
                string[] partype,
                string[] parguid,
                ulong[] paroffsetsecDec,
                string memtype,
                ref int npdCount,
                int[] npdpreviousparnum,
                int[] npdnextparnum,
                string[] npdstartsecHex,
                ulong[] npdstartsecDec,
                ulong[] npdstartbDec,
                string[] npdstartbHex,
                string[] npdendsecHex,
                ulong[] npdendsecDec,
                ulong[] npdendbDec,
                string[] npdendbHex,
                ulong[] npdsizesecDec,
                string[] npdsizesecHex,
                ulong[] npdsizebDec,
                string[] npdsizebHex,
                ulong[] npdsizekbDec,
                ulong sectorSize)
            {
                ulong startIndex = startoffset_byte * 2UL;
                partitionCount = 0;

                // 边界检查，防止截取越界
                if (wholefiledataHex.Length < (int)startIndex + 64)
                {
                    LogWarns("Partition analysis boundary issue.", ref warnInformation);
                    return false;
                }

                // 计算分区数量
                while (true)
                {
                    int offsetCheck = (int)(startIndex + 64UL + 256UL * (ulong)partitionCount);
                    if (offsetCheck + 32 > wholefiledataHex.Length)
                        break;

                    if (wholefiledataHex.Substring(offsetCheck, 32) == "00000000000000000000000000000000")
                        break;

                    partitionCount++;
                }

                if (partitionCount == 0)
                {
                    LogWarns("No partition found.", ref warnInformation);
                    return true;
                }

                int substringLen = partitionCount * 256;
                if (wholefiledataHex.Length < (int)startIndex + substringLen)
                {
                    LogWarns("Partition parse boundary issue.", ref warnInformation);
                    return false;
                }

                string str1 = wholefiledataHex.Substring((int)startIndex, substringLen);

                // 解析每个分区信息
                for (int idx = 1; idx <= partitionCount; ++idx)
                {
                    parstartsecHex[idx] = GetHexSubstring(str1, 78, idx);
                    parstartsecDec[idx] = Convert.ToUInt64(parstartsecHex[idx], 16);
                    parstartbDec[idx] = parstartsecDec[idx] * sectorSize;
                    parstartbHex[idx] = parstartbDec[idx].ToString("X");

                    parendsecHex[idx] = GetHexSubstring(str1, 94, idx);
                    parendsecDec[idx] = Convert.ToUInt64(parendsecHex[idx], 16);
                    parendbDec[idx] = parendsecDec[idx] * sectorSize;
                    parendbHex[idx] = parendbDec[idx].ToString("X");

                    parsizesecDec[idx] = parstartsecDec[idx] <= parendsecDec[idx] + 1UL
                        ? parendsecDec[idx] - parstartsecDec[idx] + 1UL
                        : 0UL;
                    parsizebDec[idx] = parsizesecDec[idx] * sectorSize;
                    parsizebHex[idx] = parsizebDec[idx].ToString("X");
                    parsizekbDec[idx] = parsizebDec[idx] / 1024UL;

                    parnameHex[idx] = GetPartitionName(str1, idx);
                    parnameAscii[idx] = parnameHex[idx] == null
                        ? "-"
                        : Encoding.ASCII.GetString(HexToBytes(parnameHex[idx]));

                    if (parnameHex[idx] == null)
                        LogWarns("Partition " + idx.ToString() + " has no name.", ref warnInformation);

                    partype[idx] = GetPartitionType(str1, idx);
                    parguid[idx] = GetPartitionGuid(str1, idx);
                }

                // 处理分区（空洞、排序等）
                ProcessPartitions(partitionCount, memtype, sectorSize,
                                  ref npdCount,
                                  npdpreviousparnum,
                                  npdnextparnum,
                                  npdstartsecHex,
                                  npdstartsecDec,
                                  npdstartbDec,
                                  npdstartbHex,
                                  npdendsecHex,
                                  npdendsecDec,
                                  npdendbDec,
                                  npdendbHex,
                                  npdsizesecDec,
                                  npdsizesecHex,
                                  npdsizebDec,
                                  npdsizebHex,
                                  npdsizekbDec,
                                  ref warnInformation);

                return true;
            }

            string GetHexSubstring(string str, int start, int index)
            {
                return str.Substring(start + 256 * (index - 1), 2)
                       + str.Substring(start - 2 + 256 * (index - 1), 2)
                       + str.Substring(start - 4 + 256 * (index - 1), 2)
                       + str.Substring(start - 6 + 256 * (index - 1), 2)
                       + str.Substring(start - 8 + 256 * (index - 1), 2)
                       + str.Substring(start - 10 + 256 * (index - 1), 2)
                       + str.Substring(start - 12 + 256 * (index - 1), 2)
                       + str.Substring(start - 14 + 256 * (index - 1), 2);
            }

            string GetPartitionName(string str, int idx)
            {
                bool flag = false;
                string name = "";
                for (int i = 35; i >= 0; i--)
                {
                    string str2 = str.Substring(256 * (idx - 1) + 112 + i * 4, 2);
                    if (str2 == "00" || str2 == "20")
                    {
                        if (flag)
                        {
                            name = "2B" + name;
                            LogWarns($"Partition {idx} has spaces in name.", ref warninfo);
                        }
                    }
                    else
                    {
                        flag = true;
                        name = str2 + name;
                    }
                }
                return name;
            }

            string GetPartitionType(string str, int idx)
            {
                return str.Substring(256 * (idx - 1) + 6, 2)
                       + str.Substring(256 * (idx - 1) + 4, 2)
                       + str.Substring(256 * (idx - 1) + 2, 2)
                       + str.Substring(256 * (idx - 1), 2)
                       + "-"
                       + str.Substring(256 * (idx - 1) + 10, 2)
                       + str.Substring(256 * (idx - 1) + 8, 2)
                       + "-"
                       + str.Substring(256 * (idx - 1) + 14, 2)
                       + str.Substring(256 * (idx - 1) + 12, 2)
                       + "-"
                       + str.Substring(256 * (idx - 1) + 16, 4)
                       + "-"
                       + str.Substring(256 * (idx - 1) + 20, 12);
            }

            string GetPartitionGuid(string str, int idx)
            {
                return str.Substring(256 * (idx - 1) + 32 + 6, 2)
                       + str.Substring(256 * (idx - 1) + 32 + 4, 2)
                       + str.Substring(256 * (idx - 1) + 32 + 2, 2)
                       + str.Substring(256 * (idx - 1) + 32, 2)
                       + "-"
                       + str.Substring(256 * (idx - 1) + 32 + 10, 2)
                       + str.Substring(256 * (idx - 1) + 32 + 8, 2)
                       + "-"
                       + str.Substring(256 * (idx - 1) + 32 + 14, 2)
                       + str.Substring(256 * (idx - 1) + 32 + 12, 2)
                       + "-"
                       + str.Substring(256 * (idx - 1) + 32 + 16, 4)
                       + "-"
                       + str.Substring(256 * (idx - 1) + 32 + 20, 12);
            }

            void ProcessPartitions(
                int partitionCount,
                string memtype,
                ulong sectorSize,
                ref int npdCount,
                int[] npdpreviousparnum,
                int[] npdnextparnum,
                string[] npdstartsecHex,
                ulong[] npdstartsecDec,
                ulong[] npdstartbDec,
                string[] npdstartbHex,
                string[] npdendsecHex,
                ulong[] npdendsecDec,
                ulong[] npdendbDec,
                string[] npdendbHex,
                ulong[] npdsizesecDec,
                string[] npdsizesecHex,
                ulong[] npdsizebDec,
                string[] npdsizebHex,
                ulong[] npdsizekbDec,
                ref string warnInformation)
            {
                string[] array = new string[partitionCount + 1];
                for (int i = 0; i <= partitionCount; ++i)
                {
                    if (i == 0)
                    {
                        parstartsec_dec[i] = 0UL;
                        parendsec_dec[i] = memtype == "emmc" ? 33UL : 5UL;
                    }
                    array[i] = i.ToString() + " " + parstartsec_dec[i].ToString() + " " + parendsec_dec[i].ToString();
                }
                // 根据起始扇区对分区排序
                Array.Sort(array, (x, y) =>
                    ulong.Parse(x.Split(' ')[1]).CompareTo(ulong.Parse(y.Split(' ')[1])));

                int[] numArray1 = new int[partitionCount + 1];
                ulong[] numArray2 = new ulong[partitionCount + 1];
                ulong[] numArray3 = new ulong[partitionCount + 1];

                // 解析排序后数据
                for (int i = 0; i <= partitionCount; ++i)
                {
                    numArray1[i] = int.Parse(array[i].Split(' ')[0]);
                    numArray2[i] = ulong.Parse(array[i].Split(' ')[1]);
                    numArray3[i] = ulong.Parse(array[i].Split(' ')[2]);
                }

                // 检查相邻分区是否连续
                for (int i = 0; i < partitionCount; ++i)
                {
                    if (numArray3[i] + 1UL != numArray2[i + 1])
                    {
                        if (numArray3[i] + 1UL < numArray2[i + 1])
                        {
                            npdCount++;
                            npdpreviousparnum[npdCount] = i;
                            npdnextparnum[npdCount] = i + 1;
                            npdstartsecDec[npdCount] = numArray3[i] + 1UL;
                            npdstartsecHex[npdCount] = npdstartsecDec[npdCount].ToString("X");
                            npdstartbDec[npdCount] = npdstartsecDec[npdCount] * sectorSize;
                            npdstartbHex[npdCount] = npdstartbDec[npdCount].ToString("X");
                            npdendsecDec[npdCount] = numArray2[i + 1] - 1UL;
                            npdendsecHex[npdCount] = npdendsecDec[npdCount].ToString("X");
                            npdendbDec[npdCount] = npdendsecDec[npdCount] * sectorSize;
                            npdendbHex[npdCount] = npdendbDec[npdCount].ToString("X");
                            npdsizesecDec[npdCount] = npdendsecDec[npdCount] - npdstartsecDec[npdCount] + 1UL;
                            npdsizesecHex[npdCount] = npdsizesecDec[npdCount].ToString("X");
                            npdsizebDec[npdCount] = npdsizesecDec[npdCount] * sectorSize;
                            npdsizebHex[npdCount] = npdsizebDec[npdCount].ToString("X");
                            npdsizekbDec[npdCount] = npdsizebDec[npdCount] / 1024UL;
                            // 假设记录分区空洞大小
                            paroffsetsec_dec[i + 1] = npdsizesecDec[npdCount];
                        }
                        else
                        {
                            LogWarns($"Partition {i} overlap.", ref warnInformation);
                        }
                    }
                }
            }

            // 工具方法：将警告信息添加到warninfo
            void LogWarns(string message, ref string warnInformation)
            {
                warnInformation += "[Warn]" + message + Environment.NewLine;
            }

            // 工具方法：在输出日志里追加内容
            void AppendOutput(string message)
            {
                outputLog += message + Environment.NewLine;
            }

            // 将十六进制字符串转换成字节
            static byte[] HexToBytes(string hex)
            {
                int length = hex.Length / 2;
                byte[] bytes = new byte[length];
                for (int i = 0; i < length; ++i)
                {
                    bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
                }
                return bytes;
            }
        }
    }
}