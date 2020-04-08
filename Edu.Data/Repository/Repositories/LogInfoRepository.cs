using Edu.Entity;
namespace Edu.Data
{

    public partial class DLogInfo :GenericRepository<LogInfo>
	{
		EduContext db;
		public DLogInfo(EduContext DbContext)
			: base(DbContext)
		{
			db = DbContext;
		}
	}
}
 