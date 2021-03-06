# SimpleDAL
一个简单的ORM，以前老项目用的

```c#
class Program
{
    static IDBFactory dbo = DataFactory.CreateDataFactory();
    static void Main(string[] args)
    {          
        UserInfo userAdd = new UserInfo() { 
            UserName="刘备",
            Gander="男",
            Age=56
        };
        int Id = dbo.InsertModel<UserInfo>(userAdd);
        Console.WriteLine(string.Format("添加一个用户[刘备]，返回ID：{0}\r\n", Id));

        var user=dbo.GetModel<UserInfo>(Id);
        Console.WriteLine(string.Format("根据主键读取对象：key={0}的用户名：{1},年龄：{2}\r\n", Id, user.UserName, user.Age));

        user.Age = 60;
        bool isUpdated = dbo.UpdateModel<UserInfo>(user);
        Console.WriteLine(string.Format("更新用户：{0}的年龄：{1}\r\n", user.UserName, user.Age));

        var lst = dbo.GetList<UserInfo>(5);
        foreach (var item in lst)
        {
            Console.WriteLine(string.Format("会员信息列表：{0}的年龄：{1}", item.UserName, item.Age));
        }

        int totalCount=0;
        int totalPage=0;
        int pageIndex = 2;
        int pageSize = 3;
        var pages = dbo.GetPager<UserInfo>("", "AutoID desc", pageIndex, pageSize, ref totalCount, ref totalPage);
        Console.WriteLine(string.Format("\r\n分页信息：总记录页{0} 总页数：{1}，当前第{2}页，每页显示{3}条记录",
            totalCount, totalPage, pageIndex, pageSize));
        foreach (var item in pages)
        {
            Console.WriteLine(string.Format("会员信息分页列表：{0}的年龄：{1}", item.UserName, item.Age));
        }
        
        //批量插入数据
        DataTable dt = new DataTable();
        dt.Columns.Add("ID", typeof(Int32));
        dt.Columns.Add("Name", typeof(System.String));
        dt.Columns.Add("Xinbie", typeof(string));
        var dr = dt.NewRow();
        dr["ID"] = 0; //ID是自增字段，这里只是起占位作用
        dr["Name"] = "小李";
        dr["Xinbie"] = "男";
        dt.Rows.Add(dr);
        var dr2 = dt.NewRow();
        dr2["ID"] = 0;
        dr2["Name"] = "阿飞";
        dr2["Xinbie"] = "男";
        dt.Rows.Add(dr2);
        var dr3 = dt.NewRow();
        dr3["ID"] = 0;
        dr3["Name"] = "林诗音";
        dr3["Xinbie"] = "女";
        dt.Rows.Add(dr3);
        dbo.BulkInsert<UserInfo>(dt);

        dbo.DeleteModel<UserInfo>(user);
        Console.WriteLine(string.Format("\r\n删除用户：{0}", user.AutoID));

        Console.ReadLine();
    }
}
