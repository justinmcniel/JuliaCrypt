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
    public class FileButtonConverterBase : IValueConverter
    {
        protected virtual string Prefix { get; } = "";
        protected virtual string Suffix { get; } = "";
        protected virtual string Fallback { get; } = "";
        protected virtual bool MustExist { get; } = true;
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is FileInfo inputFile)
            {
                string? res = null;
                if (inputFile != null && (!MustExist || inputFile.Exists))
                {
                    res = $"{Prefix}{inputFile.Name}{Suffix}";
                }
                return res ?? Fallback;
            }
            return Fallback;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return Fallback;
        }
    }
}
