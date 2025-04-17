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

using EDLLibrary.code.module;
using EDLLibrary.code.Utility;

namespace EDLLibrary;

public class Flash
{
    public Comm comm = new();
    private static Flash _instance;
    private static readonly object _lockObj = new();
    private readonly int BUFFER_SECTORS = 256;
    public string storageType;
    private int SectorSize;
    public bool verbose;
    private string deviceName;
    public static bool isOpen;
    private Stream portBaseStream;

    private Flash()
    {
    }

    public static Flash Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lockObj)
                {
                    _instance ??= new Flash();
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// 使用指定参数初始化Flash实例
    /// </summary>
    /// <param name="portName">串口名称</param>
    /// <param name="storageType">存储类型（如"ufs", "emmc"）</param>
    /// <summary>
    public void Initialize(string portName, string storagetype)
    {
        lock (_lockObj)
        {
            deviceName = portName;
            storageType = storagetype;
            SectorSize = storagetype == "ufs" ? 4096 : 512;
            comm.intSectorSize =
                storagetype == "ufs" ? comm.SECTOR_SIZE_UFS : comm.SECTOR_SIZE_EMMC;
        }
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

        if (!comm.serialPort.IsOpen)
        {
            try
            {
                //关闭基础流
                portBaseStream?.Dispose();
            }
            catch { }
            comm.serialPort.Close();
            comm.serialPort.PortName = deviceName;
            comm.serialPort.BaudRate = 9600;
            comm.serialPort.Parity = Parity.None;
            comm.serialPort.ReadTimeout = 100;
            comm.serialPort.WriteTimeout = 100;
            comm.Open();
            //保存串口基础流,用于重新连接到串口时,关闭基础流
            portBaseStream = comm.serialPort.BaseStream;
        }
        else
        {
            return;
        }
    }

    public bool Sahara<T>(T programmerFile)
    {
        try
        {
            Stream programmerStream = null;
            if (programmerFile is string filePath)
            {
                var fileSize = new FileInfo(filePath).Length;

                const int bufferSize = 2097152; // 2MB缓冲区
                programmerStream = new BufferedStream(
                    new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read),
                    bufferSize
                );
            }
            else
            {
                programmerStream = programmerFile is Stream stream
                    ? stream
                    : throw new ArgumentException("Unsupported programmer file type.");
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
            Log.W(
                "Sahara Flasher Error Status: " + ex.Message,
                new Exception(comm.serialPort.PortName),
                false
            );
            return false;
        }
    }

    private void Ping()
    {
        Log.W(comm.serialPort.PortName, "send nop command");
        string command = string.Format(Firehose.Nop, verbose ? "1" : "0");
        if (!comm.SendCommand(command, checkAck: true))
        {
            throw new Exception("ping target failed");
        }
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

    /// <summary>
    /// 下载程序员固件到设备
    /// </summary>
    public void SaharaDownloadProgrammer(Stream programmerStream)
    {
        if (!comm.IsOpen)
        {
            throw new Exception("port " + comm.serialPort.PortName + " is not open.");
        }

        Log.W(comm.serialPort.PortName, $"[{comm.serialPort.PortName}]:start flash.");

        sahara_hello_packet helloPacket = ReceiveAndValidateHelloPacket();

        SendHelloResponse();

        TransferProgrammer(programmerStream);

        SendDoneAndVerifyResponse();

        Thread.Sleep(1000);
    }

    /// <summary>
    /// 接收并验证Hello包
    /// </summary>
    private sahara_hello_packet ReceiveAndValidateHelloPacket()
    {
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
        sahara_hello_packet sahara_hello_packet = (sahara_hello_packet)
            CommandFormat.BytesToStuct(comm.recData, typeof(sahara_hello_packet));
        sahara_hello_packet.Reserved = new uint[6];
        int retryCount = 5;

        // 尝试接收Hello包，最多尝试5次
        while (retryCount-- > 0 && sahara_hello_packet.Command != 1)
        {
            comm.GetRecDataIgnoreExcep();

            if (comm.recData == null || comm.recData.Length == 0)
            {
                comm.recData = new byte[48];
            }
            Log.W(comm.serialPort.PortName, "cannot receive hello packet,trying to reset status!");
            sahara_hello_packet = (sahara_hello_packet)
                CommandFormat.BytesToStuct(comm.recData, typeof(sahara_hello_packet));
            Thread.Sleep(500);

            if (sahara_hello_packet.Command == 1)
            {
                continue;
            }

            Log.W(comm.serialPort.PortName, "try to reset status.");

            if (comm.IsOpen)
            {
                Log.W(comm.serialPort.PortName, "清理端口数据，准备发送SAHARA_SWITCH_MODE_PACKET");
                comm.serialPort.DiscardInBuffer();
                comm.serialPort.DiscardOutBuffer();
            }

            // 初始化并发送Hello响应以重置状态
            sahara_hello_response hello_resp = default;
            hello_resp.Reserved = new uint[6];
            hello_resp.Command = 2u;
            hello_resp.Length = 48u;
            hello_resp.Version = 2u;
            hello_resp.Version_min = 1u;
            hello_resp.Mode = 3u;
            byte[] respBytes = CommandFormat.StructToBytes(hello_resp);
            comm.WritePort(respBytes, 0, respBytes.Length);
            comm.GetRecDataIgnoreExcep();
            Log.W(comm.serialPort.PortName, "Switch mode back");

            // 发送Sahara切换模式包
            sahara_switch_Mode_packet.Command = 12u;
            sahara_switch_Mode_packet.Length = 12u;
            sahara_switch_Mode_packet.Mode = 0u;
            respBytes = CommandFormat.StructToBytes(sahara_switch_Mode_packet);
            Array.ConstrainedCopy(respBytes, 0, array, 0, 12);
            comm.WritePort(array, 0, array.Length);
        }

        if (sahara_hello_packet.Command != 1)
        {
            throw new Exception("Failed to receive valid hello packet after multiple attempts");
        }

        Log.W(comm.serialPort.PortName, "received hello packet");
        return sahara_hello_packet;
    }

    /// <summary>
    /// 尝试接收Hello包,成功返回true，失败返回false
    /// </summary>
    public bool TryReceiveHelloPacket(out sahara_hello_packet helloPacket)
    {
        // 初始化输出参数
        helloPacket = default;

        // 创建并初始化Sahara切换模式包（为后续可能使用做准备）
        sahara_switch_Mode_packet switchModePacket = default;
        switchModePacket.Command = 19u;
        switchModePacket.Length = 8u;
        byte[] array = new byte[12];

        // 清理并准备接收数据
        comm.GetRecDataIgnoreExcep();

        if (comm.recData == null || comm.recData.Length == 0)
        {
            comm.recData = new byte[48];
        }

        // 尝试解析接收到的Hello包
        try
        {
            helloPacket = (sahara_hello_packet)
                CommandFormat.BytesToStuct(comm.recData, typeof(sahara_hello_packet));
            helloPacket.Reserved = new uint[6];

            // 验证是否为有效的Hello包
            if (helloPacket.Command == 1)
            {
                Log.W(comm.serialPort.PortName, "成功接收hello数据包");

                return true;
            }
        }
        catch (Exception ex)
        {
            Log.W(comm.serialPort.PortName, $"解析Hello包时发生异常: {ex.Message}");
            return false;
        }

        Log.W(comm.serialPort.PortName, "未能接收有效的hello数据包");
        return false;
    }

    /// <summary>
    /// 发送Hello响应包
    /// </summary>
    private void SendHelloResponse()
    {
        sahara_hello_response response = default;
        response.Reserved = new uint[6];
        response.Command = 2u;
        response.Length = 48u;
        response.Version = 2u;
        response.Version_min = 1u;
        byte[] responseBytes = CommandFormat.StructToBytes(response);
        comm.WritePort(responseBytes, 0, responseBytes.Length);
        Log.W(comm.serialPort.PortName, "filePath: MemoryStream");
    }

    /// <summary>
    /// 传输引导文件
    /// </summary>
    private void TransferProgrammer(Stream programmerStream)
    {
        if (programmerStream == null)
        {
            throw new Exception("Cannot find programmer data in MemoryStream.");
        }

        Log.W(comm.serialPort.PortName, "download programmer from MemoryStream");

        // 使用MemoryStream初始化FileTransfer
        FileTransfer fileTransfer = new(comm.serialPort.PortName, programmerStream);

        bool transferComplete;
        sahara_packet packet;

        // 循环处理设备的请求直到传输完成
        do
        {
            transferComplete = false;
            comm.GetRecData();
            packet = (sahara_packet)CommandFormat.BytesToStuct(comm.recData, typeof(sahara_packet));

            switch (packet.Command)
            {
                case 3u: // 标准读取请求
                    ProcessStandardReadRequest(fileTransfer);
                    break;

                case 18u: // 64位读取请求
                    Process64BitReadRequest(fileTransfer);
                    break;

                case 4u: // 传输结束请求
                    ValidateEndTransfer();
                    transferComplete = true;
                    break;

                default:
                    string msg = $"invalid command:{packet.Command}";
                    Log.W(comm.serialPort.PortName, msg);
                    break;
            }
        } while (!transferComplete);

        fileTransfer.CloseTransfer();
    }

    /// <summary>
    /// 处理标准读取请求
    /// </summary>
    private void ProcessStandardReadRequest(FileTransfer fileTransfer)
    {
        sahara_readdata_packet readPacket = (sahara_readdata_packet)
            CommandFormat.BytesToStuct(comm.recData, typeof(sahara_readdata_packet));
        string msg =
            $"sahara read data:imgID {readPacket.Image_id}, offset {readPacket.Offset}, length {readPacket.SLength}";
        fileTransfer.Transfer(comm.serialPort, (int)readPacket.Offset, (int)readPacket.SLength);
        Log.W(comm.serialPort.PortName, msg);
    }

    /// <summary>
    /// 处理64位读取请求
    /// </summary>
    private void Process64BitReadRequest(FileTransfer fileTransfer)
    {
        sahara_64b_readdata_packet readPacket = (sahara_64b_readdata_packet)
            CommandFormat.BytesToStuct(comm.recData, typeof(sahara_64b_readdata_packet));
        string msg =
            $"sahara read 64b data:imgID {readPacket.Image_id}, offset {readPacket.Offset}, length {readPacket.SLength}";
        fileTransfer.Transfer(comm.serialPort, (int)readPacket.Offset, (int)readPacket.SLength);
        Log.W(comm.serialPort.PortName, msg);
    }

    /// <summary>
    /// 验证传输结束请求
    /// </summary>
    private void ValidateEndTransfer()
    {
        sahara_end_transfer_packet endPacket = (sahara_end_transfer_packet)
            CommandFormat.BytesToStuct(comm.recData, typeof(sahara_end_transfer_packet));

        if (endPacket.Status != 0)
        {
            Log.W(
                comm.serialPort.PortName,
                $"sahara read end error with status:{endPacket.Status}"
            );
        }
    }

    /// <summary>
    /// 发送完成包并验证响应
    /// </summary>
    private void SendDoneAndVerifyResponse()
    {
        Log.W(comm.serialPort.PortName, "Send done packet");
        sahara_packet donePacket = new sahara_packet();
        donePacket.Command = 5u;
        donePacket.Length = 8u;
        byte[] packetBytes = CommandFormat.StructToBytes(donePacket, 8);

        // 清零多余的字节
        for (int i = 8; i < packetBytes.Length; i++)
        {
            packetBytes[i] = 0;
        }

        comm.WritePort(packetBytes, 0, packetBytes.Length);
        comm.GetRecData();

        if (comm.recData.Length == 0)
        {
            comm.recData = new byte[48];
        }

        // 解析完成响应
        sahara_done_response doneResponse = (sahara_done_response)
            CommandFormat.BytesToStuct(comm.recData, typeof(sahara_done_response));

        if (doneResponse.Command != 6)
        {
            throw new Exception("programmer transfer error " + doneResponse.Command);
        }

        Log.W(comm.serialPort.PortName, "programmer transferred successfully");
    }

    public string GetBlob()
    {
        //获取响应并提取 <sig> 值
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
            "936E3A8E573CAD07C167644B61217835D85AD4FDDB7D840A2B7225432FCDA13A7C192CFA979ED16517E6970B1B07DF6C516FEC81F6968FCF7FFDDBC397A162C2CA3E5D76124AA1769F1B2164B39B76930B4CC67519F7F339877677F4E8AF25828682BCBF4E593A57E7E30532699253E0B1CC5D9D0D554AF2BD46D56F18D6E5290BA4A0CAC2431F9F19C4C1A39D7664FFAB48A9E11A559386819835B84DF5675E70D25FDB5123E7B040FE21108F0AE6D7D9D267F2C9C61AD054C68493DC4D33F74D0CF2D4AADCD430152DB67C22A181AD6D7761637F70CBDA884CDC11337203837790E6845CA5A8767930B9C26FDA71272564CA34763D352F5FE42AB738FB38A5",
        ];

        // Initially assume all signatures will fail
        bool allFailed = true;

        if (comm.IsOpen)
        {
            foreach (var sig in sigs)
            {
                try
                {
                    var result = SendSignature(sig);

                    if (result)
                    {
                        Log.W(comm.serialPort.PortName, "send sig successful: " + sig);

                        allFailed = false;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Log.W(comm.serialPort.PortName, "send sig failed: " + ex.Message);
                }
            }

            if (allFailed)
            {
                Log.W(comm.serialPort.PortName, "all sign failed: " + null);
            }
        }

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
        return !comm.GetResponse(waiteACK: true)
            ? throw new Exception("authentication failed")
            : flag;
    }

    public string ConfigureDDR()
    {
        try
        {
            Log.W(comm.serialPort.PortName, "发送配置命令");

            string command = string.Format(
                Firehose.Configure,
                verbose ? "1" : "0",
                SectorSize * BUFFER_SECTORS,
                storageType,
                0
            );

            bool flag = comm.SendCommand(command, checkAck: true);

            if (!flag)
            {
                if (comm.LastErrorType == ResponseErrorType.AuthenticationError)
                {
                    Log.W(comm.serialPort.PortName, "需要进行签名认证");
                    return "needsig";
                }
                else
                {
                    // 其他错误情况
                    Log.W(comm.serialPort.PortName, "配置失败：通信错误");
                    return "failed";
                }
            }
            Log.W(comm.serialPort.PortName, "最大缓冲区扇区数: " + comm.m_dwBufferSectors);
            return "success";
        }
        catch (Exception ex)
        {
            // 异常处理保持不变
            Log.W(comm.serialPort.PortName, "配置DDR失败: " + ex.Message);
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

    public bool Readpartitiontable()
    {
        string outputfilepath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "tool",
            "edl",
            "gpt"
        );
        if (!Directory.Exists(outputfilepath))
        {
            Directory.CreateDirectory(outputfilepath);
        }

        if (storageType == "ufs")
        {
            for (int i = 0; i <= 5; i++)
            {
                string outputFile = Path.Combine(outputfilepath, $"gpt_main{i}.bin");
                Read("0", 6, i, outputFile);
            }
        }
        else
        {
            string outputFile = Path.Combine(outputfilepath, $"gpt_main0.bin");
            Read("0", 34, 0, outputFile);
        }
        return true;
    }

    public void Read(
        string start_sector,
        int num_partition_sectors,
        int physical_partition_number,
        string outputfilepath
    )
    {
        Log.W("开始读取");
        ProgressManager.UpdateProgress(0);
        FileTransfer fileTransfer = new(comm.serialPort.PortName, outputfilepath, true);
        fileTransfer.ReadAndWriteToFile(
            this,
            start_sector,
            num_partition_sectors.ToString(),
            "0",
            "0",
            SectorSize.ToString(),
            physical_partition_number.ToString()
        );
        //关闭流
        fileTransfer.CloseTransfer();
        ProgressManager.UpdateProgress(0);
    }

    public void WriteRawFileToDevice(
        string filepath,
        string file_sector_offset,
        string start_sector,
        string num_partition_sectors,
        string physical_partition_number,
        string label
    )
    {
        //< program SECTOR_SIZE_IN_BYTES = "4096" file_sector_offset = "0" filename = "recovery.img" label = "recovery" num_partition_sectors = "16384" partofsingleimage = "false" physical_partition_number = "0" readbackverify = "false" size_in_KB = "65536.0" sparse = "false" start_byte_hex = "0x28000000" start_sector = "163840" />
        Log.W(
            comm.serialPort.PortName,
            "Write file " + filepath + " to partition[" + label + "] sector " + start_sector
        );
        FileTransfer fileTransfer = new(comm.serialPort.PortName, filepath);
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
        Log.W(comm.serialPort.PortName, $"Image {filepath} transferred successfully");

        fileTransfer.CloseTransfer();
        comm.CleanBuffer();
    }

    public void WriteSparseFileToDevice(
        string filepath,
        string file_sector_offset,
        string start_sector,
        string num_partition_sectors,
        string physical_partition_number,
        string label,
        CancellationToken token
    )
    {
        //< program SECTOR_SIZE_IN_BYTES = "4096" file_sector_offset = "0" filename = "recovery.img" label = "recovery" num_partition_sectors = "16384" partofsingleimage = "false" physical_partition_number = "0" readbackverify = "false" size_in_KB = "65536.0" sparse = "false" start_byte_hex = "0x28000000" start_sector = "163840" />

        Log.W(
            comm.serialPort.PortName,
            "Write sparse file " + filepath + " to partition[" + label + "] sector " + start_sector
        );

        FileTransfer fileTransfer = new(comm.serialPort.PortName, filepath);
        fileTransfer.WriteSparseFileToDevice(
            this,
            start_sector,
            num_partition_sectors,
            filepath,
            file_sector_offset,
            SectorSize.ToString(),
            physical_partition_number,
            "", token
        );
        fileTransfer.CloseTransfer();
        Log.W(comm.serialPort.PortName, $"Image {filepath} transferred successfully");
        comm.CleanBuffer();
    }

    public void Erase(string start_sector, int num_partition_sectors, int physical_partition_number)
    {
        string str = string.Format(
            Firehose.FIREHOSE_ERASE,
            SectorSize,
            num_partition_sectors,
            start_sector,
            physical_partition_number
        );
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
                Log.W(
                    comm.serialPort.PortName,
                    $"Failed to apply patch: {patchFilePath}",
                    throwEx: false
                );
                break;
            }
        }
    }

    private bool ApplyPatchesToDevice(string patchFilePath)
    {
        bool result = true;
        Log.W(comm.serialPort.PortName, "open patch file " + patchFilePath);

        XmlDocument xmlDocument = new();
        XmlReader xmlReader = XmlReader.Create(
            patchFilePath,
            new XmlReaderSettings { IgnoreComments = true }
        );
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
                    ApplyPatch(
                        pszDiskOffsetSector,
                        pszSectorOffsetByte,
                        pszPatchValue,
                        pszPatchSize,
                        pszSectorSizeInBytes,
                        pszPhysicalPartitionNumber
                    );
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

    private void ApplyPatch(
        string pszDiskOffsetSector,
        string pszSectorOffsetByte,
        string pszPatchValue,
        string pszPatchSize,
        string pszSectorSizeInBytes,
        string pszPhysicalPartitionNumber
    )
    {
        Log.W(
            comm.serialPort.PortName,
            "ApplyPatch sector "
                + pszDiskOffsetSector
                + ", offset "
                + pszSectorOffsetByte
                + ", value "
                + pszPatchValue
                + ", size "
                + pszPatchSize
        );
        string text = "";
        string command = string.Format(
            Firehose.FIREHOSE_PATCH,
            pszSectorSizeInBytes,
            pszSectorOffsetByte,
            pszPhysicalPartitionNumber,
            pszPatchSize,
            pszDiskOffsetSector,
            pszPatchValue,
            text
        );
        comm.SendCommand(command, checkAck: true);
    }
}