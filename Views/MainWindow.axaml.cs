using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Styling;
using JuliaCrypt.DataFlowManagers;
using JuliaCrypt.ViewModels;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using Microsoft.CSharp.RuntimeBinder;
using JuliaCrypt.Managers.CryptographicManagers;
using JuliaCrypt.Managers.KeyManagers;

namespace JuliaCrypt
{
    public partial class MainWindow : Window
    {
        //ImagePanel.IsVisible = !ImagePanel.IsVisible;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InputFileClickHandler(object? sender, RoutedEventArgs args) =>
            App.MWvmInstance.InputFile = FilePicker.ChooseInputFile();

        public void InvalidInputFileSelected() => FlyoutBase.ShowAttachedFlyout(InputFileButton);

        private void OutputFileClickHandler(object? sender, RoutedEventArgs args) =>
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

        private void SaveSettings(object? sender, RoutedEventArgs args)
        {
            dynamic obj = new System.Dynamic.ExpandoObject();
            obj.EncryptionOptions = CryptographicManager.Serialize();
            obj.KeyOptions = KeyManager.Serialize();

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

        private void LoadSettings(object? sender, RoutedEventArgs args)
        {
            var settingsFile = FilePicker.ChooseInputFile(title: "Select Settings File");
            if (settingsFile == null || !settingsFile.Exists) {  return; }

            string serialized = File.ReadAllText(settingsFile.FullName);
            dynamic? obj = JsonConvert.DeserializeObject(serialized);
            

            if (obj == null) { return; } // Failed to Load

            try
            { CryptographicManager.Deserialize(obj.EncryptionOptions); }
            catch (RuntimeBinderException) { }
            try
            { KeyManager.Deserialize(obj.KeyOptions); }
            catch (RuntimeBinderException) { }
        }

        private bool ReadyToProcess()
        {
            bool? tmp;
            tmp= CryptographicManager.GetManager(CryptographicManager.SelectedManager)?.ValidSettings;
            bool validCrypto = tmp != null && (bool)tmp;

            tmp = KeyManager.GetManager(KeyManager.SelectedManager)?.ValidSettings;
            bool validKey = tmp != null && (bool)tmp;

            tmp = App.MWvmInstance.InputFile?.Exists;
            bool validInput = tmp != null && (bool)tmp && App.MWvmInstance.InputFile?.Length > 0;

            bool validOutput = App.MWvmInstance.OutputFile != null;

            return validCrypto && validKey && validInput && validOutput;
        }

        public event EventHandler? OnOperationFinished;

        private void ProcessEncrypt(object? sender, RoutedEventArgs args)
        {
            if (!ReadyToProcess())
            { 
                FlyoutBase.ShowAttachedFlyout(EncryptAndEncode);
                return;
            }
                
            CryptographicManager cryptoManager =
                CryptographicManager.GetManager(CryptographicManager.SelectedManager)
                ?? throw new NullReferenceException("Failed to get active Cryptographic Manager");

            KeyManager keyManager =
                KeyManager.GetManager(KeyManager.SelectedManager)
                ?? throw new NullReferenceException("Failed to get active Key Manager");

            if (!ThreadPool.QueueUserWorkItem((object? poolState) =>
            {

                byte[] state = File.ReadAllBytes(App.MWvmInstance.InputFile!.FullName);

                state = cryptoManager.Encrypt(state, keyManager);

                File.WriteAllBytes(App.MWvmInstance.OutputFile!.FullName, state);
                OnOperationFinished?.Invoke(this, EventArgs.Empty);
            }))
            { throw new Exception("Failed to Queue Encryption and Encoding"); }
        }

        private void ProcessDecrypt(object? sender, RoutedEventArgs args)
        {
            if (!ReadyToProcess())
            { 
                FlyoutBase.ShowAttachedFlyout(DecryptAndDecode);
                return;
            }
                
            CryptographicManager cryptoManager =
                CryptographicManager.GetManager(CryptographicManager.SelectedManager)
                ?? throw new NullReferenceException("Failed to get active Cryptographic Manager");

            KeyManager keyManager =
                KeyManager.GetManager(KeyManager.SelectedManager)
                ?? throw new NullReferenceException("Failed to get active Key Manager");

            if (!ThreadPool.QueueUserWorkItem((object? poolState) =>
            {

                byte[] state = File.ReadAllBytes(App.MWvmInstance.InputFile!.FullName);

                state = cryptoManager.Decrypt(state, keyManager);

                File.WriteAllBytes(App.MWvmInstance.OutputFile!.FullName, state);
                OnOperationFinished?.Invoke(this, EventArgs.Empty);
            }))
            { throw new Exception("Failed to Queue Decryption and Decoding"); }
        }
    }
}