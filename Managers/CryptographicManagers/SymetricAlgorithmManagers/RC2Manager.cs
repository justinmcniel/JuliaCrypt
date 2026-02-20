using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JuliaCrypt.Managers.CryptographicManagers.SymetricAlgorithmManagers
{
    internal class RC2Manager : SymetricAlgorithmManager
    {
        protected override string Identifier { get => "LEGACY: RC2"; }

        protected override Func<SymmetricAlgorithm> ManagerCreator { get => RC2.Create; }

        protected override bool CreateKeySizeRadioButtons { get => false; }

        public override void OnSelected()
        {
            App.MWInstance.EncryptionSubOptions.Children.Clear(); //Just in case something else is already there
            Panel subOptionPanel = App.MWInstance.EncryptionSubOptions;

            StackPanel keySizePanel = new()
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                Spacing = 15,
            };
            keySizePanel.Children.Add(new TextBlock()
            {
                Text = "Key Size (bits): ",
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = Avalonia.Media.FontWeight.SemiBold
            });

            ComboBox keySizeComboBox = new()
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                AutoScrollToSelectedItem = true,
            };
            EventHandler<SelectionChangedEventArgs> onChange = (sender, args) =>
            {
                if (sender == keySizeComboBox)
                {
                    foreach(var selected in args.AddedItems)
                    {
                        if (selected is int size)
                        {
                            SelectedKeyBitSize = (uint)size;
                        }
                    }
                }
            };
            keySizeComboBox.SelectionChanged += onChange;

            foreach(var key in LegalKeySizes)
            { 
                keySizeComboBox.Items.Add(key);
                if (key == SelectedKeyBitSize)
                {
                    keySizeComboBox.SelectedIndex = keySizeComboBox.Items.Count - 1;
                }
            }

            keySizePanel.Children.Add(keySizeComboBox);
            subOptionPanel.Children.Add(keySizePanel);

            _ = CreateRadioButtons();
        }
    }
}
