using Edu.Entity;
namespace Edu.Data
{

    public partial class DConferenceDiscuss : GenericRepository<ConferenceDiscuss>
	{
		EduContext db;
		public DConferenceDiscuss(EduContext DbContext)
			: base(DbContext)
		{
			db = DbContext;
		}
	}
}
 