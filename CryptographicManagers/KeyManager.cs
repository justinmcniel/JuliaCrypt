using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuliaCrypt.CryptographicManagers
{
    public abstract class KeyManager
    {
        private static Dictionary<string, Type> _keyManagerTypes = new();
        public static IEnumerable<Type> KeyManagerTypes
        { get => _keyManagerTypes.Values; }

        private static Dictionary<Type, KeyManager> _keyManagers = new();
        public static IEnumerable<KeyManager> KeyManagers
        { get => _keyManagers.Values; }

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
        }

        private static KeyManager? GetManagerInstance(Type managerType)
        {
            var instance = Activator.CreateInstance(managerType);
            if (instance is KeyManager manager)
            {
                return manager;
            }
            return null;
        }

        public static KeyManager? GetManager(Type managerType) =>
            _keyManagers[managerType];

        public static KeyManager? GetManager(string identifier) =>
            GetManager(_keyManagerTypes[identifier]);

        protected abstract void Initialize();
        protected abstract string Identifier { get; }
        protected abstract void OnSelected();
        protected abstract void OnDeselected();
        protected abstract byte[] RequestKey(uint bitsize);
        protected abstract void Seed(byte[] seed);

    }
}
