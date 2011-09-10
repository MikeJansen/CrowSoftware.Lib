using System;
using System.Reflection;

namespace CrowSoftware.Common.Container
{
    public static class ContainerUtil
    {
        private static IContainerManager containerManager;

        public static IContainerManager Initialize() { return Initialize(null, false); }

        public static IContainerManager Initialize(bool plain) { return Initialize(null, plain); }

        public static IContainerManager Initialize(Assembly rootAssembly) { return Initialize(rootAssembly, false); }

        public static IContainerManager Initialize(Assembly rootAssembly, bool plain)
        {
            containerManager = new WindsorContainerManager(rootAssembly, plain);
            return containerManager;
        }


        public static void Dispose()
        {
            ((IDisposable)containerManager).Dispose();
        }
    }
}
