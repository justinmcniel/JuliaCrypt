using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using JuliaCrypt.CryptographicManagers;
using JuliaCrypt.DataFlowManagers;
using JuliaCrypt.ViewModels;
using System;
using System.Diagnostics;
using System.IO;

namespace JuliaCrypt
{
    public partial class MainWindow : Window
    {
        //ImagePanel.IsVisible = !ImagePanel.IsVisible;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InputFileClickHandler(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
            App.MWvmInstance.InputFile = FilePicker.ChooseInputFile();

        public void InvalidInputFileSelected() => FlyoutBase.ShowAttachedFlyout(InputFileButton);

        private void OutputFileClickHandler(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
            App.MWvmInstance.OutputFile = FilePicker.ChooseOutputFile(title: "Select Save File.");

        private void EncryptionFamilySelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            foreach(var added in e.AddedItems)
            {
                if (added is ComboBoxItem item)
                {
                    if(item.Content is string selected)
                    {
                        Debug.WriteLine($"Selected {selected}");
                        CryptographicManager.GetManager(selected)?.OnSelected();
                    }
                }
            }
        }

        private void KeyFamilySelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            foreach(var added in e.AddedItems)
            {
                if (added is ComboBoxItem item)
                {
                    if(item.Content is string selected)
                    {
                        Debug.WriteLine($"Selected {selected}");
                        KeyManager.GetManager(selected)?.OnSelected();
                    }
                }
            }

            foreach(var added in e.RemovedItems)
            {
                if (added is ComboBoxItem item)
                {
                    if(item.Content is string deselected)
                    {
                        Debug.WriteLine($"Deselected {deselected}");
                        KeyManager.GetManager(deselected)?.OnDeselected();
                    }
                }
            }
        }
    }
}