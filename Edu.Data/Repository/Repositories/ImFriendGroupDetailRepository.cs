using Edu.Entity;
namespace Edu.Data
{

    public partial class DImFriendGroupDetail :GenericRepository<ImFriendGroupDetail>
	{
		EduContext db;
		public DImFriendGroupDetail(EduContext DbContext)
			: base(DbContext)
		{
			db = DbContext;
		}
	}
}
 