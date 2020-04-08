using Edu.Entity;
namespace Edu.Data
{

    public partial class DRecentContacts :GenericRepository<RecentContacts>
	{
		EduContext db;
		public DRecentContacts(EduContext DbContext)
			: base(DbContext)
		{
			db = DbContext;
		}
	}
}
 