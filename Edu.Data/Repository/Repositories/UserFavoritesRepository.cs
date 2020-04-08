using Edu.Entity;
namespace Edu.Data
{

    public partial class DUserFavorites : GenericRepository<UserFavorites>
	{
		EduContext db;
		public DUserFavorites(EduContext DbContext)
			: base(DbContext)
		{
			db = DbContext;
		}
	}
}
 