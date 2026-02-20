using Avalonia.Controls;
using Avalonia.Dialogs.Internal;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JuliaCrypt.Misc
{
    public static class Utilities
    {
        public static Assembly? GetJuliaCryptAssembly() => Assembly.GetExecutingAssembly();
        public static IEnumerable<Type> GetSubTypes(Type type) =>
            GetJuliaCryptAssembly()?.GetTypes().Where(t => t.IsSubclassOf(type)) ?? [];

        private static RadioButton CreateRadioButton(string? groupName, object value, Action<object>? OnChanged,
            VerticalAlignment verticalAlignment, HorizontalAlignment horizontalAlignment)
        {
            RadioButton res = new()
            {
                GroupName = groupName,
                Content = value.ToString(),
                VerticalAlignment = verticalAlignment,
                HorizontalAlignment = horizontalAlignment,

            };

            EventHandler<RoutedEventArgs> onChange = (object? sender, RoutedEventArgs args) =>
            {
                if (sender is RadioButton button && button.IsChecked == true)
                {
                    OnChanged?.Invoke(value);
                }
                args.Handled = true;
            };
            res.IsCheckedChanged += onChange;
            return res;
        }

        public static List<RadioButton> CreateRadioButtonsFromEnum<TEnum>(Panel panel, Action<TEnum>? OnChanged, Func<TEnum, bool>? Conditional = null
        ,
            VerticalAlignment verticalAlignment = VerticalAlignment.Center, HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center) 
            where TEnum : struct, Enum
        {
            List<RadioButton> res = [];

            Conditional ??= (TEnum val) => { return true; };

            var groupName = Guid.NewGuid().ToString();

            foreach(TEnum value in Enum.GetValues(typeof(TEnum)).Cast<TEnum>())
            {
                if (!Conditional!.Invoke(value)) { continue; }

                var child = CreateRadioButton(groupName, value, 
                    (val) =>
                    {
                        if (val is TEnum eVal)
                        {
                            OnChanged?.Invoke(eVal);
                        }
                        else
                        {
                            throw new Exception($"Type Mismatch between {val.GetType().Name} (actual) and {typeof(TEnum).Name} (expected)");
                        }
                    }, 
                    verticalAlignment, horizontalAlignment);
                panel.Children.Add(child);
                res.Add(child);
            }

            return res;
        }

        public static List<RadioButton> CreateRadioButtonsFromEnumerable(Panel panel, IEnumerable<object> enumerable, Action<object> OnChanged,
            VerticalAlignment verticalAlignment = VerticalAlignment.Center, HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center)
        {
            List<RadioButton> res = [];

            var groupName = Guid.NewGuid().ToString();

            foreach (var value in enumerable)
            {
                var child = CreateRadioButton(groupName, value, OnChanged, verticalAlignment, horizontalAlignment);
                panel.Children.Add(child);
                res.Add(child);
            }

            return res;
        }

        public static IEnumerable<int> LegalSizes(KeySizes[] sizes)
        {
            List<int> res = new();
            foreach (var span in sizes)
            {
                for (var legalSize = span.MinSize; legalSize <= span.MaxSize; legalSize += span.SkipSize)
                {
                    if (!res.Contains(legalSize))
                    { res.Add(legalSize); }
                    if (span.SkipSize == 0)
                    { break; }
                }
            }
            return res;
        }

        public static bool LegalSize(KeySizes[] sizes, int size) => LegalSizes(sizes).Contains(size);
    }
}
