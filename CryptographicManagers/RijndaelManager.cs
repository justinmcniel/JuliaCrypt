using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JuliaCrypt.CryptographicManagers
{ /// TODO: Move this over to a custom implementation to support multiple block sizes
    internal class RijndaelManager : SystemSymetricAlgorithmManager
    {
        protected override string Identifier { get => "LEGACY: Rijndael"; }
#pragma warning disable SYSLIB0022
        protected override Func<SymmetricAlgorithm> ManagerCreator { get => Rijndael.Create; }
#pragma warning restore SYSLIB0022
    }
}
