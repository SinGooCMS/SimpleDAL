using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;

namespace SinGoo.Simple.DAL.Utils
{
    internal sealed class RequestParameter
    {
        #region 获取参数

        /// <summary>
        /// 无长度,值参数
        /// </summary>
        /// <param name="ParameterName"></param>
        /// <param name="DbType"></param>
        /// <param name="ParamDirection"></param>
        /// <returns></returns>
        private SqlParameter MakeParameter(string ParameterName, SqlDbType DbType, ParameterDirection ParamDirection)
        {
            SqlParameter SqlParam = new SqlParameter(ParameterName, DbType);
            SqlParam.Direction = ParamDirection;
            return SqlParam;
        }
        private OleDbParameter MakeParameter(string ParameterName, OleDbType DbType, ParameterDirection ParamDirection)
        {
            OleDbParameter OleParam = new OleDbParameter(ParameterName, DbType);
            OleParam.Direction = ParamDirection;
            return OleParam;
        }

        /// <summary>
        /// 无长度参数,如int,text等
        /// </summary>
        /// <param name="ParameterName"></param>
        /// <param name="DbType"></param>
        /// <param name="ParamDirection"></param>
        /// <param name="Values"></param>
        /// <returns></returns>
        private SqlParameter MakeParameter(string ParameterName, SqlDbType DbType, ParameterDirection ParamDirection, object Values)
        {
            SqlParameter SqlParam = new SqlParameter(ParameterName, DbType);
            SqlParam.Direction = ParamDirection;
            if (!(ParamDirection == ParameterDirection.Output && Values == null)) SqlParam.Value = Values;
            return SqlParam;
        }
        private OleDbParameter MakeParameter(string ParameterName, OleDbType DbType, ParameterDirection ParamDirection, object Values)
        {
            OleDbParameter OleParam = new OleDbParameter(ParameterName, DbType);
            OleParam.Direction = ParamDirection;
            if (!(ParamDirection == ParameterDirection.Output && Values == null)) OleParam.Value = Values;
            return OleParam;
        }

        /// <summary>
        /// 有长度参数,如varchar,char等
        /// </summary>
        /// <param name="ParameterName"></param>
        /// <param name="DbType"></param>
        /// <param name="ParamDirection"></param>
        /// <param name="ParamSize"></param>
        /// <param name="Values"></param>
        /// <returns></returns>
        private SqlParameter MakeParameter(string ParameterName, SqlDbType DbType, ParameterDirection ParamDirection, int ParamSize, object Values)
        {
            SqlParameter SqlParam = new SqlParameter(ParameterName, DbType, ParamSize);
            SqlParam.Direction = ParamDirection;
            if (!(ParamDirection == ParameterDirection.Output && Values == null)) SqlParam.Value = Values;
            return SqlParam;
        }
        private OleDbParameter MakeParameter(string ParameterName, OleDbType DbType, ParameterDirection ParamDirection, int ParamSize, object Values)
        {
            OleDbParameter OleParam = new OleDbParameter(ParameterName, DbType, ParamSize);
            OleParam.Direction = ParamDirection;
            if (!(ParamDirection == ParameterDirection.Output && Values == null)) OleParam.Value = Values;
            return OleParam;
        }

        /// <summary>
        /// 无长度输入参数
        /// </summary>
        /// <param name="ParameterName"></param>
        /// <param name="DbType"></param>
        /// <param name="Values"></param>
        /// <returns></returns>
        public SqlParameter MakeInParameter(string ParameterName, SqlDbType DbType, object Values)
        {
            return MakeParameter(ParameterName, DbType, ParameterDirection.Input, Values);
        }
        public OleDbParameter MakeInParameter(string ParameterName, OleDbType DbType, object Values)
        {
            return MakeParameter(ParameterName, DbType, ParameterDirection.Input, Values);
        }

        /// <summary>
        /// 有长度输入参数
        /// </summary>
        /// <param name="ParameterName"></param>
        /// <param name="DbType"></param>
        /// <param name="ParamSize"></param>
        /// <param name="Values"></param>
        /// <returns></returns>
        public SqlParameter MakeInParameter(string ParameterName, SqlDbType DbType, int ParamSize, object Values)
        {
            return MakeParameter(ParameterName, DbType, ParameterDirection.Input, ParamSize, Values);
        }
        public OleDbParameter MakeInParameter(string ParameterName, OleDbType DbType, int ParamSize, object Values)
        {
            return MakeParameter(ParameterName, DbType, ParameterDirection.Input, ParamSize, Values);
        }

        /// <summary>
        /// 无长度输出参数
        /// </summary>
        /// <param name="ParameterName"></param>
        /// <param name="DbType"></param>
        /// <returns></returns>
        public SqlParameter MakeOutParameter(string ParameterName, SqlDbType DbType)
        {
            return MakeParameter(ParameterName, DbType, ParameterDirection.Output);
        }
        public OleDbParameter MakeOutParameter(string ParameterName, OleDbType DbType)
        {
            return MakeParameter(ParameterName, DbType, ParameterDirection.Output);
        }

        #endregion
    }
}