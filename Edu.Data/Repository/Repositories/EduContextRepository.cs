using Edu.Entity;
namespace Edu.Data
{

    public partial class DEduContext :GenericRepository<EduContext>
	{
		EduContext db;
		public DEduContext(EduContext DbContext)
			: base(DbContext)
		{
			db = DbContext;
		}
	}
}
 