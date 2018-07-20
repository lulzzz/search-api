using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Reflection.Emit;

namespace SearchV2.ApiCore
{
    public sealed class ActionDescriptor
    {
        public HttpMethod HttpMethod { get; private set; }
        public string Route { get; private set; }
        
        public MulticastDelegate Implementation { get; private set; }
        //public object ImplementationTarget { get; private set; }

        public IEnumerable<CustomAttributeBuilder> Attributes { get; private set; }


        public static ActionDescriptor Get<TIn, TOut>(string route, Func<TIn, TOut> action) => Make(route, HttpMethod.Get, action);
        public static ActionDescriptor Get<TOut>(string route, Func<TOut> action) => Make(route, HttpMethod.Get, action);

        public static ActionDescriptor Post<TIn, TOut>(string route, Func<TIn, TOut> action) => Make(route, HttpMethod.Post, action);

        public static ActionDescriptor Delete<TIn, TOut>(string route, Func<TIn, TOut> action) => Make(route, HttpMethod.Delete, action);

        //public static ActionDescriptor Make<TIn, TOut>(string route, HttpMethod httpMethod, Func<TIn, TOut> action) => Make(route, httpMethod, action);
        public static ActionDescriptor Make(string route, HttpMethod httpMethod, MulticastDelegate action)
            => new ActionDescriptor { HttpMethod = httpMethod, Route = route, Implementation = action, Attributes = new List<CustomAttributeBuilder>() };
    }

    public static class ActionDescriptorExtensions
    {
        public static ActionDescriptor Authorize(this ActionDescriptor d, string policy)
        {
            var attrs = d.Attributes as List<CustomAttributeBuilder>;
            attrs.Add(new CustomAttributeBuilder(typeof(AuthorizeAttribute).GetConstructor(new Type[] { typeof(string) }), new object[] { policy }));
            return d;
        }
    }
}
