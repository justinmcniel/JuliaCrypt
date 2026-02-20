using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using JuliaCrypt.DataFlowManagers;
using JuliaCrypt.ViewModels;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace JuliaCrypt.Managers.KeyManagers
{
    internal class KeyFileManager : KeyManager
    {
        private Stream? _readStream = null;
        private Stream? ReadStream 
        { 
            get 
            { 
                lock (this) 
                {  return _readStream ??= KeyFile?.OpenRead();  } 
            } 
        }
        public override void Dispose()
        {
            lock (this)
            { _readStream?.Dispose(); }
        }
        protected override string Identifier { get => "Load a Key from a File"; }
        protected override void Initialize() { }

        private FileInfo? _keyFile = null;
        private FileInfo? KeyFile 
        { 
            get => _keyFile;
            set
            {
                if (value != null && value.Exists)
                {
                    EventHandler OperationFinishedHandler = 
                        (object? sender, EventArgs args) => 
                        {
                            lock (this)
                            {
                                if (ReadStream != null)
                                { ReadStream!.Position = 0; }
                            }
                        };
                    lock (this)
                    {
                        _keyFile = value;
                        _readStream?.Dispose();
                        _readStream = null;
                        if (previousOperationFinishedHandler != null)
                        { App.MWInstance.OnOperationFinished -= previousOperationFinishedHandler; }
                    }
                    App.MWInstance.OnOperationFinished += OperationFinishedHandler;
                    previousOperationFinishedHandler = OperationFinishedHandler;
                }
            }
        }
        private EventHandler? previousOperationFinishedHandler;
        private Button? FileSelectorButton { get; set; } = null;

        public override bool ValidSettings { get => base.ValidSettings && KeyFile != null && KeyFile.Exists && KeyFile.Length > 0; }

        public override void OnSelected()
        {
            const string fileSelectorButtonDefaultText = "Select file to load key from.";
            FileSelectorButton = new()
            {
                FontWeight = FontWeight.Light,
                Foreground = new SolidColorBrush(Colors.WhiteSmoke),
                Background = new SolidColorBrush(Colors.SlateGray),
                Content = fileSelectorButtonDefaultText,
            };
            ToolTip.SetTip(FileSelectorButton, fileSelectorButtonDefaultText);
            EventHandler<RoutedEventArgs> onClick = (sender, args) =>
            {
                if (sender is Button button && button == FileSelectorButton)
                {
                    FileInfo? tmp = FilePicker.ChooseInputFile(title: "Select Key File");
                    if (tmp != null && tmp.Exists)
                    {
                        KeyFile = tmp;
                        FileSelectorButton.Content = $"Key File: {tmp.Name}";
                    }
                }
                args.Handled = true;
            };
            FileSelectorButton.Click += onClick;

            App.MWInstance.KeySubOptions.Children.Clear();
            App.MWInstance.KeySubOptions.Children.Add(FileSelectorButton!);
        }

        private byte[]? ReadFromStream(int bitsize)
        {
            if (KeyFile == null || !KeyFile.Exists || KeyFile.Length == 0)
            { return null; } //should I compare to bitsize?

            int bytesize = bitsize / 8;
            int offset = 0;

            byte[]? buffer = new byte[bytesize];
            int? read;
            lock (this)
            { read = ReadStream?.Read(buffer, offset, bytesize); }
            if (read == null || read < bytesize)
            {
                throw new Exception("Failed To Read enough data");
            }

            return buffer;
        }

        public override byte[]? RequestKey(uint bitsize) => ReadFromStream((int)bitsize);

        public override byte[]? RequestIV(int bitsize) => ReadFromStream(bitsize);

        protected override dynamic SerializeHelper()
        {
            dynamic res = new System.Dynamic.ExpandoObject();
            res.File = KeyFile?.FullName;
            return res;
        }

        protected override void DeserializeHelper(dynamic serialized)
        {
            try
            {
                KeyFile = new(serialized.File.ToString());

                if(FileSelectorButton != null)
                {
                    FileSelectorButton.Content = $"Key File: {KeyFile.Name}";
                }
            }
            catch (RuntimeBinderException) { }
        }
    }
}
