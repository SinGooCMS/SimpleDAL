using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace SinGoo.Simple.DAL.Utils
{
    public class SqlServerProvider : IDbProvider
    {
        public void DeriveParameters(IDbCommand command)
        {
            if (command is SqlCommand)
            {
                SqlCommandBuilder.DeriveParameters(command as SqlCommand);
            }
        }

        public string GetLastIdSql()
        {
            return "SELECT SCOPE_IDENTITY()";
        }

        public DbProviderFactory Instance()
        {
            return SqlClientFactory.Instance;
        }

        public DbParameter MakeParam(string paramName, DbType dbType, int paramSize)
        {
            if (paramSize > 0)
            {
                return new SqlParameter(paramName, (SqlDbType)dbType, paramSize);
            }
            return new SqlParameter(paramName, (SqlDbType)dbType);
        }
    }
}

