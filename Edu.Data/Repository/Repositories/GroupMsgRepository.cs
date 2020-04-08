using Edu.Entity;
namespace Edu.Data
{

    public partial class DGroupMsg :GenericRepository<GroupMsg>
	{
		EduContext db;
		public DGroupMsg(EduContext DbContext)
			: base(DbContext)
		{
			db = DbContext;
		}
	}
}
 