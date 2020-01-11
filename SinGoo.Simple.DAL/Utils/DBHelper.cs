using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;

namespace SinGoo.Simple.DAL.Utils
{
    public partial class DBBaseHelper
    {
        private static DbProviderFactory _factory = null;
        private static Hashtable _paramcache = Hashtable.Synchronized(new Hashtable());
        private static IDbProvider _provider = null;
        private static int _querycount = 0;
        private static object lockHelper = new object();

        private static void AssignParameterValues(DbParameter[] commandParameters, object[] parameterValues)
        {
            if ((commandParameters != null) && (parameterValues != null))
            {
                if (commandParameters.Length != parameterValues.Length)
                    throw new Exception("参数值个数与参数不匹配");

                int index = 0;
                int length = commandParameters.Length;
                while (index < length)
                {
                    if (parameterValues[index] is IDbDataParameter)
                    {
                        IDbDataParameter parameter = (IDbDataParameter)parameterValues[index];
                        if (parameter.Value == null)
                        {
                            commandParameters[index].Value = DBNull.Value;
                        }
                        else
                        {
                            commandParameters[index].Value = parameter.Value;
                        }
                    }
                    else if (parameterValues[index] == null)
                    {
                        commandParameters[index].Value = DBNull.Value;
                    }
                    else
                    {
                        commandParameters[index].Value = parameterValues[index];
                    }
                    index++;
                }
            }
        }

        private static void AssignParameterValues(DbParameter[] commandParameters, DataRow dataRow)
        {
            if ((commandParameters != null) && (dataRow != null))
            {
                int num = 0;
                foreach (DbParameter parameter in commandParameters)
                {
                    if ((parameter.ParameterName == null) || (parameter.ParameterName.Length <= 1))
                    {
                        throw new Exception(string.Format("请提供参数{0}一个有效的名称{1}.", num, parameter.ParameterName));
                    }
                    if (dataRow.Table.Columns.IndexOf(parameter.ParameterName.Substring(1)) != -1)
                    {
                        parameter.Value = dataRow[parameter.ParameterName.Substring(1)];
                    }
                    num++;
                }
            }
        }

        private static void AttachParameters(DbCommand command, DbParameter[] parameters)
        {
            if (command == null)
            {
                throw new Exception("DbCommand不能为空");
            }
            if (parameters != null)
            {
                foreach (DbParameter parameter in parameters)
                {
                    if (parameter != null)
                    {
                        if (((parameter.Direction == ParameterDirection.InputOutput) || (parameter.Direction == ParameterDirection.Input)) && (parameter.Value == null))
                        {
                            parameter.Value = DBNull.Value;
                        }
                        command.Parameters.Add(parameter);
                    }
                }
            }
        }

        private static DbParameter[] CloneParameters(DbParameter[] originalParameters)
        {
            DbParameter[] parameterArray = new DbParameter[originalParameters.Length];
            int index = 0;
            int length = originalParameters.Length;
            while (index < length)
            {
                parameterArray[index] = (DbParameter)((ICloneable)originalParameters[index]).Clone();
                index++;
            }
            return parameterArray;
        }

        private static DbParameter[] DiscoverSpParameterSet(DbConnection connection, string spName, bool includeReturnValueParameter)
        {
            if (connection == null)
                throw new Exception("没有连接到数据库或者已断开");

            if ((spName == null) || (spName.Length == 0))
            {
                throw new ArgumentNullException("spName");
            }
            DbCommand command = connection.CreateCommand();
            command.CommandText = spName;
            command.CommandType = CommandType.StoredProcedure;
            connection.Open();
            Provider.DeriveParameters(command);
            connection.Close();
            if (!includeReturnValueParameter)
            {
                command.Parameters.RemoveAt(0);
            }
            DbParameter[] array = new DbParameter[command.Parameters.Count];
            command.Parameters.CopyTo(array, 0);
            foreach (DbParameter parameter in array)
            {
                parameter.Value = DBNull.Value;
            }
            return array;
        }

        public static DataSet ExecuteDataSet(string commandText)
        {
            return ExecuteDataSet(CommandType.Text, commandText, new DbParameter[1]);
        }

        private static DataSet ExecuteDataSet(CommandType commandType, string commandText)
        {
            return ExecuteDataSet(commandType, commandText, new DbParameter[1]);
        }

        public static DataSet ExecuteDataSet(string commandText, params DbParameter[] parameters)
        {
            return ExecuteDataSet(CommandType.Text, commandText, parameters);
        }

        private static DataSet ExecuteDataSet(CommandType commandType, string commandText, params DbParameter[] parameters)
        {
            if ((ConnectionString == null) || (ConnectionString.Length == 0))
            {
                throw new Exception("没有提供连接字符串");
            }
            using (DbConnection connection = Factory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;
                connection.Open();
                return ExecuteDataSet(connection, commandType, commandText, parameters);
            }
        }

        private static DataSet ExecuteDataSet(DbConnection connection, CommandType commandType, string commandText)
        {
            return ExecuteDataSet(connection, commandType, commandText);
        }

        public static DataSet ExecuteDataSet(DbTransaction transaction, CommandType commandType, string commandText)
        {
            return ExecuteDataSet(transaction, commandType, commandText, new DbParameter[1]);
        }

        private static DataSet ExecuteDataSet(DbConnection connection, CommandType commandType, string commandText, params DbParameter[] parameters)
        {
            if (connection == null)
            {
                throw new Exception("没有连接到数据库或者已断开");
            }
            DbCommand command = Factory.CreateCommand();
            bool mustCloseConnection = false;
            PrepareCommand(command, connection, null, commandType, commandText, parameters, out mustCloseConnection);
            using (DbDataAdapter adapter = Factory.CreateDataAdapter())
            {
                adapter.SelectCommand = command;
                DataSet dataSet = new DataSet();
                adapter.Fill(dataSet);
                _querycount++;
                command.Parameters.Clear();
                if (mustCloseConnection)
                {
                    connection.Close();
                }
                return dataSet;
            }
        }

        public static DataSet ExecuteDataSet(DbTransaction transaction, CommandType commandType, string commandText, params DbParameter[] parameters)
        {
            if (transaction == null)
            {
                throw new Exception("没有找到事务信息");
            }
            if ((transaction != null) && (transaction.Connection == null))
            {
                throw new Exception("事务被执行或者已回滚，请提供一个打开的事务");
            }
            DbCommand command = Factory.CreateCommand();
            bool mustCloseConnection = false;
            PrepareCommand(command, transaction.Connection, transaction, commandType, commandText, parameters, out mustCloseConnection);
            using (DbDataAdapter adapter = Factory.CreateDataAdapter())
            {
                adapter.SelectCommand = command;
                DataSet dataSet = new DataSet();
                adapter.Fill(dataSet);
                command.Parameters.Clear();
                return dataSet;
            }
        }

        public static DataSet ExecuteDataSetProc(string procName, params DbParameter[] parameters)
        {
            return ExecuteDataSet(CommandType.StoredProcedure, procName, parameters);
        }

        public static DataTable ExecuteDataTable(string commandText)
        {
            return ExecuteDataSet(commandText).Tables[0];
        }

        public static DataTable ExecuteDataTable(string commandText, params DbParameter[] parameters)
        {
            return ExecuteDataSet(commandText, parameters).Tables[0];
        }

        public static DataTable ExecuteDataTableProc(string procName, params DbParameter[] parameters)
        {
            return ExecuteDataSetProc(procName, parameters).Tables[0];
        }

        public static int ExecuteNonQuery(string commandText)
        {
            return ExecuteNonQuery(CommandType.Text, commandText, new DbParameter[1]);
        }

        public static int ExecuteNonQuery(string commandText, params DbParameter[] parameters)
        {
            return ExecuteNonQuery(CommandType.Text, commandText, parameters);
        }

        public static int ExecuteNonQuery(out int id, string commandText)
        {
            return ExecuteNonQuery(out id, CommandType.Text, commandText, new DbParameter[1]);
        }

        private static int ExecuteNonQuery(CommandType commandType, string commandText, params DbParameter[] parameters)
        {
            if ((ConnectionString == null) || (ConnectionString.Length == 0))
            {
                throw new Exception("没有提供连接字符串");
            }
            using (DbConnection connection = Factory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;
                connection.Open();
                return ExecuteNonQuery(connection, commandType, commandText, parameters);
            }
        }

        public static int ExecuteNonQuery(out int id, string commandText, params DbParameter[] parameters)
        {
            return ExecuteNonQuery(out id, CommandType.Text, commandText, parameters);
        }

        public static int ExecuteNonQuery(DbTransaction transaction, CommandType commandType, string commandText)
        {
            return ExecuteNonQuery(transaction, commandType, commandText, new DbParameter[1]);
        }

        private static int ExecuteNonQuery(DbConnection connection, CommandType commandType, string commandText, params DbParameter[] parameters)
        {
            if (connection == null)
            {
                throw new Exception("没有连接到数据库或者已断开");
            }
            DbCommand command = Factory.CreateCommand();
            bool mustCloseConnection = false;
            PrepareCommand(command, connection, null, commandType, commandText, parameters, out mustCloseConnection);
            int num = command.ExecuteNonQuery();
            command.Parameters.Clear();
            _querycount++;
            if (mustCloseConnection)
            {
                connection.Close();
            }
            return num;
        }

        public static int ExecuteNonQuery(out int id, CommandType commandType, string commandText, params DbParameter[] parameters)
        {
            if ((ConnectionString == null) || (ConnectionString.Length == 0))
            {
                throw new Exception("没有提供连接字符串");
            }
            using (DbConnection connection = Factory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;
                connection.Open();
                return ExecuteNonQuery(out id, connection, commandType, commandText, parameters);
            }
        }

        public static int ExecuteNonQuery(out int id, DbTransaction transaction, CommandType commandType, string commandText)
        {
            return ExecuteNonQuery(out id, transaction, commandType, commandText, new DbParameter[1]);
        }

        public static int ExecuteNonQuery(DbTransaction transaction, CommandType commandType, string commandText, params DbParameter[] parameters)
        {
            if (transaction == null)
            {
                throw new Exception("没有找到事务信息");
            }
            if ((transaction != null) && (transaction.Connection == null))
            {
                throw new Exception("事务被执行或者已回滚，请提供一个打开的事务");
            }
            DbCommand command = Factory.CreateCommand();
            bool mustCloseConnection = false;
            PrepareCommand(command, transaction.Connection, transaction, commandType, commandText, parameters, out mustCloseConnection);
            int num = command.ExecuteNonQuery();
            _querycount++;
            command.Parameters.Clear();
            return num;
        }

        private static int ExecuteNonQuery(out int id, DbConnection connection, CommandType commandType, string commandText, params DbParameter[] parameters)
        {
            if (connection == null)
            {
                throw new Exception("没有连接到数据库或者已断开");
            }
            DbCommand command = Factory.CreateCommand();
            bool mustCloseConnection = false;
            PrepareCommand(command, connection, null, commandType, commandText, parameters, out mustCloseConnection);
            int num = command.ExecuteNonQuery();
            command.Parameters.Clear();
            command.CommandType = CommandType.Text;
            command.CommandText = Provider.GetLastIdSql();
            id = (int)command.ExecuteScalar();
            _querycount++;
            if (mustCloseConnection)
            {
                connection.Close();
            }
            return num;
        }

        private static int ExecuteNonQuery(out int id, DbTransaction transaction, CommandType commandType, string commandText, params DbParameter[] parameters)
        {
            if (transaction == null)
            {
                throw new Exception("没有找到事务信息");
            }
            if ((transaction != null) && (transaction.Connection == null))
            {
                throw new Exception("事务被执行或者已回滚，请提供一个打开的事务");
            }
            DbCommand command = Factory.CreateCommand();
            bool mustCloseConnection = false;
            PrepareCommand(command, transaction.Connection, transaction, commandType, commandText, parameters, out mustCloseConnection);
            int num = command.ExecuteNonQuery();
            _querycount++;
            command.Parameters.Clear();
            command.CommandType = CommandType.Text;
            command.CommandText = Provider.GetLastIdSql();
            id = (int)command.ExecuteScalar();
            return num;
        }

        public static int ExecuteNonQueryProc(string procName, params DbParameter[] parameters)
        {
            return ExecuteNonQuery(CommandType.StoredProcedure, procName, parameters);
        }

        public static int ExecuteNonQueryProc(out int id, string procName, params DbParameter[] parameters)
        {
            return ExecuteNonQuery(out id, CommandType.StoredProcedure, procName, parameters);
        }

        public static DbDataReader ExecuteReader(string commandText)
        {
            return ExecuteReader(CommandType.Text, commandText);
        }

        private static DbDataReader ExecuteReader(CommandType commandType, string commandText)
        {
            return ExecuteReader(commandType, commandText, new DbParameter[1]);
        }

        public static DbDataReader ExecuteReader(string commandText, params DbParameter[] parameters)
        {
            return ExecuteReader(CommandType.Text, commandText, parameters);
        }

        private static DbDataReader ExecuteReader(CommandType commandType, string commandText, params DbParameter[] parameters)
        {
            DbDataReader reader;
            if ((ConnectionString == null) || (ConnectionString.Length == 0))
            {
                throw new Exception("没有提供连接字符串");
            }
            DbConnection connection = null;
            try
            {
                connection = Factory.CreateConnection();
                connection.ConnectionString = ConnectionString;
                connection.Open();
                reader = ExecuteReader(connection, null, commandType, commandText, parameters, DbConnectionOwnerShip.Internal);
                return reader;
            }
            catch (Exception ex)
            {
                if (connection != null)
                {
                    connection.Close();
                }
                //throw;
                throw new Exception(ex.Message);
            }

            return null;
        }

        private static DbDataReader ExecuteReader(DbConnection connection, CommandType commandType, string commandText)
        {
            return ExecuteReader(connection, commandType, commandText, new DbParameter[1]);
        }

        public static DbDataReader ExecuteReader(DbTransaction transaction, CommandType commandType, string commandText)
        {
            return ExecuteReader(transaction, commandType, commandText, new DbParameter[1]);
        }

        private static DbDataReader ExecuteReader(DbConnection connection, CommandType commandType, string commandText, params DbParameter[] parameters)
        {
            return ExecuteReader(connection, null, commandType, commandText, parameters, DbConnectionOwnerShip.External);
        }

        public static DbDataReader ExecuteReader(DbTransaction transaction, CommandType commandType, string commandText, params DbParameter[] parameters)
        {
            if (transaction == null)
            {
                throw new Exception("没有找到事务信息");
            }
            if ((transaction != null) && (transaction.Connection == null))
            {
                throw new Exception("事务被执行或者已回滚，请提供一个打开的事务");
            }
            return ExecuteReader(transaction.Connection, transaction, commandType, commandText, parameters, DbConnectionOwnerShip.External);
        }

        private static DbDataReader ExecuteReader(DbConnection connection, DbTransaction transaction, CommandType commandType, string commandText, DbParameter[] parameters, DbConnectionOwnerShip owership)
        {
            DbDataReader reader2;
            if (connection == null)
            {
                throw new Exception("没有连接到数据库或者已断开");
            }
            bool mustCloseConnection = false;
            DbCommand command = Factory.CreateCommand();
            try
            {
                DbDataReader reader;
                PrepareCommand(command, connection, transaction, commandType, commandText, parameters, out mustCloseConnection);
                if (owership == DbConnectionOwnerShip.External)
                {
                    reader = command.ExecuteReader();
                }
                else
                {
                    reader = command.ExecuteReader(CommandBehavior.CloseConnection);
                }
                _querycount++;
                bool flag2 = true;
                foreach (DbParameter parameter in command.Parameters)
                {
                    if (parameter.Direction != ParameterDirection.Input)
                    {
                        flag2 = false;
                    }
                }
                if (flag2)
                {
                    command.Parameters.Clear();
                }
                reader2 = reader;
            }
            catch
            {
                if (mustCloseConnection)
                {
                    connection.Close();
                }
                throw;
            }
            return reader2;
        }

        public static DbDataReader ExecuteReaderProc(string procName, params DbParameter[] parameters)
        {
            return ExecuteReader(CommandType.StoredProcedure, procName, parameters);
        }

        public static object ExecuteScalar(string commandText)
        {
            return ExecuteScalar(CommandType.Text, commandText);
        }

        private static object ExecuteScalar(CommandType commandType, string commandText)
        {
            return ExecuteScalar(commandType, commandText, new DbParameter[1]);
        }

        public static object ExecuteScalar(string commandText, params DbParameter[] parameters)
        {
            return ExecuteScalar(CommandType.Text, commandText, parameters);
        }

        private static object ExecuteScalar(CommandType commandType, string commandText, params DbParameter[] parameters)
        {
            if ((ConnectionString == null) || (ConnectionString.Length == 0))
            {
                throw new Exception("没有提供连接字符串");
            }
            using (DbConnection connection = Factory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;
                connection.Open();
                return ExecuteScalar(connection, commandType, commandText, parameters);
            }
        }

        public static object ExecuteScalar(DbTransaction transaction, CommandType commandType, string commandText)
        {
            return ExecuteScalar(transaction, commandType, commandText, new DbParameter[1]);
        }

        private static object ExecuteScalar(DbConnection connection, CommandType commandType, string commandText, params DbParameter[] parameters)
        {
            if (connection == null)
            {
                throw new Exception("没有连接到数据库或者已断开");
            }
            bool mustCloseConnection = false;
            DbCommand command = Factory.CreateCommand();
            PrepareCommand(command, connection, null, commandType, commandText, parameters, out mustCloseConnection);
            object obj2 = command.ExecuteScalar();
            command.Parameters.Clear();
            if (mustCloseConnection)
            {
                connection.Close();
            }
            return obj2;
        }

        public static object ExecuteScalar(DbTransaction transaction, CommandType commandType, string commandText, params DbParameter[] parameters)
        {
            if (transaction == null)
            {
                throw new Exception("没有找到事务信息");
            }
            if ((transaction != null) && (transaction.Connection == null))
            {
                throw new Exception("事务被执行或者已回滚，请提供一个打开的事务");
            }
            bool mustCloseConnection = false;
            DbCommand command = Factory.CreateCommand();
            PrepareCommand(command, transaction.Connection, transaction, commandType, commandText, parameters, out mustCloseConnection);
            object obj2 = command.ExecuteScalar();
            _querycount++;
            command.Parameters.Clear();
            return obj2;
        }

        public static object ExecuteScalarProc(string procName, params DbParameter[] parameters)
        {
            return ExecuteScalar(CommandType.StoredProcedure, procName, parameters);
        }

        public static bool Exists(string commandText)
        {
            return Exists(commandText, null);
        }

        public static bool Exists(string commandText, params DbParameter[] parameters)
        {
            return Exists(CommandType.Text, commandText, parameters);
        }

        private static bool Exists(CommandType commandType, string commandText, params DbParameter[] parameters)
        {
            int result = 0;
            object objTemp = ExecuteScalar(commandType, commandText, parameters);
            if (objTemp != null && objTemp != DBNull.Value)
            {
                if (Int32.TryParse(objTemp.ToString(), out result))
                    return result>0;
            }

            return false;
        }

        public static bool ExistsProc(string procedureName, params DbParameter[] parameters)
        {
            return Exists(CommandType.StoredProcedure, procedureName, parameters);
        }

        public static string FilterBadChar(string strchar)
        {
            if (string.IsNullOrEmpty(strchar))
            {
                return "";
            }
            return strchar.Replace("'", "");
        }

        public static int GetMaxID(string tableName, string columnName)
        {
            int result = 0;
            object objTemp = ExecuteScalar("SELECT MAX(" + columnName + ") FROM " + tableName);
            if (objTemp != null && objTemp != DBNull.Value)
            {
                if (Int32.TryParse(objTemp.ToString(), out result))
                    return result;
            }

            return 0;
        }

        public static int GetRecordCount(string strTableName, string strCondition, string strIndexField="*")
        {
            string sql = string.Format("SELECT COUNT({0}) FROM {1} ", strIndexField, strTableName);
            if (!string.IsNullOrEmpty(strCondition))
                sql += " where " + strCondition;

            return (int)ExecuteScalar(sql);
        }

        public static DbParameter MakeInParam(string ParamName, DbType DbType, int Size, object Value)
        {
            return MakeParam(ParamName, DbType, Size, ParameterDirection.Input, Value);
        }

        public static DbParameter MakeOutParam(string ParamName, DbType DbType, int Size)
        {
            return MakeParam(ParamName, DbType, Size, ParameterDirection.Output, null);
        }

        private static DbParameter MakeParam(string ParamName, DbType DbType, int Size, ParameterDirection Direction, object Value)
        {
            DbParameter parameter = Provider.MakeParam(ParamName, DbType, Size);
            parameter.Direction = Direction;
            if ((Direction != ParameterDirection.Output) || (Value != null))
            {
                parameter.Value = Value;
            }
            return parameter;
        }

        private static void PrepareCommand(DbCommand command, DbConnection connection, DbTransaction transaction, CommandType commandType, string commandText, DbParameter[] parameters, out bool mustCloseConnection)
        {
            if (command == null)
            {
                throw new Exception("DbCommand不能为空");
            }
            if ((commandText == null) || (commandText.Length == 0))
            {
                throw new Exception("参数commandText不能为空");
            }
            if (connection.State != ConnectionState.Open)
            {
                mustCloseConnection = true;
                connection.Open();
            }
            else
            {
                mustCloseConnection = false;
            }
            command.Connection = connection;
            command.CommandText = commandText;
            if (transaction != null)
            {
                if (transaction.Connection == null)
                {
                    throw new Exception("事务被执行或者已回滚，请提供一个打开的事务");
                }
                command.Transaction = transaction;
            }
            command.CommandType = commandType;
            if (parameters != null)
            {
                AttachParameters(command, parameters);
            }
        }
        /// <summary>
        /// 数据库连接
        /// </summary>
        public static string ConnectionString
        {
            get;
            set;
        }

        public static DbProviderFactory Factory
        {
            get
            {
                if (_factory == null)
                {
                    _factory = Provider.Instance();
                }
                return _factory;
            }
        }

        public static IDbProvider Provider
        {
            get
            {
                if (_provider == null)
                {
                    lock (lockHelper)
                    {
                        if (_provider == null)
                        {
                            try
                            {
                                _provider = new SqlServerProvider();
                            }
                            catch
                            {
                                throw new Exception("请检查Config/data.config中ConnectionStrings配置是否正确");
                            }
                        }
                    }
                }
                return _provider;
            }
        }

        public static int QueryCount
        {
            get
            {
                return _querycount;
            }
            set
            {
                _querycount = value;
            }
        }

        private enum DbConnectionOwnerShip
        {
            Internal,
            External
        }
    }
}

