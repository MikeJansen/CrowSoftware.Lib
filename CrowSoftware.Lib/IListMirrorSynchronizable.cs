using System;

namespace CrowSoftware.Common
{
    public interface IListMirrorSynchronizable<T>: IComparable<T>
    {
        bool IsEnabled { get; set; }

        void Update(T master);

        int CompareKey(T master);
    }
}
