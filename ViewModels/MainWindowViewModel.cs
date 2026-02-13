using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace JuliaCrypt.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            if (App.MWvmInstance != null) throw new Exception("Reinstantiating MainWindowViewModel");
        }
        
        private FileInfo? _inputFile = null;
        public FileInfo? InputFile 
        {
            get => _inputFile;
            set
            {
                if (value != null && value!.Exists)
                {
                    _inputFile = value;
                    OnPropertyChanged();
                }
                else
                {
                    App.MWInstance.InvalidInputFileSelected();
                }
            }
        }
        
        private FileInfo? _outputFile = null;
        public FileInfo? OutputFile 
        {
            get => _outputFile;
            set
            {
                if (value != null)
                {
                    _outputFile = value;
                    OnPropertyChanged();
                }
                else
                {
                    Debug.WriteLine("Attempted to set OutputFile as NULL");
                }
            }
        }
    }
}
