using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JuliaCrypt
{
    public static class Utilities
    {
        public static Assembly? GetJuliaCryptAssembly() => Assembly.GetExecutingAssembly();
        public static IEnumerable<Type> GetSubTypes(Type type) =>
            GetJuliaCryptAssembly()?.GetTypes().Where(t => t.IsSubclassOf(type)) ?? [];
    }
}
