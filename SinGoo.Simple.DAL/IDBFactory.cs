using System;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace SinGoo.Simple.DAL
{
    public interface IDBFactory
    {
        void SetConnStr(string connStr);

        #region ------执行SQL语句------

        /// <summary>
        /// 执行一条SQL语句
        /// </summary>
        /// <param name="strSQL">需要执行的sql语句</param>
        bool ExecSQL(string strSQL);

        /// <summary>
        /// 执行带分隔符(如go)的语句,一般是文件
        /// </summary>
        /// <param name="strSQL"></param>
        /// <param name="strSplitter"></param>
        /// <returns></returns>
        bool ExecSQLWithSplit(string strSQL, string strSplitter);

        #endregion

        #region ------执行存储过程------

        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="strCommandText"></param>
        /// <param name="arrParam"></param>
        /// <returns></returns>
        bool ExecProc(string strCommandText, DbParameter[] arrParam);
        /// <summary>
        /// 执行存储过程并返回值
        /// </summary>
        /// <param name="strCommandText"></param>
        /// <param name="arrParam"></param>
        /// <returns></returns>
        object ExecProcReValue(string strCommandText, DbParameter[] arrParam);
        /// <summary>
        /// 执行存储过程并返回datareader
        /// </summary>
        /// <param name="strCommandText"></param>
        /// <param name="arrParam"></param>
        /// <returns></returns>
        IDataReader ExecProcReReader(string strCommandText, DbParameter[] arrParam);
        /// <summary>
        /// 返回一个dataset
        /// </summary>
        /// <param name="strCommandText"></param>
        /// <param name="arrParam"></param>
        /// <returns></returns>
        DataSet ExecProcReDS(string strCommandText, DbParameter[] arrParam);
        /// <summary>
        /// 返回一个datatable
        /// </summary>
        /// <param name="strCommandText"></param>
        /// <param name="arrParam"></param>
        /// <returns></returns>
        DataTable ExecProcReDT(string strCommandText, DbParameter[] arrParam);

        #endregion

        #region ------查询操作------

        #region 返回一个object对象

        /// <summary>
        /// 返回一个object对象
        /// </summary>
        /// <param name="strSQL">需要执行的sql语句</param>
        /// <returns>object</returns>
        object GetObject(string strSQL);

        #endregion

        #region 返回一个值

        /// <summary>
        /// 返回一个值
        /// </summary>
        /// <typeparam name="T">数据类型 如int,string</typeparam>
        /// <param name="strSQL">查询语句</param>
        /// <returns></returns>
        T GetValue<T>(string strSQL);

        #endregion

        #region 返回一个datareader,该连接不能断开,在数据传送完成后自动断开

        /// <summary>
        /// 返回一个datareader
        /// </summary>
        /// <param name="strSql">需要执行的sql语句</param>
        /// <returns>IDataReader</returns>
        IDataReader GetDataReader(string strSql);

        #endregion

        #region 返回一个datatable

        /// <summary>
        /// 返回一个datatable
        /// </summary>
        /// <param name="strSQL">需要执行的sql语句</param>
        /// <returns>DataTable</returns>
        DataTable GetDataTable(string strSQL);

        #endregion

        #region 返回一个dataset

        /// <summary>
        /// 返回一个dataset
        /// </summary>
        /// <param name="strSQL">需要执行的sql语句</param>
        /// <returns>DataSet</returns>
        DataSet GetDataSet(string strSQL);

        #endregion

        #region 返回分页内容

        DataTable GetPagerDataTable(string strFilter, string strTableName, string strCondition, string strSort, int intPageIndex, int intPageSize, ref int intTotalCount, ref int intTotalPage);

        IList<T> GetPager<T>(string strCondition, string strSort, int intPageIndex, int intPageSize, ref int intTotalCount, ref int intTotalPage) where T : class;
        IList<T> GetPager<T>(string strFilter, string strCondition, string strSort, int intPageIndex, int intPageSize, ref int intTotalCount, ref int intTotalPage) where T : class;

        #endregion

        #region 返回一个实体

        /// <summary>
        /// 返回一个实体类
        /// </summary>
        /// <typeparam name="T">实体</typeparam>
        /// <param name="strSQL">查询语句</param>
        /// <returns></returns>
        T GetModel<T>(string strSQL) where T : class;

        /// <summary>
        /// 返回一个实体类
        /// </summary>
        /// <typeparam name="T">实体</typeparam>
        /// <param name="reader">数据流</param>
        /// <returns></returns>
        T GetModel<T>(IDataReader reader) where T : class;

        /// <summary>
        /// 返回一个实体类
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="KeyValue"></param>
        /// <returns></returns>
        T GetModel<T>(object KeyValue) where T : class;

        #endregion

        #region 返回一个IList

        /// <summary>
        /// 返回一个List
        /// </summary>
        /// <typeparam name="T">实体</typeparam>
        /// <param name="strSQL">查询语句</param>
        /// <returns></returns>
        IList<T> GetList<T>(string strSQL) where T : class;

        /// <summary>
        /// 返回一个List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="condition"></param>
        /// <param name="topNum"></param>
        /// <returns></returns>
        IList<T> GetList<T>(int topNum = 0, string condition = "", string filter = "*") where T : class;

        #endregion

        #region 扩展实用查询
        /// <summary>
        /// 获取记录数
        /// </summary>
        /// <param name="strTable"></param>
        /// <returns></returns>
        int? GetCount(string strTable);
        /// <summary>
        /// 获取记录数
        /// </summary>
        /// <param name="strTable"></param>
        /// <param name="strCondition"></param>
        /// <returns></returns>
        int? GetCount(string strTable, string strCondition);
        #endregion

        #endregion

        #region ------插入操作------

        /// <summary>
        /// 插入到数据库并返回主键值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        int InsertModel<T>(T model) where T : class;

        /// <summary>
        /// 插入到指定表并返回主键值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="strTableName"></param>
        /// <returns></returns>
        int InsertModel<T>(T model, string strTableName) where T : class;

        #endregion

        #region ------更新数据------

        /// <summary>
        /// 更新数据表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        bool UpdateModel<T>(T model) where T : class;

        #endregion

        #region ------删除数据------

        /// <summary>
        /// 从数据库表中删除数据
        /// </summary>
        /// <param name="strTableName">数据库表名</param>
        /// <param name="strCondition">删除记录的条件,可以指定多个记录</param>
        /// <returns>bool</returns>
        bool DeleteTable(string strTableName, string strCondition);

        /// <summary>
        /// 删除实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="intID"></param>
        /// <returns></returns>
        bool DeleteModel<T>(T model) where T : class;

        #endregion
    }
}
