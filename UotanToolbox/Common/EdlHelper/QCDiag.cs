using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UotanToolbox.Common.EdlHelper
{
    internal class QCDiag
    {
        public class NVItem
        {
            public ushort Item { get; set; }
            public ushort Index { get; set; }
            public byte[] Data { get; set; }
            public ushort Status { get; set; }
            public string Name { get; set; }
            public NVItem(ushort item, ushort index, byte[] data, ushort status, string name)
            {
                Item = item;
                Index = index;
                Data = data;
                Status = status;
                Name = name;
            }
        }
        Dictionary<int, string> qcError = new Dictionary<int, string>
            {
                { 1, "None" },
                { 2, "Unknown" },
                { 3, "Open Port Fail" },
                { 4, "Port not open" },
                { 5, "Buffer too small" },
                { 6, "Read data fail" },
                { 7, "Open file fail" },
                { 8, "File not open" },
                { 9, "Invalid parameter" },
                { 10, "Send write ram failed" },
                { 11, "Send command failed" },
                { 12, "Offline phone failed" },
                { 13, "Erase rom failed" },
                { 14, "Timeout" },
                { 15, "Go cmd failed" },
                { 16, "Set baudrate failed" },
                { 17, "Say hello failed" },
                { 18, "Write port failed" },
                { 19, "Failed to read nv" },
                { 20, "Failed to write nv" },
                { 21, "Last failed but not recovery" },
                { 22, "Backup file wasn't found" },
                { 23, "Incorrect SPC Code" },
                { 24, "Hello pkt isn't needed" },
                { 25, "Not active" }
            };
        Dictionary<int, string> diagError = new Dictionary<int, string>
            {
                { 20, "Generic error" },
                { 21, "Bad argument" },
                { 22, "Data too large" },
                { 24, "Not connected" },
                { 25, "Send pkt failed" },
                { 26, "Receive pkt failed" },
                { 27, "Extract pkt failed" },
                { 29, "Open port failed" },
                { 30, "Bad command" },
                { 31, "Protected" },
                { 32, "No media" },
                { 33, "Empty" },
                { 34, "List done" }
            };
        public struct NVItem_type
        {
            public ushort Item;
            public byte[] RawData = new byte[128];
            public ushort Status;
            public NVItem_type(ushort item, byte[] rawData, ushort status)
            {
                Item = item;
                Status = status;
            }
            public NVItem_type()
            {
                Item = 0;
                RawData = new byte[128];
                Status = 0;
            }
        }
        public struct SubNVItem_type
        {
            public ushort Item;
            public ushort Index;
            public byte[] RawData = new byte[128];
            public ushort Status;
            public SubNVItem_type(ushort item, ushort index, byte[] rawData, ushort status)
            {
                Item = item;
                Index = index;
                Status = status;
            }
            public SubNVItem_type()
            {
                Item = 0;
                Index = 0;
                RawData = new byte[128];
                Status = 0;
            }
        }
        public enum EFSCmds
        {
            EFS2_DIAG_HELLO = 0,  // Parameter negotiation packet
            EFS2_DIAG_QUERY = 1,  // Send information about EFS2 params
            EFS2_DIAG_OPEN = 2,   // Open a file
            EFS2_DIAG_CLOSE = 3,  // Close a file
            EFS2_DIAG_READ = 4,   // Read a file
            EFS2_DIAG_WRITE = 5,  // Write a file
            EFS2_DIAG_SYMLINK = 6,  // Create a symbolic link
            EFS2_DIAG_READLINK = 7,  // Read a symbolic link
            EFS2_DIAG_UNLINK = 8,  // Remove a symbolic link or file
            EFS2_DIAG_MKDIR = 9,   // Create a directory
            EFS2_DIAG_RMDIR = 10,  // Remove a directory
            EFS2_DIAG_OPENDIR = 11,  // Open a directory for reading
            EFS2_DIAG_READDIR = 12,  // Read a directory
            EFS2_DIAG_CLOSEDIR = 13,  // Close an open directory
            EFS2_DIAG_RENAME = 14,  // Rename a file or directory
            EFS2_DIAG_STAT = 15,  // Obtain information about a named file
            EFS2_DIAG_LSTAT = 16,  // Obtain information about a symbolic link
            EFS2_DIAG_FSTAT = 17,  // Obtain information about a file descriptor
            EFS2_DIAG_CHMOD = 18,  // Change file permissions
            EFS2_DIAG_STATFS = 19,  // Obtain file system information
            EFS2_DIAG_ACCESS = 20,  // Check a named file for accessibility
            EFS2_DIAG_NAND_DEV_INFO = 21,  // Get NAND device info
            EFS2_DIAG_FACT_IMAGE_START = 22,  // Start data output for Factory Image
            EFS2_DIAG_FACT_IMAGE_READ = 23,  // Get data for Factory Image
            EFS2_DIAG_FACT_IMAGE_END = 24,  // End data output for Factory Image
            EFS2_DIAG_PREP_FACT_IMAGE = 25,  // Prepare file system for image dump
            EFS2_DIAG_PUT_DEPRECATED = 26,  // Write an EFS item file
            EFS2_DIAG_GET_DEPRECATED = 27,  // Read an EFS item file
            EFS2_DIAG_ERROR = 28,  // Send an EFS Error Packet back through DIAG
            EFS2_DIAG_EXTENDED_INFO = 29,  // Get Extra information.
            EFS2_DIAG_CHOWN = 30,  // Change ownership
            EFS2_DIAG_BENCHMARK_START_TEST = 31,  // Start Benchmark
            EFS2_DIAG_BENCHMARK_GET_RESULTS = 32,  // Get Benchmark Report
            EFS2_DIAG_BENCHMARK_INIT = 33,  // Init/Reset Benchmark
            EFS2_DIAG_SET_RESERVATION = 34,  // Set group reservation
            EFS2_DIAG_SET_QUOTA = 35,  // Set group quota
            EFS2_DIAG_GET_GROUP_INFO = 36,  // Retrieve Q&R values
            EFS2_DIAG_DELTREE = 37,  // Delete a Directory Tree
            EFS2_DIAG_PUT = 38,  // Write a EFS item file in order
            EFS2_DIAG_GET = 39,  // Read a EFS item file in order
            EFS2_DIAG_TRUNCATE = 40,  // Truncate a file by the name
            EFS2_DIAG_FTRUNCATE = 41,  // Truncate a file by a descriptor
            EFS2_DIAG_STATVFS_V2 = 42  // Obtains extensive file system info
        }
        [Flags]
        public enum FileOpenMode
        {
            O_RDONLY = 0,
            O_WRONLY = 1,
            O_RDWR = 2
        }
        public const FileOpenMode O_ACCMODE = FileOpenMode.O_RDONLY | FileOpenMode.O_WRONLY | FileOpenMode.O_RDWR;

        public const int FS_DIAG_MAX_READ_REQ = 1024;
    }
}
