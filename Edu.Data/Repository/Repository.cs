using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Data.Entity;
using System.Reflection;
using System.Data;
using System.Web;

using Edu.Entity;
using Edu.Tools;

namespace Edu.Data
{
    public class UpdateClass<T> where T : class
    {
        public T GetT(T todl, T tNew, List<string> strForm)
        {
            foreach (System.Reflection.PropertyInfo p in tNew.GetType().GetProperties())
            {
                if (strForm.Contains(p.Name))
                {
                    object newobj = p.GetValue(tNew, null);
                    object oldobj = p.GetValue(todl, null);
                    p.SetValue(todl, newobj, null);
                }

            }
            return todl;
        }
    }

    public class GenericRepository<TEntity> where TEntity : class
    {


        protected EduContext context;
        public DbSet<TEntity> dbSet;
        public GenericRepository(EduContext context)
        {
            this.context = context;
            this.dbSet = context.Set<TEntity>();
        }

        public virtual IQueryable<TEntity> GetIQueryable(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "")
        {
            IQueryable<TEntity> query = dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                return orderBy(query);
            }
            else
            {
                return query;
            }
        }

        public virtual IEnumerable<TEntity> Get(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "")
        {
            IQueryable<TEntity> query = dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                return orderBy(query).ToList();
            }
            else
            {
                return query.ToList();
            }
        }

        public virtual int GetCount(
       Expression<Func<TEntity, bool>> filter = null,
       Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
       string includeProperties = "")
        {
            IQueryable<TEntity> query = dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                return orderBy(query).Count();
            }
            else
            {
                return query.Count();
            }
        }
        public virtual int GetAllCount()
        {
            return dbSet.Count();
        }
        /// <summary>
        /// 得到分页实体
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <param name="paging"></param>
        /// <returns></returns>
        public virtual Paging GetPaging(
              Expression<Func<TEntity, bool>> filter,
             Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy,
            int pageNo, int PageSiz = 0
            )
        {
            Paging paging = new Paging();
            paging.PageNumber = pageNo;
            if (PageSiz != 0)
            {
                paging.PageSiz = PageSiz;
            }
            IQueryable<TEntity> query = dbSet;
            if (filter != null)
            {
                query = query.Where(filter);
            }
            paging.Amount = query.Count();
            paging.Entity = orderBy(query).Skip(paging.PageSiz * paging.PageNumber).Take(paging.PageSiz);
            paging.EntityList = orderBy(query).Skip(paging.PageSiz * paging.PageNumber).Take(paging.PageSiz).ToList(); ;
            return paging;
        }

        public virtual IQueryable<TEntity> GetAll()
        {
            return dbSet;
        }

        public virtual TEntity GetByID(object id)
        {
            return dbSet.Find(id);
        }

        public virtual void Insert(TEntity entity)
        {
            dbSet.Add(entity);
        }
        public virtual void Delete(object id)
        {
          
            TEntity entityToDelete = dbSet.Find(id);
            Delete(entityToDelete);
        }

        public virtual void Delete(TEntity entityToDelete)
        {
            if (context.Entry(entityToDelete).State == System.Data.Entity.EntityState.Detached)
            {
                dbSet.Attach(entityToDelete);
            }
            dbSet.Remove(entityToDelete);

        }

        public void Delete(IEnumerable<TEntity> records)
        {
            foreach (TEntity record in records)
            {
                Delete(record);
            }
        }
        public void Delete(Expression<Func<TEntity, bool>> criteria)
        {
            IEnumerable<TEntity> records = this.Get(criteria);
            foreach (TEntity record in records)
            {

                Delete(record);
            }
        }

        public virtual void Update(TEntity entityToUpdate)
        {

            dbSet.Attach(entityToUpdate);
            context.Entry(entityToUpdate).State = System.Data.Entity.EntityState.Modified;
        }
        public virtual TEntity Update(TEntity tOldObj, TEntity tNewObj, List<string> strForm)
        {
            var query = new UpdateClass<TEntity>().GetT(tOldObj, tNewObj, strForm);
            return query;
        }
        public virtual TEntity Update(TEntity tOldObj, TEntity tNewObj, string[] strForm)
        {
            var query = new UpdateClass<TEntity>().GetT(tOldObj, tNewObj, new List<string>(strForm.AsEnumerable()));
            return query;
        }
        public virtual IEnumerable<TEntity> GetWithRawSql(string query, params object[] parameters)
        {
            return dbSet.SqlQuery(query, parameters).ToList();
        }
        public virtual void BulkInsert<T>(IEnumerable<T> entities, int batchSize = 1000)
        {
            var provider = new BulkOperationProvider(context);
            provider.Insert(entities, batchSize);
        }


    }
}
