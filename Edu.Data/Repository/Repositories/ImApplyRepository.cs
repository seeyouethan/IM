using Edu.Entity;
namespace Edu.Data
{

    public partial class DImApply :GenericRepository<ImApply>
	{
		EduContext db;
		public DImApply(EduContext DbContext)
			: base(DbContext)
		{
			db = DbContext;
		}
	}
}
 