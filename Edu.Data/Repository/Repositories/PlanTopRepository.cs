using Edu.Entity;
namespace Edu.Data
{

    public partial class DPlanTop :GenericRepository<PlanTop>
	{
		EduContext db;
		public DPlanTop(EduContext DbContext)
			: base(DbContext)
		{
			db = DbContext;
		}
	}
}
 