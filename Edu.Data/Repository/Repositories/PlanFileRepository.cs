using Edu.Entity;
namespace Edu.Data
{

    public partial class DPlanFile :GenericRepository<PlanFile>
	{
		EduContext db;
		public DPlanFile(EduContext DbContext)
			: base(DbContext)
		{
			db = DbContext;
		}
	}
}
 