using Edu.Entity;
namespace Edu.Data
{

    public partial class DUserRole :GenericRepository<UserRole>
	{
		EduContext db;
		public DUserRole(EduContext DbContext)
			: base(DbContext)
		{
			db = DbContext;
		}
	}
}
 