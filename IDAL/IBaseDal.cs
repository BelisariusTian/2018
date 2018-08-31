using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace IDAL
{
    public interface IBaseDal<T> where T : class, new()
    {
        IQueryable<T> LoadEntities(System.Linq.Expressions.Expression<Func<T, bool>> whereLambda);

        IQueryable<T> LoadEntities(string whereLambda, params object[] whereLambdaValues);

        IQueryable<T> LoadPageEntities<S>(int pageIndex, int pageSize, out int totalCount, System.Linq.Expressions.Expression<Func<T, bool>> whereLambda, System.Linq.Expressions.Expression<Func<T, S>> orderbyLambda, bool isAsc);
     
        IQueryable<T> LoadPageEntities<S>(int pageIndex, int pageSize, out int totalCount, string whereLambda, System.Linq.Expressions.Expression<Func<T, S>> orderbyLambda, bool isAsc, params object[] whereLambdaValues);

        bool DeleteEntity(T entity);
        bool DeleteEntity(Guid id);
        bool DeleteEntity(int id);
        bool DeleteEntity(List<long> ids);

        bool EditEntity(T entity);

		bool EditEntities(List<T> entities);

		bool DeleteListByLogical(List<Guid> ids);
        bool DeleteListByLogical(List<int> ids);
        bool DeleteListByLogical(List<long> ids);

        T AddEntity(T entity);
		bool AddEnties(List<T> entities);

		int ExecuteSql(string sql, SqlParameter[] sp);

        IEnumerable<T1> SqlQuery<T1>(string sql, SqlParameter[] sp);
    }
}
