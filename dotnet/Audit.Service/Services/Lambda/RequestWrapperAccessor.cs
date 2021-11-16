using System;
using Audit.Service.Lambda;

namespace Audit.Service.Services.Lambda
{
    /// <summary>
    /// A simple wrapper around a APIGatewayProxyRequest instance.
    /// </summary>
    public class RequestWrapperAccessor : IRequestWrapperAccessor
    {
        [ThreadStatic]
        public static RequestWrapper LocalRequestWrapper;

        public RequestWrapper RequestWrapper
        {
            get => LocalRequestWrapper;
            set => LocalRequestWrapper = value;
        }
    }
}