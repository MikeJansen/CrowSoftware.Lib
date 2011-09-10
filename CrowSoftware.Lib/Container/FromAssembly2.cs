using System.Collections.Generic;
using System.Reflection;
using Castle.MicroKernel.Registration;
using Castle.Windsor.Installer;

namespace CrowSoftware.Lib.Container
{
    /// <summary>
    /// Additional methods for getting installers from assemblies for Castle Windsor
    /// </summary>
    /// <remarks>
    /// <para>
    /// Integrate into Castle Windsor eventually and request pull
    /// </para>
    /// <para>
    /// TODO: Provide a fluent interface to set options like excluding assemblies, etc.
    /// </para>
    /// </remarks>
    public static class FromAssembly2
    {
        /// <summary>
        /// Search for installers in assemblies referenced by the entry assembly (do not use for ASP.NET
        /// or other applicatons running in an unmanaged host.)
        /// </summary>
        /// <returns>Composite installer with all referenced assemblies</returns>
        public static IWindsorInstaller Referenced() { return Referenced(null); }

        /// <summary>
        /// Search for installers in assemblies referenced by the specified assembly (use for ASP.NET, e.g. 
        /// in Application_Start)
        /// </summary>
        /// <param name="rootAssembly"></param>
        /// <returns>Composite installer with all referenced assemblies</returns>
        public static IWindsorInstaller Referenced(Assembly rootAssembly)
        {
            rootAssembly = rootAssembly ?? Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly() ?? Assembly.GetExecutingAssembly();
            Dictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();
            GetReferencedAssemblies(assemblies, rootAssembly);

            CompositeInstaller installer = new CompositeInstaller();
            foreach (Assembly assembly in assemblies.Values)
            {
                installer.Add(FromAssembly.Instance(assembly));
            }
            return installer;
        }

        private static void GetReferencedAssemblies(Dictionary<string, Assembly> assemblies, Assembly assembly)
        {

            assemblies[assembly.FullName] = assembly;
            foreach (AssemblyName referencedAssemblyName in assembly.GetReferencedAssemblies())
            {
                if (!assemblies.ContainsKey(referencedAssemblyName.FullName) &&
                    IsIncludedAssembly(referencedAssemblyName))
                {
                    Assembly referencedAssembly = Assembly.Load(referencedAssemblyName);
                    assemblies[referencedAssembly.FullName] = referencedAssembly;
                    GetReferencedAssemblies(assemblies, referencedAssembly);
                }
            }
        }

        private static bool IsIncludedAssembly(AssemblyName referencedAssembly)
        {
            bool isIncluded = true;
            string name = referencedAssembly.Name;

            if (name == "mscorlib" ||
                name == "System" ||
                name == "EntityFramework" ||
                name.StartsWith("System.") ||
                name.StartsWith("Microsoft.") ||
                name.StartsWith("Castle."))
                isIncluded = false;

            return isIncluded;
        }
    }
}
