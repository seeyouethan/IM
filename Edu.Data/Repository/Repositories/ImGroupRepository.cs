using Edu.Entity;
namespace Edu.Data
{

    public partial class DImGroup :GenericRepository<ImGroup>
	{
		EduContext db;
		public DImGroup(EduContext DbContext)
			: base(DbContext)
		{
			db = DbContext;
		}
	}
}
 