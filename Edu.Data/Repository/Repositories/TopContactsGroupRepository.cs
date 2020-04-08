using Edu.Entity;
namespace Edu.Data
{

    public partial class DTopContactsGroup :GenericRepository<TopContactsGroup>
	{
		EduContext db;
		public DTopContactsGroup(EduContext DbContext)
			: base(DbContext)
		{
			db = DbContext;
		}
	}
}
 