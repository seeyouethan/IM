using Edu.Entity;
namespace Edu.Data
{

    public partial class DImFriendGroup :GenericRepository<ImFriendGroup>
	{
		EduContext db;
		public DImFriendGroup(EduContext DbContext)
			: base(DbContext)
		{
			db = DbContext;
		}
	}
}
 