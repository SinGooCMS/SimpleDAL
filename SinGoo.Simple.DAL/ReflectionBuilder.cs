using System;
using System.Data.SqlClient;
using System.Reflection;
using System.Reflection.Emit;
using System.Data;

namespace SinGoo.Simple.DAL
{
    /// <summary>
    /// 通过datareader反射出实体并填充值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ReflectionBuilder<T>
    {
        private PropertyInfo[] properties;

        private ReflectionBuilder() { }

        /// <summary>
        /// 赋值给实体,并返回实体
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public T Build(IDataReader reader)
        {
            //创建model的实体 
            T result = (T)Activator.CreateInstance(typeof(T));

            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (properties[i] != null && !reader.IsDBNull(i))
                {
                    properties[i].SetValue(result, reader[i], null);
                }
            }

            return result;
        }
        /// <summary>
        /// 创建实体
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static ReflectionBuilder<T> CreateBuilder(IDataReader reader)
        {
            ReflectionBuilder<T> result = new ReflectionBuilder<T>();

            result.properties = new PropertyInfo[reader.FieldCount];
            for (int i = 0; i < reader.FieldCount; i++)
            {
                result.properties[i] = typeof(T).GetProperty(reader.GetName(i));
            }

            return result;
        }
    }
}
