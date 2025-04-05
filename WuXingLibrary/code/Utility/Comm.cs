using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;

namespace WuXingLibrary.code.Utility;

public class Comm
{
    public int writeCount;

    public bool isReadDump = true;

    public bool isWriteDump;

    public bool ignoreResponse = true;

    public SerialPort serialPort;

    private readonly bool _keepReading;



    public byte[] recData;
    public int MAX_SECTOR_STR_LEN = 20;

    public int SECTOR_SIZE_UFS = 4096;

    public int SECTOR_SIZE_EMMC = 512;

    public int m_dwBufferSectors;

    public int intSectorSize;


    public string edlAuthErr = "error: only nop and sig tag can be recevied before authentication";

    public bool needEdlAuth;

    public bool isSupportPartialReset;

    public bool isDdr4;

    public bool isDdr5;

    public string storageInfo = "";

    public bool isGetChipNum;

    public string chipNum = "";

    public bool IsOpen
    {
        get
        {
            int num = 5;
            while (num-- > 0 && !serialPort.IsOpen)
            {
                Log.W(serialPort.PortName, "wait for port open.");
                Thread.Sleep(50);
            }
            return serialPort.IsOpen;
        }
    }

    public Comm()
    {
        _keepReading = false;
    }



    public byte[] ReadPortData()
    {
        byte[] result = null;
        if (serialPort.IsOpen)
        {
            int bytesToRead = serialPort.BytesToRead;
            if (bytesToRead > 0)
            {
                result = new byte[bytesToRead];
                try
                {
                    serialPort.Read(result, 0, bytesToRead);
                    return result;
                }
                catch (TimeoutException ex)
                {
                    Log.W(serialPort.PortName, ex, stopFlash: false);
                    return result;
                }
            }
        }
        return result;
    }

    public byte[] ReadPortData(int offset, int count)
    {
        byte[] array = new byte[count];
        try
        {
            serialPort.Read(array, offset, count);
            return array;
        }
        catch (TimeoutException ex)
        {
            Log.W(serialPort.PortName, ex, stopFlash: false);
            return array;
        }
    }

    public void Open()
    {
        Close();
        serialPort.Open();
        if (!serialPort.IsOpen)
        {
            string text = "open serial port failed!";
            Log.W(serialPort.PortName, text);
        }
    }


    public void Close()
    {
        serialPort.Close();
    }

    public void CleanBuffer()
    {
        serialPort.DiscardOutBuffer();
        serialPort.DiscardInBuffer();
    }

    public void WritePort(byte[] send, int offSet, int count)
    {
        if (IsOpen)
        {
            _ = _keepReading;
            int num = 0;
            Exception ex = new TimeoutException();
            bool flag = false;
            while (num++ <= 6 && ex != null && ex.GetType() == typeof(TimeoutException))
            {
                try
                {
                    serialPort.WriteTimeout = 2000;
                    serialPort.Write(send, offSet, count);
                    flag = true;
                    if (isWriteDump)
                    {
                        Log.W(serialPort.PortName, "write to port:");
                        Dump(send);
                    }
                    ex = null;
                }
                catch (TimeoutException ex2)
                {
                    ex = ex2;
                    Log.W(serialPort.PortName, "write time out try agian " + num);
                    Thread.Sleep(500);
                }
                catch (Exception ex3)
                {
                    Log.W(serialPort.PortName, "write failed:" + ex3.Message);
                }
            }
            if (!flag)
            {
                Log.W(serialPort.PortName, ex, stopFlash: true);
                throw new Exception("write time out,maybe device was disconnected.");
            }
        }
        writeCount++;
    }

    public bool SendCommand(string command)
    {
        return SendCommand(command, checkAck: false);
    }

    public bool SendCommand(string command, bool checkAck)
    {
        byte[] bytes = Encoding.Default.GetBytes(command);
        Log.W(serialPort.PortName, "send command:" + command);
        WritePort(bytes, 0, bytes.Length);
        if (checkAck)
        {
            return GetResponse(checkAck);
        }
        return false;
    }

    private int SubstringCount(string str, string substring)
    {
        if (str.Contains(substring))
        {
            string text = str.Replace(substring, "");
            return (str.Length - text.Length) / substring.Length;
        }
        return 0;
    }

    public bool ChkRspAck(out string msg)
    {
        msg = null;
        byte[] binary = ReadDataFromPort();
        string[] array = Dump(binary, waitACK: true);
        string value = "<response value=\"ACK\"";
        int num = 10;
        while ((array.Length != 2 || array[1].IndexOf(value) < 0) && num-- >= 0)
        {
            Thread.Sleep(10);
            binary = ReadDataFromPort();
            array = Dump(binary, waitACK: true);
        }
        if (array.Length == 2 && array[1].IndexOf(value) >= 0)
        {
            CleanBuffer();
            return true;
        }
        msg = "did not detect ACK from target.";
        return false;
    }

    public bool ChkRspAck(out string msg, int chunkCount)
    {
        msg = null;
        byte[] binary = ReadDataFromPort();
        string[] array = Dump(binary, waitACK: true);
        string text = "<response value=\"ACK\"";
        int num = 10;
        while ((array.Length != 2 || array[1].IndexOf(text) < 0) && num-- >= 0)
        {
            Thread.Sleep(10);
            binary = ReadDataFromPort();
            array = Dump(binary, waitACK: true);
        }
        if (array.Length == 2 && array[1].IndexOf(text) >= 0)
        {
            int i = SubstringCount(array[1], text);
            num = 10;
            for (; i < chunkCount * 2; i += SubstringCount(array[1], text))
            {
                if (num-- <= 0)
                {
                    break;
                }
                Thread.Sleep(10);
                binary = ReadDataFromPort();
                array = Dump(binary, waitACK: true);
            }
            if (chunkCount * 2 > i)
            {
                Log.W(serialPort.PortName, "ACK count don't match!");
                throw new Exception("ACK count don't match!");
            }
            Log.W(serialPort.PortName, $"{chunkCount} chunks match {i} ack");
            CleanBuffer();
            writeCount = 0;
            return true;
        }
        msg = array[1];
        return false;
    }

    public byte[] GetRecDataIgnoreExcep()
    {
        byte[] array = ReadDataFromPort();
        if (array != null && array.Length != 0 && isReadDump)
        {
            Log.W(serialPort.PortName, "read from port:");
            Dump(array);
        }
        return array;
    }

    public byte[] GetRecData()
    {
        byte[] array = ReadDataFromPort() ?? throw new Exception("can not read from port " + serialPort.PortName);
        if (array.Length != 0 && isReadDump)
        {
            Log.W(serialPort.PortName, "read from port:");
            Dump(array);
        }
        return array;
    }

    private byte[] ReadDataFromPort()
    {
        int num = 10;
        recData = null;
        recData = ReadPortData();
        while (num-- >= 0 && recData == null)
        {
            Thread.Sleep(50);
            recData = ReadPortData();
        }
        return recData;
    }

    //private bool WaitForAck()
    //{
    //    bool flag = false;
    //    int num = 10;
    //    while (num-- > 0 && !flag)
    //    {
    //        byte[] binary = ReadDataFromPort();
    //        string[] array = Dump(binary);
    //        flag = array.Length == 2 && array[1].IndexOf("<response value=\"ACK\" />") >= 0;
    //        Thread.Sleep(50);
    //    }
    //    return flag;
    //}

    public bool GetResponse(bool waiteACK)
    {
        bool flag = false;
        Log.W(serialPort.PortName, "get response from target");
        if (!waiteACK)
        {
            return ReadDataFromPort() != null;
        }
        int num = 2;
        if (waiteACK)
        {
            num = 3;
        }
        while (num-- > 0 && !flag)
        {
            List<XmlDocument> responseXml = GetResponseXml(waiteACK);
            _ = responseXml.Count;
            foreach (XmlDocument item in responseXml)
            {
                foreach (XmlNode childNode in item.SelectSingleNode("data").ChildNodes)
                {

                    foreach (XmlAttribute attribute in childNode.Attributes)
                    {
                        if (childNode.Name.ToLower() == "ERROR: Only nop and sig tag can be recevied before authentication.")
                        {
                            return false;
                        }
                        if (attribute.Name.ToLower() == "maxpayloadsizetotargetinbytes")
                        {
                            m_dwBufferSectors = Convert.ToInt32(attribute.Value) / intSectorSize;
                            flag = true;

                        }
                        if (attribute.Value.ToLower() == "ack")
                        {
                            flag = true;
                        }
                        if (attribute.Value == "WARN: NAK: MaxPayloadSizeToTargetInBytes sent by host 1048576 larger than supported 16384")
                        {
                            flag = true;
                        }
                        if (attribute.Value.Contains("UFS Inquiry Command Output"))
                        {
                            Log.W(serialPort.PortName, "StorageInfo: " + attribute.Value);
                            storageInfo = attribute.Value;
                        }
                        if (attribute.Value.Contains("INFO: quick_reset") && !isSupportPartialReset)
                        {
                            Log.W(serialPort.PortName, "quick_reset: " + attribute.Value);
                            isSupportPartialReset = true;
                        }
                    }
                }
            }
            if (waiteACK)
            {
                Thread.Sleep(50);
            }
        }
        return flag;
    }


    /// <summary>
    /// 读取数据并写入文件方法，可用于较大数据的读取与持久化
    /// </summary>
    public void ReadAndWriteToFile(string outputFilePath, long totalBytesToRead)
    {
        long bytesReadTotal = 0; // 已读取总字节数
        List<byte> initialBuffer = [];
        const int readTimeout = 5000; // 读取超时时间（毫秒）
        DateTime lastReadTime = DateTime.Now;

        try
        {
            string directoryPath = Path.GetDirectoryName(outputFilePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);  // 如果目录不存在则创建
            }

            Console.WriteLine("Waiting for ACK response...");

            // 等待 ACK 响应
            if (!WaitForAckResponse(TimeSpan.FromSeconds(5), initialBuffer)) // 超时时间设置为 5 秒
            {
                Console.WriteLine("ACK response not received within timeout. Operation aborted.");
                return;
            }

            Console.WriteLine("ACK response received. Starting file write operation...");

            using (FileStream fileStream = new(outputFilePath, FileMode.Create, FileAccess.Write))
            {
                // 先将初始缓冲区数据写入文件
                if (initialBuffer.Count > 0)
                {
                    int bytesToWrite = (int)Math.Min(initialBuffer.Count, totalBytesToRead - bytesReadTotal);
                    fileStream.Write([.. initialBuffer], 0, bytesToWrite);
                    bytesReadTotal += bytesToWrite;
                    lastReadTime = DateTime.Now; // 更新最后读取时间
                }

                // 循环读取数据到文件
                while (bytesReadTotal < totalBytesToRead)
                {
                    int availableBytes = serialPort.BytesToRead;

                    // 如果没有数据则等待超时检测
                    if (availableBytes == 0)
                    {
                        if ((DateTime.Now - lastReadTime).TotalMilliseconds > readTimeout)
                        {
                            Console.WriteLine("Read operation timed out.");
                            break;
                        }
                        continue;
                    }

                    // 计算此次可读取的字节数量（避免超过预期总量）
                    int bytesToRead = (int)Math.Min(availableBytes, totalBytesToRead - bytesReadTotal);

                    // 分配读取缓冲
                    byte[] buffer = new byte[bytesToRead];

                    // 从串口中读取数据
                    int bytesRead = serialPort.Read(buffer, 0, bytesToRead);

                    // 将读取到的数据写入文件
                    fileStream.Write(buffer, 0, bytesRead);

                    // 更新已读取总量
                    bytesReadTotal += bytesRead;
                    lastReadTime = DateTime.Now;

                    // 计算进度并更新日志
                    float progress = (float)Math.Round((double)bytesReadTotal / totalBytesToRead, 4);
                    if (bytesReadTotal % 1024000 == 0) // 每读取 1024 字节记录一次日志
                    {
                        Log.W("read: " + bytesReadTotal + "/" + totalBytesToRead + " bytes read.", serialPort.PortName, null);
                    }
                    Flash.UpdateDeviceStatus(progress, bytesReadTotal, totalBytesToRead, "reading");
                }
            }

            // 如果未读取到足够的字节数，提示可能未全部完成
            if (bytesReadTotal < totalBytesToRead)
            {
                CleanBuffer();
                //MessageBox.Show("Operation completed but not all data was read from the port.");
            }
            else
            {
                CleanBuffer();
                Log.W("File read and written successfully.", serialPort.PortName, null);
                Console.WriteLine("File read and written successfully.");
            }
        }
        catch (Exception ex)
        {
            CleanBuffer();
            Log.W("Error: " + ex.Message, serialPort.PortName, null);
            //MessageBox.Show($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// 等待 ACK 响应的辅助方法
    /// </summary>
    private bool WaitForAckResponse(TimeSpan timeout, List<byte> initialBuffer)
    {
        DateTime startTime = DateTime.Now;
        List<byte> dataBuffer = [];

        while (DateTime.Now - startTime < timeout)
        {
            int availableBytes = serialPort.BytesToRead;
            if (availableBytes == 0)
            {
                continue;
            }

            int bytesToRead = Math.Min(availableBytes, serialPort.ReadBufferSize);
            byte[] buffer = new byte[bytesToRead];
            _ = serialPort.Read(buffer, 0, bytesToRead);
            dataBuffer.AddRange(buffer);

            string response = Encoding.UTF8.GetString([.. dataBuffer]);
            if (response.Contains("<response value=\"ACK\" rawmode=\"true\" />"))
            {
                Console.WriteLine("ACK response received.");

                // 移除ACK响应
                int ackIndex = response.IndexOf("<response value=\"ACK\" rawmode=\"true\" />");
                dataBuffer.RemoveRange(0, ackIndex + "<response value=\"ACK\" rawmode=\"true\" />".Length);

                // 移除XML结束标签
                string remaining = Encoding.UTF8.GetString([.. dataBuffer]);
                int endIndex = remaining.IndexOf("</data>");
                if (endIndex >= 0)
                {
                    dataBuffer.RemoveRange(0, endIndex + "</data>".Length);
                }

                initialBuffer.AddRange(dataBuffer);
                return true;
            }
        }

        Console.WriteLine("Timeout reached, ACK not received.");
        return false;
    }

    /// <summary>
    /// 发送 XML 字符串
    /// </summary>
    public string SendXml(string xml)
    {
        CleanBuffer();

        byte[] bytes = Encoding.UTF8.GetBytes(xml.Trim());
        serialPort.Write(bytes, 0, bytes.Length);
        return ReadFromDevice();
    }

    /// <summary>
    /// 从设备读取字符串
    /// </summary>
    public string ReadFromDevice()
    {
        int maxAttempts = 100;
        int delayMilliseconds = 100;
        byte[] recData = null;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            try
            {
                int bytesToRead = serialPort.BytesToRead;
                if (bytesToRead > 0)
                {
                    recData = new byte[bytesToRead];
                    serialPort.Read(recData, 0, bytesToRead);
                    break;
                }
            }
            catch (Exception ex)
            {
                // 处理读取过程中的异常
                Console.WriteLine($"读取设备时发生异常：{ex.Message}");
            }

            // 如果读取为空，等待一段时间后重试
            Thread.Sleep(delayMilliseconds);
        }

        // 如果仍然为空，返回空字符串
        if (recData == null)
        {
            return string.Empty;
        }
        CleanBuffer();
        return Encoding.Default.GetString(recData);
    }


    public List<XmlDocument> GetResponseXml(bool waiteACK)
    {
        List<XmlDocument> list = [];
        byte[] binary = ReadDataFromPort();
        string[] array = Dump(binary, waiteACK);
        if (array.Length >= 2)
        {
            foreach (string item in Regex.Split(array[1], "\\<\\?xml").ToList())
            {
                if (!string.IsNullOrEmpty(item))
                {
                    if (item.ToLower().IndexOf(edlAuthErr) >= 0)
                    {
                        needEdlAuth = true;
                    }
                    XmlDocument xmlDocument = new();
                    xmlDocument.LoadXml("<?xml " + item);
                    list.Add(xmlDocument);
                }
            }
            return list;
        }
        return list;
    }

    //private string GetResponseXmlStr()
    //{
    //    byte[] binary = ReadDataFromPort();
    //    return Dump(binary)[1];
    //}

    private string[] Dump(byte[] binary)
    {
        return Dump(binary, waitACK: false);
    }

    private string[] Dump(byte[] binary, bool waitACK)
    {
        if (binary == null)
        {
            Log.W(serialPort.PortName, "no Binary dump");
            return ["", ""];
        }
        StringBuilder stringBuilder = new();
        StringBuilder stringBuilder2 = new();
        new StringBuilder();
        new StringBuilder();
        for (int i = 0; i < binary.Length; i++)
        {
            stringBuilder2.Append(Convert.ToChar(binary[i]).ToString());
        }
        if (waitACK)
        {
            Log.W(serialPort.PortName, "response:" + stringBuilder2.ToString() + "\r\n\r\n", throwEx: false);
        }
        return
        [
            stringBuilder.ToString(),
            stringBuilder2.ToString()
        ];
    }
}
