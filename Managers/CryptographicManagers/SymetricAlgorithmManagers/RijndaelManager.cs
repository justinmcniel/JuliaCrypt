using JuliaCrypt.SymetricAlgorithms;
using JuliaCrypt.SymetricAlgorithms.CryptoTransforms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JuliaCrypt.Managers.CryptographicManagers.SymetricAlgorithmManagers
{ /// TODO: Move this over to a custom implementation to support multiple block sizes
    internal class RijndaelManager : SymetricAlgorithmManager
    {
        protected override string Identifier { get => "LEGACY: Rijndael"; }
        protected override Func<SymmetricAlgorithm> ManagerCreator { get => SymmetricAlgorithmBase<RijndaelTransform>.Create; }
    }
}
