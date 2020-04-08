using Edu.Entity;
using Edu.Tools;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Edu.Service
{

    public partial class UnitOfWork : IDisposable
    {

        public EduContext context { get; set; }
        public UnitOfWork()
        {
            context = new EduContext();
        }
        #region 数据存储
        public OperationResult Save()
        {
            try
            {
                if (context.SaveChanges() > 0)
                {
                    return new OperationResult(OperationResultType.Success, "操作成功。");
                }
                return new OperationResult(OperationResultType.NoChanged, "未发生任何改变。");
            }
            catch (DbUpdateException e)
            {
                if (e.InnerException != null && e.InnerException.InnerException is SqlException)
                {
                    SqlException sqlEx = e.InnerException.InnerException as SqlException;
                    string msg = DataHelper.GetSqlExceptionMessage(sqlEx.Number);
                    throw PublicHelper.ThrowDataAccessException("提交数据更新时发生异常：" + msg, sqlEx);
                }
                throw;
            }

        }
        /// <summary>
        /// 保存并返回信息
        /// </summary>
        /// <returns></returns>
        public string SaveRMsg()
        {
            try
            {
                int s = context.SaveChanges();
                if (s > 0)
                {
                    return true.ToString();
                }
                else
                {
                    return "没有更改";
                }

            }
            catch (DbEntityValidationException ex)
            {
                string Message = "error:";
                if (ex.InnerException == null)
                    Message += ex.Message + ",";
                else if (ex.InnerException.InnerException == null)
                    Message += ex.InnerException.Message + ",";
                else if (ex.InnerException.InnerException.InnerException == null)
                    Message += ex.InnerException.InnerException.Message + ",";
                return Message;
            }
            //catch (Exception ex)
            //{
            //    return ex.Message;
            //}
            //catch (DbEntityValidationException dbEx) { 
            //    return dbEx.Message;
            //}
        }
        #endregion

        #region 资源释放
        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
