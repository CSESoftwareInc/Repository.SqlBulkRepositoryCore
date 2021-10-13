using System;

namespace CSESoftware.Repository.SqlBulkRepositoryCore.RepositoryHelpers
{
    internal class BulkRepositoryException : Exception
    {
        internal BulkRepositoryException(string message = "Error within the Bulk Repository.", Exception exception = null) :
            base(message, exception)
        {
        }
    }
}
