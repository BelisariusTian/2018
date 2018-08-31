using Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;

namespace DAL
{
    public class DbContextFactory
    {
        public static DbContext CreateCurrentDbContext()
        {
            DbContext dbContext = (DbContext)CallContext.GetData("dbContext");
            if (dbContext == null)
            {
                dbContext = new mcaaEntities();
                CallContext.SetData("dbContext", dbContext);
            }
            return dbContext;
        }
    }
}
