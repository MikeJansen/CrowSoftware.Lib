using System.Data.Common;

namespace CrowSoftware.Common
{
    public interface IDataReaderInitializable
    {
        void Initialize(DbDataReader reader);
    }
}
