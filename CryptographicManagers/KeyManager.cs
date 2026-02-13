using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuliaCrypt.CryptographicManagers
{
    public abstract class KeyManager : IDisposable
    {
        private static Dictionary<string, Type> _keyManagerTypes = new();
        public static IEnumerable<Type> KeyManagerTypes
        { get => _keyManagerTypes.Values; }

        private static Dictionary<Type, KeyManager> _keyManagers = new();
        public static IEnumerable<KeyManager> KeyManagers
        { get => _keyManagers.Values; }
        public static IEnumerable<string> KeyManagerNames
        { get => _keyManagerTypes.Keys; }

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


            foreach (var name in KeyManagerNames)
            {
                App.MWInstance.KeyFamilySelector.Items.Add(new ComboBoxItem()
                {
                    Content = name,
                });
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
            try { GetManager(_keyManagerTypes[identifier]); }
            catch (KeyNotFoundException) { return null; }
            return null; // why is it needed here, and not in Cryptographic Manager? WTF
        }

        public bool ShouldRequestIV { get => false; }

        public abstract void Dispose();
        protected abstract string Identifier { get; }
        protected abstract void Initialize();
        public abstract void OnSelected();
        public abstract void OnDeselected();
        public abstract byte[] RequestKey(uint bitsize);
        public abstract byte[]? RequestIV(int bitsize);
        public abstract void Seed(byte[] seed);

    }
}
