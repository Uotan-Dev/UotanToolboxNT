using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

using EDLLibrary.code.data;
using EDLLibrary.code.module;

namespace EDLLibrary.code.Utility;

public class FileTransfer : IDisposable
{
    protected Stream fileStream;

    public string filePath;

    public string portName;

    private long fileLength;

    private long totalBytesRead = 0;
    private readonly bool disposeStream = false;

    public FileTransfer(string port, string filePath)
    {
        portName = port;
        this.filePath = filePath;
        fileStream = OpenFile(filePath, isWriteFile: false);
        disposeStream = true;
    }

    public FileTransfer(string port, string filePath, bool isWriteFile)
    {
        portName = port;
        this.filePath = filePath;
        CreateFile(filePath);
        fileStream = OpenFile(filePath, isWriteFile);
        disposeStream = true;
    }

    private bool CreateFile(string filePath)
    {
        try
        {
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            }
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
            }
            else
            {
                File.Delete(filePath);
                File.Create(filePath).Close();
            }
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    public FileTransfer(string port, Stream stream, string filePath = null)
    {
        portName = port;
        fileStream = stream;
        this.filePath = filePath;
        if (stream is FileStream fs)
        {
            fileLength = fs.Length;
        }
        else if (stream.CanSeek)
        {
            fileLength = stream.Length;
        }
        else
        {
            fileLength = 0;
        }
        disposeStream = false; // 外部管理流的生命周期
    }

    private Stream OpenFile(string filePath, bool isWriteFile)
    {
        this.filePath = filePath;
        if (MemImg.isHighSpeed)
        {
            fileLength = MemImg.MapImg(filePath);
            Log.W(portName, "Image " + filePath + " ,quick transfer");
        }
        try
        {
            FileInfo fileInfo = new(filePath);
            fileLength = fileInfo.Length;
            if (isWriteFile)
            {
                return File.OpenWrite(filePath);
            }
            else
            {
                return File.OpenRead(filePath);
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    public int Transfer(SerialPort port, int offset, int size)
    {
        if (!port.IsOpen)
        {
            Log.W(port.PortName, "端口未打开");
            return 0;
        }
        byte[] bytesFromStream = GetBytesFromStream(offset, size, out int n);
        port.Write(bytesFromStream, 0, n);
        return n;
    }

    /// <summary>
    /// 读取数据
    /// </summary>
    public void ReadAndWriteToFile(
        Flash portCnn,
        string strPartitionStartSector,
        string strPartitionSectorNumber,
        string strFileStartSector,
        string strFileSectorOffset,
        string sector_size,
        string physical_partition_number
    )
    {
        long bytesReadTotal = 0; // 已读取总字节数
        List<byte> initialBuffer = [];
        const int readTimeout = 5000; // 读取超时时间（毫秒）
        DateTime lastReadTime = DateTime.Now;

        try
        {
            long num = Convert.ToInt64(strPartitionSectorNumber);
            if (num == 0L)
            {
                num = 2147483647L;
            }
            long num2 = Convert.ToInt64(strFileStartSector);
            long num3 = Convert.ToInt64(strFileSectorOffset);
            long num4 = Convert.ToInt64(sector_size);
            long num5 = Convert.ToInt64(physical_partition_number);
            long num6 = num2 + num;
            long totalBytesToRead = num * num4;

            // 发送读取命令
            string command = string.Format(
                Firehose.FIREHOSE_READ,
                num4,
                num,
                strPartitionStartSector,
                num5
            );
            portCnn.comm.SendCommand(command);

            Log.W("Waiting for ACK response...");

            // 等待 ACK 响应
            if (!WaitForAckResponse(portCnn, TimeSpan.FromSeconds(5), initialBuffer)) // 超时时间设置为 5 秒
            {
                Log.W("ACK response not received within timeout. Operation aborted.");
                return;
            }

            Log.W("ACK response received. Starting file write operation...");

            // 先将初始缓冲区数据写入文件
            if (initialBuffer.Count > 0)
            {
                int bytesToWrite = (int)
                    Math.Min(initialBuffer.Count, totalBytesToRead - bytesReadTotal);
                fileStream.Write([.. initialBuffer], 0, bytesToWrite);
                bytesReadTotal += bytesToWrite;
                lastReadTime = DateTime.Now; // 更新最后读取时间
            }

            // 循环读取数据到文件
            while (bytesReadTotal < totalBytesToRead)
            {
                int availableBytes = portCnn.comm.serialPort.BytesToRead;

                // 如果没有数据则等待超时检测
                if (availableBytes == 0)
                {
                    if ((DateTime.Now - lastReadTime).TotalMilliseconds > readTimeout)
                    {
                        Log.W("Read operation timed out.");
                        break;
                    }
                    continue;
                }

                // 计算此次可读取的字节数量
                int bytesToRead = (int)Math.Min(availableBytes, totalBytesToRead - bytesReadTotal);

                // 分配读取缓冲
                byte[] buffer = new byte[bytesToRead];

                // 从串口中读取数据
                int bytesRead = portCnn.comm.serialPort.Read(buffer, 0, bytesToRead);

                // 将读取到的数据写入文件
                fileStream.Write(buffer, 0, bytesRead);

                // 更新已读取总量
                bytesReadTotal += bytesRead;
                lastReadTime = DateTime.Now;

                // 计算进度并更新日志
                float progress = (float)Math.Round((double)bytesReadTotal / totalBytesToRead, 4);
                if (bytesReadTotal % 10240000 == 0)
                {
                    Log.W(
                        "read: " + bytesReadTotal + "/" + totalBytesToRead + " bytes read.",
                        null
                    );
                }
                ProgressManager.UpdateProgress(progress, bytesReadTotal, totalBytesToRead);
            }

            if (bytesReadTotal < totalBytesToRead)
            {
                portCnn.comm.CleanBuffer();
                Log.W("Read operation not completed successfully.", null);
                throw new Exception("Error: 接受的数据不足");
            }
            else
            {
                portCnn.comm.CleanBuffer();
                Log.W("File read and written successfully.", null);
            }
        }
        catch (Exception ex)
        {
            portCnn.comm.CleanBuffer();
            Log.W("Error: " + ex.Message, portCnn.comm.serialPort.PortName, null);
            throw new Exception("Error: " + ex.Message);
        }
    }

    /// <summary>
    /// 等待 ACK 响应的辅助方法
    /// </summary>
    public bool WaitForAckResponse(Flash portCnn, TimeSpan timeout, List<byte> initialBuffer)
    {
        DateTime startTime = DateTime.Now;
        List<byte> dataBuffer = [];

        while (DateTime.Now - startTime < timeout)
        {
            int availableBytes = portCnn.comm.serialPort.BytesToRead;
            if (availableBytes == 0)
            {
                continue;
            }

            int bytesToRead = Math.Min(availableBytes, portCnn.comm.serialPort.ReadBufferSize);
            byte[] buffer = new byte[bytesToRead];
            _ = portCnn.comm.serialPort.Read(buffer, 0, bytesToRead);
            dataBuffer.AddRange(buffer);

            string response = Encoding.UTF8.GetString([.. dataBuffer]);
            if (response.Contains("<response value=\"ACK\" rawmode=\"true\" />"))
            {
                Log.W("ACK response received.");

                // 移除ACK响应
                int ackIndex = response.IndexOf("<response value=\"ACK\" rawmode=\"true\" />");
                dataBuffer.RemoveRange(
                    0,
                    ackIndex + "<response value=\"ACK\" rawmode=\"true\" />".Length
                );

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

        Log.W("Timeout reached, ACK not received.");
        return false;
    }

    public void WriteFile(
        Flash portCnn,
        string strPartitionStartSector,
        string strPartitionSectorNumber,
        string pszImageFile,
        string strFileStartSector,
        string strFileSectorOffset,
        string sector_size,
        string physical_partition_number,
        string addtionalFirehose,
        bool chkAck,
        int? chunkCount
    )
    {
        long num = Convert.ToInt64(strPartitionSectorNumber);
        if (num == 0L)
        {
            num = 2147483647L;
        }
        long num2 = Convert.ToInt64(strFileStartSector);
        long num3 = Convert.ToInt64(strFileSectorOffset);
        long num4 = Convert.ToInt64(sector_size);
        long num5 = Convert.ToInt64(physical_partition_number);
        long num6 = (GetFileSize() + num4 - 1) / num4;
        if (num6 - num2 > num)
        {
            num6 = num2 + num;
        }
        else
        {
            num = num6 - num2;
        }
        string command = string.Format(
            Firehose.FIREHOSE_PROGRAM,
            num4,
            num,
            strPartitionStartSector,
            num5,
            addtionalFirehose
        );
        portCnn.comm.SendCommand(command);
        for (long num7 = num2; num7 < num6; num7 += portCnn.comm.m_dwBufferSectors)
        {
            long num8 = num6 - num7;
            num8 = num8 < portCnn.comm.m_dwBufferSectors ? num8 : portCnn.comm.m_dwBufferSectors;
            long offset = num3 + num4 * num7;
            int size = (int)(num4 * num8);
            byte[] bytesFromFile = GetBytesFromFile(offset, size, out _);
            portCnn.comm.WritePort(bytesFromFile, 0, bytesFromFile.Length);
        }
        if (chkAck)
        {
            if (!portCnn.comm.ChkRspAck(out string msg, chunkCount.Value))
            {
                throw new Exception(msg);
            }
        }
    }

    public void WriteFile(
        Flash portCnn,
        string strPartitionStartSector,
        string strPartitionSectorNumber,
        string pszImageFile,
        string strFileStartSector,
        string strFileSectorOffset,
        string sector_size,
        string physical_partition_number,
        string addtionalFirehose,
        bool chkAck,
        int? chunkCount,
        int filldatasize
    )
    {
        long num = Convert.ToInt64(strPartitionSectorNumber);
        if (num == 0L)
        {
            num = 2147483647L;
        }
        long num2 = Convert.ToInt64(strFileStartSector);
        long num3 = Convert.ToInt64(strFileSectorOffset);
        long num4 = Convert.ToInt64(sector_size);
        long num5 = Convert.ToInt64(physical_partition_number);
        long num6 = num2 + num;
        string command = string.Format(
            Firehose.FIREHOSE_PROGRAM,
            num4,
            num,
            strPartitionStartSector,
            num5,
            addtionalFirehose
        );
        portCnn.comm.SendCommand(command);
        long offset = num3 + num4 * num2;
        for (long num7 = num2; num7 < num6; num7 += portCnn.comm.m_dwBufferSectors)
        {
            long num8 = num6 - num7;
            num8 = num8 < portCnn.comm.m_dwBufferSectors ? num8 : portCnn.comm.m_dwBufferSectors;
            int size = (int)(num4 * num8);
            byte[] unitBytesFromFile = GetUnitBytesFromFile(offset, size, filldatasize, out _);
            portCnn.comm.WritePort(unitBytesFromFile, 0, unitBytesFromFile.Length);
        }
        if (chkAck)
        {
            if (!portCnn.comm.ChkRspAck(out string msg, chunkCount.Value))
            {
                throw new Exception(msg);
            }
        }
    }

    public void WriteSparseFileToDevice(
        Flash portCnn,
        string pszPartitionStartSector,
        string pszPartitionSectorNumber,
        string pszImageFile,
        string pszFileStartSector,
        string pszSectorSizeInBytes,
        string pszPhysicalPartitionNumber,
        string addtionalFirehose,
        CancellationToken token
    )
    {
        long num = Convert.ToInt32(pszPartitionStartSector);
        int num2 = Convert.ToInt32(pszPartitionSectorNumber);
        int num3 = Convert.ToInt32(pszFileStartSector);
        long num4 = 0L;
        int num5 = Convert.ToInt32(pszSectorSizeInBytes);
        Convert.ToInt32(pszPhysicalPartitionNumber);
        SparseImageHeader structure = default;
        string text = "";
        if (num3 != 0)
        {
            text = "ERROR_BAD_FORMAT";
            Log.W(portCnn.comm.serialPort.PortName, text);
        }
        if (num5 == 0)
        {
            text = "ERROR_BAD_FORMAT";
            Log.W(portCnn.comm.serialPort.PortName, "ERROR_BAD_FORMAT");
        }
        int size = Marshal.SizeOf(structure);
        structure = (SparseImageHeader)
            CommandFormat.BytesToStuct(
                GetBytesFromFile(num4, size, out _),
                typeof(SparseImageHeader)
            );
        num4 += structure.uFileHeaderSize;
        if (structure.uMagic != 3978755898u)
        {
            text = "ERROR_BAD_FORMAT";
            Log.W(portCnn.comm.serialPort.PortName, "ERROR_BAD_FORMAT " + structure.uMagic);
        }
        if (structure.uMajorVersion != 1)
        {
            text = "ERROR_UNSUPPORTED_TYPE";
            Log.W(
                portCnn.comm.serialPort.PortName,
                "ERROR_UNSUPPORTED_TYPE " + structure.uMajorVersion
            );
        }
        if (structure.uBlockSize % num5 != 0L)
        {
            text = "ERROR_BAD_FORMAT";
            Log.W(portCnn.comm.serialPort.PortName, "ERROR_BAD_FORMAT " + structure.uBlockSize);
        }
        if (num2 != 0 && structure.uBlockSize * structure.uTotalBlocks / num5 > num2)
        {
            text = "ERROR_FILE_TOO_LARGE";
            Log.W(
                portCnn.comm.serialPort.PortName,
                "ERROR_FILE_TOO_LARGE size "
                    + structure.uBlockSize * structure.uTotalBlocks / num5
                    + " ullPartitionSectorNumber "
                    + num2
            );
        }
        if (!string.IsNullOrEmpty(text))
        {
            throw new Exception(text);
        }
        int num6 = 0;
        for (int i = 1; i <= structure.uTotalChunks; i++)
        {
            token.ThrowIfCancellationRequested();
            size = Marshal.SizeOf(default(SparseChunkHeader));
            SparseChunkHeader sparseChunkHeader = (SparseChunkHeader)
                CommandFormat.BytesToStuct(
                    GetBytesFromFile(num4, size, out _, out _),
                    typeof(SparseChunkHeader)
                );
            num4 += structure.uChunkHeaderSize;
            long num7 = structure.uBlockSize * sparseChunkHeader.uChunkSize;
            long num8 = num7 / num5;
            if (sparseChunkHeader.uChunkType == 51905)
            {
                if (sparseChunkHeader.uTotalSize != structure.uChunkHeaderSize + num7)
                {
                    Log.W(portCnn.comm.serialPort.PortName, "ERROR_BAD_FORMAT");
                    Log.W(
                        portCnn.comm.serialPort.PortName,
                        "vboygdi aa ERROR_BAD_FORMAT chunkHeader.uTotalSize: "
                            + sparseChunkHeader.uTotalSize
                            + " imageHeader.uChunkHeaderSize: "
                            + structure.uChunkHeaderSize
                            + " uChunkBytes: "
                            + num7
                    );
                    throw new Exception("ERROR_BAD_FORMAT");
                }
                string strPartitionStartSector = num.ToString();
                string strPartitionSectorNumber = num8.ToString();
                string strFileStartSector = (num4 / num5).ToString();
                string strFileSectorOffset = (num4 % num5).ToString();
                num6++;
                bool chkAck = false;
                int value = 0;
                if (structure.uTotalChunks <= 10)
                {
                    if (i == structure.uTotalChunks)
                    {
                        value = num6;
                        chkAck = true;
                    }
                }
                else
                {
                    if (num6 % 10 == 0)
                    {
                        value = 10;
                        chkAck = true;
                    }
                    if (i == structure.uTotalChunks)
                    {
                        value = num6 % 10;
                        chkAck = true;
                    }
                }
                WriteFile(
                    portCnn,
                    strPartitionStartSector,
                    strPartitionSectorNumber,
                    pszImageFile,
                    strFileStartSector,
                    strFileSectorOffset,
                    pszSectorSizeInBytes,
                    pszPhysicalPartitionNumber,
                    addtionalFirehose,
                    chkAck,
                    value
                );

                num4 += num5 * num8;
                num += num8;
                Log.W(
                    portCnn.comm.serialPort.PortName,
                    "SPARSE_CHUNK_TYPE_RAW: ChunkSectors: " + num8
                );
            }
            else if (sparseChunkHeader.uChunkType == 51907)
            {
                if (sparseChunkHeader.uTotalSize != structure.uChunkHeaderSize)
                {
                    Log.W(portCnn.comm.serialPort.PortName, "ERROR_BAD_FORMAT");
                    Log.W(
                        portCnn.comm.serialPort.PortName,
                        "vboygdi bb ERROR_BAD_FORMAT chunkHeader.uTotalSize: "
                            + sparseChunkHeader.uTotalSize
                            + " imageHeader.uChunkHeaderSize: "
                            + structure.uChunkHeaderSize
                            + " uChunkBytes: "
                            + num7
                    );
                }
                num += num8;
                if (i == structure.uTotalChunks)
                {
                    int num9 = num6 % 10;
                    if (num9 > 0)
                    {
                        if (!portCnn.comm.ChkRspAck(out string msg, num9))
                        {
                            throw new Exception(msg);
                        }
                    }
                }
                Log.W(
                    portCnn.comm.serialPort.PortName,
                    "SPARSE_CHUNK_TYPE_DONTCARE: ChunkSectors: " + num8
                );
            }
            else if (sparseChunkHeader.uChunkType == 51906)
            {
                if (sparseChunkHeader.uTotalSize != structure.uChunkHeaderSize + 4)
                {
                    Log.W(portCnn.comm.serialPort.PortName, "ERROR_BAD_FORMAT");
                    throw new Exception("ERROR_BAD_FORMAT");
                }
                string strPartitionStartSector2 = num.ToString();
                string strPartitionSectorNumber2 = num8.ToString();
                string strFileStartSector2 = (num4 / num5).ToString();
                string strFileSectorOffset2 = (num4 % num5).ToString();
                num6++;
                bool chkAck2 = false;
                int value2 = 0;
                if (structure.uTotalChunks <= 10)
                {
                    if (i == structure.uTotalChunks)
                    {
                        value2 = num6;
                        chkAck2 = true;
                    }
                }
                else
                {
                    if (num6 % 10 == 0)
                    {
                        value2 = 10;
                        chkAck2 = true;
                    }
                    if (i == structure.uTotalChunks)
                    {
                        value2 = num6 % 10;
                        chkAck2 = true;
                    }
                }
                WriteFile(
                    portCnn,
                    strPartitionStartSector2,
                    strPartitionSectorNumber2,
                    pszImageFile,
                    strFileStartSector2,
                    strFileSectorOffset2,
                    pszSectorSizeInBytes,
                    pszPhysicalPartitionNumber,
                    addtionalFirehose,
                    chkAck2,
                    value2,
                    filldatasize: 4
                );
                num4 += 4;
                num += num8;
                Log.W(
                    portCnn.comm.serialPort.PortName,
                    "SPARSE_CHUNK_TYPE_FILL: ChunkSectors: " + num8
                );
            }
            else
            {
                Log.W(
                    portCnn.comm.serialPort.PortName,
                    "ERROR_UNSUPPORTED_TYPE " + sparseChunkHeader.uChunkType
                );
            }
        }
    }

    public byte[] GetBytesFromFile(long offset, int size, out int n)
    {
        byte[] array;
        if (MemImg.isHighSpeed)
        {
            array = MemImg.GetBytesFromFile(filePath, offset, size, out _);
            n = array.Length;
        }
        else
        {
            long length = fileStream.Length;
            array = new byte[size];
            fileStream.Seek(offset, SeekOrigin.Begin);
            n = fileStream.Read(array, 0, size);
            _ = offset / (float)length;
        }

        totalBytesRead += n;

        UpdateProgressAndSpeed(offset + n, fileLength);
        return array;
    }

    public byte[] GetBytesFromStream(long offset, int size, out int n)
    {
        byte[] buffer;
        if (MemImg.isHighSpeed)
        {
            buffer = MemImg.GetBytesFromFile(filePath, offset, size, out _);
            n = buffer.Length;
        }
        else
        {
            if (fileStream == null)
            {
                throw new InvalidOperationException("Stream 未初始化.");
            }

            if (fileStream.CanSeek)
            {
                fileStream.Seek(offset, SeekOrigin.Begin);
            }
            buffer = new byte[size];
            n = fileStream.Read(buffer, 0, size);
        }

        totalBytesRead += n;

        UpdateProgressAndSpeed(offset + n, fileLength);
        return buffer;
    }

    public byte[] GetUnitBytesFromFile(long offset, int size, int Unitsize, out int n)
    {
        int num2 = 0;
        byte[] array;
        if (MemImg.isHighSpeed)
        {
            array = MemImg.GetBytesFromFile(filePath, offset, size, out _);
            n = array.Length;
        }
        else
        {
            long length = fileStream.Length;
            int num = size / Unitsize;
            array = new byte[size];
            byte[] array2 = new byte[Unitsize];
            fileStream.Seek(offset, SeekOrigin.Begin);
            n = fileStream.Read(array2, 0, Unitsize);
            Log.W("vboytest size: " + size + " loopNum: " + num);
            for (int i = 1; i <= num; i++)
            {
                Array.ConstrainedCopy(array2, 0, array, num2, Unitsize);
                num2 += Unitsize;
            }
            _ = offset / (float)length;
        }
        totalBytesRead += n;

        UpdateProgressAndSpeed(offset + n, fileLength);
        return array;
    }

    public byte[] GetBytesFromFile(long offset, int size, out int n, out float percent)
    {
        byte[] array;
        if (MemImg.isHighSpeed)
        {
            array = MemImg.GetBytesFromFile(filePath, offset, size, out percent);
            n = array.Length;
        }
        else
        {
            long length = fileStream.Length;
            array = new byte[size];
            fileStream.Seek(offset, SeekOrigin.Begin);
            n = fileStream.Read(array, 0, size);
            percent = offset / (float)length;
        }

        // 更新已读取的总字节数
        totalBytesRead += n;

        // 更新进度
        UpdateProgressAndSpeed(offset + n, fileLength);
        return array;
    }

    /// <summary>
    /// 更新进度与速度的方法
    /// </summary>
    private void UpdateProgressAndSpeed(long currentOffset, long totalLength)
    {
        float progress = (float)Math.Round(Math.Min(currentOffset / (float)totalLength, 1.0f), 4);
        double downloaded = totalBytesRead;

        ProgressManager.UpdateProgress(progress, (long?)downloaded, totalLength);
    }

    public long GetFileSize()
    {
        if (fileLength != 0L)
        {
            return fileLength;
        }
        return fileStream.Length;
    }

    public void CloseTransfer()
    {
        Dispose();
    }

    public void Dispose()
    {
        if (fileStream != null && disposeStream)
        {
            fileStream.Close();
            fileStream.Dispose();
        }
        fileStream = null;
    }

    ~FileTransfer()
    {
        Dispose();
    }
}