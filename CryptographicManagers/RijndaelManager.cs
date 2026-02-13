using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JuliaCrypt.CryptographicManagers
{
    internal class RijndaelManager : SystemSymetricAlgorithmManager
    {
        protected override string Identifier { get => "LEGACY: Rijndael"; }

        protected override Func<SymmetricAlgorithm> ManagerCreator { get => Rijndael.Create; }
    }
}
