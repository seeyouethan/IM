using Edu.Entity;
namespace Edu.Data
{

    public partial class DPlanDiscuss :GenericRepository<PlanDiscuss>
	{
		EduContext db;
		public DPlanDiscuss(EduContext DbContext)
			: base(DbContext)
		{
			db = DbContext;
		}
	}
}
 