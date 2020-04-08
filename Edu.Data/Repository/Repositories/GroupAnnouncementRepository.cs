using Edu.Entity;
namespace Edu.Data
{

    public partial class DGroupAnnouncement : GenericRepository<GroupAnnouncement>
	{
		EduContext db;
		public DGroupAnnouncement(EduContext DbContext)
			: base(DbContext)
		{
			db = DbContext;
		}
	}
}
 