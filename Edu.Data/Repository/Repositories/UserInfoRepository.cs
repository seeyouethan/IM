using Edu.Entity;
namespace Edu.Data
{

    public partial class DUserInfo :GenericRepository<UserInfo>
	{
		EduContext db;
		public DUserInfo(EduContext DbContext)
			: base(DbContext)
		{
			db = DbContext;
		}
	}
}
 