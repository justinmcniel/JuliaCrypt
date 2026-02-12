using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace JuliaCrypt
{
    public static class Utilities
    {
        public static Assembly? GetJuliaCryptAssembly() => Assembly.GetExecutingAssembly();
        public static IEnumerable<Type> GetSubTypes(Type type) =>
            GetJuliaCryptAssembly()?.GetTypes().Where(t => t.IsSubclassOf(type)) ?? [];

        public static List<RadioButton> CreateRadioButtonsFromEnum<TEnum>(Panel parent, Type EnumType, Action<TEnum>? OnChanged,
            VerticalAlignment verticalAlignment = VerticalAlignment.Center, HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center) 
            where TEnum : struct, Enum
        {
            List<RadioButton> res = new();

            if (typeof(TEnum) != EnumType)
            {
                throw new InvalidCastException($"Tried to create Radio Buttons for {EnumType.Name} with a call back taking {typeof(TEnum).Name}");
            }

            var groupName = Guid.NewGuid().ToString();

            foreach(TEnum value in Enum.GetValues(EnumType).Cast<TEnum>())
            {
                RadioButton child = new()
                {
                    GroupName = groupName,
                    Content = value.ToString(),
                    VerticalAlignment = verticalAlignment,
                    HorizontalAlignment = horizontalAlignment,

                };
                
                EventHandler<RoutedEventArgs> onChange = (object? sender, RoutedEventArgs args) =>
                {
                    if (sender is RadioButton button)
                    {
                        if (button.IsChecked == true)
                        {
                            OnChanged?.Invoke(value);
                        }
                    }
                    args.Handled = true;
                };
                child.IsCheckedChanged += onChange;
                parent.Children.Add(child);
                res.Add(child);
            }

            return res;
        }
    }
}
