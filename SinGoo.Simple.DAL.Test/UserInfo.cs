using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SinGoo.Simple.DAL;

namespace ConsoleTest
{
    [Table("cms_User")]
    public class UserInfo
    {
        [PrimaryKey]
        [Writeable(false)]
        public int AutoID { get; set; }
        public string UserName { get; set; }
        public string Gander { get; set; }
        public int Age { get; set; }
    }
}
