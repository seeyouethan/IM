using Edu.Entity;
namespace Edu.Data
{

    public partial class DUserNoticeSwitch : GenericRepository<UserNoticeSwitch>
	{
		EduContext db;
		public DUserNoticeSwitch(EduContext DbContext)
			: base(DbContext)
		{
			db = DbContext;
		}
	}
}
 