using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Collections.Concurrent;
using System.Reflection.Emit;

namespace SinGoo.Simple.DAL
{
    public class AttrAssistant
    {
        /// <summary>
        /// 表名
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetTableName(Type type)
        {
            var tableAttr = type.GetCustomAttributes(false).Where(attr => attr.GetType().Name == "TableAttribute").SingleOrDefault() as dynamic;
            if (tableAttr != null)
                return tableAttr.Name;

            return "";
        }

        /// <summary>
        /// 只支持单主键（一般主键ID是自增的）
        /// </summary>
        /// <param name="pi"></param>
        /// <returns></returns>
        public static string GetKey(Type type)
        {
            var keyAttr= type.GetProperties().Where(p => p.GetCustomAttributes(true).Any(a => a is PrimaryKeyAttribute)).FirstOrDefault();
            return keyAttr == null ? "" : keyAttr.Name;
        }

        /// <summary>
        /// 只支持单主键（一般主键ID是自增的）
        /// </summary>
        /// <param name="pi"></param>
        /// <returns></returns>
        public static bool IsKey(PropertyInfo pi)
        {
            return pi.GetCustomAttributes(true).Any(a => a is PrimaryKeyAttribute);
        }

        /// <summary>
        /// 是否可写(对于插入更新)
        /// </summary>
        /// <param name="pi"></param>
        /// <returns></returns>
        public static bool IsWriteable(PropertyInfo pi)
        {
            var attributes = pi.GetCustomAttributes(typeof(WriteableAttribute), false);
            if (attributes.Length != 1) return true;

            var writeAttribute = (WriteableAttribute)attributes[0];
            return writeAttribute.Write;
        }
    }
}
