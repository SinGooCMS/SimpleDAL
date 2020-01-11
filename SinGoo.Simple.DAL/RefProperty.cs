using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace SinGoo.Simple.DAL
{
    internal class RefProperty
    {
        public static decimal GetSafePropertyDecimal<T>(T model, string strFindName)
        {
            string strResult = GetSafePropertyString<T>(model, strFindName);
            decimal decReturn = 0.0m;
            if (decimal.TryParse(strResult, out decReturn))
                return decReturn;

            return 0.0m;
        }
        public static int GetSafePropertyInt32<T>(T model, string strFindName)
        {
            string strResult = GetSafePropertyString<T>(model, strFindName);
            int intReturn = 0;
            if (int.TryParse(strResult, out intReturn))
                return intReturn;

            return 0;
        }
        public static string GetSafePropertyString<T>(T model, string strFindName)
        {
            object obj = GetPropertyValue<T>(model, strFindName);
            return obj == null ? string.Empty : obj.ToString();
        }

        public static object GetPropertyValue<T>(T model, string strFindName)
        {
            Type modelEntityType = model.GetType();
            PropertyInfo[] arrProperty = modelEntityType.GetProperties();
            if (arrProperty != null && arrProperty.Length > 0)
            {
                foreach (PropertyInfo item in arrProperty)
                {
                    if (item.Name.Equals(strFindName))
                    {
                        return item.GetValue(model, null);
                    }
                }
            }

            return null;
        }
    }
}
