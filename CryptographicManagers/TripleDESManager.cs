using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JuliaCrypt.CryptographicManagers
{
    internal class TripleDESManager : SystemSymetricAlgorithmManager
    {
        protected override string Identifier { get => "LEGACY: TDES (Triple Data Encryption Standard)"; }

        protected override Func<SymmetricAlgorithm> ManagerCreator { get => TripleDES.Create; }
    }
}
