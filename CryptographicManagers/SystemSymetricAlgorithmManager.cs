using Avalonia.Controls;
using Avalonia.Layout;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JuliaCrypt.CryptographicManagers
{
    internal abstract class SystemSymetricAlgorithmManager : CryptographicManager
    {
        // serialize needs to do key size, mode, and padding
        protected abstract Func<SymmetricAlgorithm> ManagerCreator { get; }

        private SymmetricAlgorithm? _engine = null;
        protected SymmetricAlgorithm Engine
        {
            get
            {
                if (_engine == null)
                { // lazy loading just in case
                    _engine = ManagerCreator.Invoke();
                }
                return _engine;
            }
            set => _engine ??= value;
        }

        protected new void Initialize()
        {
            base.Initialize();
            Engine = ManagerCreator.Invoke();
        }

        public override byte[] Encrypt(byte[] plaintext, KeyManager key)
        {
            byte[] keyBytes = key.RequestKey(SelectedKeyBitSize);
            byte[]? IV = key.ShouldRequestIV ? key.RequestIV(Engine.IV.Length) : null;
            IV ??= Engine.IV;

            if (plaintext == null || plaintext.Length <= 0)
            { throw new ArgumentNullException($"Recieved no Plaintext ({Identifier} Encrypt)"); }

            if (keyBytes == null)
            { throw new ArgumentNullException($"Recieved no Key ({Identifier} Encrypt)"); }
            if (keyBytes.Length * 8 < SelectedKeyBitSize)
            { throw new ArgumentException($"Was looking for {SelectedKeyBitSize} bits of key, but only got {keyBytes.Length} bits ({Identifier} Encrypt)"); }
            Engine.Key = keyBytes;

            if (IV == null || IV.Length <= 0)
            { throw new ArgumentNullException($"Recieved no IV ({Identifier} Encrypt)"); }
            Engine.IV = IV;

            byte[] ciphertext;

            ICryptoTransform encryptor = Engine.CreateEncryptor(Engine.Key, Engine.IV);

            using (MemoryStream memStream = new())
            {
                using (CryptoStream cryptoStream = new(memStream, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter sWriter = new(cryptoStream))
                    {
                        sWriter.Write(plaintext);
                    }
                }
                ciphertext = memStream.ToArray();
            }

            return ciphertext;
        }
        public override byte[] Decrypt(byte[] ciphertext, KeyManager key)
        {
            byte[] keyBytes = key.RequestKey(SelectedKeyBitSize);
            byte[]? IV = key.ShouldRequestIV ? key.RequestIV(Engine.IV.Length) : null;
            IV ??= Engine.IV;

            if (ciphertext == null || ciphertext.Length <= 0)
            { throw new ArgumentNullException("Recieved no Plaintext (SymmetricAlgorithm Encrypt)"); }

            if (keyBytes == null)
            { throw new ArgumentNullException("Recieved no Key (SymmetricAlgorithm Encrypt)"); }
            if (keyBytes.Length * 8 < SelectedKeyBitSize)
            { throw new ArgumentException($"Was looking for {SelectedKeyBitSize} bits of key, but only got {keyBytes.Length} bits (SymmetricAlgorithm Encrypt)"); }
            Engine.Key = keyBytes;

            if (IV == null || IV.Length <= 0)
            { throw new ArgumentNullException("Recieved no IV (SymmetricAlgorithm Encrypt)"); }
            Engine.IV = IV;

            byte[] plaintext;

            ICryptoTransform decryptor = Engine.CreateDecryptor(Engine.Key, Engine.IV);

            using (MemoryStream memStream = new())
            {
                using (CryptoStream cryptoStream = new(memStream, decryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter sWriter = new(cryptoStream))
                    {
                        sWriter.Write(ciphertext);
                    }
                }
                plaintext = memStream.ToArray();
            }

            return plaintext;
        }

        public List<List<RadioButton>> CreateRadioButtons(VerticalAlignment verticalAlignment = VerticalAlignment.Center, HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left)
        {
            Panel subOptionPanel = App.MWInstance.EncryptionSubOptions;
            List<List<RadioButton>> res = new();

            if (CreateBlockSizeRadioButtons)
            {
                StackPanel blockSizePanel = new()
                {
                    Orientation = Orientation.Horizontal,
                    VerticalAlignment = verticalAlignment,
                    HorizontalAlignment = horizontalAlignment,
                    Spacing = 15,
                };
                subOptionPanel.Children.Add(blockSizePanel);
                blockSizePanel.Children.Add(new TextBlock()
                {
                    Text = "Block Size: ",
                    VerticalAlignment = VerticalAlignment.Center,
                    FontWeight = Avalonia.Media.FontWeight.SemiBold
                });

                List<object> blockSizes = [.. LegalBlockSizes];
                var blockSizeButtons = Utilities.CreateRadioButtonsFromEnumerable(blockSizePanel, blockSizes, 
                    (object arg) =>
                    {
                        if (arg is int blockSize)
                        {
                            BlockSize = blockSize;
                        }
                        else
                        {
                            throw new Exception($"Type Mismatch between {arg.GetType().Name} (actual) and int (expected)");
                        }
                    }, verticalAlignment, horizontalAlignment);
                blockSizeButtons.Where(b => b.Content?.ToString() == BlockSize.ToString()).First().IsChecked = true;
                res.Add(blockSizeButtons);
            }

            if (CreateKeySizeRadioButtons)
            {
                StackPanel keySizePanel = new()
                {
                    Orientation = Orientation.Horizontal,
                    VerticalAlignment = verticalAlignment,
                    HorizontalAlignment = horizontalAlignment,
                    Spacing = 15,
                };
                subOptionPanel.Children.Add(keySizePanel);
                keySizePanel.Children.Add(new TextBlock()
                {
                    Text = "Key Size (bits): ",
                    VerticalAlignment = VerticalAlignment.Center,
                    FontWeight = Avalonia.Media.FontWeight.SemiBold
                });
                List<object> keySizes = [.. LegalKeySizes];
                var keySizeButtons = Utilities.CreateRadioButtonsFromEnumerable(keySizePanel, keySizes,
                    (object arg) =>
                    {
                        if (arg is int keySize)
                        {
                            SelectedKeyBitSize = (uint)keySize;
                        }
                        else
                        {
                            throw new Exception($"Type Mismatch between {arg.GetType().Name} (actual) and int (expected)");
                        }
                    }, verticalAlignment, horizontalAlignment);
                keySizeButtons.Where(b => b.Content?.ToString() == SelectedKeyBitSize.ToString()).First().IsChecked = true;
                res.Add(keySizeButtons);
            }

            if (CreateModeRadioButtons)
            {
                StackPanel modePanel = new()
                {
                    Orientation = Orientation.Horizontal,
                    VerticalAlignment = verticalAlignment,
                    HorizontalAlignment = horizontalAlignment,
                    Spacing = 15,
                };
                subOptionPanel.Children.Add(modePanel);
                modePanel.Children.Add(new TextBlock()
                {
                    Text = "Mode: ",
                    VerticalAlignment = VerticalAlignment.Center,
                    FontWeight = Avalonia.Media.FontWeight.SemiBold
                });
                var modeButtons = Utilities.CreateRadioButtonsFromEnum(modePanel,
                    (CipherMode arg) =>
                    {
                        if (arg is CipherMode mode)
                        {
                            Mode = mode;
                        }
                        else
                        {
                            throw new Exception($"Type Mismatch between {arg.GetType().Name} (actual) and CipherMode (expected)");
                        }
                    }, verticalAlignment, horizontalAlignment);
                modeButtons.Where(b => b.Content?.ToString() == Mode.ToString()).First().IsChecked = true;
                res.Add(modeButtons);
            }

            if (CreatePaddingRadioButtons)
            {
                StackPanel paddingPanel = new()
                {
                    Orientation = Orientation.Horizontal,
                    VerticalAlignment = verticalAlignment,
                    HorizontalAlignment = horizontalAlignment,
                    Spacing = 15,
                };
                subOptionPanel.Children.Add(paddingPanel);
                paddingPanel.Children.Add(new TextBlock()
                {
                    Text = "Padding: ",
                    VerticalAlignment = VerticalAlignment.Center,
                    FontWeight = Avalonia.Media.FontWeight.SemiBold
                });
                var paddingButtons = Utilities.CreateRadioButtonsFromEnum(paddingPanel,
                    (PaddingMode arg) =>
                    {
                        if (arg is PaddingMode padding)
                        {
                            Padding = padding;
                        }
                        else
                        {
                            throw new Exception($"Type Mismatch between {arg.GetType().Name} (actual) and PaddingMode (expected)");
                        }
                    }, verticalAlignment, horizontalAlignment);
                paddingButtons.Where(b => b.Content?.ToString() == Padding.ToString()).First().IsChecked = true;
                res.Add(paddingButtons);
            }

            return res;
        }

        protected virtual bool CreateBlockSizeRadioButtons { get => LegalBlockSizes.Count() > 1; }
        public int BlockSize { get => Engine.BlockSize; protected set => Engine.BlockSize = value; }
        public int FeedbackSize { get => Engine.FeedbackSize; }
        protected virtual bool CreateKeySizeRadioButtons { get => LegalKeySizes.Count() > 1; }
        public override uint SelectedKeyBitSize { get => (uint)Engine.KeySize; protected set => Engine.KeySize = (int)value; }
        public IEnumerable<int> LegalBlockSizes 
        {
            get
            {
                List<int> res = new();
                foreach (var sizeRange in Engine.LegalBlockSizes)
                {
                    if (sizeRange.SkipSize == 0)
                    { res.Add(sizeRange.MinSize); }
                    else
                    {
                        for (int size = sizeRange.MinSize; size <= sizeRange.MaxSize; size += sizeRange.SkipSize)
                        { res.Add((int)size); }
                    }
                }
                return res;
            }
        }
        public IEnumerable<int> LegalKeySizes
        {
            get
            {
                List<int> res = new();
                foreach(var sizeRange in Engine.LegalKeySizes)
                {
                    if (sizeRange.SkipSize == 0)
                    { res.Add(sizeRange.MaxSize); }
                    else
                    {
                        for (int size = sizeRange.MinSize; size <= sizeRange.MaxSize; size += sizeRange.SkipSize)
                        { res.Add((int)size); }
                    }
                }
                return res;
            }
        }
        protected override long FamilyBiggestKeyBitSize 
        { 
            get
            {
                long res = 0;
                foreach(var size in LegalKeySizes)
                {
                    res = Math.Max(res, size);
                }
                return res;
            }
        }

        protected virtual bool CreateModeRadioButtons { get => true; }
        public CipherMode Mode { get => Engine.Mode; protected set => Engine.Mode = value; }
        protected virtual bool CreatePaddingRadioButtons { get => true; }
        public PaddingMode Padding { get => Engine.Padding; protected set => Engine.Padding = value; }

        public override void OnSelected()
        {
            base.OnSelected();
            _ = CreateRadioButtons();
        }

        public override void Dispose()
        {
            Engine.Clear();
            Engine.Dispose();
        }
    }
}
