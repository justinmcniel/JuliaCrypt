using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuliaCrypt.Converters
{
    public class FileToOutputButtonConverter : FileButtonConverterBase
    {
        protected override string Prefix { get; } = "Save to: ";
        protected override string Fallback { get; } = "Select Save Destination";
        protected override bool MustExist => false;
    }
}
