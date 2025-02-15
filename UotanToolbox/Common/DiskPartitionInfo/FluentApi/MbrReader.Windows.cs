#pragma warning disable CA1416 // This call site is reachable on all platforms.
#if Windows
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using DiskPartitionInfo.Mbr;

namespace DiskPartitionInfo.FluentApi
{
    internal partial class MbrReader : IMbrReader
    {
        /// <inheritdoc/>
        public MasterBootRecord FromPhysicalDriveNumber(int driveNumber)
            => FromPath(@"\\.\PhysicalDrive" + driveNumber);

        /// <inheritdoc/>
        public MasterBootRecord FromVolumeLetter(string volumeLetter)
        {
            if (!volumeLetter.EndsWith(':'))
                volumeLetter += ':';

            var partition = GetPartitions(volumeLetter).FirstOrDefault();

            if (partition is null)
                throw new ArgumentException(
                    $"Could not find a drive for volume {volumeLetter}", nameof(volumeLetter));

            var drive = GetDrives(partition["DeviceID"].ToString()!).FirstOrDefault();

            if (drive is null)
                throw new ArgumentException(
                    $"Could not find a drive for volume {volumeLetter}", nameof(volumeLetter));

            return FromPath(drive["DeviceID"].ToString()!);
        }

        private static IEnumerable<ManagementBaseObject> GetPartitions(string volumeLetter)
            => ExecuteWmicQuery($@"
                ASSOCIATORS OF
                    {{Win32_LogicalDisk.DeviceID='{volumeLetter}'}}
                WHERE
                    AssocClass = Win32_LogicalDiskToPartition");

        private static IEnumerable<ManagementBaseObject> GetDrives(string partitionId)
            => ExecuteWmicQuery($@"
                ASSOCIATORS OF
                    {{Win32_DiskPartition.DeviceID='{partitionId}'}}
                WHERE
                    AssocClass = Win32_DiskDriveToDiskPartition");

        private static IEnumerable<ManagementBaseObject> ExecuteWmicQuery(string query)
        {
            var queryResults = new ManagementObjectSearcher(query);
            var result = queryResults.Get();

            return result is null
                ? Enumerable.Empty<ManagementBaseObject>()
                : result.OfType<ManagementBaseObject>();
        }
    }
}
#endif
#pragma warning restore CA1416 // This call site is reachable on all platforms.
