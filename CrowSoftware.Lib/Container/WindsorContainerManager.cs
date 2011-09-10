using System;
using System.Reflection;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using CrowSoftware.Lib.Container;

namespace CrowSoftware.Common.Container
{
    public class WindsorContainerManager : IWindsorContainerManager, IDisposable
    {
        private IWindsorContainer container;

        public WindsorContainerManager() : this(null, false) { }

        public WindsorContainerManager(Assembly rootAssembly, bool plain)
        {
            container = new WindsorContainer();
            container.Register(Component.For<IContainerManager>().Instance(this));
            if (!plain)
            {
                container.Install(FromAssembly2.Referenced(rootAssembly));
            }
        }

        public T GetInstance<T>()
        {
            return container.Resolve<T>();
        }

        public T GetInstance<T>(string key)
        {
            return container.Resolve<T>(key);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                container.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        public void Register(Type iface, Type implementation)
        {
            container.Register(Component.For(iface).ImplementedBy(implementation).LifeStyle.Transient);
        }

        public void Register(Type iface, object instance)
        {
            container.Register(Component.For(iface).Instance(instance));
        }


        public void Register<TIface, TImpl>()
            where TIface : class
            where TImpl : class,TIface
        {
            container.Register(Component.For<TIface>().ImplementedBy<TImpl>().LifeStyle.Transient);
        }

        public void Register<T>(T instance) where T : class
        {
            container.Register(Component.For<T>().Instance(instance));
        }

        public void Register<TIface, TImpl>(string key)
            where TIface : class
            where TImpl : class,TIface
        {
            container.Register(Component.For<TIface>().ImplementedBy<TImpl>().LifeStyle.Transient.Named(key));
        }

        public void Release(object instance)
        {
            container.Release(instance);
        }

        public IWindsorContainer WindsorContainer
        {
            get { return container; }
        }
    }
}
