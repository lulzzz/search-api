using Microsoft.AspNetCore.Mvc;
using SearchV2.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using static SearchV2.ApiCore.Api;

namespace SearchV2.ApiCore
{
    [Route("molecules")]
    [MoleculesControllerNameConvention]
    public abstract class ControllerCore<TId, TFilterQuery, TData> where TData : IWithReference<TId>
    {
        internal static string CatPropName => nameof(ControllerCore<TId, TFilterQuery, TData>._catalogDb);

        protected ICatalogDb<TId, TFilterQuery, TData> _catalogDb;

        [HttpGet]
        [Route("{id}")]
        public Task<TData> One([FromRoute]TId id) => _catalogDb.OneAsync(id);

        // sample

        //public ControllerCore(ISearchStrategy<TSearchQuery, TFilterQuery> _searchStrategy)
        //{
        //    searchStrategy = _searchStrategy;
        //}

        //private ISearchStrategy<TSearchQuery, TFilterQuery> searchStrategy;

        //[HttpGet]
        //[Route("search/{searchType}")]
        //public Task<TData> Find(SearchRequest<TSearchQuery, TFilterQuery> request) 
        //    => 
        //    this.searchStrategy
        //    .FindAsync(
        //        request.Query.Search, 
        //        request.Query.Filters, 
        //        request.PageNumber, 
        //        request.PageSize);

    }

    public class SearchRequest<TSearchQuery, TFilterQuery>
    {
        public class Body
        {
            public TFilterQuery Filters { get; set; }
            public TSearchQuery Search { get; set; }
        }

        [FromQuery]
        public int PageSize { get; set; } = 12;

        [FromQuery]
        public int PageNumber { get; set; } = 1;

        [FromBody]
        public Body Query { get; set; }
    }

    public static class ControllerBuilder
    {
        static readonly ModuleBuilder mb = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("SearchV2.ApiCore.Dynamic"), AssemblyBuilderAccess.Run)
            .DefineDynamicModule("MainModule");


        static TypeBuilder CreateTypeBuilder(this ModuleBuilder moduleBuilder, string typename)
            => moduleBuilder.DefineType(typename,
                TypeAttributes.Public |
                TypeAttributes.Class |
                TypeAttributes.AutoClass |
                TypeAttributes.AnsiClass |
                TypeAttributes.BeforeFieldInit |
                TypeAttributes.AutoLayout |
                TypeAttributes.Sealed,
                null);
        

        public static TypeInfo CreateControllerClass<TId, TFilterQuery, TData>(ICatalogDb<TId, TFilterQuery, TData> catalog, params SearchRegistration<TId>[] searches) where TData : IWithReference<TId>
        {
            var type = mb.CreateTypeBuilder("MoleculesController");
            var baseType = typeof(ControllerCore<TId, TFilterQuery, TData>);
            type.SetParent(baseType);

            
            var strategies = new(Type type, FieldInfo field)[searches.Length];

            for (int i = 0; i < searches.Length; i++)
            {
                var item = searches[i];
                var tSearchQuery = item._type.GetGenericArguments()[1];
                var strategyType = typeof(ISearchStrategy<,>).MakeGenericType(tSearchQuery, typeof(TFilterQuery));
                var strategyField = type.DefineField("str" + i, strategyType, FieldAttributes.Private);

                var requestType = typeof(SearchRequest<,>).MakeGenericType(tSearchQuery, typeof(TFilterQuery));
                var requestBodyType = requestType.GetNestedType("Body");

                var actionBuilder = type.DefineMethod(
                    "Find" + i, 
                    MethodAttributes.Public, 
                    CallingConventions.HasThis, 
                    typeof(Task<object>),
                    new[] { requestType });
                actionBuilder.SetCustomAttribute(
                    new CustomAttributeBuilder(
                        typeof(HttpPostAttribute).GetConstructors().Single(c => c.GetParameters().Length == 1 && c.GetParameters()[0].ParameterType == typeof(string)),
                        new object[] { $"search/{item._routeSuffix}" }));

                var aIL = actionBuilder.GetILGenerator();
                aIL.Emit(OpCodes.Ldarg_0);
                aIL.Emit(OpCodes.Ldfld, strategyField);
                aIL.Emit(OpCodes.Ldarg_1);
                aIL.Emit(OpCodes.Call, requestType.GetProperty("Query").GetMethod);
                aIL.Emit(OpCodes.Call, requestBodyType.GetProperty("Search").GetMethod);
                aIL.Emit(OpCodes.Ldarg_1);
                aIL.Emit(OpCodes.Call, requestType.GetProperty("Query").GetMethod);
                aIL.Emit(OpCodes.Call, requestBodyType.GetProperty("Filters").GetMethod);
                aIL.Emit(OpCodes.Ldarg_1);
                aIL.Emit(OpCodes.Call, requestType.GetProperty("PageNumber").GetMethod);
                aIL.Emit(OpCodes.Ldarg_1);
                aIL.Emit(OpCodes.Call, requestType.GetProperty("PageSize").GetMethod);
                aIL.Emit(OpCodes.Callvirt, strategyType.GetMethod("FindAsync"));
                aIL.Emit(OpCodes.Ret);

                strategies[i] = (strategyType, strategyField);
            }

            var constructor = type.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard | CallingConventions.HasThis,
                new[] { typeof(ICatalogDb<TId, TFilterQuery, TData>) }.Union(strategies.Select(s => s.type)).ToArray());

            var cIL = constructor.GetILGenerator();

            var catalogField = baseType.GetField(ControllerCore<TId, TFilterQuery, TData>.CatPropName, BindingFlags.Instance | BindingFlags.NonPublic);

            cIL.Emit(OpCodes.Ldarg_0);
            cIL.Emit(OpCodes.Ldarg_1);
            cIL.Emit(OpCodes.Stfld, catalogField);

            for (int i = 0; i < strategies.Length; i++)
            {
                cIL.Emit(OpCodes.Ldarg_0);
                cIL.Emit(OpCodes.Ldarg, i + 2);
                cIL.Emit(OpCodes.Stfld, strategies[i].field);
            }

            cIL.Emit(OpCodes.Ret);

            return type.CreateTypeInfo();
        }
    }
}
