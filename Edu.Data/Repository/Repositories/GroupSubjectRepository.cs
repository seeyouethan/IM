using Edu.Entity;
namespace Edu.Data
{

    public partial class DGroupSubject : GenericRepository<GroupSubject>
	{
		EduContext db;
		public DGroupSubject(EduContext DbContext)
			: base(DbContext)
		{
			db = DbContext;
		}
	}
}
 