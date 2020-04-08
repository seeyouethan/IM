using Edu.Entity;
namespace Edu.Data
{

    public partial class DUserDevice :GenericRepository<UserDevice>
	{
		EduContext db;
		public DUserDevice(EduContext DbContext)
			: base(DbContext)
		{
			db = DbContext;
		}
	}
}
 