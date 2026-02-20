using JuliaCrypt.Managers.KeyManagers;
using JuliaCrypt.Misc;
using JuliaCrypt.SymetricAlgorithms.CryptoTransforms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JuliaCrypt.SymetricAlgorithms
{
    public class SymmetricAlgorithmBase<TTransform> : SymmetricAlgorithm, IDisposable where TTransform : CryptoTransformBase, new()
    {
        private int _blockSize = 128;
        public override int BlockSize 
        { 
            get => _blockSize; 
            set
            {
                var legalSizes = Utilities.LegalSizes(LegalBlockSizes);
                if (!legalSizes.Contains(value))
                { throw new ArgumentException($"Legal block sizes only include {string.Join(", ", legalSizes.ToArray())} not {value}", nameof(value)); }
                _blockSize = value;
            }
        }
        public override KeySizes[] LegalBlockSizes => _legalityChecker.LegalBlockSizes;

        private byte[] _iv = new byte[32];
        public override byte[] IV 
        { 
            get
            {
                if (_iv.Length < BlockSize / 8)
                {
                    var prev = _iv;
                    _iv = new byte[Math.Max(BlockSize / 8, prev.Length)]; // should I be taking the max here, I'm doing it to preserve IVs if they set the IV, change the block size down, check the IV, then change the block size back up
                    Array.Fill<byte>(_iv, 0);
                    prev.CopyTo(_iv, 0);
                }
                return _iv;
            }
            set
            {
                if (value == null)
                { throw new ArgumentException("IV cannot be null", nameof(value)); }
                _iv = value;
            }
        }

        private byte[] _key = new byte[32];
        public override byte[] Key
        {
            get => _key;
            set
            {
                var legalSizes = Utilities.LegalSizes(LegalKeySizes);
                if (!legalSizes.Contains(value.Length * 8))
                { throw new ArgumentException($"Legal key sizes only include {string.Join(", ", legalSizes.ToArray())} not {value.Length * 8}", nameof(value)); }
                _key = value;
            }
        }
        public override int KeySize 
        { 
            get => _key.Length * 8; 
            set
            {
                var legalSizes = Utilities.LegalSizes(LegalKeySizes);
                if (!legalSizes.Contains(value))
                { throw new ArgumentException($"Legal key sizes only include {string.Join(", ", legalSizes.ToArray())} not {value}", nameof(value)); }
                var prev = _key.Take(Math.Min(_key.Length, value / 8)).ToArray();
                _key = new byte[value / 8];
                Array.Fill<byte>(_key, 0);
                prev.CopyTo(_key, 0);
            }
        }
        public override KeySizes[] LegalKeySizes => _legalityChecker.LegalKeySizes;

        private CipherMode _mode = CipherMode.ECB;
        public override CipherMode Mode
        {
            get => _mode;
            set
            {
                if (!LegalModes.Contains(value))
                {  throw new CryptographicException($"Legal modes are found in {nameof(LegalModes)}, {value} was not present", nameof(value)); }
                _mode = value;
            }
        }
        public CipherMode[] LegalModes => _legalityChecker.LegalModes;

        public override int FeedbackSize 
        {
            get => throw new NotImplementedException(); 
            set => throw new NotImplementedException();
        }
        public KeySizes[] LegalFeedbackSizes => _legalityChecker.LegalFeedbackSizes;

        private PaddingMode _padding = PaddingMode.None;
        public override PaddingMode Padding
        {
            get => _padding;
            set
            {
                if (!LegalPaddings.Contains(value))
                { throw new ArgumentException($"Legal paddings are found in {nameof(LegalPaddings)}, {value} was not present", nameof(value)); }
                _padding = value;
            }
        }
        public PaddingMode[] LegalPaddings => _legalityChecker.LegalPaddings;

        private static CryptoTransformBase _legalityChecker = new TTransform();

        public SymmetricAlgorithmBase()
        {
            //
        }

        public static new SymmetricAlgorithm Create() => new SymmetricAlgorithmBase<TTransform>();

        public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[]? rgbIV)
        {
            var res = new TTransform()
            {
                Key = rgbKey,
                IV = rgbIV,
                Operation = CryptoTransformBase.Operations.Encrypt,
                BlockSize = BlockSize,
                Mode = Mode,
                //FeedbackSize = FeedbackSize,
                Padding = Padding,
            };
            return res;
        }

        public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[]? rgbIV)
        {
            var res = new TTransform()
            {
                Key = rgbKey,
                IV = rgbIV,
                Operation = CryptoTransformBase.Operations.Decrypt,
                BlockSize = BlockSize,
                Mode = Mode,
                //FeedbackSize = FeedbackSize,
                Padding = Padding,
            };
            return res;
        }

        public override void GenerateIV()
        {
            IV = new byte[BlockSize / 8];
            var iv = KeyManager.GetManager(KeyManager.SelectedManager)?.RequestIV(IV.Length)?.
                Take(IV.Length)?.ToArray(); //guard in case key manager returns extra
            if (iv == null) { return; } //or should we throw an exception
            
            for(var i = 0; i < IV.Length; i++)
            {
                try
                { IV[i] = iv[i]; }
                catch (IndexOutOfRangeException)
                { IV[i] = 0; }
            }
        }

        public override void GenerateKey()
        {
            Key = new byte[KeySize / 8];
            var key = KeyManager.GetManager(KeyManager.SelectedManager)?.RequestKey((uint)Key.Length)?.
                Take(Key.Length)?.ToArray(); //guard in case key manager returns extra
            if (key == null) { return; } //or should we throw an exception

            for (var i = 0; i < Key.Length; i++)
            {
                try
                { Key[i] = key[i]; }
                catch (IndexOutOfRangeException)
                { Key[i] = 0; }
            }
        }

        public new void Clear()
        {
            base.Clear();
        }

        public new void Dispose()
        {
            base.Dispose();
        }
    }
}
