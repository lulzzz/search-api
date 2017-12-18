using Microsoft.AspNetCore.Mvc;
using SearchV2.Abstractions;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace SearchV2.ApiCore
{
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
        
        public static TypeInfo CreateControllerClass<TId, TFilterQuery, TData>(ICatalogDb<TId, TFilterQuery, TData> catalog, params Api.SearchRegistration<TId>[] searches) where TData : IWithReference<TId>
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
                
                actionBuilder.DefineParameter(1, ParameterAttributes.In, "request");
                
                actionBuilder.SetCustomAttribute(
                    new CustomAttributeBuilder(
                        typeof(HttpPostAttribute).GetConstructors().Single(c => c.GetParameters().Length == 1 && c.GetParameters()[0].ParameterType == typeof(string)),
                        new object[] { $"search/{item._routeSuffix}" }));

                var impl = baseType.GetMethod(ControllerCore<TId, TFilterQuery, TData>.ActionImplementationName).MakeGenericMethod(tSearchQuery);

                var aIL = actionBuilder.GetILGenerator();
                aIL.Emit(OpCodes.Ldarg_0);
                aIL.Emit(OpCodes.Ldfld, strategyField);
                aIL.Emit(OpCodes.Ldarg_1);
                aIL.Emit(OpCodes.Call, impl);
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

        [Route("molecules")]
        public abstract class ControllerCore<TId, TFilterQuery, TData> where TData : IWithReference<TId>
        {
            protected ICatalogDb<TId, TFilterQuery, TData> _catalogDb;

            [HttpGet]
            [Route("{id}")]
            public Task<TData> One([FromRoute]TId id) => _catalogDb.OneAsync(id);

            internal static string CatPropName => nameof(_catalogDb);
            internal static string ActionImplementationName => nameof(FindInternal);

            public static Task<object> FindInternal<TSearchQuery>(ISearchStrategy<TSearchQuery, TFilterQuery> s, SearchRequest<TSearchQuery, TFilterQuery> request)
                => s.FindAsync(
                        request.Query.Search,
                        request.Query.Filters,
                        request.PageNumber.Value,
                        request.PageSize.Value);
        }
    }
}
