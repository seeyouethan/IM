using Edu.Entity;
namespace Edu.Data
{

    public partial class DPlan :GenericRepository<Plan>
	{
		EduContext db;
		public DPlan(EduContext DbContext)
			: base(DbContext)
		{
			db = DbContext;
		}
	}
}
 