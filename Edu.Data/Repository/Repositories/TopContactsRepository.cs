using Edu.Entity;
namespace Edu.Data
{

    public partial class DTopContacts :GenericRepository<TopContacts>
	{
		EduContext db;
		public DTopContacts(EduContext DbContext)
			: base(DbContext)
		{
			db = DbContext;
		}
	}
}
 