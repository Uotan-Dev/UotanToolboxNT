using System;

namespace UotanToolbox.Common.PatchHelper
{
    internal class MagiskBootHelper
    {
        // Unpack a boot image
        public void Unpack(string bootImg, bool skipDecompression = false, bool dumpHeader = false)
        {
            throw new NotImplementedException();
        }

        // Repack a boot image
        public void Repack(string origBootImg, string? outBootImg = null, bool skipCompression = false)
        {
            throw new NotImplementedException();
        }

        // Verify boot image signature
        public bool Verify(string bootImg, string? certificate = null)
        {
            throw new NotImplementedException();
        }

        public void Sign(string bootImg, string? name = null, string? certificate = null, string? privateKey = null)
        {
            throw new NotImplementedException();
        }

        public void HexPatch(string file, string hexPattern1, string hexPattern2)
        {
            throw new NotImplementedException();
        }

        // Perform cpio commands
        public void Cpio(string inCpio, params string[] commands)
        {
            throw new NotImplementedException();
        }

        // Perform dtb actions
        public void Dtb(string file, string action, params string[] args)
        {
            throw new NotImplementedException();
        }

        // Split image.*-dtb into kernel and kernel_dtb
        public void Split(string file)
        {
            throw new NotImplementedException();
        }

        // Calculate SHA1 checksum
        public string Sha1(string file)
        {
            throw new NotImplementedException();
        }

        // Cleanup the current working directory
        public void Cleanup()
        {
            throw new NotImplementedException();
        }

        // Compress a file
        public void Compress(string inFile, string? outFile = null, string? format = null)
        {
            throw new NotImplementedException();
        }

        // Decompress a file
        public void Decompress(string inFile, string? outFile = null)
        {
            throw new NotImplementedException();
        }
    }
}
