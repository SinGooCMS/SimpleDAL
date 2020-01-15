using System;

namespace SinGoo.Simple.DAL
{
    /// <summary>
    /// 数据表名称
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {
        public TableAttribute(string tableName)
        {
            this._tableName = tableName;
        }

        private string _tableName = "";
        public string Name { get { return _tableName; } }
    }

    /// <summary>
    /// 关键字
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class PrimaryKeyAttribute : Attribute
    {
    }

    /// <summary>
    /// 是否可写（添加更新时）
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class WriteableAttribute : Attribute
    {
        public WriteableAttribute(bool write)
        {
            this._write = write;
        }

        private bool _write = true;
        public bool Write { get { return _write; } }
    }
}
