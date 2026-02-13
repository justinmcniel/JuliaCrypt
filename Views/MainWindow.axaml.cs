using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Styling;
using JuliaCrypt.CryptographicManagers;
using JuliaCrypt.DataFlowManagers;
using JuliaCrypt.ViewModels;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Newtonsoft.Json;

namespace JuliaCrypt
{
    public partial class MainWindow : Window
    {
        //ImagePanel.IsVisible = !ImagePanel.IsVisible;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InputFileClickHandler(object? sender, RoutedEventArgs e) =>
            App.MWvmInstance.InputFile = FilePicker.ChooseInputFile();

        public void InvalidInputFileSelected() => FlyoutBase.ShowAttachedFlyout(InputFileButton);

        private void OutputFileClickHandler(object? sender, RoutedEventArgs e) =>
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
        }

        private void SaveSettings(object? sender, RoutedEventArgs e)
        {
            dynamic obj = new System.Dynamic.ExpandoObject();
            obj.EncryptionOptions = CryptographicManager.Serialize();

            Semaphore sem = new(0, 1);
            string serialized = "";
            if(!ThreadPool.QueueUserWorkItem((object? state) => 
                {
                    lock (obj)
                    {
                        serialized = JsonConvert.SerializeObject(obj);
                    }
                    sem.Release();
                }))
            {
                throw new Exception("Failed to queue serialization");
            }

            var saveFile = FilePicker.ChooseOutputFile(title: "Select Settings File");
            if (saveFile == null) { return; }
            
            sem.WaitOne();

            if (serialized == null || serialized.Length <= 0) { throw new Exception("Failed to Serialize"); }
            if (saveFile.Exists) { saveFile.Delete(); }
            using (var writeStream = saveFile.CreateText())
            {
                writeStream.Write(serialized);
            }
        }

        private void LoadSettings(object? sender, RoutedEventArgs e)
        {
            var settingsFile = FilePicker.ChooseInputFile(title: "Select Settings File");
            if (settingsFile == null || !settingsFile.Exists) {  return; }

            string serialized = File.ReadAllText(settingsFile.FullName);
            dynamic? obj = JsonConvert.DeserializeObject(serialized);
            

            if (obj == null) { return; } // Failed to Load

            CryptographicManager.Deserialize(obj.EncryptionOptions);
        }
    }
}