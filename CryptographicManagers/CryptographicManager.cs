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
    public abstract class CryptographicManager : IDisposable
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
            try
            {
                var instance = Activator.CreateInstance(managerType);
                if (instance is CryptographicManager manager)
                {
                    return manager;
                }
            }
            catch (MissingMethodException)
            { return null; } //is an abstract class
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

        public abstract void Dispose();

        private static long _biggestKeySizeBits = 0;
        public static long BiggestKeyBitSize
        {
            get => _biggestKeySizeBits;
            protected set => _biggestKeySizeBits = Math.Max(value, _biggestKeySizeBits);
        }

        protected abstract string Identifier { get; }
        public abstract uint SelectedKeyBitSize { get; protected set; }
        protected abstract long FamilyBiggestKeyBitSize { get; }
        protected void Initialize()
        {
            BiggestKeyBitSize = FamilyBiggestKeyBitSize;
        }
        public virtual void OnSelected()
        {
            App.MWInstance.EncryptionSubOptions.Children.Clear(); //Just in case something else is already 
        }

        public abstract byte[] Encrypt(byte[] plaintext, KeyManager key);
        public abstract byte[] Decrypt(byte[] ciphertext, KeyManager key);
    }
}
