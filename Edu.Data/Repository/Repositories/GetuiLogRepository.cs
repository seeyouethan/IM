using Edu.Entity;
namespace Edu.Data
{

    public partial class DGetuiLog : GenericRepository<GetuiLog>
	{
		EduContext db;
		public DGetuiLog(EduContext DbContext)
			: base(DbContext)
		{
			db = DbContext;
		}
	}
}
 