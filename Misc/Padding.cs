using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JuliaCrypt.Misc
{
    public class Padding
    {
        public static PaddingMode[] SupportedPaddings { get => [PaddingMode.None, PaddingMode.Zeros]; }
        public static byte[] Pad(byte[] buffer, int blockSizeBytes, PaddingMode mode)
        {
            int addedBytes = blockSizeBytes - (buffer.Length % blockSizeBytes);
            byte[] res = new byte[buffer.Length + addedBytes];
            for (int i = 0; i < buffer.Length; i++)
            { res[i] = buffer[i]; } //for some reason buffer.CopyTo(res) was ambiguous

            switch (mode)
            {
                case PaddingMode.ANSIX923:
                case PaddingMode.ISO10126:
                case PaddingMode.PKCS7:
                    throw new NotImplementedException($"{mode} is not implemented");
                case PaddingMode.Zeros:
                    Array.Fill<byte>(res, 0, buffer.Length, addedBytes);
                    return res;
                case PaddingMode.None:
                default:
                    return buffer;
            }
        }

        public static byte[] Unpad(byte[] buffer, PaddingMode mode)
        {
            switch (mode)
            {
                case PaddingMode.ANSIX923:
                case PaddingMode.ISO10126:
                case PaddingMode.PKCS7:
                    int x = 5; // to shut up it asking me to use a switch expression until I implement one of the paddings
                    throw new NotImplementedException($"{x}: {mode} is not implemented");
                case PaddingMode.Zeros:
                case PaddingMode.None:
                default:
                    return buffer;
            }
        }
    }
}
