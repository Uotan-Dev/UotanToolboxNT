namespace WuXingLibrary.code.module;

public class Firehose
{
    private static readonly string _reset_to_edl = "<?xml version=\"1.0\" ?><data><power verbose=\"{0}\"  value=\"reset_to_edl\" DelayInSeconds=\"1\"/></data>";

    private static readonly string _partial_reset = "<?xml version=\"1.0\" ?><data><power verbose=\"{0}\"  value=\"partial_reset\"/></data>";

    private static readonly string _reset = "<?xml version=\"1.0\" ?><data><power verbose=\"{0}\"  value=\"reset\"/></data>";

    private static readonly string _set_boot_partition = "<?xml version=\"1.0\" ?><data><setbootablestoragedrive verbose=\"{0}\"  value=\"1\"/></data>";

    private static readonly string _nop = "<?xml version=\"1.0\" ?><data><nop verbose=\"{0}\"  value=\"ping\"/></data>";

    private static readonly string _configure = "<?xml version=\"1.0\" ?><data><configure verbose=\"{0}\" AlwaysValidate=\"0\"  ZlpAwareHost=\"1\"  MaxPayloadSizeToTargetInBytes=\"{1}\" MemoryName=\"{2}\" SkipStorageInit=\"{3}\"/></data>";



    private static readonly string _peek = "<?xml version=\"1.0\" ?><data><peek size_in_bytes=\"4\" address64=\"0x00780198\"/></data>";

    private static readonly string _peekCupid = "<?xml version=\"1.0\" ?><data><peek size_in_bytes=\"10\" address64=\"0x00786134\"/></data>";

    private static readonly string _xblgpt = "<?xml version=\"1.0\" ?><data><xblgpt lun=\"{0}\"/></data>";

    private static readonly string _getStorageInfo = "<?xml version=\"1.0\" ?><data><getStorageInfo physical_partition_number=\"0\"/></data>";

    public static string Partial_Reset => _partial_reset;

    public static string Reset_To_Edl => _reset_to_edl;

    public static string Reset => _reset;

    public static string SetBootPartition => _set_boot_partition;

    public static string Nop => _nop;

    public static string Configure => _configure;

    public static int Payload_size => 1048576;

    public static int MAX_PATCH_VALUE_LEN => 50;



    public static string PEEK => _peek;

    public static string PEEKCPUID => _peekCupid;

    public static string XBLGPT => _xblgpt;

    public static string GETSTORAGEINFO => _getStorageInfo;
    public static string FIREHOSE_ERASE => "<?xml version=\"1.0\" ?><data><erase SECTOR_SIZE_IN_BYTES=\"{0}\" num_partition_sectors=\"{1}\" start_sector=\"{2}\" physical_partition_number=\"{3}\"/></data>";

    public static string FIREHOSE_PROGRAM => "<?xml version=\"1.0\" ?><data><program SECTOR_SIZE_IN_BYTES=\"{0}\" num_partition_sectors=\"{1}\" start_sector=\"{2}\" physical_partition_number=\"{3}\" {4}/></data>";

    public static string FIREHOSE_READ => "<?xml version=\"1.0\" ?><data><read SECTOR_SIZE_IN_BYTES=\"{0}\" num_partition_sectors=\"{1}\" start_sector=\"{2}\" physical_partition_number=\"{3}\"/></data>";
    public static string FIREHOSE_FIRMWAREWRITE => "<?xml version=\"1.0\" ?><data><firmwarewrite SECTOR_SIZE_IN_BYTES=\"{0}\" num_partition_sectors=\"{1}\" start_sector=\"{2}\" physical_partition_number=\"{3}\" Version=\"1\" /></data>";

    public static string FIREHOSE_SHA256DIGEST => "<?xml version=\"1.0\" ?><data><getsha256digest SECTOR_SIZE_IN_BYTES=\"{0}\" num_partition_sectors=\"{1}\" start_sector=\"{2}\" physical_partition_number=\"{3}\"/></data>";

    public static string FIREHOSE_PATCH => "<?xml version=\"1.0\" ?><data><patch SECTOR_SIZE_IN_BYTES=\"{0}\" byte_offset=\"{1}\" filename=\"DISK\" physical_partition_number=\"{2}\" size_in_bytes=\"{3}\" start_sector=\"{4}\" value=\"{5}\" what=\"Update\" {6}/></data>";

    public static string REQ_AUTH => "<?xml version=\"1.0\" ?><data> <sig TargetName=\"req\" verbose=\"1\"/></data>";

    public static string AUTH => "<?xml version=\"1.0\" ?><data> <sig TargetName=\"sig\" value=\"{0}\" verbose=\"1\"/></data>";

    public static string AUTHP => "<?xml version=\"1.0\" ?><data> <sig TargetName=\"sig\" size_in_bytes=\"{0}\" verbose=\"1\"/></data>";
}
