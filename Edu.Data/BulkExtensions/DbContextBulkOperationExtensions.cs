using System.Collections.Generic;
using System.Data.Entity;

namespace Edu.Data
{
  internal static class DbContextBulkOperationExtensions
  {
    public const int DefaultBatchSize = 1000;

    public static void BulkInsert<T>(this DbContext context, IEnumerable<T> entities, int batchSize = DefaultBatchSize)
    {
      var provider = new BulkOperationProvider(context);
      provider.Insert(entities, batchSize);
    }
  }
}
