using Castle.Windsor;

namespace CrowSoftware.Common.Container
{
    public interface IWindsorContainerManager: IContainerManager
    {
        IWindsorContainer WindsorContainer { get; }
    }
}
