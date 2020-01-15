using System;

namespace SinGoo.Simple.DAL
{
    public class ConnStore
    {
        public static string DefConnStr
        {
            get
            {
                return System.Configuration.ConfigurationManager.ConnectionStrings["ConnStr"].ConnectionString;
            }
        }
    }
}
