using JuliaCrypt.Misc;
using JuliaCrypt.SymetricAlgorithms.CryptoTransforms.Rijndael;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JuliaCrypt.SymetricAlgorithms.CryptoTransforms
{
    public class RijndaelTransform : CryptoTransformBase
    {
        public RijndaelTransform() : base() 
        { 
            OnModeChanged += (object? sender, CipherMode newMode) =>
            {
                //setup for new mode, cleanup for the old mode, will the modes be in the base??
            };
            Nb = BlockSizeBytes / 4;
            Nk = Key.Length / 4;
            Nr = Rijndael.Utilities.Nr(Nk, Nb);
            NrM1 = Nr - 1;
            ExpandedKey = new(Key, Nb, Nr, Nk);
        }

        public override KeySizes[] LegalBlockSizes { get => [new KeySizes(128, 256, 64)]; }
        public override CipherMode[] LegalModes { get => [CipherMode.ECB]; }
        public override KeySizes[] LegalFeedbackSizes { get => throw new NotImplementedException(); }
        public override KeySizes[] LegalKeySizes { get => [new KeySizes(128, 256, 64)]; }

        private int Nr { get; }
        private int NrM1 { get; } // important to cache this so encryption and decryption take the same time, and don't involve an additional subtraction per block
        private int Nb { get; }
        private int Nk { get; }
        private Key ExpandedKey { get; }

        protected override byte[] EncryptBlock(ArraySegment<byte> block, bool final = false)
        {
            State state = new(block);

            // initial round
            state.AddRoundKey(ExpandedKey[0]);

            for (int i = 1; i < Nr; i++)
            { // intermediate rounds
                state.SubBytes();
                state.ShiftRows();
                state.MixColumns();
                state.AddRoundKey(ExpandedKey[i]);
            }

            // final round
            state.SubBytes();
            state.ShiftRows();
            state.AddRoundKey(ExpandedKey[Nr]);

            return state.GetBytes();
        }

        protected override byte[] DecryptBlock(ArraySegment<byte> block, bool final = false)
        {
            State state = new(block);

            // inverse final round
            state.AddRoundKey(ExpandedKey[Nr]);
            state.InvShiftRows();
            state.InvSubBytes();

            for (int i = NrM1; i > 0; i--)
            { // intermediate rounds
                state.AddRoundKey(ExpandedKey[i]);
                state.InvMixColumns();
                state.InvShiftRows();
                state.InvSubBytes();
            }

            // inverse initial round
            state.AddRoundKey(ExpandedKey[0]);

            return state.GetBytes();
        }
    }
}
