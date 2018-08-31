using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Dynamic;

namespace DAL
{
	public class BaseDal<T> where T : class, new()
	{
		public DbContext db = DbContextFactory.CreateCurrentDbContext();

		public IQueryable<T> LoadEntities(System.Linq.Expressions.Expression<Func<T, bool>> whereLambda)
		{
			return db.Set<T>().Where<T>(whereLambda);
		}

		public IQueryable<T> LoadEntities(string whereLambda, params object[] whereLambdaValues)
		{
			return db.Set<T>().Where<T>(whereLambda, whereLambdaValues);
		}

		public IQueryable<T> LoadPageEntities<S>(int pageIndex, int pageSize, out int totalCount, System.Linq.Expressions.Expression<Func<T, bool>> whereLambda, System.Linq.Expressions.Expression<Func<T, S>> orderbyLambda, bool isAsc)
		{
			var temp = db.Set<T>().Where<T>(whereLambda);
			totalCount = temp.Count();
			if (isAsc)
			{
				temp = temp.OrderBy<T, S>(orderbyLambda).Skip<T>((pageIndex - 1) * pageSize).Take<T>(pageSize);
			}
			else
			{
				temp = temp.OrderByDescending<T, S>(orderbyLambda).Skip<T>((pageIndex - 1) * pageSize).Take<T>(pageSize);
			}
			return temp;
		}
		public IQueryable<T> LoadPageEntities<S>(int pageIndex, int pageSize, out int totalCount, string whereLambda, System.Linq.Expressions.Expression<Func<T, S>> orderbyLambda, bool isAsc, params object[] whereLambdaValues)
		{
			var temp = db.Set<T>().Where<T>(whereLambda, whereLambdaValues);
			totalCount = temp.Count();
			if (isAsc)
			{
				temp = temp.OrderBy<T, S>(orderbyLambda).Skip<T>((pageIndex - 1) * pageSize).Take<T>(pageSize);
			}
			else
			{
				temp = temp.OrderByDescending<T, S>(orderbyLambda).Skip<T>((pageIndex - 1) * pageSize).Take<T>(pageSize);
			}
			return temp;
		}

		public bool DeleteEntity(T entity)
		{
			db.Entry<T>(entity).State = System.Data.EntityState.Deleted;
			return true;
		}
		public bool DeleteEntity(int id)
		{
			var entity = db.Set<T>().Find(id);
			db.Set<T>().Remove(entity);
			return true;
		}
		public bool DeleteEntity(List<long> ids)
		{
			foreach (var id in ids)
			{
				DeleteEntity(id);
			}
			return true;
		}

		public bool DeleteEntity(long id)
		{
			var entity = db.Set<T>().Find(id);
			db.Set<T>().Remove(entity);
			return true;
		}
		public bool DeleteEntity(Guid id)
		{
			var entity = db.Set<T>().Find(id);
			db.Set<T>().Remove(entity);
			return true;
		}
		public bool DeleteListByLogical(List<Guid> ids)
		{
			foreach (var id in ids)
			{
				var entity = db.Set<T>().Find(id);
				//修改当前DelFalg的值
				db.Entry(entity).Property("DelFlag").CurrentValue = (int)Model.DelFlag.逻辑删除状态;
				//把DelFalg列设置成修改状态
				db.Entry(entity).Property("DelFlag").IsModified = true;
			}
			return true;
		}

		public bool DeleteListByLogical(List<int> ids)
		{
			foreach (var id in ids)
			{
				var entity = db.Set<T>().Find(id);
				//修改当前DelFalg的值
				db.Entry(entity).Property("DelFlag").CurrentValue = (int)Model.DelFlag.逻辑删除状态;
				//把DelFalg列设置成修改状态
				db.Entry(entity).Property("DelFlag").IsModified = true;
			}
			return true;
		}
		public bool DeleteListByLogical(List<long> ids)
		{
			foreach (var id in ids)
			{
				var entity = db.Set<T>().Find(id);
				//修改当前DelFalg的值
				db.Entry(entity).Property("DelFlag").CurrentValue = (int)Model.DelFlag.逻辑删除状态;
				//把DelFalg列设置成修改状态
				db.Entry(entity).Property("DelFlag").IsModified = true;
			}
			return true;
		}

		public bool EditEntity(T entity)
		{
			db.Entry<T>(entity).State = System.Data.EntityState.Modified;
			return true;
		}

		public bool EditEntities(List<T> entities)
		{
			foreach (var item in entities)
			{
				db.Entry<T>(item).State = System.Data.EntityState.Modified;
			}
			return true;
		}

		public T AddEntity(T entity)
		{
			db.Set<T>().Add(entity);
			return entity;
		}

		public bool AddEnties(List<T> entities){
			foreach (var item in entities)
			{
				db.Set<T>().Add(item);
			}
			return true;
		}

        public int ExecuteSql(string sql, SqlParameter[] sp)
        {
            return db.Database.ExecuteSqlCommand(sql, sp);
        }

        public IEnumerable<T1> SqlQuery<T1>(string sql, SqlParameter[] sp)
        {
            return db.Database.SqlQuery<T1>(sql, sp);
        }
    }
}
