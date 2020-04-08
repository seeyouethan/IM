using Edu.Entity;
namespace Edu.Data
{

    public partial class DConferenceMsg : GenericRepository<ConferenceMsg>
	{
		EduContext db;
		public DConferenceMsg(EduContext DbContext)
			: base(DbContext)
		{
			db = DbContext;
		}
	}
}
 