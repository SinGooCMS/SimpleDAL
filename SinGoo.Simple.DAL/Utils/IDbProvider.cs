using System;
using System.Data;
using System.Data.Common;

namespace SinGoo.Simple.DAL.Utils
{
    public interface IDbProvider
    {
        void DeriveParameters(IDbCommand command);
        string GetLastIdSql();
        DbProviderFactory Instance();
        DbParameter MakeParam(string paramName, DbType dbType, int paramSize);
    }
}

