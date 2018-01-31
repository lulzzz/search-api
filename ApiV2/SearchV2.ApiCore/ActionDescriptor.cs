using System;
using System.Net.Http;
using System.Reflection;

namespace SearchV2.ApiCore
{
    public sealed class ActionDescriptor
    {
        public HttpMethod HttpMethod { get; private set; }
        public string Route { get; private set; }
        
        public MulticastDelegate Implementation { get; private set; }
        //public object ImplementationTarget { get; private set; }

        public static ActionDescriptor Get<TIn, TOut>(string route, Func<TIn, TOut> action) => Make(route, HttpMethod.Get, action);
        public static ActionDescriptor Post<TIn, TOut>(string route, Func<TIn, TOut> action) => Make(route, HttpMethod.Post, action);

        //public static ActionDescriptor Make<TIn, TOut>(string route, HttpMethod httpMethod, Func<TIn, TOut> action) => Make(route, httpMethod, action);
        public static ActionDescriptor Make(string route, HttpMethod httpMethod, MulticastDelegate action)
            => new ActionDescriptor { HttpMethod = httpMethod, Route = route, Implementation = action };
    }
}
