using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;


namespace ROMLibrary
{
    internal class OpsHelper
    {
        private static uint[] key = { 2665723345, 2634345054, 2882870631, 2949361618 };
        private static byte[] mbox5 =
        [
                0x60, 0x8a, 0x3f, 0x2d, 0x68, 0x6b, 0xd4, 0x23, 0x51, 0x0c,
            0xd0, 0x95, 0xbb, 0x40, 0xe9, 0x76, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x0a, 0x00
        ];
        private static byte[] mbox6 =
        [
                0xAA, 0x69, 0x82, 0x9E, 0x5D, 0xDE, 0xB1, 0x3D, 0x30, 0xBB,
            0x81, 0xA3, 0x46, 0x65, 0xa3, 0xe1, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x0a, 0x00
        ];
        private static byte[] mbox4 =
        [
                0xC4, 0x5D, 0x05, 0x71, 0x99, 0xDD, 0xBB, 0xEE, 0x29, 0xA1,
            0x6D, 0xC7, 0xAD, 0xBF, 0xA4, 0x3F, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x0a, 0x00
        ];
        private static byte[] mbox;
        private static byte[] sbox = HexStringToByteArray("c66363a5c66363a5f87c7c84f87c7c84ee777799ee777799f67b7b8df67b7b8d" +
                         "fff2f20dfff2f20dd66b6bbdd66b6bbdde6f6fb1de6f6fb191c5c55491c5c554" +
                         "60303050603030500201010302010103ce6767a9ce6767a9562b2b7d562b2b7d" +
                         "e7fefe19e7fefe19b5d7d762b5d7d7624dababe64dababe6ec76769aec76769a" +
                         "8fcaca458fcaca451f82829d1f82829d89c9c94089c9c940fa7d7d87fa7d7d87" +
                         "effafa15effafa15b25959ebb25959eb8e4747c98e4747c9fbf0f00bfbf0f00b" +
                         "41adadec41adadecb3d4d467b3d4d4675fa2a2fd5fa2a2fd45afafea45afafea" +
                         "239c9cbf239c9cbf53a4a4f753a4a4f7e4727296e47272969bc0c05b9bc0c05b" +
                         "75b7b7c275b7b7c2e1fdfd1ce1fdfd1c3d9393ae3d9393ae4c26266a4c26266a" +
                         "6c36365a6c36365a7e3f3f417e3f3f41f5f7f702f5f7f70283cccc4f83cccc4f" +
                         "6834345c6834345c51a5a5f451a5a5f4d1e5e534d1e5e534f9f1f108f9f1f108" +
                         "e2717193e2717193abd8d873abd8d87362313153623131532a15153f2a15153f" +
                         "0804040c0804040c95c7c75295c7c75246232365462323659dc3c35e9dc3c35e" +
                         "3018182830181828379696a1379696a10a05050f0a05050f2f9a9ab52f9a9ab5" +
                         "0e0707090e07070924121236241212361b80809b1b80809bdfe2e23ddfe2e23d" +
                         "cdebeb26cdebeb264e2727694e2727697fb2b2cd7fb2b2cdea75759fea75759f" +
                         "1209091b1209091b1d83839e1d83839e582c2c74582c2c74341a1a2e341a1a2e" +
                         "361b1b2d361b1b2ddc6e6eb2dc6e6eb2b45a5aeeb45a5aee5ba0a0fb5ba0a0fb" +
                         "a45252f6a45252f6763b3b4d763b3b4db7d6d661b7d6d6617db3b3ce7db3b3ce" +
                         "5229297b5229297bdde3e33edde3e33e5e2f2f715e2f2f711384849713848497" +
                         "a65353f5a65353f5b9d1d168b9d1d1680000000000000000c1eded2cc1eded2c" +
                         "4020206040202060e3fcfc1fe3fcfc1f79b1b1c879b1b1c8b65b5bedb65b5bed" +
                         "d46a6abed46a6abe8dcbcb468dcbcb4667bebed967bebed97239394b7239394b" +
                         "944a4ade944a4ade984c4cd4984c4cd4b05858e8b05858e885cfcf4a85cfcf4a" +
                         "bbd0d06bbbd0d06bc5efef2ac5efef2a4faaaae54faaaae5edfbfb16edfbfb16" +
                         "864343c5864343c59a4d4dd79a4d4dd766333355663333551185859411858594" +
                         "8a4545cf8a4545cfe9f9f910e9f9f9100402020604020206fe7f7f81fe7f7f81" +
                         "a05050f0a05050f0783c3c44783c3c44259f9fba259f9fba4ba8a8e34ba8a8e3" +
                         "a25151f3a25151f35da3a3fe5da3a3fe804040c0804040c0058f8f8a058f8f8a" +
                         "3f9292ad3f9292ad219d9dbc219d9dbc7038384870383848f1f5f504f1f5f504" +
                         "63bcbcdf63bcbcdf77b6b6c177b6b6c1afdada75afdada754221216342212163" +
                         "2010103020101030e5ffff1ae5ffff1afdf3f30efdf3f30ebfd2d26dbfd2d26d" +
                         "81cdcd4c81cdcd4c180c0c14180c0c142613133526131335c3ecec2fc3ecec2f" +
                         "be5f5fe1be5f5fe1359797a2359797a2884444cc884444cc2e1717392e171739" +
                         "93c4c45793c4c45755a7a7f255a7a7f2fc7e7e82fc7e7e827a3d3d477a3d3d47" +
                         "c86464acc86464acba5d5de7ba5d5de73219192b3219192be6737395e6737395" +
                         "c06060a0c06060a019818198198181989e4f4fd19e4f4fd1a3dcdc7fa3dcdc7f" +
                         "4422226644222266542a2a7e542a2a7e3b9090ab3b9090ab0b8888830b888883" +
                         "8c4646ca8c4646cac7eeee29c7eeee296bb8b8d36bb8b8d32814143c2814143c" +
                         "a7dede79a7dede79bc5e5ee2bc5e5ee2160b0b1d160b0b1daddbdb76addbdb76" +
                         "dbe0e03bdbe0e03b6432325664323256743a3a4e743a3a4e140a0a1e140a0a1e" +
                         "924949db924949db0c06060a0c06060a4824246c4824246cb85c5ce4b85c5ce4" +
                         "9fc2c25d9fc2c25dbdd3d36ebdd3d36e43acacef43acacefc46262a6c46262a6" +
                         "399191a8399191a8319595a4319595a4d3e4e437d3e4e437f279798bf279798b" +
                         "d5e7e732d5e7e7328bc8c8438bc8c8436e3737596e373759da6d6db7da6d6db7" +
                         "018d8d8c018d8d8cb1d5d564b1d5d5649c4e4ed29c4e4ed249a9a9e049a9a9e0" +
                         "d86c6cb4d86c6cb4ac5656faac5656faf3f4f407f3f4f407cfeaea25cfeaea25" +
                         "ca6565afca6565aff47a7a8ef47a7a8e47aeaee947aeaee91008081810080818" +
                         "6fbabad56fbabad5f0787888f07878884a25256f4a25256f5c2e2e725c2e2e72" +
                         "381c1c24381c1c2457a6a6f157a6a6f173b4b4c773b4b4c797c6c65197c6c651" +
                         "cbe8e823cbe8e823a1dddd7ca1dddd7ce874749ce874749c3e1f1f213e1f1f21" +
                         "964b4bdd964b4bdd61bdbddc61bdbddc0d8b8b860d8b8b860f8a8a850f8a8a85" +
                         "e0707090e07070907c3e3e427c3e3e4271b5b5c471b5b5c4cc6666aacc6666aa" +
                         "904848d8904848d80603030506030305f7f6f601f7f6f6011c0e0e121c0e0e12" +
                         "c26161a3c26161a36a35355f6a35355fae5757f9ae5757f969b9b9d069b9b9d0" +
                         "178686911786869199c1c15899c1c1583a1d1d273a1d1d27279e9eb9279e9eb9" +
                         "d9e1e138d9e1e138ebf8f813ebf8f8132b9898b32b9898b32211113322111133" +
                         "d26969bbd26969bba9d9d970a9d9d970078e8e89078e8e89339494a7339494a7" +
                         "2d9b9bb62d9b9bb63c1e1e223c1e1e221587879215878792c9e9e920c9e9e920" +
                         "87cece4987cece49aa5555ffaa5555ff5028287850282878a5dfdf7aa5dfdf7a" +
                         "038c8c8f038c8c8f59a1a1f859a1a1f809898980098989801a0d0d171a0d0d17" +
                         "65bfbfda65bfbfdad7e6e631d7e6e631844242c6844242c6d06868b8d06868b8" +
                         "824141c3824141c3299999b0299999b05a2d2d775a2d2d771e0f0f111e0f0f11" +
                         "7bb0b0cb7bb0b0cba85454fca85454fc6dbbbbd66dbbbbd62c16163a2c16163a");

        private static byte[] HexStringToByteArray(string hex)
        {
            int length = hex.Length;
            byte[] bytes = new byte[length / 2];
            for (int i = 0; i < length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }
        private static uint Gsbox(uint offset)
        {
            byte[] bytes = new byte[4];
            Array.Copy(sbox, offset, bytes, 0, 4);
            return BitConverter.ToUInt32(bytes, 0);
        }
        private static uint[] KeyUpdate(uint[] iv1, uint[] asbox)
        {
            uint d = iv1[0] ^ asbox[0];
            uint a = iv1[1] ^ asbox[1];
            uint b = iv1[2] ^ asbox[2];
            uint c = iv1[3] ^ asbox[3];
            uint e = Gsbox((b >> 16 & 0xff) * 8 + 2) ^ Gsbox((a >> 8 & 0xff) * 8 + 3) ^ Gsbox((c >> 24) * 8 + 1) ^ Gsbox((d & 0xff) * 8) ^ asbox[4];
            uint h = Gsbox((c >> 16 & 0xff) * 8 + 2) ^ Gsbox((b >> 8 & 0xff) * 8 + 3) ^ Gsbox((d >> 24) * 8 + 1) ^ Gsbox((a & 0xff) * 8) ^ asbox[5];
            uint i = Gsbox((d >> 16 & 0xff) * 8 + 2) ^ Gsbox((c >> 8 & 0xff) * 8 + 3) ^ Gsbox((a >> 24) * 8 + 1) ^ Gsbox((b & 0xff) * 8) ^ asbox[6];
            a = Gsbox((d >> 8 & 0xff) * 8 + 3) ^ Gsbox((a >> 16 & 0xff) * 8 + 2) ^ Gsbox((b >> 24) * 8 + 1) ^ Gsbox((c & 0xff) * 8) ^ asbox[7];

            int g = 8;

            for (int f = 0; f < asbox[0x3c] - 2; f++)
            {
                d = e >> 24;
                uint m = h >> 16;
                uint s = h >> 24;
                uint z = e >> 16;
                uint l = i >> 24;
                uint t = e >> 8;
                e = Gsbox((i >> 16 & 0xff) * 8 + 2) ^ Gsbox((h >> 8 & 0xff) * 8 + 3) ^ Gsbox((a >> 24) * 8 + 1) ^ Gsbox((e & 0xff) * 8) ^ asbox[g];
                h = Gsbox((a >> 16 & 0xff) * 8 + 2) ^ Gsbox((i >> 8 & 0xff) * 8 + 3) ^ Gsbox(d * 8 + 1) ^ Gsbox((h & 0xff) * 8) ^ asbox[g + 1];
                i = Gsbox((z & 0xff) * 8 + 2) ^ Gsbox((a >> 8 & 0xff) * 8 + 3) ^ Gsbox(s * 8 + 1) ^ Gsbox((i & 0xff) * 8) ^ asbox[g + 2];
                a = Gsbox((t & 0xff) * 8 + 3) ^ Gsbox((m & 0xff) * 8 + 2) ^ Gsbox(l * 8 + 1) ^ Gsbox((a & 0xff) * 8) ^ asbox[g + 3];
                g += 4;
            }

            return
            [
                Gsbox((i >> 16 & 0xff) * 8) & 0xff0000 ^ Gsbox((h >> 8 & 0xff) * 8 + 1) & 0xff00 ^ Gsbox((a >> 24) * 8 + 3) & 0xff000000 ^ Gsbox((e & 0xff) * 8 + 2) & 0xFF ^ asbox[g],
            Gsbox((a >> 16 & 0xff) * 8) & 0xff0000 ^ Gsbox((i >> 8 & 0xff) * 8 + 1) & 0xff00 ^ Gsbox((e >> 24) * 8 + 3) & 0xff000000 ^ Gsbox((h & 0xff) * 8 + 2) & 0xFF ^ asbox[g + 3],
            Gsbox((e >> 16 & 0xff) * 8) & 0xff0000 ^ Gsbox((a >> 8 & 0xff) * 8 + 1) & 0xff00 ^ Gsbox((h >> 24) * 8 + 3) & 0xff000000 ^ Gsbox((i & 0xff) * 8 + 2) & 0xFF ^ asbox[g + 2],
            Gsbox((h >> 16 & 0xff) * 8) & 0xff0000 ^ Gsbox((e >> 8 & 0xff) * 8 + 1) & 0xff00 ^ Gsbox((i >> 24) * 8 + 3) & 0xff000000 ^ Gsbox((a & 0xff) * 8 + 2) & 0xFF ^ asbox[g + 1]
            ];
        }
        private static uint[] KeyUpdate(uint[] iv1, byte[] asbox)
        {
            uint d = iv1[0] ^ asbox[0];
            uint a = iv1[1] ^ asbox[1];
            uint b = iv1[2] ^ asbox[2];
            uint c = iv1[3] ^ asbox[3];
            uint e = Gsbox((b >> 16 & 0xff) * 8 + 2) ^ Gsbox((a >> 8 & 0xff) * 8 + 3) ^ Gsbox((c >> 24) * 8 + 1) ^ Gsbox((d & 0xff) * 8) ^ asbox[4];
            uint h = Gsbox((c >> 16 & 0xff) * 8 + 2) ^ Gsbox((b >> 8 & 0xff) * 8 + 3) ^ Gsbox((d >> 24) * 8 + 1) ^ Gsbox((a & 0xff) * 8) ^ asbox[5];
            uint i = Gsbox((d >> 16 & 0xff) * 8 + 2) ^ Gsbox((c >> 8 & 0xff) * 8 + 3) ^ Gsbox((a >> 24) * 8 + 1) ^ Gsbox((b & 0xff) * 8) ^ asbox[6];
            a = Gsbox((d >> 8 & 0xff) * 8 + 3) ^ Gsbox((a >> 16 & 0xff) * 8 + 2) ^ Gsbox((b >> 24) * 8 + 1) ^ Gsbox((c & 0xff) * 8) ^ asbox[7];
            int g = 8;
            for (int f = 0; f < asbox[0x3c] - 2; f++)
            {
                d = e >> 24;
                uint m = h >> 16;
                uint s = h >> 24;
                uint z = e >> 16;
                uint l = i >> 24;
                uint t = e >> 8;
                e = Gsbox((i >> 16 & 0xff) * 8 + 2) ^ Gsbox((h >> 8 & 0xff) * 8 + 3) ^ Gsbox((a >> 24) * 8 + 1) ^ Gsbox((e & 0xff) * 8) ^ asbox[g];
                h = Gsbox((a >> 16 & 0xff) * 8 + 2) ^ Gsbox((i >> 8 & 0xff) * 8 + 3) ^ Gsbox(d * 8 + 1) ^ Gsbox((h & 0xff) * 8) ^ asbox[g + 1];
                i = Gsbox((z & 0xff) * 8 + 2) ^ Gsbox((a >> 8 & 0xff) * 8 + 3) ^ Gsbox(s * 8 + 1) ^ Gsbox((i & 0xff) * 8) ^ asbox[g + 2];
                a = Gsbox((t & 0xff) * 8 + 3) ^ Gsbox((m & 0xff) * 8 + 2) ^ Gsbox(l * 8 + 1) ^ Gsbox((a & 0xff) * 8) ^ asbox[g + 3];
                g += 4;
            }
            return
            [
            Gsbox((i >> 16 & 0xff) * 8) & 0xff0000 ^ Gsbox((h >> 8 & 0xff) * 8 + 1) & 0xff00 ^ Gsbox((a >> 24) * 8 + 3) & 0xff000000 ^ Gsbox((e & 0xff) * 8 + 2) & 0xFF ^ asbox[g],
        Gsbox((a >> 16 & 0xff) * 8) & 0xff0000 ^ Gsbox((i >> 8 & 0xff) * 8 + 1) & 0xff00 ^ Gsbox((e >> 24) * 8 + 3) & 0xff000000 ^ Gsbox((h & 0xff) * 8 + 2) & 0xFF ^ asbox[g + 3],
        Gsbox((e >> 16 & 0xff) * 8) & 0xff0000 ^ Gsbox((a >> 8 & 0xff) * 8 + 1) & 0xff00 ^ Gsbox((h >> 24) * 8 + 3) & 0xff000000 ^ Gsbox((i & 0xff) * 8 + 2) & 0xFF ^ asbox[g + 2],
        Gsbox((h >> 16 & 0xff) * 8) & 0xff0000 ^ Gsbox((e >> 8 & 0xff) * 8 + 1) & 0xff00 ^ Gsbox((i >> 24) * 8 + 3) & 0xff000000 ^ Gsbox((a & 0xff) * 8 + 2) & 0xFF ^ asbox[g + 1]
            ];
        }
        private static byte[] KeyCustom(byte[] inp, uint[] rkey, int outlength = 0, bool encrypt = false)
        {
            List<byte> outp = new List<byte>();
            int pos = outlength;
            int ptr = 0;
            int length = inp.Length;

            if (outlength != 0)
            {
                while (pos < rkey.Length)
                {
                    if (length == 0)
                    {
                        break;
                    }
                    byte buffer = inp[pos];
                    outp.Add((byte)(rkey[pos] ^ buffer));
                    rkey[pos] = buffer;
                    length -= 1;
                    pos += 1;
                }
            }
            if (length > 0xF)
            {
                int temp = length;
                for (ptr = 0; ptr < temp; ptr += 0x10)
                {
                    rkey = KeyUpdate(rkey, mbox);

                    if (pos < 0x10)
                    {
                        int slen = (0xF - pos >> 2) + 1;
                        uint[] tmp = new uint[slen];
                        for (int i = 0; i < slen; i++)
                        {
                            if (pos + i * 4 + ptr + 4 <= inp.Length)
                            {
                                tmp[i] = rkey[i] ^ BitConverter.ToUInt32(inp, pos + i * 4 + ptr);
                            }
                            else
                            {
                                tmp[i] = rkey[i] ^ BitConverter.ToUInt32(inp.Skip(pos + i * 4 + ptr).ToArray().Concat(new byte[4]).ToArray(), 0);
                            }
                        }
                        foreach (uint t in tmp)
                        {
                            outp.AddRange(BitConverter.GetBytes(t));
                        }

                        if (encrypt)
                        {
                            rkey = tmp;
                        }
                        else
                        {
                            for (int i = 0; i < slen; i++)
                            {
                                int startIndex = pos + i * 4 + ptr;
                                if (startIndex >= 0 && startIndex + 4 <= inp.Length)
                                {
                                    rkey[i] = BitConverter.ToUInt32(inp, startIndex);
                                }
                                else
                                {
                                    byte[] tempArray = new byte[4];
                                    if (startIndex >= 0 && startIndex < inp.Length)
                                    {
                                        Array.Copy(inp, startIndex, tempArray, 0, Math.Min(4, inp.Length - startIndex));
                                    }
                                    rkey[i] = BitConverter.ToUInt32(tempArray, 0);
                                }
                            }
                        }
                    }
                    length -= 0x10;
                }
            }
            if (length != 0)
            {
                rkey = KeyUpdate(rkey, sbox);
                int j = pos;
                int m = 0;
                while (length > 0)
                {
                    byte[] data = inp.Skip(j + ptr).Take(4).ToArray();
                    if (data.Length < 4)
                    {
                        Array.Resize(ref data, 4);
                    }
                    uint tmp = BitConverter.ToUInt32(data, 0);
                    outp.AddRange(BitConverter.GetBytes(tmp ^ rkey[m]));

                    if (encrypt)
                    {
                        rkey[m] = tmp ^ rkey[m];
                    }
                    else
                    {
                        rkey[m] = tmp;
                    }
                    length -= 4;
                    j += 4;
                    m += 1;
                }
            }

            return outp.ToArray();
        }

        private static string ExtractXml(string filename, uint[] key, string path)
        {
            string sfilename = Path.Combine(path, "settings.xml");
            long filesize = new FileInfo(filename).Length;

            using (FileStream rf = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                rf.Seek(filesize - 0x200, SeekOrigin.Begin);
                byte[] hdr = new byte[0x200];
                rf.ReadExactly(hdr, 0, 0x200);
                int xmllength = BitConverter.ToInt32(hdr, 0x18);
                int xmlpad = 0x200 - xmllength % 0x200;
                rf.Seek(filesize - 0x200 - (xmllength + xmlpad), SeekOrigin.Begin);
                byte[] inp = new byte[xmllength + xmlpad];
                rf.ReadExactly(inp, 0, xmllength + xmlpad);
                byte[] outp = KeyCustom(inp, key, 0);
                if (!Encoding.UTF8.GetString(outp).Contains("xml ", StringComparison.CurrentCulture))
                {
                    return null;
                }
                using (FileStream wf = new FileStream(sfilename, FileMode.Create, FileAccess.Write))
                {
                    wf.Write(outp, 0, xmllength);
                }

                string input = Encoding.UTF8.GetString(outp.ToArray(), 0, xmllength);

                int index = input.IndexOf('<');
                if (index != -1)
                {
                    return input.Substring(index);
                }
                return input;
            }
        }
        private static string DecryptFile(uint[] rkey, string filename, string path, string wfilename, long start, int length)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                Console.WriteLine($"Extracting {wfilename}");
                using (FileStream rf = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    rf.Seek(start, SeekOrigin.Begin);
                    byte[] data = new byte[length];
                    rf.ReadExactly(data, 0, length);

                    if (length % 4 != 0)
                    {
                        int padding = 4 - length % 4;
                        Array.Resize(ref data, length + padding);
                    }

                    byte[] outp = KeyCustom(data, rkey, 0, false);
                    sha256.TransformBlock(outp, 0, length, outp, 0);

                    string outputPath = Path.Combine(path, wfilename);
                    using (FileStream wf = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                    {
                        wf.Write(outp, 0, length);
                    }
                }

                if (length % 0x1000 > 0)
                {
                    byte[] padding = new byte[0x1000 - length % 0x1000];
                    sha256.TransformBlock(padding, 0, padding.Length, padding, 0);
                }

                sha256.TransformFinalBlock([], 0, 0);
                return Convert.ToHexString(sha256.Hash).ToLower();
            }
        }
        private static int EncryptSubSub(uint[] rkey, byte[] data, FileStream wf)
        {
            int length = data.Length;
            if (length % 4 != 0)
            {
                int padding = 4 - length % 4;
                Array.Resize(ref data, length + padding);
            }
            byte[] outp = KeyCustom(data, rkey, 0, true);
            wf.Write(outp, 0, length);
            return length;
        }

        private static int EncryptSub(uint[] rkey, FileStream rf, FileStream wf)
        {
            byte[] data = new byte[rf.Length];
            rf.ReadExactly(data);
            return EncryptSubSub(rkey, data, wf);
        }

        private static int EncryptFile(uint[] key, string filename, string wfilename)
        {
            Console.WriteLine($"Encrypting {filename}");
            using (FileStream rf = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                using (FileStream wf = new FileStream(wfilename, FileMode.Create, FileAccess.Write))
                {
                    return EncryptSub(key, rf, wf);
                }
            }
        }
        private static string CalcDigest(string filename)
        {
            using (FileStream rf = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] buffer = new byte[0x1000];
                    int bytesRead;
                    while ((bytesRead = rf.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        sha256.TransformBlock(buffer, 0, bytesRead, buffer, 0);
                    }

                    if (rf.Length % 0x1000 > 0)
                    {
                        byte[] padding = new byte[0x1000 - rf.Length % 0x1000];
                        sha256.TransformBlock(padding, 0, padding.Length, padding, 0);
                    }

                    sha256.TransformFinalBlock(new byte[0], 0, 0);
                    return BitConverter.ToString(sha256.Hash).Replace("-", "").ToLower();
                }
            }
        }

        private static int CopySub(FileStream rf, FileStream wf, long start, long length)
        {
            rf.Seek(start, SeekOrigin.Begin);
            int rlen = 0;
            byte[] buffer = new byte[0x100000]; // 1 MB buffer

            while (length > 0)
            {
                int size = (int)Math.Min(length, buffer.Length);
                int bytesRead = rf.Read(buffer, 0, size);
                if (bytesRead == 0)
                {
                    break;
                }
                wf.Write(buffer, 0, bytesRead);
                rlen += bytesRead;
                length -= bytesRead;
            }
            return rlen;
        }
        private static int CopyFile(string filename, string path, string wfilename, long start, long length)
        {
            Console.WriteLine($"Extracting {wfilename}");
            string outputPath = Path.Combine(path, wfilename);

            using (FileStream rf = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                using (FileStream wf = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {
                    return CopySub(rf, wf, start, length);
                }
            }
        }
        private static (XElement, long) EncryptItem(uint[] key, XElement item, string directory, long pos, FileStream wf)
        {
            string filename;
            try
            {
                filename = item.Attribute("Path")?.Value ?? item.Attribute("filename")?.Value;
            }
            catch
            {
                filename = item.Attribute("filename")?.Value;
            }

            if (string.IsNullOrEmpty(filename))
            {
                return (item, pos);
            }

            Console.WriteLine($"Encrypting {filename}, pos={pos}");
            filename = Path.Combine(directory, filename);
            long start = pos / 0x200;
            if (item.Attribute("FileOffsetInSrc")?.Value != start.ToString())
            {
                throw new InvalidOperationException($"FileOffsetInSrc mismatch: {item}");
            }
            item.SetAttributeValue("FileOffsetInSrc", start.ToString());

            long size = new FileInfo(filename).Length;
            if (item.Attribute("SizeInByteInSrc")?.Value != size.ToString())
            {
                throw new InvalidOperationException($"SizeInByteInSrc mismatch: {item}");
            }
            item.SetAttributeValue("SizeInByteInSrc", size.ToString());

            long sectors = size / 0x200;
            if (size % 0x200 != 0)
            {
                sectors += 1;
            }
            if (item.Attribute("SizeInSectorInSrc")?.Value != sectors.ToString())
            {
                throw new InvalidOperationException($"SizeInSectorInSrc mismatch: {item}");
            }
            item.SetAttributeValue("SizeInSectorInSrc", sectors.ToString());

            using (FileStream rf = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                int rlen = EncryptSub(key, rf, wf);
                pos += rlen;
                if (rlen % 0x200 != 0)
                {
                    int sublen = 0x200 - rlen % 0x200;
                    wf.Write(new byte[sublen], 0, sublen);
                    pos += sublen;
                }
            }

            return (item, pos);
        }
        private static (XElement, long) CopyItem(XElement item, string directory, long pos, FileStream wf)
        {
            string filename;
            try
            {
                filename = item.Attribute("Path")?.Value ?? item.Attribute("filename")?.Value;
            }
            catch
            {
                filename = item.Attribute("filename")?.Value;
            }

            if (string.IsNullOrEmpty(filename))
            {
                return (item, pos);
            }

            Console.WriteLine($"Copying {filename} @ pos={pos}");
            filename = Path.Combine(directory, filename);
            long start = pos / 0x200;
            if (item.Attribute("FileOffsetInSrc")?.Value != start.ToString())
            {
                throw new InvalidOperationException($"FileOffsetInSrc mismatch: {item}");
            }
            item.SetAttributeValue("FileOffsetInSrc", start.ToString());

            long size = new FileInfo(filename).Length;
            if (item.Attribute("SizeInByteInSrc")?.Value != size.ToString())
            {
                throw new InvalidOperationException($"SizeInByteInSrc mismatch: {item}");
            }
            item.SetAttributeValue("SizeInByteInSrc", size.ToString());

            long sectors = size / 0x200;
            if (size % 0x200 != 0)
            {
                sectors += 1;
            }
            if (item.Attribute("SizeInSectorInSrc")?.Value != sectors.ToString())
            {
                throw new InvalidOperationException($"SizeInSectorInSrc mismatch: {item}");
            }
            item.SetAttributeValue("SizeInSectorInSrc", sectors.ToString());

            using (FileStream rf = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                int rlen = CopySub(rf, wf, 0, (int)size);
                pos += rlen;
                if (rlen % 0x200 != 0)
                {
                    int sublen = 0x200 - rlen % 0x200;
                    wf.Write(new byte[sublen], 0, sublen);
                    pos += sublen;
                }
            }
            Console.WriteLine(pos);
            return (item, pos);
        }
        public static void decrypt(string filename, string path)
        {
            byte[][] mboxes = { mbox5, mbox6, mbox4 };
            string[] mboxNames = { "MBox5", "MBox6", "MBox4" };
            string xml = "";
            for (int i = 0; i < mboxes.Length; i++)
            {
                mbox = mboxes[i];
                xml = ExtractXml(filename, key, path);
                if (xml != null)
                {
                    Console.WriteLine(mboxNames[i]);
                    break;
                }
                if (i == mboxes.Length - 1)
                {
                    Console.WriteLine("Unsupported key !");
                    Environment.Exit(0);
                }
            }
            XElement root = XElement.Parse(xml);
            foreach (XElement child in root.Elements())
            {
                if (child.Name == "SAHARA")
                {
                    foreach (XElement item in child.Elements("File"))
                    {
                        string wfilename = item.Attribute("Path").Value;
                        long start = long.Parse(item.Attribute("FileOffsetInSrc").Value) * 0x200;
                        int length = int.Parse(item.Attribute("SizeInByteInSrc").Value);
                        DecryptFile(key, filename, path, wfilename, start, length);
                    }
                }
                else if (child.Name == "UFS_PROVISION")
                {
                    foreach (XElement item in child.Elements("File"))
                    {
                        string wfilename = item.Attribute("Path").Value;
                        long start = long.Parse(item.Attribute("FileOffsetInSrc").Value) * 0x200;
                        int length = int.Parse(item.Attribute("SizeInByteInSrc").Value);
                        CopyFile(filename, path, wfilename, start, length);
                    }
                }
                else if (child.Name.LocalName.Contains("Program"))
                {
                    foreach (XElement item in child.Elements())
                    {
                        if (item.Attribute("filename") != null)
                        {
                            bool sparse = item.Attribute("sparse").Value == "true";
                            string wfilename = item.Attribute("filename").Value;
                            if (string.IsNullOrEmpty(wfilename)) continue;
                            long start = long.Parse(item.Attribute("FileOffsetInSrc").Value) * 0x200;
                            long length = long.Parse(item.Attribute("SizeInByteInSrc").Value);
                            string sha256 = item.Attribute("Sha256").Value;
                            CopyFile(filename, path, wfilename, start, length);
                            string csha256 = CalcDigest(Path.Combine(path, wfilename));
                            if (sha256 != csha256 && !sparse)
                            {
                                Console.WriteLine("Sha256 fail.");
                            }
                        }
                        else
                        {
                            foreach (XElement subitem in item.Elements())
                            {
                                if (subitem.Attribute("filename") != null)
                                {
                                    string wfilename = subitem.Attribute("filename").Value;
                                    bool sparse = subitem.Attribute("sparse").Value == "true";
                                    if (string.IsNullOrEmpty(wfilename)) continue;
                                    long start = long.Parse(subitem.Attribute("FileOffsetInSrc").Value) * 0x200;
                                    long length = long.Parse(subitem.Attribute("SizeInByteInSrc").Value);
                                    string sha256 = subitem.Attribute("Sha256").Value;
                                    CopyFile(filename, path, wfilename, start, length);
                                    string csha256 = CalcDigest(Path.Combine(path, wfilename));
                                    if (sha256 != csha256 && !sparse)
                                    {
                                        Console.WriteLine("Sha256 fail.");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            Console.WriteLine("Done. Extracted files to " + path);
            Environment.Exit(0);
        }
        /*注意，该段注释代码仍然存在问题，无法正确压缩文件
        static void encrypt()
        {
            string mboxVersion = "4";
            mbox = mboxVersion switch
            {
                "4" => mbox4,
                "5" => mbox5,
                "6" => mbox6,
                _ => mbox5
            };
            string directory = "C:\\Users\\zicai\\source\\repos\\ops-xml\\bin\\Debug\\net9.0";
            string settings = Path.Combine(directory, "settings.xml");
            XElement root = XElement.Load(settings);
            string outfilename = Path.Combine(Directory.GetParent(directory).FullName, "2.ops");
            string projid = null;
            string firmware = null;

            if (File.Exists(outfilename))
            {
                File.Delete(outfilename);
            }


            using (FileStream wf = new FileStream(outfilename, FileMode.Create, FileAccess.Write))
            {
                long pos = 0;
                foreach (XElement child in root.Elements())
                {
                    if (child.Name == "BasicInfo")
                    {
                        projid = child.Attribute("Project")?.Value;
                        firmware = child.Attribute("Version")?.Value;
                    }
                    else if (child.Name == "SAHARA")
                    {
                        foreach (XElement item in child.Elements("File"))
                        {
                            var result = EncryptItem(key, item, directory, pos, wf);
                            pos = result.Item2;
                        }
                    }
                    else if (child.Name == "UFS_PROVISION")
                    {
                        foreach (XElement item in child.Elements("File"))
                        {
                            var result = CopyItem(item, directory, pos, wf);
                            pos = result.Item2;
                        }
                    }
                    else if (child.Name.LocalName.Contains("Program"))
                    {
                        foreach (XElement item in child.Elements())
                        {
                            if (item.Attribute("filename") != null)
                            {
                                pos = long.Parse(item.Attribute("FileOffsetInSrc").Value) * 0x200;
                                var result = CopyItem(item, directory, pos, wf);
                            }
                            else
                            {
                                foreach (XElement subitem in item.Elements())
                                {
                                    var result = CopyItem(subitem, directory, pos, wf);
                                    pos = result.Item2;
                                }
                            }
                        }
                    }
                }

                long configpos = pos / 0x200;
                int rlength;
                using (FileStream rf = new FileStream(settings, FileMode.Open, FileAccess.Read))
                {
                    byte[] data = new byte[rf.Length];
                    rf.Read(data, 0, data.Length);
                    rlength = data.Length;
                    if (rlength % 0x10 != 0)
                    {
                        Array.Resize(ref data, rlength + (0x10 - (rlength % 0x10)));
                    }
                    int rlen = EncryptSubSub(key, data, wf);
                    if ((rlen + pos) % 0x200 != 0)
                    {
                        long sublen = 0x200 - ((rlen + pos) % 0x200);
                        wf.Write(new byte[sublen], 0, (int)sublen);
                        pos += sublen;
                    }
                }

                projid ??= args.Contains("--projid") ? args[Array.IndexOf(args, "--projid") + 1] : "18801";
                firmware ??= args.Contains("--firmwarename") ? args[Array.IndexOf(args, "--firmwarename") + 1] : "fajita_41_J.42_191214";

                byte[] hdr = new byte[0x200];
                using (MemoryStream ms = new MemoryStream(hdr))
                using (BinaryWriter writer = new BinaryWriter(ms))
                {
                    writer.Write(2);
                    writer.Write(1);
                    writer.Write(0);
                    writer.Write(0);
                    writer.Write(0x7CEF);
                    writer.Write((int)configpos);
                    writer.Write(rlength);
                    writer.Write(Encoding.UTF8.GetBytes(projid.PadRight(0x10, '\0')));
                    writer.Write(Encoding.UTF8.GetBytes(firmware.PadRight(0x200 - 0x30, '\0')));
                }
                wf.Write(hdr, 0, hdr.Length);


            }
            using (MD5 md5 = MD5.Create())
            using (FileStream md5File = new FileStream(Path.Combine(Directory.GetParent(directory).FullName, "md5sum_pack.md5"), FileMode.Create, FileAccess.Write))
            {
                byte[] md5Hash = md5.ComputeHash(File.ReadAllBytes(outfilename));
                string md5String = BitConverter.ToString(md5Hash).Replace("-", "").ToLower();
                byte[] md5Bytes = Encoding.UTF8.GetBytes($"{md5String}  {Path.GetFileName(outfilename)}\n");
                md5File.Write(md5Bytes, 0, md5Bytes.Length);
            }
            Console.WriteLine("Done. Created " + outfilename);
            Environment.Exit(0);

        }*/
    }
}
