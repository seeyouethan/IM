using Edu.Entity;
namespace Edu.Data
{

    public partial class DGroupNotice : GenericRepository<GroupNotice>
	{
		EduContext db;
		public DGroupNotice(EduContext DbContext)
			: base(DbContext)
		{
			db = DbContext;
		}
	}
}
 