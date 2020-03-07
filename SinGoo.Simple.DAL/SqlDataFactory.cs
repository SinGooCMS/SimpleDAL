using System;
using System.Data;
using System.Collections;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using SinGoo.Simple.DAL.Utils;

namespace SinGoo.Simple.DAL
{
    /// <summary>
    /// 对DBHelper的再次封装
    /// </summary>    
    sealed class SqlDataFactory : IDBFactory
    {
        DBBaseHelper dbHelper = null;

        public SqlDataFactory()
        {
            dbHelper = new DBBaseHelper(ConnStore.DefConnStr); //默认的连接字符串
        }
        /// <summary>
        /// 自定义的连接字符串
        /// </summary>
        /// <param name="strConnString"></param>
        public SqlDataFactory(string strConnString)
        {
            if (!string.IsNullOrEmpty(strConnString))
                dbHelper = new DBBaseHelper(strConnString); //自定义连接字符串
            else
                dbHelper = new DBBaseHelper(ConnStore.DefConnStr); //默认的连接字符串
        }

        #region ------执行sql语句------
        /// <summary>
        /// 执行一条sql语句
        /// </summary>
        /// <param name="strSQL"></param>
        public bool ExecSQL(string strSQL)
        {
            if (string.IsNullOrEmpty(strSQL))
                return false;

            return dbHelper.ExecuteNonQuery(strSQL) > 0;
        }

        /// <summary>
        /// 运行带go的多条语句
        /// </summary>
        /// <param name="strSQL"></param>
        /// <param name="strSplitter"></param>
        /// <returns></returns>
        public bool ExecSQLWithSplit(string strSQL, string strSplitter)
        {
            try
            {
                int startPos = 0;

                do
                {
                    int lastPos = strSQL.IndexOf(strSplitter, startPos);
                    int len = (lastPos > startPos ? lastPos : strSQL.Length) - startPos;
                    string query = strSQL.Substring(startPos, len);

                    if (query.Trim().Length > 0)
                    {
                        try
                        {
                            ExecSQL(query);
                        }
                        catch { ;}
                    }

                    if (lastPos == -1)
                        break;
                    else
                        startPos = lastPos + strSplitter.Length;
                } while (startPos < strSQL.Length);

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region ------执行存储过程------
        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="strCommandText"></param>
        /// <param name="arrParam"></param>
        /// <returns></returns>
        public bool ExecProc(string strCommandText, System.Data.Common.DbParameter[] arrParam)
        {
            return dbHelper.ExecuteNonQueryProc(strCommandText, arrParam) > 0;
        }
        /// <summary>
        /// 执行存储过程并返回值
        /// </summary>
        /// <param name="strCommandText"></param>
        /// <param name="arrParam"></param>
        /// <returns></returns>
        public object ExecProcReValue(string strCommandText, System.Data.Common.DbParameter[] arrParam)
        {
            return dbHelper.ExecuteScalarProc(strCommandText, arrParam);
        }
        /// <summary>
        /// 执行存储过程获取datareader
        /// </summary>
        /// <param name="strCommandText"></param>
        /// <param name="arrParam"></param>
        /// <returns></returns>
        public IDataReader ExecProcReReader(string strCommandText, System.Data.Common.DbParameter[] arrParam)
        {
            return dbHelper.ExecuteReaderProc(strCommandText, arrParam);
        }
        /// <summary>
        /// 执行存储过程获取dataset
        /// </summary>
        /// <param name="strCommandText"></param>
        /// <param name="arrParam"></param>
        /// <returns></returns>
        public DataSet ExecProcReDS(string strCommandText, System.Data.Common.DbParameter[] arrParam)
        {
            return dbHelper.ExecuteDataSetProc(strCommandText, arrParam);
        }
        /// <summary>
        /// 执行存储过程获取datatable
        /// </summary>
        /// <param name="strCommandText"></param>
        /// <param name="arrParam"></param>
        /// <returns></returns>
        public DataTable ExecProcReDT(string strCommandText, System.Data.Common.DbParameter[] arrParam)
        {
            return dbHelper.ExecuteDataTableProc(strCommandText, arrParam);
        }
        #endregion

        #region ------数据查询------

        #region 返回一个对象
        /// <summary>
        /// sql语句返回查询对象
        /// </summary>
        /// <param name="strSQL">需要执行的SQL语句</param>
        /// <returns>返回object对象</returns>
        public object GetObject(string strSQL)
        {
            object objTemp = dbHelper.ExecuteScalar(strSQL);
            if (objTemp != null && DBNull.Value != objTemp)
                return objTemp;

            return null;
        }
        #endregion

        #region 返回一个值

        /// <summary>
        /// 返回一个值 指定值类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="strSQL"></param>
        /// <returns></returns>
        public T GetValue<T>(string strSQL)
        {
            object objTemp = GetObject(strSQL);
            return null == objTemp ? default(T) : (T)objTemp;
        }

        #endregion

        #region 返回一个向前只读数据流 DataReader
        /// <summary>
        /// 返回一个向前只读数据流
        /// </summary>
        /// <param name="strSQL"></param>
        /// <returns></returns>
        public IDataReader GetDataReader(string strSQL)
        {
            return dbHelper.ExecuteReader(strSQL);
        }
        #endregion

        #region 返回一个数据表 DataTable
        /// <summary>
        /// 返回当前数据集中的第一个数据表
        /// </summary>
        /// <param name="strSQL"></param>
        /// <returns></returns> 
        public DataTable GetDataTable(string strSQL)
        {
            return dbHelper.ExecuteDataTable(strSQL);
        }

        #endregion

        #region 返回一个数据集 DataSet
        /// <summary>
        /// 返回一个数据集,无数据返回null
        /// </summary>
        /// <param name="strSQL"></param>
        /// <returns></returns>
        public DataSet GetDataSet(string strSQL)
        {
            return dbHelper.ExecuteDataSet(strSQL);
        }

        #endregion

        #region 返回分页内容

        public DataTable GetPagerDataTable(string strFilter, string strTableName, string strCondition, string strSort, int intPageIndex, int intPageSize, ref int intTotalCount, ref int intTotalPage)
        {
            if (string.IsNullOrEmpty(strCondition))
                strCondition = "1=1";

            //总记录数
            intTotalCount = GetCount(strTableName, strCondition) ?? 0;
            //总页数
            intTotalPage = intTotalCount % intPageSize == 0 ? intTotalCount / intPageSize : (intTotalCount / intPageSize) + 1;
            //起始页号
            int startPage = (intPageIndex - 1) * intPageSize + 1;
            //截止页号
            int endPage = intPageIndex * intPageSize;

            StringBuilder builder = new StringBuilder();
            builder.AppendFormat(@"SELECT  {0}
                                FROM(SELECT ROW_NUMBER() OVER(ORDER BY {3}) AS RowNum,{0}
                                          FROM  {1}
                                          WHERE {2}
                                        ) AS result
                                WHERE  RowNum >= {4}   AND RowNum <= {5}
                                ORDER BY {3}", strFilter, strTableName, strCondition, strSort, startPage, endPage);

            return dbHelper.ExecuteDataTable(builder.ToString());
        }

        public IList<T> GetPager<T>(string strCondition, string strSort, int intPageIndex, int intPageSize, ref int intTotalCount, ref int intTotalPage) where T : class
        {
            return GetPager<T>("*", strCondition, strSort, intPageIndex, intPageSize, ref intTotalCount, ref intTotalPage);
        }
        public IList<T> GetPager<T>(string strFilter, string strCondition, string strSort, int intPageIndex, int intPageSize, ref int intTotalCount, ref int intTotalPage) where T : class
        {
            //返回实体列表
            IList<T> listResult = new List<T>();
            //实体
            T tItem = default(T);
            //表名
            string tableName = AttrAssistant.GetTableName(typeof(T));

            if (string.IsNullOrEmpty(strCondition))
                strCondition = "1=1";

            //总记录数
            intTotalCount = GetCount(tableName, strCondition) ?? 0;
            //总页数
            intTotalPage = intTotalCount % intPageSize == 0 ? intTotalCount / intPageSize : (intTotalCount / intPageSize) + 1;
            //起始页号
            int startPage = (intPageIndex - 1) * intPageSize + 1;
            //截止页号
            int endPage = intPageIndex * intPageSize;

            StringBuilder builder = new StringBuilder();
            builder.AppendFormat(@"SELECT  {0}
                                FROM(SELECT ROW_NUMBER() OVER(ORDER BY {3}) AS RowNum,{0}
                                          FROM  {1}
                                          WHERE {2}
                                        ) AS result
                                WHERE  RowNum >= {4}   AND RowNum <= {5}
                                ORDER BY {3}", strFilter, tableName, strCondition, strSort, startPage, endPage);

            var reader = dbHelper.ExecuteReader(builder.ToString());
            ReflectionBuilder<T> refBuilder = ReflectionBuilder<T>.CreateBuilder(reader);
            while (reader.Read())
            {
                tItem = refBuilder.Build(reader);
                listResult.Add(tItem);
            }

            reader.Close();
            return listResult;
        }
        #endregion

        #region 返回一个Model实体

        /// <summary>
        /// 返回一个实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="strSQL">查询语句</param>
        /// <returns></returns>
        public T GetModel<T>(string strSQL) where T : class
        {
            //返回实体
            T tReturn = default(T);
            SqlDataReader reader = (SqlDataReader)GetDataReader(strSQL);

            ReflectionBuilder<T> builder = ReflectionBuilder<T>.CreateBuilder(reader);
            while (reader.Read())
            {
                tReturn = builder.Build(reader);
            }

            reader.Close();
            return tReturn;
        }

        /// <summary>
        /// 返回一个实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        public T GetModel<T>(IDataReader reader) where T : class
        {
            //返回实体
            T tReturn = default(T);

            ReflectionBuilder<T> builder = ReflectionBuilder<T>.CreateBuilder(reader);
            while (reader.Read())
            {
                tReturn = builder.Build(reader);
            }

            reader.Close();
            return tReturn;
        }

        /// <summary>
        /// 返回一个实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="KeyValue"></param>
        /// <returns></returns>
        public T GetModel<T>(object KeyValue) where T : class
        {
            string tableName = AttrAssistant.GetTableName(typeof(T));
            string key = AttrAssistant.GetKey(typeof(T));

            return GetModel<T>("select top 1 * from " + tableName + " where " + key + " = " + KeyValue);
        }

        #endregion

        #region 返回一个IList

        /// <summary>
        /// 返回List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="strSQL"></param>
        /// <returns></returns>
        public IList<T> GetList<T>(string strSQL) where T : class
        {
            //返回实体列表
            IList<T> listResult = new List<T>();
            //实体
            T tItem = default(T);

            SqlDataReader reader = (SqlDataReader)GetDataReader(strSQL);
            ReflectionBuilder<T> builder = ReflectionBuilder<T>.CreateBuilder(reader);
            while (reader.Read())
            {
                tItem = builder.Build(reader);
                listResult.Add(tItem);
            }

            reader.Close();
            return listResult;
        }

        /// <summary>
        /// 返回List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="condition"></param>
        /// <param name="topNum"></param>
        /// <returns></returns>
        public IList<T> GetList<T>(int topNum = 0, string condition = "", string filter = "*") where T : class
        {
            string tableName = AttrAssistant.GetTableName(typeof(T));
            StringBuilder builder = new StringBuilder("select ");

            if (string.IsNullOrEmpty(filter))
                filter = "*";
            if (topNum > 0)
                builder.AppendFormat(" top {0} ", topNum);

            builder.AppendFormat(" {0} from {1} ", filter, tableName);
            if (!string.IsNullOrEmpty(condition))
                builder.AppendFormat(" where {0} ", condition);

            return GetList<T>(builder.ToString());
        }

        #endregion

        #region 扩展实用查询

        /// <summary>
        /// 获取表记录数 没有将返回NULL
        /// </summary>
        /// <param name="strTable"></param>
        /// <returns></returns>
        public int? GetCount(string strTable)
        {
            return GetCount(strTable, "");
        }

        public int? GetCount(string strTable, string strCondition)
        {
            return dbHelper.GetRecordCount(strTable, strCondition, "*");
        }

        #endregion

        #endregion

        #region ------插入数据------

        /// <summary>
        /// 插入数据并返回主键值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public int InsertModel<T>(T model) where T : class
        {
            return InsertModel(model, AttrAssistant.GetTableName(typeof(T)));
        }
        /// <summary>
        /// 插入数据并返回主键值 指定表 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="strTableName"></param>
        /// <returns></returns>
        public int InsertModel<T>(T model, string strTableName) where T : class
        {
            int intReturnIDKey = new int();

            Type modelEntityType = model.GetType();
            PropertyInfo[] arrProperty = modelEntityType.GetProperties();
            StringBuilder builderSQL = new StringBuilder();
            StringBuilder builderParams = new StringBuilder(" ( ");
            List<SqlParameter> listParams = new List<SqlParameter>();

            foreach (PropertyInfo property in arrProperty)
            {
                if (property.GetType() != typeof(System.DBNull) && AttrAssistant.IsWriteable(property))
                {
                    object obj = property.GetValue(model, null);

                    builderSQL.Append(property.Name + " , ");
                    builderParams.Append("@" + property.Name + " , ");
                    listParams.Add(new SqlParameter("@" + property.Name, obj));
                }
            }

            builderSQL.Remove(builderSQL.Length - 2, 2);
            builderParams.Remove(builderParams.Length - 2, 2);

            builderSQL.Append(" ) values ");
            builderSQL.Append(builderParams.ToString() + " ) ");
            builderSQL.Append(";select @@IDENTITY;"); //返回最新的ID

            object objTemp = dbHelper.ExecuteScalar(" insert into " + strTableName + " ( " + builderSQL.ToString(), listParams.ToArray());
            if (int.TryParse(objTemp == null ? string.Empty : objTemp.ToString(), out intReturnIDKey))
                return intReturnIDKey;
            else
                return 0;
        }

        #region 批量插入

        /// <summary>
        /// 大数据批量插入,数据源和目标表字段需要对应
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="targetTableName"></param>
        public void BulkInsert(IDataReader dr, string targetTableName)
        {
            using (IDbConnection conn = new SqlConnection(dbHelper.ConnectionString))
            {
                conn.Open();
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(dbHelper.ConnectionString))
                {
                    bulkCopy.DestinationTableName = targetTableName;
                    bulkCopy.WriteToServer(dr);                    
                }
            }
        }

        /// <summary>
        /// 大数据批量插入,数据源和目标表字段需要对应
        /// </summary>
        /// <param name="dr"></param>
        public void BulkInsert<T>(IDataReader dr) where T : class
        {
            BulkInsert(dr, AttrAssistant.GetTableName(typeof(T)));
        }

        /// <summary>
        /// 大数据批量插入,数据源和目标表字段需要对应
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="targetTableName"></param>
        public void BulkInsert(DataTable dt, string targetTableName)
        {
            using (IDbConnection conn = new SqlConnection(dbHelper.ConnectionString))
            {
                conn.Open();
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(dbHelper.ConnectionString))
                {
                    bulkCopy.DestinationTableName = targetTableName;
                    bulkCopy.WriteToServer(dt);
                }
            }
        }

        /// <summary>
        /// 大数据批量插入,数据源和目标表字段需要对应
        /// </summary>
        /// <param name="dt"></param>
        public void BulkInsert<T>(DataTable dt) where T : class
        {
            BulkInsert(dt, AttrAssistant.GetTableName(typeof(T)));
        }

        #endregion

        #endregion

        #region ------更新数据------

        /// <summary>
        /// 更新数据表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool UpdateModel<T>(T model) where T : class
        {
            Type modelEntityType = model.GetType();
            PropertyInfo[] arrProperty = modelEntityType.GetProperties();
            StringBuilder builderSQL = new StringBuilder();
            List<SqlParameter> listParams = new List<SqlParameter>();

            foreach (PropertyInfo property in arrProperty)
            {
                //有些自定义的字段不存在数据库，不可写（sqlsugar中是ignore忽略）
                if (property.GetType() != typeof(System.DBNull) && AttrAssistant.IsWriteable(property))
                {
                    object obj = property.GetValue(model, null);
                    builderSQL.Append(property.Name + "=@" + property.Name + " , ");
                    listParams.Add(new SqlParameter("@" + property.Name, obj));
                }
            }

            builderSQL.Remove(builderSQL.Length - 2, 2);

            //主键值
            int intIDValue = new int();
            string key = AttrAssistant.GetKey(typeof(T));
            if (!string.IsNullOrEmpty(key))
            {
                intIDValue = RefProperty.GetSafePropertyInt32<T>(model, key);
                builderSQL.Append(" where " + key + "=" + intIDValue);
            }

            return dbHelper.ExecuteNonQuery(" update " + AttrAssistant.GetTableName(typeof(T)) + " set " + builderSQL.ToString(), listParams.ToArray()) > 0;
        }

        #endregion

        #region ------删除数据------

        /// <summary>
        /// 返回true/false
        /// </summary>
        /// <param name="TableName"></param>
        /// <param name="Condition"></param>
        /// <returns></returns>
        public bool DeleteTable(string TableName, string Condition)
        {
            string sql = string.Format(" delete from {0} ", TableName);
            if (!string.IsNullOrEmpty(Condition))
                sql += string.Format(" where {0} ", Condition); ;

            return dbHelper.ExecuteNonQuery(sql, null) > 0;
        }

        /// <summary>
        /// 删除实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool DeleteModel<T>(T model) where T : class
        {
            //主键值
            int intIDValue = new int();
            string key = AttrAssistant.GetKey(typeof(T));
            if (!string.IsNullOrEmpty(key))
                intIDValue = RefProperty.GetSafePropertyInt32<T>(model, key);

            return DeleteTable(AttrAssistant.GetTableName(typeof(T)), key + "=" + intIDValue.ToString());
        }

        #endregion

    }
}
