using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;

using WuXingLibrary.code.module;

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
    public ResponseErrorType LastErrorType;

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
        byte[] array =
            ReadDataFromPort()
            ?? throw new Exception("can not read from port " + serialPort.PortName);
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
        LastErrorType = ResponseErrorType.None; // 重置错误类型

        Log.W(serialPort.PortName, "get response from target");

        // 如果不需要等待ACK，直接尝试读取端口数据并返回结果
        if (!waiteACK)
        {
            return ReadDataFromPort() != null;
        }

        // 设置尝试次数
        int maxAttempts = waiteACK ? 3 : 2;

        // 在尝试次数范围内循环，直到成功获取响应或达到最大尝试次数
        for (int attempt = 0; attempt < maxAttempts && !flag; attempt++)
        {
            List<XmlDocument> responseXml = GetResponseXml(waiteACK);

            foreach (XmlDocument item in responseXml)
            {
                // 添加空引用检查
                XmlNode dataNode = item.SelectSingleNode("data");
                if (dataNode == null)
                    continue;

                foreach (XmlNode childNode in dataNode.ChildNodes)
                {
                    // 检查节点是否为错误节点
                    if (childNode.Name.ToLower() == "error")
                    {
                        // 检查节点内容是否包含认证错误信息
                        string nodeText = childNode.InnerText;
                        if (
                            !string.IsNullOrEmpty(nodeText)
                            && nodeText.IndexOf(
                                "only nop and sig tag",
                                StringComparison.OrdinalIgnoreCase
                            ) >= 0
                            && nodeText.IndexOf(
                                "authentication",
                                StringComparison.OrdinalIgnoreCase
                            ) >= 0
                        )
                        {
                            LastErrorType = ResponseErrorType.AuthenticationError;
                            Log.W(serialPort.PortName, "认证错误: 需要先进行认证");
                            return false;
                        }
                    }

                    foreach (XmlAttribute attribute in childNode.Attributes)
                    {
                        string attrValue = attribute.Value;
                        if (
                            !string.IsNullOrEmpty(attrValue)
                            && attrValue.IndexOf(
                                "only nop and sig tag",
                                StringComparison.OrdinalIgnoreCase
                            ) >= 0
                            && attrValue.IndexOf(
                                "authentication",
                                StringComparison.OrdinalIgnoreCase
                            ) >= 0
                        )
                        {
                            LastErrorType = ResponseErrorType.AuthenticationError;
                            Log.W(serialPort.PortName, "认证错误: 需要先进行认证");
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

                        if (
                            attribute.Value
                            == "WARN: NAK: MaxPayloadSizeToTargetInBytes sent by host 1048576 larger than supported 16384"
                        )
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
        if (!flag)
        {
            LastErrorType = ResponseErrorType.OtherError;
            Log.W(serialPort.PortName, "未收到有效响应");
        }

        return flag;
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
                Log.W($"读取设备时发生异常：{ex.Message}");
            }

            Thread.Sleep(delayMilliseconds);
        }

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
            Log.W(
                serialPort.PortName,
                "response:" + stringBuilder2.ToString() + "\r\n\r\n",
                throwEx: false
            );
        }
        return [stringBuilder.ToString(), stringBuilder2.ToString()];
    }
}