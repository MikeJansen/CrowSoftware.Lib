
namespace CrowSoftware.Common.Config
{
    public interface IConfigManager
    {
        void ProtectConnectionStrings();
        void ProtectSection(string section);
    }
}
