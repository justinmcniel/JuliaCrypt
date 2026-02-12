using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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

        public static CryptographicManager? GetManager(Type managerType) => 
            _cryptographicManagers[managerType];
        public static CryptographicManager? GetManager(string identifier) => 
            GetManager(_cryptographicManagerTypes[identifier]);

        protected abstract void Initialize();
        protected abstract string Identifier { get; }
        protected abstract void OnSelected();
        protected abstract void OnDeselected();

        protected abstract byte[] Encrypt(FileInfo plaintext, KeyManager key);
        protected abstract byte[] Decrypt(FileInfo plaintext, KeyManager key);
    }
}
