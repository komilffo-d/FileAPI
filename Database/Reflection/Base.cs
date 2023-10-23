using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Database.Reflection
{
    internal static class Base
    {
        private static List<FieldInfo> GetConstants(Type type)
        {
            FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public |
                 BindingFlags.Static | BindingFlags.FlattenHierarchy);

            return fieldInfos.Where(fi => fi.IsLiteral && !fi.IsInitOnly).ToList();
        }
    }
}
