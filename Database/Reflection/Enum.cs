using System.ComponentModel;
using System.Reflection;

namespace Database.Reflection
{
    public static class EnumReflection
    {

        public static string GetDescription<T>(this T eValue) where T : struct
        {
            Type type = eValue.GetType();
            if (!type.IsEnum)
                throw new ArgumentException("Тип аргумента должен быть перечислением", eValue.ToString());

            MemberInfo[] memberInfo = type.GetMember(eValue.ToString());
            if (memberInfo != null && memberInfo.Length > 0)
            {
                object[] attrs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attrs != null && attrs.Length > 0)
                {
                    return ((DescriptionAttribute)attrs[0]).Description;
                }
            }
            return eValue.ToString();
        }
    }
}
