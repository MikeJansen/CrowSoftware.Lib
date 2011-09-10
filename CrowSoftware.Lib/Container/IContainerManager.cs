
using System;
namespace CrowSoftware.Common.Container
{
    public interface IContainerManager
    {
        T GetInstance<T>();
        T GetInstance<T>(string key);
        void Register(Type iface, Type implementation);
        void Register(Type iface, object instance);
        void Register<TIface, TImpl>() where TIface:class where TImpl : class,TIface;
        void Register<T>(T instance) where T : class;
        void Register<TIface, TImpl>(string key) where TIface:class where TImpl:class,TIface;
        void Release(object instance);
    }
}
