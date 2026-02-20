using Avalonia.Controls;
using Avalonia.Layout;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace JuliaCrypt.Managers.CryptographicManagers.SymetricAlgorithmManagers
{
    internal class AESManager : SymetricAlgorithmManager
    {
        protected override string Identifier { get => "AES (Advanced Encryption Standard)"; }

        protected override Func<SymmetricAlgorithm> ManagerCreator { get => Aes.Create; }
    }
}
