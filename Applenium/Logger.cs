using System;
using System.Reflection;
using log4net;
using log4net.Config;

namespace Applenium
{
    /// <summary>
    ///     Logger - public class to deal with Logs
    /// </summary>
    public static class Logger
    {
        private static readonly ILog Ilogger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly bool IsDebugEnabled;
        private static readonly bool IsInfoEnabled;
        private static readonly bool IsWarnEnabled;
        private static readonly bool IsErrorEnabled;
        private static readonly bool IsFatalEnabled;

        static Logger()
        {
            XmlConfigurator.Configure();
            IsDebugEnabled = Ilogger.IsDebugEnabled;
            IsInfoEnabled = Ilogger.IsInfoEnabled;
            IsWarnEnabled = Ilogger.IsWarnEnabled;
            IsErrorEnabled = Ilogger.IsErrorEnabled;
            IsFatalEnabled = Ilogger.IsFatalEnabled;
        }

        /// <summary>
        ///     Debug is public function to report debug events
        /// </summary>
        public static void Debug(string message, Exception ex = null)
        {
            if (!IsDebugEnabled)
            {
                return;
            }
            if (ex == null)
            {
                Ilogger.Debug(message);
            }
            else
            {
                Ilogger.Debug(message, ex);
            }
        }

        /// <summary>
        ///     Info is public function to report Information events
        /// </summary>
        public static void Info(string message, Exception ex = null)
        {
            if (!IsInfoEnabled)
            {
                return;
            }
            if (ex == null)
            {
                using (ThreadContext.Stacks["Status"].Push("[INFO]"))
                {
                    Ilogger.Info(message);
                }
            }
            else
            {
                Ilogger.Info(message, ex);
            }
        }

        /// <summary>
        ///     public function to report warnings
        /// </summary>
        public static void Warn(string message, Exception ex = null)
        {
            if (!IsWarnEnabled)
            {
                return;
            }
            if (ex == null)
            {
                using (ThreadContext.Stacks["Status"].Push("[WARN]"))
                {
                    Ilogger.Warn(message);
                }
            }
            else
            {
                Ilogger.Warn(message, ex);
            }
        }

        /// <summary>
        ///     is public function to report error events
        /// </summary>
        public static void Error(string message, Exception ex = null)
        {
            if (!IsErrorEnabled)
            {
                return;
            }
            if (ex == null)
            {
                using (ThreadContext.Stacks["Status"].Push("[ERROR]"))
                {
                    Ilogger.Error(message);
                }
            }
            else
            {
                Ilogger.Error(message, ex);
            }
        }

        /// <summary>
        ///     Debug is public function to report fatal events
        /// </summary>
        public static void Fatal(string message, Exception ex = null)
        {
            if (!IsFatalEnabled)
            {
                return;
            }
            if (ex == null)
            {
                using (ThreadContext.Stacks["Status"].Push("[FATAL]"))
                {
                    Ilogger.Fatal(message);
                }

            }
            else
            {
                Ilogger.Fatal(message, ex);
            }
        }

        /// <summary>
        ///     To report passed test/step or scenario
        /// </summary>
        public static void Passed(string message)
        {
            if (!IsInfoEnabled)
            {
                return;
            }
            using (ThreadContext.Stacks["Status"].Push("[PASSED]"))
            {
                Ilogger.Info(message);
            }
        }

        /// <summary>
        ///     To report failed test/step or scenario
        /// </summary>
        public static void Failed(string message)
        {
            if (!IsErrorEnabled)
            {
                return;
            }
            using (ThreadContext.Stacks["Status"].Push("[FAILED]"))
            {
                Ilogger.Error(message);
            }
        }

        /// <summary>
        ///     To report Done passed test/step or scenario
        /// </summary>
        public static void Done(string message)
        {
            if (!IsInfoEnabled)
            {
                return;
            }
            using (ThreadContext.Stacks["Status"].Push("[DONE]"))
            {
                Ilogger.Info(message);
            }
        }
    }
}