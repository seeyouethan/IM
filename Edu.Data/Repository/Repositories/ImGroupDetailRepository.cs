using Edu.Entity;
namespace Edu.Data
{

    public partial class DImGroupDetail :GenericRepository<ImGroupDetail>
	{
		EduContext db;
		public DImGroupDetail(EduContext DbContext)
			: base(DbContext)
		{
			db = DbContext;
		}
	}
}
 