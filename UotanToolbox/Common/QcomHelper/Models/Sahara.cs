using System.Runtime.InteropServices;

namespace UotanToolbox.Common.QcomHelper.Models
{
    class Sahara
    {
        // Sahara 协议版本常量
        public static class SaharaConstants
        {
            public const int SAHARA_VERSION = 2;
            public const int SAHARA_VERSION_SUPPORTED = 5;
            public const int MAX_SAHARA_MAPPINGS = 64;
            public const int MAX_IMAGE_ID_STRING_LENGTH = 8;
            public const int MAX_SAHARA_COMMAND = 7;  // SAHARA_EXEC_CMD_LAST
            public const int COMMAND_ID_8 = 8;
            public const int DLOAD_DEBUG_STRLEN_BYTES = 20;
        }

        // Sahara 命令 ID 枚举
        public enum SaharaCommandId : uint
        {
            SAHARA_NO_CMD_ID = 0x00,
            SAHARA_HELLO_ID = 0x01,               // 从设备发送到主机
            SAHARA_HELLO_RESP_ID = 0x02,          // 从主机发送到设备
            SAHARA_READ_DATA_ID = 0x03,           // 从设备发送到主机
            SAHARA_END_IMAGE_TX_ID = 0x04,        // 从设备发送到主机
            SAHARA_DONE_ID = 0x05,                // 从主机发送到设备
            SAHARA_DONE_RESP_ID = 0x06,           // 从设备发送到主机
            SAHARA_RESET_ID = 0x07,               // 从主机发送到设备
            SAHARA_RESET_RESP_ID = 0x08,          // 从设备发送到主机
            SAHARA_MEMORY_DEBUG_ID = 0x09,        // 从设备发送到主机
            SAHARA_MEMORY_READ_ID = 0x0A,         // 从主机发送到设备
            SAHARA_CMD_READY_ID = 0x0B,           // 从设备发送到主机
            SAHARA_CMD_SWITCH_MODE_ID = 0x0C,     // 从主机发送到设备
            SAHARA_CMD_EXEC_ID = 0x0D,            // 从主机发送到设备
            SAHARA_CMD_EXEC_RESP_ID = 0x0E,       // 从设备发送到主机
            SAHARA_CMD_EXEC_DATA_ID = 0x0F,       // 从主机发送到设备
            SAHARA_64_BITS_MEMORY_DEBUG_ID = 0x10,// 从设备发送到主机
            SAHARA_64_BITS_MEMORY_READ_ID = 0x11, // 从主机发送到设备
            SAHARA_64_BITS_READ_DATA_ID = 0x12,
            SAHARA_RESET_STATE_MACHINE_ID = 0x13,
            SAHARA_LAST_CMD_ID,
            SAHARA_MAX_CMD_ID = 0x7FFFFFFF   
        }

        // Sahara 镜像类型枚举
        public enum SaharaImageType : uint
        {
            SAHARA_IMAGE_TYPE_BINARY = 0,         // 二进制格式
            SAHARA_IMAGE_TYPE_ELF,                // ELF 格式
            SAHARA_IMAGE_UNKNOWN = 0x7FFFFFFF
        }

        // Sahara 状态码枚举
        public enum SaharaStatus : uint
        {
            SAHARA_STATUS_SUCCESS = 0x00,
            SAHARA_NAK_INVALID_CMD = 0x01,
            SAHARA_NAK_PROTOCOL_MISMATCH = 0x02,
            SAHARA_NAK_INVALID_TARGET_PROTOCOL = 0x03,
            SAHARA_NAK_INVALID_HOST_PROTOCOL = 0x04,
            SAHARA_NAK_INVALID_PACKET_SIZE = 0x05,
            SAHARA_NAK_UNEXPECTED_IMAGE_ID = 0x06,
            SAHARA_NAK_INVALID_HEADER_SIZE = 0x07,
            SAHARA_NAK_INVALID_DATA_SIZE = 0x08,
            SAHARA_NAK_INVALID_IMAGE_TYPE = 0x09,
            SAHARA_NAK_INVALID_TX_LENGTH = 0x0A,
            SAHARA_NAK_INVALID_RX_LENGTH = 0x0B,
            SAHARA_NAK_GENERAL_TX_RX_ERROR = 0x0C,
            SAHARA_NAK_READ_DATA_ERROR = 0x0D,
            SAHARA_NAK_UNSUPPORTED_NUM_PHDRS = 0x0E,
            SAHARA_NAK_INVALID_PDHR_SIZE = 0x0F,
            SAHARA_NAK_MULTIPLE_SHARED_SEG = 0x10,
            SAHARA_NAK_UNINIT_PHDR_LOC = 0x11,
            SAHARA_NAK_INVALID_DEST_ADDR = 0x12,
            SAHARA_NAK_INVALID_IMG_HDR_DATA_SIZE = 0x13,
            SAHARA_NAK_INVALID_ELF_HDR = 0x14,
            SAHARA_NAK_UNKNOWN_HOST_ERROR = 0x15,
            SAHARA_NAK_TIMEOUT_RX = 0x16,
            SAHARA_NAK_TIMEOUT_TX = 0x17,
            SAHARA_NAK_INVALID_HOST_MODE = 0x18,
            SAHARA_NAK_INVALID_MEMORY_READ = 0x19,
            SAHARA_NAK_INVALID_DATA_SIZE_REQUEST = 0x1A,
            SAHARA_NAK_MEMORY_DEBUG_NOT_SUPPORTED = 0x1B,
            SAHARA_NAK_INVALID_MODE_SWITCH = 0x1C,
            SAHARA_NAK_CMD_EXEC_FAILURE = 0x1D,
            SAHARA_NAK_EXEC_CMD_INVALID_PARAM = 0x1E,
            SAHARA_NAK_EXEC_CMD_UNSUPPORTED = 0x1F,
            SAHARA_NAK_EXEC_DATA_INVALID_CLIENT_CMD = 0x20,
            SAHARA_NAK_HASH_TABLE_AUTH_FAILURE = 0x21,
            SAHARA_NAK_HASH_VERIFICATION_FAILURE = 0x22,
            SAHARA_NAK_HASH_TABLE_NOT_FOUND = 0x23,
            SAHARA_NAK_LAST_CODE = 0x24,
            SAHARA_NAK_MAX_CODE = 0x7FFFFFFF
        }

        // Sahara 模式枚举
        public enum SaharaMode : uint
        {
            SAHARA_MODE_IMAGE_TX_PENDING = 0x0,
            SAHARA_MODE_IMAGE_TX_COMPLETE = 0x1,
            SAHARA_MODE_MEMORY_DEBUG = 0x2,
            SAHARA_MODE_COMMAND = 0x3,
            SAHARA_MODE_LAST = 0x4,
            SAHARA_MODE_MAX = 0x7FFFFFFF
        }

        // Sahara 执行命令 ID 枚举
        public enum SaharaExecCmdId : uint
        {
            SAHARA_EXEC_CMD_NOP = 0x00,
            SAHARA_EXEC_CMD_SERIAL_NUM_READ = 0x01,
            SAHARA_EXEC_CMD_MSM_HW_ID_READ = 0x02,
            SAHARA_EXEC_CMD_OEM_PK_HASH_READ = 0x03,
            SAHARA_EXEC_CMD_SWITCH_DMSS = 0x04,
            SAHARA_EXEC_CMD_SWITCH_STREAMING = 0x05,
            SAHARA_EXEC_CMD_READ_DEBUG_DATA = 0x06,
            SAHARA_EXEC_CMD_LAST = 0x7,
            SAHARA_EXEC_CMD_MAX = 0x7FFFFFFF
        }

        // Sahara 协议状态枚举
        public enum SaharaState
        {
            SAHARA_WAIT_HELLO,
            SAHARA_WAIT_COMMAND,
            SAHARA_WAIT_RESET_RESP,
            SAHARA_WAIT_DONE_RESP,
            SAHARA_WAIT_MEMORY_READ,
            SAHARA_WAIT_CMD_EXEC_RESP,
            SAHARA_WAIT_MEMORY_TABLE,
            SAHARA_WAIT_MEMORY_REGION,
        }

        // Sahara 数据包结构
        [StructLayout(LayoutKind.Sequential)]
        public struct SaharaPacketHeader
        {
            public uint Command;
            public uint Length;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SaharaPacketHello
        {
            public SaharaPacketHeader Header;
            public uint Version;
            public uint VersionSupported;
            public uint CmdPacketLength;
            public uint Mode;
            public uint Reserved0;
            public uint Reserved1;
            public uint Reserved2;
            public uint Reserved3;
            public uint Reserved4;
            public uint Reserved5;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SaharaPacketHelloResp
        {
            public SaharaPacketHeader Header;
            public uint Version;
            public uint VersionSupported;
            public uint Status;
            public uint Mode;
            public uint Reserved0;
            public uint Reserved1;
            public uint Reserved2;
            public uint Reserved3;
            public uint Reserved4;
            public uint Reserved5;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SaharaPacketReadData
        {
            public SaharaPacketHeader Header;
            public uint ImageId;
            public uint DataOffset;
            public uint DataLength;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SaharaPacketReadData64Bit
        {
            public SaharaPacketHeader Header;
            public ulong ImageId;
            public ulong DataOffset;
            public ulong DataLength;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SaharaPacketEndImageTx
        {
            public SaharaPacketHeader Header;
            public uint ImageId;
            public uint Status;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SaharaPacketDone
        {
            public SaharaPacketHeader Header;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SaharaPacketDoneResp
        {
            public SaharaPacketHeader Header;
            public uint ImageTxStatus;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SaharaPacketReset
        {
            public SaharaPacketHeader Header;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SaharaPacketResetResp
        {
            public SaharaPacketHeader Header;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SaharaPacketMemoryDebug
        {
            public SaharaPacketHeader Header;
            public uint MemoryTableAddr;
            public uint MemoryTableLength;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SaharaPacketMemoryDebug64Bit
        {
            public SaharaPacketHeader Header;
            public ulong MemoryTableAddr;
            public ulong MemoryTableLength;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SaharaPacketMemoryRead
        {
            public SaharaPacketHeader Header;
            public uint MemoryAddr;
            public uint MemoryLength;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SaharaPacketMemoryRead64Bit
        {
            public SaharaPacketHeader Header;
            public ulong MemoryAddr;
            public ulong MemoryLength;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SaharaPacketCmdReady
        {
            public SaharaPacketHeader Header;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SaharaPacketCmdSwitchMode
        {
            public SaharaPacketHeader Header;
            public uint Mode;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SaharaPacketCmdExec
        {
            public SaharaPacketHeader Header;
            public uint ClientCommand;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SaharaPacketCmdExecResp
        {
            public SaharaPacketHeader Header;
            public uint ClientCommand;
            public uint RespLength;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SaharaPacketCmdExecData
        {
            public SaharaPacketHeader Header;
            public uint ClientCommand;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DloadDebugType
        {
            public uint SavePref;
            public uint MemBase;
            public uint Length;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SaharaConstants.DLOAD_DEBUG_STRLEN_BYTES)]
            public string Desc;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SaharaConstants.DLOAD_DEBUG_STRLEN_BYTES)]
            public string Filename;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DloadDebugType64Bit
        {
            public ulong SavePref;  // 强制 8 字节对齐
            public ulong MemBase;
            public ulong Length;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SaharaConstants.DLOAD_DEBUG_STRLEN_BYTES)]
            public string Desc;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SaharaConstants.DLOAD_DEBUG_STRLEN_BYTES)]
            public string Filename;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SaharaPacketResetStateMachine
        {
            public SaharaPacketHeader Header;
            public uint Version;
            public uint VersionSupported;
            public uint Status;
            public uint Mode;
            public uint Reserved0;
            public uint Reserved1;
            public uint Reserved2;
            public uint Reserved3;
            public uint Reserved4;
            public uint Reserved5;
        }

        public class ImageMapping
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
        public static T ByteArrayToStructure<T>(byte[] bytes) where T : struct
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                return Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
            }
            finally
            {
                handle.Free();
            }
        }
    }
}
