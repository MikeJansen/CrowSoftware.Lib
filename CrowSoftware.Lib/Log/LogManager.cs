using System;
using System.Reflection;
using System.Text;
using Castle.Core.Logging;

namespace CrowSoftware.Common.Log
{
    public class LogManager : ILogManager
    {
        public ILoggerFactory LoggerFactory { get; set; }

        public void EnterMethod(ILogger logger, MethodBase currentMethod)
        {
            if (logger.IsDebugEnabled)
            {
                logger.DebugFormat("ENTER {0}.{1} ", currentMethod.DeclaringType.FullName, currentMethod.Name);
            }
        }

        public void ExitMethod(ILogger logger, MethodBase currentMethod)
        {
            if (logger.IsDebugEnabled)
            {
                logger.DebugFormat("EXIT {0}.{1} ", currentMethod.DeclaringType.FullName, currentMethod.Name);
            }
        }

        public ILogger GetLogger(Type type)
        {
            return LoggerFactory.Create(type);
        }

        public void DebugBinaryDump(ILogger logger, byte[] buffer, string format, params object[] args)
        {
            StringBuilder builder = new StringBuilder();
            StringBuilder hexBuilder = new StringBuilder();
            StringBuilder asciiBuilder = new StringBuilder();
            builder.AppendFormat(format, args);
            builder.AppendLine();
            for (int index = 0; index < buffer.Length; index++)
            {
                if (index > 0 && index % 16 == 0)
                {
                    builder.Append(hexBuilder.ToString());
                    builder.Append(asciiBuilder.ToString());
                    builder.AppendLine();
                    hexBuilder.Clear();
                    asciiBuilder.Clear();
                }
                hexBuilder.AppendFormat("{0:X2} ", buffer[index]);
                asciiBuilder.Append(Encoding.ASCII.GetString(buffer, index, 1));
            }
            if (hexBuilder.Length > 0)
            {
                builder.Append(hexBuilder.ToString());
                builder.Append(asciiBuilder.ToString());
                builder.AppendLine();
            }
            logger.Debug(builder.ToString());
        }
    }
}
