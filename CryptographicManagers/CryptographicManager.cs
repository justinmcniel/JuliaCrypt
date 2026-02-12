using Avalonia.Controls;
using HarfBuzzSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace JuliaCrypt.CryptographicManagers
{
    public abstract class CryptographicManager
    {
        private static Dictionary<string, Type> _cryptographicManagerTypes = new();
        public static IEnumerable<Type> CryptographicManagerTypes 
        { get => _cryptographicManagerTypes.Values; }

        private static Dictionary<Type, CryptographicManager> _cryptographicManagers = new();
        public static IEnumerable<CryptographicManager> CryptographicManagers
        { get => _cryptographicManagers.Values; }

        public static IEnumerable<string> CryptographicManagerNames
        { get => _cryptographicManagerTypes.Keys; }

        public static void InitializeManagers() 
        {
            var subtypes = Utilities.GetSubTypes(typeof(CryptographicManager));

            foreach(Type managerType in subtypes)
            {
                var manager = GetManagerInstance(managerType);
                if (manager != null)
                {
                    _cryptographicManagerTypes.Add(manager.Identifier, managerType);
                    _cryptographicManagers.Add(managerType, manager);
                }
            }

            _ = Parallel.ForEach(CryptographicManagers, manager =>
            { manager.Initialize(); });

            foreach (var name in CryptographicManagerNames)
            {
                App.MWInstance.EncryptionFamilySelector.Items.Add(new ComboBoxItem()
                {
                    Content = name,
                });
            }
        }

        private static CryptographicManager? GetManagerInstance(Type managerType)
        {
            var instance = Activator.CreateInstance(managerType);
            if (instance is CryptographicManager manager)
            {
                return manager;
            }
            return null;
        }

        public static CryptographicManager? GetManager(Type managerType)
        {
            try { return _cryptographicManagers[managerType]; }
            catch (KeyNotFoundException) { return null; }
        }
        public static CryptographicManager? GetManager(string identifier)
        {
            try { return GetManager(_cryptographicManagerTypes[identifier]); }
            catch (KeyNotFoundException) { return null; }
        }

        protected abstract string Identifier { get; }
        public abstract uint SelectedKeySize { get; }
        protected abstract void Initialize();
        public abstract void OnSelected();
        public abstract void OnDeselected();

        public abstract byte[] Encrypt(byte[] plaintext, KeyManager key);
        public abstract byte[] Decrypt(byte[] ciphertext, KeyManager key);
    }
}
