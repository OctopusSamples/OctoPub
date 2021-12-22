using System;
using System.Reflection;
using Audit.Service.Domain.Interceptor;
using MethodDecorator.Fody.Interfaces;
using NLog;

[module: LogMethod]

namespace Audit.Service.Domain.Interceptor
{
    /// <summary>
    /// A Fody interceptor that logs when a method is entered, exited, or throws an exception.
    /// </summary>
    public class LogMethodAttribute : Attribute, IMethodDecorator
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private MethodBase? method;

        /// <summary>
        /// Instance, method and args can be captured here and stored in attribute instance fields.
        /// </summary>
        /// <param name="instance">The instance being called.</param>
        /// <param name="method">The method being called.</param>
        /// <param name="args">The method args.</param>
        public void Init(object instance, MethodBase method, object[] args)
        {
            this.method = method;
        }

        /// <summary>
        /// Called when a method is entered.
        /// </summary>
        public void OnEntry()
        {
            Logger.Trace("Entering into {0}", method?.DeclaringType?.Name + " " + method?.Name);
        }

        /// <summary>
        /// Called when a method is exited.
        /// </summary>
        public void OnExit()
        {
            Logger.Trace("Exiting into {0}", method?.DeclaringType?.Name + " " + method?.Name);
        }

        /// <summary>
        /// Called when an exception is thrown.
        /// </summary>
        /// <param name="exception">The exception that was thrown.</param>
        public void OnException(Exception exception)
        {
            Logger.Trace(exception, "Exception {0}", method?.DeclaringType?.Name + " " + method?.Name);
        }
    }
}