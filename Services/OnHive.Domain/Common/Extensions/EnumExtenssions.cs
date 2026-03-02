using System.ComponentModel;

namespace EHive.Core.Library.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum instance)
        {            
            var enumType = instance.GetType();
            var memberInfos = enumType.GetMember(instance.ToString());
            var enumValueMemberInfo = memberInfos.FirstOrDefault(m => m.DeclaringType == enumType);
            var attributes = enumValueMemberInfo?.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes != null && attributes.Any())
            {
                return (attributes[0] as DescriptionAttribute)?.Description ?? instance.ToString();
            }
            else
            {
                return instance.ToString();
            }
        }
    }
}