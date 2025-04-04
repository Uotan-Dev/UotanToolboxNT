using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using WuXingLibrary.code.module;
using WuXingLibrary.code.Utility;

namespace WuXingLibrary;

public class Flash
{
    public Comm comm = new();

    private readonly int BUFFER_SECTORS = 256;
    private readonly string storageType;
    private readonly int SectorSize;
    public bool verbose;
    private readonly string deviceName;
    public static bool isOpen;


    private static float progress;
    private static long processed;
    private static long totalSize;

    // 定义进度更新事件
    public static event EventHandler<ProgressEventArgs> ProgressUpdated;







    public Flash(string portName, string storagetype)
    {
        deviceName = portName;
        storageType = storagetype;
        SectorSize = storageType == "ufs" ? 4096 : 512;  // 如果是 "ufs" 则为 4096，否则为 512
        comm.intSectorSize = storageType == "ufs" ? comm.SECTOR_SIZE_UFS : comm.SECTOR_SIZE_EMMC;
    }

    internal static void UpdateDeviceStatus(float? _progress, long? _processed, long? _totalSize, string status)
    {
        if (_progress.HasValue)
            progress = _progress.Value;
        if (_processed.HasValue)
            processed = _processed.Value;
        if (_totalSize.HasValue)
            totalSize = _totalSize.Value;
        // 触发进度更新事件
        OnProgressUpdated(new ProgressEventArgs(progress, processed, totalSize, status));
    }



    // 触发进度更新事件
    protected static void OnProgressUpdated(ProgressEventArgs e)
    {
        ProgressUpdated?.Invoke(null, e);
    }

    // 进度事件参数类
    public class ProgressEventArgs(float progress, long processed, long totalSize, string status) : EventArgs
    {
        public float Progress { get; } = progress;
        public long Processed { get; } = processed;
        public long TotalSize { get; } = totalSize;
        public string Status { get; } = status;
    }


    public class SparseFileChecker
    {
        private const int SPARSE_HEADER_MAGIC = -316211398; // 稀疏文件的魔数

        public static bool IsSparseFile(string filePath)
        {
            try
            {
                // 读取文件的前 28 个字节，因为稀疏文件头的大小是 28 个字节
                byte[] headerBytes = new byte[28];
                using (FileStream fs = new(filePath, FileMode.Open, FileAccess.Read))
                {
                    if (fs.Read(headerBytes, 0, headerBytes.Length) != headerBytes.Length)
                    {
                        return false;
                    }
                }

                // 从字节数组中读取魔数
                int magic = BitConverter.ToInt32(headerBytes, 0);

                // 检查魔数是否匹配
                return magic == SPARSE_HEADER_MAGIC;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
                return false;
            }
        }
    }


    public void RegisterPort()
    {
        if (comm == null)
        {
            throw new InvalidOperationException("comm is not initialized.");
        }

        comm.serialPort ??= new SerialPort();

        comm.serialPort.Close();
        comm.serialPort.PortName = deviceName;
        comm.serialPort.BaudRate = 9600;
        comm.serialPort.Parity = Parity.None;
        comm.serialPort.ReadTimeout = 100;
        comm.serialPort.WriteTimeout = 100;
        comm.Open();
    }

    public bool Sahara<T>(T programmerFile)
    {
        try
        {
            Stream programmerStream = null;
            if (programmerFile is string filePath)
            {
                var fileSize = new FileInfo(filePath).Length;

                // 如果文件小于2MB，直接读取到内存
                if (fileSize <= 2097152) // 2MB
                {
                    using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    byte[] fileBytes = new byte[fileSize];
                    fileStream.Read(fileBytes, 0, (int)fileSize);
                    programmerStream = new MemoryStream(fileBytes);
                }
                else
                {
                    // 对于大文件，使用BufferedStream包装FileStream以提高性能
                    const int bufferSize = 2097152; // 2MB缓冲区
                    programmerStream = new BufferedStream(
                        new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read),
                        bufferSize);
                }
            }
            else if (programmerFile is Stream stream)
            {
                programmerStream = stream;
            }
            else
            {
                throw new ArgumentException("Unsupported programmer file type.");
            }

            using (programmerStream)
            {
                SaharaDownloadProgrammer(programmerStream);
            }

            Ping();
            return true;
        }
        catch (Exception ex)
        {
            Log.W("Sahara Flasher Error Status: " + ex.Message, new Exception(comm.serialPort.PortName), false);
            return false;
        }
    }



    public void SaharaDownloadProgrammer(Stream programmerStream)
    {
        if (comm.IsOpen)
        {
            string msg = string.Format("[{0}]:{1}", comm.serialPort.PortName, "start flash.");
            Log.W(comm.serialPort.PortName, msg);

            // 创建并初始化Sahara切换模式包
            sahara_switch_Mode_packet sahara_switch_Mode_packet = default;
            sahara_switch_Mode_packet.Command = 19u;
            sahara_switch_Mode_packet.Length = 8u;
            byte[] array = new byte[12];
            comm.GetRecDataIgnoreExcep();

            if (comm.recData == null || comm.recData.Length == 0)
            {
                comm.recData = new byte[48];
            }

            // 解析接收到的Hello包
            sahara_hello_packet sahara_hello_packet = (sahara_hello_packet)CommandFormat.BytesToStuct(comm.recData, typeof(sahara_hello_packet));
            sahara_hello_packet.Reserved = new uint[6];
            sahara_hello_response sahara_hello_response = default;
            sahara_hello_response.Reserved = new uint[6];
            int num = 3;

            // 尝试接收Hello包，最多尝试3次
            while (num-- > 0 && sahara_hello_packet.Command != 1)
            {
                msg = "cannot receive hello packet,MiFlash is trying to reset status!";
                Log.W(comm.serialPort.PortName, msg);
                comm.GetRecDataIgnoreExcep();

                if (comm.recData == null || comm.recData.Length == 0)
                {
                    comm.recData = new byte[48];
                }

                sahara_hello_packet = (sahara_hello_packet)CommandFormat.BytesToStuct(comm.recData, typeof(sahara_hello_packet));
                Thread.Sleep(500);

                if (sahara_hello_packet.Command == 1)
                {
                    continue;
                }

                msg = "try to reset status.";
                Log.W(comm.serialPort.PortName, msg);

                if (comm.IsOpen)
                {
                    Log.W(comm.serialPort.PortName, "清理端口数据，准备发送SAHARA_SWITCH_MODE_PACKET");
                    comm.serialPort.DiscardInBuffer();
                    comm.serialPort.DiscardOutBuffer();
                }

                // 初始化并发送Hello响应
                sahara_hello_response = default;
                sahara_hello_response.Reserved = new uint[6];
                sahara_hello_response.Command = 2u;
                sahara_hello_response.Length = 48u;
                sahara_hello_response.Version = 2u;
                sahara_hello_response.Version_min = 1u;
                sahara_hello_response.Mode = 3u;
                byte[] array2 = CommandFormat.StructToBytes(sahara_hello_response);
                comm.WritePort(array2, 0, array2.Length);
                comm.GetRecDataIgnoreExcep();
                msg = "Switch mode back";
                Log.W(comm.serialPort.PortName, msg);

                // 发送Sahara切换模式包
                sahara_switch_Mode_packet.Command = 12u;
                sahara_switch_Mode_packet.Length = 12u;
                sahara_switch_Mode_packet.Mode = 0u;
                array2 = CommandFormat.StructToBytes(sahara_switch_Mode_packet);
                Array.ConstrainedCopy(array2, 0, array, 0, 12);
                comm.WritePort(array, 0, array.Length);
            }

            if (sahara_hello_packet.Command == 1)
            {
                msg = "received hello packet";
                Log.W(comm.serialPort.PortName, msg);

                // 初始化并发送第二次Hello响应
                sahara_hello_response = default;
                sahara_hello_response.Reserved = new uint[6];
                sahara_hello_response.Command = 2u;
                sahara_hello_response.Length = 48u;
                sahara_hello_response.Version = 2u;
                sahara_hello_response.Version_min = 1u;
                byte[] array3 = CommandFormat.StructToBytes(sahara_hello_response);
                comm.WritePort(array3, 0, array3.Length);
                Log.W(comm.serialPort.PortName, "filePath: " + "MemoryStream");

                if (programmerStream != null)
                {
                    Log.W(comm.serialPort.PortName, "download programmer from MemoryStream");

                    // 使用MemoryStream初始化FileTransfer
                    FileTransfer fileTransfer = new(comm.serialPort.PortName, programmerStream);

                    bool flag;
                    sahara_packet sahara_packet;

                    do
                    {
                        flag = false;
                        comm.GetRecData();
                        sahara_packet = (sahara_packet)CommandFormat.BytesToStuct(comm.recData, typeof(sahara_packet));

                        switch (sahara_packet.Command)
                        {
                            case 3u:
                                // 处理读取数据包
                                sahara_readdata_packet sahara_readdata_packet = (sahara_readdata_packet)CommandFormat.BytesToStuct(comm.recData, typeof(sahara_readdata_packet));
                                msg = $"sahara read data:imgID {sahara_readdata_packet.Image_id}, offset {sahara_readdata_packet.Offset}, length {sahara_readdata_packet.SLength}";
                                fileTransfer.Transfer(comm.serialPort, (int)sahara_readdata_packet.Offset, (int)sahara_readdata_packet.SLength);
                                Log.W(comm.serialPort.PortName, msg);
                                break;
                            case 18u:
                                // 处理64位读取数据包
                                sahara_64b_readdata_packet sahara_64b_readdata_packet = (sahara_64b_readdata_packet)CommandFormat.BytesToStuct(comm.recData, typeof(sahara_64b_readdata_packet));
                                msg = $"sahara read 64b data:imgID {sahara_64b_readdata_packet.Image_id}, offset {sahara_64b_readdata_packet.Offset}, length {sahara_64b_readdata_packet.SLength}";
                                fileTransfer.Transfer(comm.serialPort, (int)sahara_64b_readdata_packet.Offset, (int)sahara_64b_readdata_packet.SLength);
                                Log.W(comm.serialPort.PortName, msg);
                                break;
                            case 4u:
                                // 处理结束传输包
                                sahara_end_transfer_packet sahara_end_transfer_packet = (sahara_end_transfer_packet)CommandFormat.BytesToStuct(comm.recData, typeof(sahara_end_transfer_packet));
                                _ = $"sahara read end imgID:{sahara_end_transfer_packet.Image_id} status:{sahara_end_transfer_packet.Status}";

                                if (sahara_end_transfer_packet.Status != 0)
                                {
                                    Log.W(comm.serialPort.PortName, $"sahara read end error with status:{sahara_end_transfer_packet.Status}");
                                }
                                flag = true;
                                break;
                            default:
                                msg = $"invalid command:{sahara_packet.Command}";
                                Log.W(comm.serialPort.PortName, msg);
                                break;
                        }
                    }
                    while (!flag);

                    // 发送完成包
                    Log.W(comm.serialPort.PortName, "Send done packet");
                    sahara_packet.Command = 5u;
                    sahara_packet.Length = 8u;
                    byte[] array5 = CommandFormat.StructToBytes(sahara_packet, 8);

                    // 清零多余的字节
                    for (int i = 8; i < array5.Length; i++)
                    {
                        array5[i] = 0;
                    }
                    comm.WritePort(array5, 0, array5.Length);
                    comm.GetRecData();

                    if (comm.recData.Length == 0)
                    {
                        comm.recData = new byte[48];
                    }

                    // 解析完成响应
                    sahara_done_response sahara_done_response = (sahara_done_response)CommandFormat.BytesToStuct(comm.recData, typeof(sahara_done_response));

                    if (sahara_done_response.Command == 6)
                    {
                        msg = "programmer transferred successfully";
                        Log.W(comm.serialPort.PortName, msg);
                        fileTransfer.CloseTransfer();
                        Thread.Sleep(1000);
                        return;
                    }

                    msg = "programmer transfer error " + sahara_done_response.Command;
                    throw new Exception(msg);
                }

                throw new Exception("Cannot find programmer data in MemoryStream.");
            }

            Log.W(comm.serialPort.PortName, "port " + comm.serialPort.PortName + " is not open.");
            throw new Exception("port " + comm.serialPort.PortName + " is not open.");
        }
    }



    private void Ping()
    {
        Log.W(comm.serialPort.PortName, "send nop command");
        //FlashingDevice.UpdateDeviceStatus(comm.serialPort.PortName, 0f, "ping target via firehose", "flashing", isDone: false);
        string command = string.Format(Firehose.Nop, verbose ? "1" : "0");
        if (!comm.SendCommand(command, checkAck: true))
        {
            throw new Exception("ping target failed");
        }
        //FlashingDevice.UpdateDeviceStatus(comm.serialPort.PortName, 1f, "ping target via firehose", "flashing", isDone: false);
    }


    private string ExtractSigValue(string response)
    {
        int startIndex = response.IndexOf("<data>");
        int endIndex = response.IndexOf("</data>") + "</data>".Length;

        // 确保两个标签都存在
        if (startIndex != -1 && endIndex != -1)
        {
            string xmlPart = response.Substring(startIndex, endIndex - startIndex);
            XDocument doc = XDocument.Parse(xmlPart);

            var sigElement = doc.Descendants("sig").FirstOrDefault();
            if (sigElement != null)
            {
                return sigElement.Attribute("value")?.Value;
            }
        }
        return null;
    }

    public string GetBlob()
    {
        // 1. 获取响应并提取 <sig> 值
        comm.CleanBuffer();
        string response0 = comm.SendXml(Firehose.REQ_AUTH);
        Log.W("get blob response", comm.serialPort.PortName, response0);
        string sigValue = ExtractSigValue(response0);

        if (!string.IsNullOrEmpty(sigValue))
        {
            comm.CleanBuffer();
            return sigValue;

        }
        return null;
    }




    public bool BypassSendSig()
    {
        string[] sigs =
        [
    "BF35D6013A39D6166BE0387E6B9B00FD0E096283F811EDE81594866CF676B41B1A32EA67FBAB4F6D90E45C944B53302A1DA32D94F30A68E1EB116672B02920089AA938F91464D6926C42A93D0EAE88E549A49C00FCF9B1B89EF68A7CD23DEBEB88C01D850ACD52A832BB80134C4B0E2A7A1422E2530C19B309EBA1FF7E123A34DD3B83DCFACDCE45F303D135FE58899E531E1CF7155D48BFF18AB3E5FC1A2E85FBB015DE2A3CFC8EE51AA453F7DEBC4A095861DA1637C8DF4D9CF64EC4A5F45486AD73FB036965B94E1EE8F4077FFB54E90AF0AB52BF02E499517FB7D1028ABCBA1B98951843B2A8C964B4D94801BAF630C6179FA6F86371830A484F2792D491",
        "600000010800936E3A8E573CAD07C167644B61217835D85AD4FDDB7D840A2B7225432FCDA13A7C192CFA979ED16517E6970B1B07DF6C516FEC81F6968FCF7FFDDBC397A162C2CA3E5D76124AA1769F1B2164B39B76930B4CC67519F7F339877677F4E8AF25828682BCBF4E593A57E7E30532699253E0B1CC5D9D0D554AF2BD46D56F18D6E5290BA4A0CAC2431F9F19C4C1A39D7664FFAB48A9E11A559386819835B84DF5675E70D25FDB5123E7B040FE21108F0AE6D7D9D267F2C9C61AD054C68493DC4D33F74D0CF2D4AADCD430152DB67C22A181AD6D7761637F70CBDA884CDC11337203837790E6845CA5A8767930B9C26FDA71272564CA34763D352F5FE4",
        "936E3A8E573CAD07C167644B61217835D85AD4FDDB7D840A2B7225432FCDA13A7C192CFA979ED16517E6970B1B07DF6C516FEC81F6968FCF7FFDDBC397A162C2CA3E5D76124AA1769F1B2164B39B76930B4CC67519F7F339877677F4E8AF25828682BCBF4E59600000110532699253E0B1CC5D9D0D554AF2BD46D56F18D6E5290BA4A0CAC2431F9F19C4C1A39D7664FFAB48A9E11A559386819835B84DF5675E70D25FDB5123E7B040FE21108F0AE6D7D9D267F2C9C61AD054C68493DC4D33F74D0CF2D4AADCD430152DB67C22A181AD6D7761637F70CBDA884CDC11337203837790E6845CA5A8767930B9C26FDA71272564CA34763D352F5FE42AB738FB38A5",
        "936E3A8E573CAD07C167644B61217835D85AD4FDDB7D840A2B7225432FCDA13A7C192CFA979ED16517E6970B1B07DF6C516FEC81F6968FCF7FFDDBC397A162C2CA3E5D76124AA1769F1B2164B39B76930B4CC67519F7F339877677F4E8AF25828682BCBF4E593A57E7E30532699253E0B1CC5D9D0D554AF2BD46D56F18D6E5290BA4A0CAC2431F9F19C4C1A39D7664FFAB48A9E11A559386819835B84DF5675E70D25FDB5123E7B040FE21108F0AE6D7D9D267F2C9C61AD054C68493DC4D33F74D0CF2D4AADCD430152DB67C22A181AD6D7761637F70CBDA884CDC11337203837790E6845CA5A8767930B9C26FDA71272564CA34763D352F5FE42AB738FB38A5"
];

        // Initially assume all signatures will fail
        bool allFailed = true;

        if (comm.IsOpen)
        {
            foreach (var sig in sigs)
            {
                try
                {
                    // Attempt to send each signature
                    var result = SendSignature(sig);

                    if (result)
                    {
                        Log.W(comm.serialPort.PortName, "send sig successful: " + sig);
                        // If one signature succeeds, set allFailed to false and stop checking further
                        allFailed = false;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Log.W(comm.serialPort.PortName, "send sig failed: " + ex.Message);
                }
            }

            // If all signatures failed, log an error
            if (allFailed)
            {
                Log.W(comm.serialPort.PortName, "all sign failed: " + null);
            }
        }

        // Return true if at least one signature passed, false otherwise
        return !allFailed;
    }


    // 发送 XML 消息和十六进制签名
    public bool SendSignature(string sig)

    {
        bool flag;
        string aUTHP = Firehose.AUTHP;
        Log.W(comm.serialPort.PortName, "siged:" + sig);
        List<byte> list = [];
        for (int i = 0; i < sig.Length; i += 2)
        {
            string s = sig.Substring(i, 2);
            list.Add(byte.Parse(s, NumberStyles.AllowHexSpecifier));
        }
        byte[] array = [.. list];
        string command = string.Format(aUTHP, array.Length);
        flag = comm.SendCommand(command, checkAck: true);
        if (!flag)
        {
            throw new Exception("authentication failed");
        }
        StringBuilder stringBuilder = new();
        StringBuilder stringBuilder2 = new();
        byte[] array2 = array;
        for (int j = 0; j < array2.Length; j++)
        {
            byte b = array2[j];
            stringBuilder.Append(b + " ");
            stringBuilder2.Append("0x" + b.ToString("X2") + " ");
        }
        comm.WritePort(array, 0, array.Length);
        if (!comm.GetResponse(waiteACK: true))
        {
            throw new Exception("authentication failed");
        }
        return flag;
    }





    public string ConfigureDDR()
    {
        try
        {
            Log.W(comm.serialPort.PortName, "send configure command");
            string command = string.Format(Firehose.Configure, verbose ? "1" : "0", SectorSize * BUFFER_SECTORS, storageType, 0);
            bool flag = comm.SendCommand(command, checkAck: true);
            if (!flag)
            {
                return "needsig";

            }
            Log.W(comm.serialPort.PortName, "max buffer sector is " + comm.m_dwBufferSectors);
            return "success";
        }
        catch (Exception ex)
        {
            Log.W(comm.serialPort.PortName, "configure ddr failed: " + ex.Message);
            return "failed";
        }

    }



    public void Close()
    {
        comm.Close();
    }


    public void SetBootPartition()
    {
        string text = "Set Boot Partition ";
        string command = string.Format(Firehose.SetBootPartition, verbose ? "1" : "0");
        if (!comm.SendCommand(command, checkAck: true))
        {
            throw new Exception("set boot partition failed");
        }
        Log.W(comm.serialPort.PortName, text);
    }


    public void Readpartitiontable()
    {
        //改成程序所在目录下的tool\gpt文件夹下
        string outputfilepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tool", "gpt");
        //删除文件夹下的所有文件
        if (Directory.Exists(outputfilepath))
        {
            DirectoryInfo di = new(outputfilepath);
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
        }
        if (!Directory.Exists(outputfilepath))
        {
            Directory.CreateDirectory(outputfilepath);
        }
        if (storageType == "ufs")
        {
            for (int i = 0; i <= 5; i++)
            {
                UpdateDeviceStatus(0, 0, 0, "readstrart");
                string str = string.Format(Firehose.FIREHOSE_READ, SectorSize, 6, 0, i);
                comm.SendCommand(str);
                long totalSize = SectorSize * 6;

                string outputFile = Path.Combine(outputfilepath, $"gpt_main{i}.bin");

                comm.ReadAndWriteToFile(outputFile, totalSize);
                UpdateDeviceStatus(0, 0, 0, "readed");
            }
        }
        else
        {
            UpdateDeviceStatus(0, 0, 0, "readstrart");
            string str = string.Format(Firehose.FIREHOSE_READ, SectorSize, 34, 0, 0);
            comm.SendCommand(str);
            long totalSize = SectorSize * 34;
            string outputFile = Path.Combine(outputfilepath, $"gpt_main0.bin");
            comm.ReadAndWriteToFile(outputFile, totalSize);
            UpdateDeviceStatus(0, 0, 0, "readed");
        }
    }

    public void Read(string start_sector, int num_partition_sectors, int physical_partition_number, string outputfilepath)
    {
        //ConfigureDDR(comm.intSectorSize, BUFFER_SECTORS, storageType, 0);
        UpdateDeviceStatus(0, 0, 0, "readstrart");

        string str = string.Format(Firehose.FIREHOSE_READ, SectorSize, num_partition_sectors, start_sector, physical_partition_number);
        comm.SendCommand(str);
        long totalSize = SectorSize * (long)num_partition_sectors;
        comm.ReadAndWriteToFile(outputfilepath, totalSize);
        UpdateDeviceStatus(0, 0, 0, "readed");

    }

    public void WriteRawFileToDevice(string filepath, string file_sector_offset, string start_sector, string num_partition_sectors, string physical_partition_number, string label)
    {
        UpdateDeviceStatus(0, 0, 0, "flashstart");
        //< program SECTOR_SIZE_IN_BYTES = "4096" file_sector_offset = "0" filename = "recovery.img" label = "recovery" num_partition_sectors = "16384" partofsingleimage = "false" physical_partition_number = "0" readbackverify = "false" size_in_KB = "65536.0" sparse = "false" start_byte_hex = "0x28000000" start_sector = "163840" />
        Log.W(comm.serialPort.PortName, "Write file " + filepath + " to partition[" + label + "] sector " + start_sector);
        // 初始化文件传输对象
        FileTransfer fileTransfer = new(comm.serialPort.PortName, filepath);

        // 写入文件
        fileTransfer.WriteFile(
            this,
            start_sector,
            num_partition_sectors,
            filepath,
            file_sector_offset,
            "0",
            SectorSize.ToString(),
            physical_partition_number,
            "",
            chkAck: true,
            1
        );

        // 使用字符串插值记录日志
        Log.W(comm.serialPort.PortName, $"Image {filepath} transferred successfully");
        UpdateDeviceStatus(0, 0, 0, "flashed");
        fileTransfer.CloseTransfer();
        comm.CleanBuffer();
    }


    public void WriteSparseFileToDevice(string filepath, string file_sector_offset, string start_sector, string num_partition_sectors, string physical_partition_number, string label)
    {
        UpdateDeviceStatus(0, 0, 0, "flashstart");
        //< program SECTOR_SIZE_IN_BYTES = "4096" file_sector_offset = "0" filename = "recovery.img" label = "recovery" num_partition_sectors = "16384" partofsingleimage = "false" physical_partition_number = "0" readbackverify = "false" size_in_KB = "65536.0" sparse = "false" start_byte_hex = "0x28000000" start_sector = "163840" />

        Log.W(comm.serialPort.PortName, "Write sparse file " + filepath + " to partition[" + label + "] sector " + start_sector);

        // 初始化文件传输对象
        FileTransfer fileTransfer = new(comm.serialPort.PortName, filepath);
        // 写入文件
        fileTransfer.WriteSparseFileToDevice(
            this,
            start_sector,
            num_partition_sectors,
            filepath,
            file_sector_offset,
            SectorSize.ToString(),
            physical_partition_number,
            ""
        );
        fileTransfer.CloseTransfer();
        // 使用字符串插值记录日志
        Log.W(comm.serialPort.PortName, $"Image {filepath} transferred successfully");
        UpdateDeviceStatus(0, 0, 0, "flashd");
        comm.CleanBuffer();
    }


    public void Erase(int start_sector, int num_partition_sectors, int physical_partition_number)
    {
        string str = string.Format(Firehose.FIREHOSE_ERASE, SectorSize, num_partition_sectors, start_sector, physical_partition_number);
        comm.SendCommand(str);
    }



    public void Reset()
    {
        comm.SendCommand(string.Format(Firehose.Reset_To_Edl, verbose ? "1" : "0"), false);
    }

    public void Reboot()
    {
        comm.SendCommand(string.Format(Firehose.Reset, verbose ? "1" : "0"), false);
    }


    public void ApplyPatches(string swPath)
    {
        string[] pathFiles = FileSearcher.SearchFiles(swPath, SoftwareImage.PatchPattern);

        foreach (var patchFilePath in pathFiles)
        {
            bool result = ApplyPatchesToDevice(patchFilePath);

            if (!result)
            {
                Log.W(comm.serialPort.PortName, $"Failed to apply patch: {patchFilePath}", throwEx: false);
                break;
            }
        }
    }
    private bool ApplyPatchesToDevice(string patchFilePath)
    {
        bool result = true;
        Log.W(comm.serialPort.PortName, "open patch file " + patchFilePath);

        XmlDocument xmlDocument = new();
        XmlReader xmlReader = XmlReader.Create(patchFilePath, new XmlReaderSettings
        {
            IgnoreComments = true
        });
        xmlDocument.Load(xmlReader);
        XmlNodeList childNodes = xmlDocument.SelectSingleNode("patches").ChildNodes;
        string text = "";
        string pszPatchSize = "0";
        string pszPatchValue = "0";
        string pszDiskOffsetSector = "0";
        string pszSectorOffsetByte = "0";
        string pszPhysicalPartitionNumber = "0";
        string pszSectorSizeInBytes = "512";
        try
        {
            foreach (XmlNode item in childNodes)
            {
                if (item.Name.ToLower() != "patch")
                {
                    continue;
                }
                foreach (XmlAttribute attribute in item.Attributes)
                {
                    switch (attribute.Name.ToLower())
                    {
                        case "byte_offset":
                            pszSectorOffsetByte = attribute.Value;
                            break;
                        case "filename":
                            text = attribute.Value;
                            break;
                        case "size_in_bytes":
                            pszPatchSize = attribute.Value;
                            break;
                        case "start_sector":
                            pszDiskOffsetSector = attribute.Value;
                            break;
                        case "value":
                            pszPatchValue = attribute.Value;
                            break;
                        case "sector_size_in_bytes":
                            pszSectorSizeInBytes = attribute.Value;
                            break;
                        case "physical_partition_number":
                            pszPhysicalPartitionNumber = attribute.Value;
                            break;
                    }
                }
                if (text.ToLower() == "disk")
                {
                    ApplyPatch(pszDiskOffsetSector, pszSectorOffsetByte, pszPatchValue, pszPatchSize, pszSectorSizeInBytes, pszPhysicalPartitionNumber);
                    Thread.Sleep(5);
                }
            }
            return result;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
        finally
        {
            xmlReader.Close();
        }
    }

    private void ApplyPatch(string pszDiskOffsetSector, string pszSectorOffsetByte, string pszPatchValue, string pszPatchSize, string pszSectorSizeInBytes, string pszPhysicalPartitionNumber)
    {
        Log.W(comm.serialPort.PortName, "ApplyPatch sector " + pszDiskOffsetSector + ", offset " + pszSectorOffsetByte + ", value " + pszPatchValue + ", size " + pszPatchSize);
        string text = "";
        string command = string.Format(Firehose.FIREHOSE_PATCH, pszSectorSizeInBytes, pszSectorOffsetByte, pszPhysicalPartitionNumber, pszPatchSize, pszDiskOffsetSector, pszPatchValue, text);
        comm.SendCommand(command, checkAck: true);
    }
}


