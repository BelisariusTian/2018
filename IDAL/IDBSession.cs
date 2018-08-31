using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace IDAL
{
    public partial interface IDBSession
    {
        DbContext db { get; }
		
        bool SaveChanges();


	}
}
