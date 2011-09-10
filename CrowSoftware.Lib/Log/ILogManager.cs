using System;
using System.Reflection;
using Castle.Core.Logging;

namespace CrowSoftware.Common.Log
{
    public interface ILogManager
    {
        void EnterMethod(ILogger logger, MethodBase currentMethod);
        void ExitMethod(ILogger logger, MethodBase currentMethod);
        ILogger GetLogger(Type type);
        void DebugBinaryDump(ILogger logger, byte[] buffer, string format, params object[] args);
    }
}
