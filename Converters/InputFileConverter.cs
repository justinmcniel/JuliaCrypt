using Avalonia.Controls;
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuliaCrypt.Converters
{
    public class FileToInputButtonConverter : FileButtonConverterBase
    {
        protected override string Prefix { get; } = "Process: ";
        protected override string Fallback { get; } = "Select file to be processed.";
    }
}
