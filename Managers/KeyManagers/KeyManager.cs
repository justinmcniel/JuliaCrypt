using Avalonia.Controls;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JuliaCrypt.Misc;

namespace JuliaCrypt.Managers.KeyManagers
{
    public abstract class KeyManager : IDisposable
    {
        private static readonly Dictionary<string, Type> _keyManagerTypes = [];
        public static IEnumerable<Type> KeyManagerTypes
        { get => _keyManagerTypes.Values; }

        private static readonly Dictionary<Type, KeyManager> _keyManagers = [];
        public static IEnumerable<KeyManager> KeyManagers
        { get => _keyManagers.Values; }
        public static IEnumerable<string> KeyManagerNames
        { get => _keyManagerTypes.Keys; }

        public static string SelectedManager { get => ((ComboBoxItem)App.MWInstance.KeyFamilySelector.SelectedItem!).Content!.ToString()!; }

        public static void InitializeManagers()
        {
            var subtypes = Utilities.GetSubTypes(typeof(KeyManager));

            foreach(Type managerType in subtypes)
            {
                var manager = GetManagerInstance(managerType);
                if (manager != null)
                {
                    _keyManagerTypes.Add(manager.Identifier, managerType);
                    _keyManagers.Add(managerType, manager);
                }
            }

            _ = Parallel.ForEach(KeyManagers, manager =>
            { manager.Initialize(); });

            var defaultManager = GetManager(typeof(KeyFileManager));
            var defaultManagerID = defaultManager?.Identifier;
            foreach (var name in KeyManagerNames)
            {
                ComboBoxItem comboBoxItem = new()
                { Content = name, };
                App.MWInstance.KeyFamilySelector.Items.Add(comboBoxItem);
                if (defaultManagerID == name)
                { 
                    App.MWInstance.KeyFamilySelector.SelectedItem = comboBoxItem;
                    defaultManager?.OnSelected();
                }
            }
        }

        private static KeyManager? GetManagerInstance(Type managerType)
        {
            try
            {
                var instance = Activator.CreateInstance(managerType);
                if (instance is KeyManager manager)
                {
                    return manager;
                }
            }
            catch (MissingMethodException)
            { return null; } //is an abstract class
            return null;
        }

        public static KeyManager? GetManager(Type managerType)
        {
            try { return _keyManagers[managerType]; }
            catch (KeyNotFoundException) { return null; }
        }

        public static KeyManager? GetManager(string identifier)
        {
            try { return GetManager(_keyManagerTypes[identifier]); }
            catch (KeyNotFoundException) { return null; }
        }

        public virtual bool ValidSettings { get => GetManager(SelectedManager) != null; }

        public abstract void Dispose();
        protected abstract string Identifier { get; }
        protected abstract void Initialize();
        public abstract void OnSelected();
        public abstract byte[]? RequestKey(uint bitsize);
        public abstract byte[]? RequestIV(int bitsize);

        public static dynamic Serialize()
        {
            dynamic res = new System.Dynamic.ExpandoObject();

            var selectedManager = SelectedManager;
            var activeManager = GetManager(selectedManager);
            if (activeManager != null && activeManager.GetType() != typeof(KeyManager))
            {
                res.ID = activeManager.Identifier;
                res.FamilyOptions = activeManager?.SerializeHelper();
            }
            else
            {
                res.ID = selectedManager;
            }
            return res;
        }

        protected abstract dynamic SerializeHelper();

        public static void Deserialize(dynamic serialized)
        {
            try
            {
                KeyManager activeManager = GetManager(serialized.ID.ToString()!);
                if (activeManager != null)
                {
                    activeManager.DeserializeHelper(serialized.FamilyOptions);
                    foreach (var item in App.MWInstance.EncryptionFamilySelector.Items)
                    {
                        if (item is ComboBoxItem cbi && cbi.Content?.ToString() == activeManager.Identifier)
                        {
                            App.MWInstance.EncryptionFamilySelector.SelectedItem = item;
                            break;
                        }
                    }
                }
            }
            catch (RuntimeBinderException) { }
        }

        protected abstract void DeserializeHelper(dynamic serialized);

    }
}
