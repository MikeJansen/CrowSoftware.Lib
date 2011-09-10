using System.Collections.Generic;

namespace CrowSoftware.Common
{
    public abstract class ListMirrorSynchronizer<T>
    {
        public abstract int CompareKey(T master, T mirror);
        public abstract int Compare(T master, T mirror);
        public abstract void Add(T master);
        public abstract void Delete(T mirror);
        public abstract void Update(T master, T mirror);
        public abstract void Commit();

        public void Synchronize(IEnumerable<T> master, IEnumerable<T> mirror)
        {
            IEnumerator<T> masterEnum = master.GetEnumerator();
            IEnumerator<T> mirrorEnum = mirror.GetEnumerator();
            bool haveMaster = masterEnum.MoveNext();
            bool haveMirror = mirrorEnum.MoveNext();

            while (haveMaster || haveMirror)
            {
                int keyCompare = 0;
                if (!haveMirror)
                {
                    keyCompare = -1;
                }
                else if (!haveMaster)
                {
                    keyCompare = 1;
                }
                else
                {
                    keyCompare = CompareKey(masterEnum.Current, mirrorEnum.Current);
                }
                
                if (keyCompare < 0)
                {
                    Add(masterEnum.Current);
                    haveMaster = masterEnum.MoveNext();
                }
                else if (keyCompare == 0)
                {
                    if (Compare(masterEnum.Current, mirrorEnum.Current) != 0)
                    {
                        Update(masterEnum.Current, mirrorEnum.Current);
                    }
                    haveMaster = masterEnum.MoveNext();
                    haveMirror = mirrorEnum.MoveNext();
                }
                else
                {
                    Delete(mirrorEnum.Current);
                    haveMirror = mirrorEnum.MoveNext();
                }

            }
            Commit();
        }

    }
}
