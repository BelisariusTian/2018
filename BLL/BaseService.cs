using DALFactory;
using IDAL;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace BLL
{
    public abstract class BaseService<T> where T : class, new()
    {
        public IDBSession GetDbSession
        {
            get
            {
                return DBSessionFactory.GetCurrentDbSession();
            }
        }

        public IBaseDal<T> CurrentDal { get; set; }

        public abstract void SetCurrentDal();

        public BaseService()
        {
            SetCurrentDal();
        }
        /// <summary>
        /// 加载数据
        /// </summary>
        /// <param name="whereLambda">常规表达式</param>
        /// <returns></returns>
        public IQueryable<T> LoadEntities(System.Linq.Expressions.Expression<Func<T, bool>> whereLambda)
        {
            return this.CurrentDal.LoadEntities(whereLambda);
        }
        /// <summary>
        /// 加载数据
        /// </summary>
        /// <param name="whereLambda">拼接型whereLambda</param>
        /// <param name="whereLambdaValues">拼接条件值</param>
        /// <returns></returns>
        public IQueryable<T> LoadEntities(string whereLambda, params object[] whereLambdaValues)
        {
            return this.CurrentDal.LoadEntities(whereLambda, whereLambdaValues);
        }
        /// <summary>
        /// 分页加载数据
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">加载条数</param>
        /// <param name="totalCount">总条数</param>
        /// <param name="whereLambda">常规表达式</param>
        /// <param name="orderbyLambda">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns></returns>
        public IQueryable<T> LoadPageEntities<S>(int pageIndex, int pageSize, out int totalCount, System.Linq.Expressions.Expression<Func<T, bool>> whereLambda, System.Linq.Expressions.Expression<Func<T, S>> orderbyLambda, bool isAsc)
        {
            return this.CurrentDal.LoadPageEntities<S>(pageIndex, pageSize, out totalCount, whereLambda, orderbyLambda, isAsc);
        }
        /// <summary>
        /// 分页加载数据
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">加载条数</param>
        /// <param name="totalCount">总条数</param>
        /// <param name="whereLambda">拼接型whereLambda</param>
        /// <param name="orderbyLambda">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="whereLambdaValues">拼接条件值</param>
        /// <returns></returns>
        public IQueryable<T> LoadPageEntities<S>(int pageIndex, int pageSize, out int totalCount, string whereLambda, System.Linq.Expressions.Expression<Func<T, S>> orderbyLambda, bool isAsc, params object[] whereLambdaValues)
        {
            return this.CurrentDal.LoadPageEntities<S>(pageIndex, pageSize, out totalCount, whereLambda, orderbyLambda, isAsc, whereLambdaValues);
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool DeleteEntity(T entity)
        {
            this.CurrentDal.DeleteEntity(entity);
            return this.GetDbSession.SaveChanges();
        }

        /// <summary>
        /// 根据int id删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool DeleteEntity(Guid id)
        {
            CurrentDal.DeleteEntity(id);
            return this.GetDbSession.SaveChanges();
        }
        /// <summary>
        /// 根据int id删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool DeleteEntity(int id)
        {
            CurrentDal.DeleteEntity(id);
            return this.GetDbSession.SaveChanges();
        }
        /// <summary>
        /// 根据ID集合批量删除
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public bool DeleteEntity(List<long> ids)
        {
            CurrentDal.DeleteEntity(ids);
            return GetDbSession.SaveChanges();
        }

        /// <summary>
        /// 根据guid批量删除
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public bool DeleteList(List<Guid> ids)
        {

            foreach (var id in ids)
            {
                CurrentDal.DeleteEntity(id);
            }

            return GetDbSession.SaveChanges();
        }
        /// <summary>
        /// 根据guid批量逻辑删除
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public bool DeleteListByLogical(List<Guid> ids)
        {
            CurrentDal.DeleteListByLogical(ids);
            return GetDbSession.SaveChanges();
        }
        /// <summary>
        /// 根据int批量逻辑删除
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public bool DeleteListByLogical(List<int> ids)
        {
            CurrentDal.DeleteListByLogical(ids);
            return GetDbSession.SaveChanges();
        }
        /// <summary>
        /// 根据ids批量逻辑删除
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public bool DeleteListByLogical(List<long> ids)
        {
            CurrentDal.DeleteListByLogical(ids);
            return GetDbSession.SaveChanges();
        }
        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool EditEntity(T entity)
        {
            this.CurrentDal.EditEntity(entity);
            return this.GetDbSession.SaveChanges();
        }


		public bool EditEntities(List<T> enties)
		{
			this.CurrentDal.EditEntities(enties);
			return this.GetDbSession.SaveChanges();
		}
		/// <summary>
		/// 添加
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		public T AddEntity(T entity)
        {
            this.CurrentDal.AddEntity(entity);
            this.GetDbSession.SaveChanges();
            return entity;
        }

		public bool AddEnties(List<T> enties) {
			this.CurrentDal.AddEnties(enties);
			return this.GetDbSession.SaveChanges();
		}
		/// <summary>
		/// 执行sql
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="sp"></param>
		/// <returns></returns>
		public int ExecuteSql(string sql, SqlParameter[] sp)
        {
            return this.CurrentDal.ExecuteSql(sql, sp);
        }
        /// <summary>
        /// 执行sql查询
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="sp"></param>
        /// <returns></returns>
        public IEnumerable<T1> SqlQuery<T1>(string sql, SqlParameter[] sp)
        {
            return this.CurrentDal.SqlQuery<T1>(sql, sp);
        }
    }
}
