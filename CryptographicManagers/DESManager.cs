using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JuliaCrypt.CryptographicManagers
{
    internal class DESManager : SystemSymetricAlgorithmManager
    {
        protected override string Identifier { get => "LEGACY: DES (Data Encryption Standard)"; }

        protected override Func<SymmetricAlgorithm> ManagerCreator { get => DES.Create; }
    }
}
