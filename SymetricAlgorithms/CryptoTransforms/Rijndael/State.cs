using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JuliaCrypt.SymetricAlgorithms.CryptoTransforms.Rijndael
{
    internal class State
    {
        public State(ArraySegment<byte> state)
        {
            _state = [.. state];
            Nb = state.Count / 4;
            if(Nb != 4 && Nb != 6 && Nb != 8)
            { throw new CryptographicException($"Invalid Block Size{state.Count * 8}"); }
        }

        protected byte[] _state;

        public void SubBytes()
        {
            for (int i = 0; i < _state.Length; i++)
            {
                _state[i] = Utilities.SubByte(_state[i]);
            }
        }

        public void InvSubBytes()
        {
            for (int i = 0; i < _state.Length; i++)
            {
                _state[i] = Utilities.InvSubByte(_state[i]);
            }
        }

        public unsafe class Columns(byte* state, int Nb)
        {
            public byte this[int i, int j]
            {
                get => state[i*Nb + j];
                set => state[i*Nb + j] = value;
            }
        }

        public unsafe void MixColumns()
        {
            fixed (byte* pState = _state)
            {
                var cols = new Columns(pState, Nb);
                for (int j = 0; j < Nb; j++)
                { MixCol(cols, j); }
            }
        }

        public unsafe void InvMixColumns()
        {
            fixed (byte* pState = _state)
            {
                var cols = new Columns(pState, Nb);
                for (int j = 0; j < Nb; j++)
                { InvMixCol(cols, j); }
            }
        }

        private static void MixCol(Columns cols, int j)
        { //a is the input array
            byte[] ax1 = new byte[4];
            byte[] ax2 = new byte[4];
            byte[] ax3 = new byte[4];

            for (int i = 0; i < 4; i++)
            {
                ax1[i] = cols[i, j];
                ax2[i] = Utilities.GMul(ax1[i], 2);
                ax3[i] = Utilities.GMul(ax1[i], 3);
            }

            cols[0, j] = (byte)(ax2[0] ^ ax3[1] ^ ax1[2] ^ ax1[3]);
            cols[1, j] = (byte)(ax1[0] ^ ax2[1] ^ ax3[2] ^ ax1[3]);
            cols[2, j] = (byte)(ax1[0] ^ ax1[1] ^ ax2[2] ^ ax3[3]);
            cols[3, j] = (byte)(ax3[0] ^ ax1[1] ^ ax1[2] ^ ax2[3]);
        }

        private static void InvMixCol(Columns cols, int j)
        { //b is the input array
            byte[] bx11 = new byte[4];
            byte[] bx13 = new byte[4];
            byte[] bx09 = new byte[4];
            byte[] bx14 = new byte[4];

            for (int i = 0; i < 4; i++)
            {
                bx11[i] = Utilities.GMul(cols[i, j], 11);
                bx13[i] = Utilities.GMul(cols[i, j], 13);
                bx09[i] = Utilities.GMul(cols[i, j], 09);
                bx14[i] = Utilities.GMul(cols[i, j], 14);
            }

            cols[0, j] = (byte)(bx14[0] ^ bx11[1] ^ bx13[2] ^ bx09[3]);
            cols[1, j] = (byte)(bx09[0] ^ bx14[1] ^ bx11[2] ^ bx13[3]);
            cols[2, j] = (byte)(bx13[0] ^ bx09[1] ^ bx14[2] ^ bx11[3]);
            cols[3, j] = (byte)(bx11[0] ^ bx13[1] ^ bx09[2] ^ bx14[3]);
        }
        public int Nb { get; }
        public unsafe void AddRoundKey(Span<UInt64> key)
        {
            fixed (byte* p = _state)
            {
                UInt64* pState = (UInt64*)p;
                pState[0] ^= key[0];
                pState[1] ^= key[1];
                if (Nb > 4)
                { 
                    pState[2] ^= key[2]; 
                    if (Nb > 6)
                    { 
                        pState[3] ^= key[3]; 
                    }
                }
            }
        }

        private static readonly int[][] shiftOffsets = 
        [
            [0, 1, 2, 3], 
            [0, 1, 2, 3], 
            [0, 1, 3, 4]
        ];

        public void ShiftRows()
        {
            int[] offsets = shiftOffsets[Nb / 2 - 2];
            int offset;
            Span<byte> spanState = _state.AsSpan();
            Span<byte> row;
            Span<byte> tmp = (new byte[Nb]).AsSpan();

            //row 0 doesn't shift, skip it
            for (int r = 1; r < 4; r++)
            {
                row = spanState.Slice(r * Nb, Nb);
                row.CopyTo(tmp);
                offset = offsets[r];
                for (int c = 0; c < Nb; c++)
                {
                    row[c] = tmp[(c + offset) % Nb];
                }
            }
        }

        public void InvShiftRows()
        {
            int[] offsets = shiftOffsets[Nb / 2 - 2];
            int offset;
            Span<byte> spanState = _state.AsSpan();
            Span<byte> row;
            Span<byte> tmp = (new byte[Nb]).AsSpan();

            //row 0 doesn't shift, skip it
            for (int r = 1; r < 4; r++)
            {
                row = spanState.Slice(r * Nb, Nb);
                row.CopyTo(tmp);
                offset = Nb - offsets[r];
                for (int c = 0; c < Nb; c++)
                {
                    row[c] = tmp[(c + offset) % Nb];
                }
            }
        }

        public byte[] GetBytes() => _state;
    }
}
