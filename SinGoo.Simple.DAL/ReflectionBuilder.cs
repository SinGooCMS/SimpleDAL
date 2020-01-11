using System;
using System.Data.SqlClient;
using System.Reflection;
using System.Reflection.Emit;
using System.Data;

namespace SinGoo.Simple.DAL
{
    /// <summary>
    /// ͨ��datareader�����ʵ�岢���ֵ
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ReflectionBuilder<T>
    {
        private PropertyInfo[] properties;

        private ReflectionBuilder() { }

        /// <summary>
        /// ��ֵ��ʵ��,������ʵ��
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public T Build(IDataReader reader)
        {
            //����model��ʵ�� 
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
        /// ����ʵ��
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
