using Edu.Entity;
namespace Edu.Data
{

    public partial class DIMMsg :GenericRepository<IMMsg>
	{
		EduContext db;
		public DIMMsg(EduContext DbContext)
			: base(DbContext)
		{
			db = DbContext;
		}
	}
}
 