using System.Reflection;
using System.Reflection.Emit;

namespace Database.Reflection
{
    //TODO: Создать динамически компилируемую библиотеку dll
    internal static class Enum
    {
/*        public static string? SetEnums<TType>(string nameEnum, List<String> keys, TType typeEnum) where TType : Type
        {
            string[] upperKeys = keys.Select(key => key.ToUpper()).ToArray();
            AppDomain currentDomain = AppDomain.CurrentDomain;


            AssemblyName aName = new AssemblyName("TempAssembly");
            AssemblyBuilder ab = currentDomain.DefineDynamicAssembly(
                aName, AssemblyBuilderAccess.RunAndSave);


            ModuleBuilder mb = ab.DefineDynamicModule(aName.Name, aName.Name + ".dll");


            EnumBuilder eb = mb.DefineEnum(nameEnum, TypeAttributes.Public, typeEnum.GetType());

            eb.DefineLiteral("Low", 0);
            eb.DefineLiteral("High", 1);

            // Create the type and save the assembly.
            Type finished = eb.CreateType();
            ab.Save(aName.Name + ".dll");
            FieldInfo? suitField = type.GetField(nameEnum, BindingFlags.NonPublic | BindingFlags.Instance);
            if (suitField != null)
            {
                return null;
            }
            foreach (var (index, key) in upperKeys.Select((index, key) => (index, key)))
            {
*//*                suitField.de
                suitField.SetValue(key, index);*//*
            }

        }*/
    }
}
