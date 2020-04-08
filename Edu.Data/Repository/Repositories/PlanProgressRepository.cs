using Edu.Entity;
namespace Edu.Data
{

    public partial class DPlanProgress :GenericRepository<PlanProgress>
	{
		EduContext db;
		public DPlanProgress(EduContext DbContext)
			: base(DbContext)
		{
			db = DbContext;
		}
	}
}
 