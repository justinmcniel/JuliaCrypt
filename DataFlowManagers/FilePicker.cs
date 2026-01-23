using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Avalonia.Platform.Storage;
using Avalonia.Controls;
using System.Diagnostics;

namespace JuliaCrypt.DataFlowManagers
{
    public class FilePicker
    {
        public static DirectoryInfo MostRecentDirectory { get; private set; } = new DirectoryInfo(Directory.GetCurrentDirectory());
        public static FileInfo? ChooseInputFile(IReadOnlyList<FilePickerFileType>? types = null, string title = "Select Input File")
        {
            var startFolderTask = App.MWInstance.StorageProvider.TryGetFolderFromPathAsync(MostRecentDirectory.FullName);
            var pickerTask = App.MWInstance.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
            {
                Title = title,
                FileTypeFilter = types,
                AllowMultiple = false,
                SuggestedStartLocation = startFolderTask.GetAwaiter().GetResult(),
            });

            var fileList = pickerTask.GetAwaiter().GetResult();
            if (fileList == null || fileList.Count == 0)
            { return null; }

            var fpath = fileList[0].TryGetLocalPath();
            if (fpath != null)
            {
                var res = new FileInfo(fpath);
                if (!res.Exists) //implicitly, Avalonia is already checking this since we said OpenOptions
                { return null; }
                MostRecentDirectory = res.Directory!;
                return res;
            }

            return null;
        }
        public static FileInfo? ChooseOutputFile(bool promptOvewrite = true, string title = "Select Output File")
        {
            var startFolderTask = App.MWInstance.StorageProvider.TryGetFolderFromPathAsync(MostRecentDirectory.FullName);
            var pickerTask = App.MWInstance.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
            {
                Title = title,
                ShowOverwritePrompt = promptOvewrite,
                SuggestedStartLocation = startFolderTask.GetAwaiter().GetResult(),
            });

            var file = pickerTask.GetAwaiter().GetResult();
            if (file == null)
            { return null; }

            var fpath = file.TryGetLocalPath();
            if (fpath != null)
            {
                var res = new FileInfo(fpath);
                MostRecentDirectory = res.Directory!;
                return res;
            }

            return null;
        }
    }
}
