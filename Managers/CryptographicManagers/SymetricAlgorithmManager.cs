using Avalonia.Controls;
using Avalonia.Layout;
using JuliaCrypt.Managers.KeyManagers;
using JuliaCrypt.Misc;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JuliaCrypt.Managers.CryptographicManagers
{
    internal abstract class SymetricAlgorithmManager : CryptographicManager
    {
        protected abstract Func<SymmetricAlgorithm> ManagerCreator { get; }

        private SymmetricAlgorithm? _engine = null;
        protected SymmetricAlgorithm Engine
        {
            get
            { // lazy loading just in case
                _engine ??= ManagerCreator.Invoke();
                return _engine;
            }
            set => _engine ??= value;
        }

        protected override void Initialize()
        {
            base.Initialize();
            Engine = ManagerCreator.Invoke();
        }

        public override byte[] Encrypt(byte[] plaintext, KeyManager key)
        {
            byte[]? keyBytes = key.RequestKey(SelectedKeyBitSize);
            var IVRequestedLength = Engine.IV.Length;
            byte[]? IV = key.RequestIV(IVRequestedLength * 8);
            IV ??= Engine.IV;

            if (plaintext == null || plaintext.Length <= 0)
            { throw new ArgumentNullException($"Recieved no Plaintext ({Identifier} Encrypt)"); }

            if (keyBytes == null)
            { throw new ArgumentNullException($"Recieved no Key ({Identifier} Encrypt)"); }
            if (keyBytes.Length * 8 < SelectedKeyBitSize)
            { throw new ArgumentException($"Was looking for {SelectedKeyBitSize} bits of key, but only got {keyBytes.Length} bits ({Identifier} Encrypt)"); }
            Engine.Key = keyBytes;

            if (IV == null || IV.Length < IVRequestedLength)
            { throw new ArgumentNullException($"Recieved no IV ({Identifier} Encrypt)"); }
            Engine.IV = IV;

            byte[] ciphertext;

            ICryptoTransform encryptor = Engine.CreateEncryptor(Engine.Key, Engine.IV);
            plaintext = Misc.Padding.Pad(plaintext, BlockSize / 8, Engine.Padding);

            using (MemoryStream memStream = new())
            {
                using (CryptoStream cryptoStream = new(memStream, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter sWriter = new(cryptoStream))
                    {
                        sWriter.BaseStream.Write(plaintext, 0, plaintext.Length);
                    }
                }
                ciphertext = memStream.ToArray();
            }

            return ciphertext;
        }
        public override byte[] Decrypt(byte[] ciphertext, KeyManager key)
        {
            byte[]? keyBytes = key.RequestKey(SelectedKeyBitSize);
            var IVRequestedLength = Engine.IV.Length;
            byte[]? IV = key.RequestIV(IVRequestedLength * 8);
            IV ??= Engine.IV;

            if (ciphertext == null || ciphertext.Length <= 0)
            { throw new ArgumentNullException(nameof(ciphertext)); }

            if (keyBytes == null)
            { throw new ArgumentNullException(nameof(key)); }
            if (keyBytes.Length * 8 < SelectedKeyBitSize)
            { throw new ArgumentException($"Was looking for {SelectedKeyBitSize} bits of key, but only got {keyBytes.Length} bits (SymmetricAlgorithm Encrypt)"); }
            Engine.Key = keyBytes;

            if (IV == null || IV.Length <IVRequestedLength)
            { throw new Exception("Recieved no IV (SymmetricAlgorithm Encrypt)"); }
            Engine.IV = IV;

            byte[] plaintext;

            ICryptoTransform decryptor = Engine.CreateDecryptor(Engine.Key, Engine.IV);

            using (MemoryStream memStream = new())
            {
                using (CryptoStream cryptoStream = new(memStream, decryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter sWriter = new(cryptoStream))
                    {
                        sWriter.BaseStream.Write(ciphertext, 0, ciphertext.Length);
                    }
                }
                plaintext = memStream.ToArray();
            }

            plaintext = Misc.Padding.Unpad(plaintext, Engine.Padding);
            return plaintext;
        }

        public List<List<RadioButton>> CreateRadioButtons(VerticalAlignment verticalAlignment = VerticalAlignment.Center, HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left)
        {
            Panel subOptionPanel = App.MWInstance.EncryptionSubOptions;
            List<List<RadioButton>> res = [];

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
                    (arg) =>
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
                    (arg) =>
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
                    (CipherMode mode) => { Mode = mode; }, 
                    Conditional: (CipherMode mode) =>
                    {
                        var tmp = Engine.Mode;
                        try 
                        {
                            Engine.Mode = mode;
                            Engine.Mode = tmp;
                            return true;
                        }
                        catch (CryptographicException)
                        {
                            Engine.Mode = tmp;
                            return false;
                        }
                    }, verticalAlignment: verticalAlignment, horizontalAlignment: horizontalAlignment);
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
                    (PaddingMode padding) => { Padding = padding; }, 
                    verticalAlignment: verticalAlignment, horizontalAlignment: horizontalAlignment);
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
        { get => Utilities.LegalSizes(Engine.LegalBlockSizes); }

        public IEnumerable<int> LegalKeySizes
        { get => Utilities.LegalSizes(Engine.LegalKeySizes); }

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

        protected override dynamic SerializeHelper()
        {
            dynamic res = new System.Dynamic.ExpandoObject();
            res.KeySize = SelectedKeyBitSize;
            res.Mode = Mode;
            res.Padding = Padding;
            res.BlockSize = BlockSize;
            return res;
        }

        protected override void DeserializeHelper(dynamic serialized)
        {
            try
            { SelectedKeyBitSize = serialized.KeySize; }
            catch (RuntimeBinderException) { }
            try
            { Mode = serialized.Mode; }
            catch (RuntimeBinderException) { }
            try
            { Padding = serialized.Padding; }
            catch (RuntimeBinderException) { }
            try
            { BlockSize = serialized.BlockSize; }
            catch (RuntimeBinderException) { }
        }
    }
}
