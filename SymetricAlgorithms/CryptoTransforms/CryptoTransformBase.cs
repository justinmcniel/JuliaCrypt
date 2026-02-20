using JuliaCrypt.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JuliaCrypt.SymetricAlgorithms.CryptoTransforms
{
    public abstract class CryptoTransformBase : ICryptoTransform
    {
        private bool operationsBegun = false;
        private int _blockSize;
        public int BlockSize
        {
            get => _blockSize;
            set
            {
                if (operationsBegun)
                { throw new InvalidOperationException("Can only set the block size before operations begin"); }
                var legalSizes = Utilities.LegalSizes(LegalBlockSizes);
                if (!legalSizes.Contains(value))
                { throw new ArgumentException($"Legal block sizes only include {string.Join(", ", legalSizes.ToArray())} not {value}", nameof(value)); }
                _blockSize = value;
                BlockSizeBytes = value / 8;
                OnBlockSizeChanged?.Invoke(this, value);
            }
        }
        protected int BlockSizeBytes { get; set; }
        public abstract KeySizes[] LegalBlockSizes { get; }
        protected event EventHandler<int>? OnBlockSizeChanged;

        private byte[]? _iv = null;
        public byte[]? IV 
        { 
            get => _iv; 
            set
            {
                if (value == null)
                { _iv = null; }
                else
                {
                    var legalSizes = Utilities.LegalSizes(LegalBlockSizes);
                    if (!legalSizes.Contains(value.Length * 8))
                    { throw new ArgumentException($"Legal IV sizes only include {string.Join(", ", legalSizes.ToArray())} not {value.Length * 8}", nameof(value)); }
                    _iv = value;
                }
            }
        }

        private CipherMode _mode = CipherMode.ECB;
        public CipherMode Mode
        {
            get => _mode;
            set
            {
                if (operationsBegun)
                { throw new InvalidOperationException("Can only set the mode before operations begin"); }
                if (!LegalModes.Contains(value))
                { throw new ArgumentException($"Legal Modes only include {string.Join(", ", LegalModes)} not {value}", nameof(value)); }
                _mode = value;
                OnModeChanged?.Invoke(this, value);
            }
        }
        public abstract CipherMode[] LegalModes { get; }
        protected event EventHandler<CipherMode>? OnModeChanged;

        public int FeedbackSize
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
        public abstract KeySizes[] LegalFeedbackSizes { get; }
        protected event EventHandler<int>? OnFeedbackSizeChanged;

        private PaddingMode _padding = PaddingMode.None;
        public PaddingMode Padding
        {
            get => _padding;
            set
            {
                if (operationsBegun)
                { throw new InvalidOperationException("Can only set the padding before operations begin"); }
                if (!LegalPaddings.Contains(value))
                { throw new ArgumentException($"Legal Paddings only include {string.Join(", ", LegalPaddings)} not {value}", nameof(value)); }
                _padding = value;
                OnPaddingChanged?.Invoke(this, value);
            }
        }
        public PaddingMode[] LegalPaddings => Misc.Padding.SupportedPaddings;

        protected event EventHandler<PaddingMode>? OnPaddingChanged;

        private byte[] _key;
        public byte[] Key 
        { 
            get => _key; 
            set
            {
                if (operationsBegun)
                { throw new InvalidOperationException("Can only set the key before operations begin"); }
                var legalSizes = Utilities.LegalSizes(LegalKeySizes);
                if (!legalSizes.Contains(value.Length * 8))
                { throw new ArgumentException($"Legal key sizes only include {string.Join(", ", legalSizes.ToArray())} not {value.Length * 8}", nameof(value)); }
                _key = value;
            }
        }
        public abstract KeySizes[] LegalKeySizes { get; }

        public bool CanReuseTransform { get => Mode == CipherMode.ECB; }

        public bool CanTransformMultipleBlocks { get => true; }

        public int InputBlockSize { get => BlockSizeBytes; }

        public int OutputBlockSize { get => BlockSizeBytes; }

        private Operations op;
        public Operations Operation
        {
            get => op;
            set
            {
                if (operationsBegun)
                { throw new InvalidOperationException("Can only set the operation before operations begin"); }
                op = value;
            }
        }
        public enum Operations
        {
            Encrypt,
            Decrypt,
        }

        public CryptoTransformBase() 
        {
            _key = new byte[Utilities.LegalSizes(LegalKeySizes).First()];
            _blockSize = Utilities.LegalSizes(LegalBlockSizes).First();
        }

        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            operationsBegun = true;
            int res = 0;
            byte[] transformed;
            int decryptInputOffset = inputOffset + inputCount - BlockSizeBytes; // we need to calculate these even for encryption to try to reduce side chanel attacks
            int decryptOutputOffset = outputOffset + inputCount - BlockSizeBytes;
            while (inputCount >= BlockSizeBytes)
            {
                switch (op)
                {
                    case Operations.Encrypt:
                        transformed = EncryptBlockHelper(new ArraySegment<byte>(inputBuffer, inputOffset, BlockSizeBytes));
                        transformed.CopyTo(outputBuffer, outputOffset);
                        break;
                    case Operations.Decrypt:
                        transformed = DecryptBlockHelper(new ArraySegment<byte>(inputBuffer, decryptInputOffset, BlockSizeBytes));
                        transformed.CopyTo(outputBuffer, decryptOutputOffset);
                        break;
                    default: throw new InvalidOperationException($"Unrecognized mode of operation: {op}");
                }

                inputOffset += BlockSizeBytes;
                decryptInputOffset -= BlockSizeBytes;

                outputOffset += transformed.Length;
                decryptOutputOffset -= transformed.Length;

                inputCount -= BlockSizeBytes;
                res = res + transformed.Length;
            }

            if (inputCount < BlockSizeBytes && inputCount > 0)
            { //Unable to process final block and padding has already been performed, just copy it
                inputBuffer.AsSpan(inputOffset, inputCount).CopyTo(outputBuffer.AsSpan(outputOffset));
            }

            return res;
        }

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            operationsBegun = true;
            List<byte> res = [];
            int decryptInputOffset = inputOffset + inputCount - BlockSizeBytes; // we need to calculate this even for encryption to try to reduce side chanel attacks
            while (inputCount >= BlockSizeBytes)
            {
                switch (op)
                {
                    case Operations.Encrypt:
                        res.AddRange(EncryptBlockHelper(new ArraySegment<byte>(inputBuffer, inputOffset, BlockSizeBytes), final: true));
                        break;
                    case Operations.Decrypt:
                        res.AddRange(DecryptBlockHelper(new ArraySegment<byte>(inputBuffer, decryptInputOffset, BlockSizeBytes), final: true));
                        break;
                    default: throw new InvalidOperationException($"Unrecognized mode of operation: {op}");
                }

                inputOffset += BlockSizeBytes;
                decryptInputOffset -= BlockSizeBytes;

                inputCount -= BlockSizeBytes;
            }

            if (inputCount < BlockSizeBytes && inputCount > 0)
            { //Unable to process final block and padding has already been performed, just copy it
                res.AddRange(inputBuffer.AsSpan(inputOffset, inputCount));
            }

            return [.. res];
        }

        public virtual void Dispose() { }

        private byte[] EncryptBlockHelper(ArraySegment<byte> block, bool final = false)
        {
            if (block.Count != BlockSizeBytes)
            { throw new Exception($"Block Sizes Do not Match Got: {block.Count} Excpected: {BlockSizeBytes}"); }
            return EncryptBlock(block, final: final);
        }

        private byte[] DecryptBlockHelper(ArraySegment<byte> block, bool final = false)
        {
            if (block.Count != BlockSizeBytes)
            { throw new Exception($"Block Sizes Do not Match Got: {block.Count} Excpected: {BlockSizeBytes}"); }
            return DecryptBlock(block, final: final);
        }

        protected abstract byte[] EncryptBlock(ArraySegment<byte> block, bool final = false);
        protected abstract byte[] DecryptBlock(ArraySegment<byte> block, bool final = false);
    }
}
