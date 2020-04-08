using Edu.Entity;
namespace Edu.Data
{

    public partial class DMenu :GenericRepository<Menu>
	{
		EduContext db;
		public DMenu(EduContext DbContext)
			: base(DbContext)
		{
			db = DbContext;
		}
	}
}
 