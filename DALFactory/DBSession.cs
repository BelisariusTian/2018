using DAL;
using IDAL;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;

namespace DALFactory
{
    public partial class DBSession : IDBSession
    {
        public DbContext db
        {
            get
            {
                return DbContextFactory.CreateCurrentDbContext();
            }
        }

		public bool SaveChanges()
        {
            try
            {
                return db.SaveChanges() > 0;
            }
            catch (DbEntityValidationException ex)
            {
                throw ex;
            }   
        }



	}
}
