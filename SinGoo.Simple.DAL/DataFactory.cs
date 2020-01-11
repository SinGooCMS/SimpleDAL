using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace SinGoo.Simple.DAL
{
    public class DataFactory
    {
        private static readonly string AssemblyName = "SinGoo.Simple.DAL";
        public static AbstrctFactory CreateDataFactory(string DataString="mssql", string strConnString=null)
        {
            string ClassName = "SinGoo.Simple.DAL" + ".";
            switch (DataString.ToUpper())
            {                
                case "OLEDB":
                    ClassName += "OleDataFactory";
                    break;
                case "SQLITE":
                    ClassName += "SQLiteDataFactory";
                    break;
                case "MSSQL":
                default:
                    ClassName += "SqlDataFactory";
                    break;
            }

            AbstrctFactory dbo = (AbstrctFactory)Assembly.Load(AssemblyName).CreateInstance(ClassName, false, BindingFlags.Default, null, new object[] { strConnString }, null, null);
            if (dbo == null)
                throw new Exception("无法创建有效的数据工厂，数据连接不正确或者已断开！");

            return dbo;
        }
    }
}