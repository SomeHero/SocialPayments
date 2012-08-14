using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using SocialPayments.Domain.CustomAttributes;

namespace SocialPayments.Domain.ExtensionMethods
{
    public static class EnumExtensionManager
    {
        public static string GetDescription(this Enum enumerator)
        {
            //get the enumerator type
            Type type = enumerator.GetType();

            //get the member info
            MemberInfo[] memberInfo = type.GetMember(enumerator.ToString());

            if (memberInfo != null && memberInfo.Length > 0)
            {
                object[] attribute = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attribute != null && attribute.Length > 0)
                    return ((DescriptionAttribute)attribute[0]).Description;
            }

            return enumerator.ToString();
        }
        public static string GetRecipientMessageStatus(this Enum enumerator)
        {
            Type type = enumerator.GetType();

            MemberInfo[] memberInfo = type.GetMember(enumerator.ToString());

            if (memberInfo != null && memberInfo.Length > 0)
            {
                object[] attribute = memberInfo[0].GetCustomAttributes(typeof(MessageStatusAttribute), false);

                if (attribute != null && attribute.Length > 0)
                    return ((MessageStatusAttribute)attribute[0]).RecipientDescription;
            }

            return enumerator.ToString();
        }
        public static string GetSenderMessageStatus(this Enum enumerator)
        {
            Type type = enumerator.GetType();

            MemberInfo[] memberInfo = type.GetMember(enumerator.ToString());

            if (memberInfo != null && memberInfo.Length > 0)
            {
                object[] attribute = memberInfo[0].GetCustomAttributes(typeof(MessageStatusAttribute), false);

                if (attribute != null && attribute.Length > 0)
                    return ((MessageStatusAttribute)attribute[0]).SenderDescription;
            }

            return enumerator.ToString();
        }

    }
}
