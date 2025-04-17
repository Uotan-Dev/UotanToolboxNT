using System.IO.MemoryMappedFiles;

namespace DeviceTreeNode.Core
{
    /// <summary>
    /// 内存映射文件，支持读写操作
    /// </summary>
    public class MemoryMappedFile : IDisposable
    {
        private System.IO.MemoryMappedFiles.MemoryMappedFile _mmf;
        private MemoryMappedViewAccessor _accessor;
        private readonly FileStream _fileStream;
        private readonly long _length;
        private readonly bool _isReadOnly;

        /// <summary>
        /// 打开只读内存映射文件
        /// </summary>
        public static MemoryMappedFile OpenRead(string path)
        {
            return new MemoryMappedFile(path, false);
        }

        /// <summary>
        /// 打开读写内存映射文件
        /// </summary>
        public static MemoryMappedFile OpenReadWrite(string path)
        {
            return new MemoryMappedFile(path, true);
        }

        private MemoryMappedFile(string path, bool readWrite)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            _isReadOnly = !readWrite;

            try
            {
                // 打开文件流
                FileMode mode = FileMode.Open;
                FileAccess access = readWrite ? FileAccess.ReadWrite : FileAccess.Read;
                _fileStream = new FileStream(path, mode, access, FileShare.Read);
                _length = _fileStream.Length;

                // 创建内存映射
                _mmf = System.IO.MemoryMappedFiles.MemoryMappedFile.CreateFromFile(
                    _fileStream,             // 文件流
                    null,                    // 映射名称
                    0,                       // 容量（0表示使用文件大小）
                    readWrite ?              // 访问模式
                        MemoryMappedFileAccess.ReadWrite :
                        MemoryMappedFileAccess.Read,
                    HandleInheritability.None, // 继承选项 
                    false                    // 不要在关闭映射时保持文件流打开
                );

                // 创建视图访问器
                _accessor = _mmf.CreateViewAccessor(0, _length, _isReadOnly ? MemoryMappedFileAccess.Read : MemoryMappedFileAccess.ReadWrite);
            }
            catch (Exception ex)
            {
                // 确保在失败时释放资源
                _fileStream?.Dispose();
                _mmf?.Dispose();
                _accessor?.Dispose();

                throw new IOException($"Failed to open memory mapped file: {path}", ex);
            }
        }

        /// <summary>
        /// 获取文件长度
        /// </summary>
        public long Length => _length;

        /// <summary>
        /// 读取指定位置的字节
        /// </summary>
        public byte ReadByte(long position)
        {
            if (position < 0 || position >= _length)
                throw new ArgumentOutOfRangeException(nameof(position));

            return _accessor.ReadByte(position);
        }

        /// <summary>
        /// 写入指定位置的字节
        /// </summary>
        public void WriteByte(long position, byte value)
        {
            if (_isReadOnly)
                throw new InvalidOperationException("Cannot write to read-only memory mapped file");

            if (position < 0 || position >= _length)
                throw new ArgumentOutOfRangeException(nameof(position));

            _accessor.Write(position, value);
        }

        /// <summary>
        /// 读取字节数组
        /// </summary>
        public byte[] ReadBytes(long position, int count)
        {
            if (position < 0 || position >= _length)
                throw new ArgumentOutOfRangeException(nameof(position));

            count = (int)Math.Min(count, _length - position);
            byte[] buffer = new byte[count];
            _accessor.ReadArray(position, buffer, 0, count);
            return buffer;
        }

        /// <summary>
        /// 写入字节数组
        /// </summary>
        public void WriteBytes(long position, byte[] buffer)
        {
            if (_isReadOnly)
                throw new InvalidOperationException("Cannot write to read-only memory mapped file");

            if (position < 0 || position >= _length)
                throw new ArgumentOutOfRangeException(nameof(position));

            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            int count = (int)Math.Min(buffer.Length, _length - position);
            _accessor.WriteArray(position, buffer, 0, count);
        }

        /// <summary>
        /// 获取整个文件的字节数组
        /// </summary>
        public byte[] ToArray()
        {
            return ReadBytes(0, (int)_length);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            _accessor?.Dispose();
            _mmf?.Dispose();
            _fileStream?.Dispose();
        }
    }
}
