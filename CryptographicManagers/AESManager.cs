using Avalonia.Controls;
using Avalonia.Layout;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuliaCrypt.CryptographicManagers
{
    internal class AESManager : CryptographicManager
    {
        protected override string Identifier { get => "AES (Advanced Encryption Standard)"; }

        protected enum KeySizes
        {
            AES_128,
            AES_192,
            AES_256,
        }

        protected static KeySizes _selectedKeySize { get;  set; } = KeySizes.AES_128;

        public override uint SelectedKeySize
        {
            get => _selectedKeySize switch
            {
                KeySizes.AES_128 => 128,
                KeySizes.AES_192 => 192,
                KeySizes.AES_256 => 256,
                _ => 256
            };
        }

        protected override void Initialize()
        {
            //
        }

        public override void OnSelected()
        {
            List<Control> SubOptionControls = new();

            StackPanel KeySize = new() 
            { 
                Orientation = Orientation.Horizontal, 
                Spacing = 10,
            };
            KeySize.Children.Add(new TextBlock() 
            { 
                Text = "Key Size: ",
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = Avalonia.Media.FontWeight.SemiBold
            });
            var keySizeButtons = Utilities.CreateRadioButtonsFromEnum(KeySize, typeof(KeySizes), 
                (KeySizes size) => { _selectedKeySize = size; });
            foreach(RadioButton button in keySizeButtons)
            {
                if(button.Content is string name)
                {
                    if(name == _selectedKeySize.ToString())
                    {
                        button.IsChecked = true;
                    }
                }
            }

            SubOptionControls.Add(KeySize);

            App.MWInstance.EncryptionSubOptions.Children.Clear(); //Just in case something else is already there
            App.MWInstance.EncryptionSubOptions.Children.AddRange(SubOptionControls);
        }

        public override void OnDeselected()
        {
            Debug.WriteLine("AES Deselected");
            App.MWInstance.EncryptionSubOptions.Children.Clear();
        }

        public override byte[] Encrypt(byte[] plaintext, KeyManager key)
        {
            throw new NotImplementedException();
        }
        public override byte[] Decrypt(byte[] ciphertext, KeyManager key)
        {
            throw new NotImplementedException();
        }
    }
}
