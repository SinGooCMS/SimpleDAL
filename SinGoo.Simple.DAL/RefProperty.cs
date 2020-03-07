using System;
using System.Reflection;
using System.Linq;

namespace SinGoo.Simple.DAL
{
    /// <summary>
    /// 读取属性值
    /// </summary>
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

        public static DateTime GetSafePropertyDateTime<T>(T model, string strFindName)
        {
            string strResult = GetSafePropertyString<T>(model, strFindName);
            DateTime dtReturn = new DateTime(1900,1,1);
            if (DateTime.TryParse(strResult, out dtReturn))
                return dtReturn;

            return new DateTime(1900, 1, 1);
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

            var pi = arrProperty.Where(p => p.Name.Equals(strFindName)).FirstOrDefault();
            if(pi!=null)
                return pi.GetValue(model, null);

            return null;
        }
    }
}
