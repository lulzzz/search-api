using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Reflection.Emit;

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

        public static TypeInfo CreateControllerClass(string routePrefix, string controllerName, params ActionDescriptor[] actions)
        {
            var type = mb.CreateTypeBuilder($"{controllerName}Controller"); // + Guid.NewGuid().ToString("N")
            type.SetCustomAttribute(new CustomAttributeBuilder(typeof(ControllerAttribute).GetConstructor(new Type[] { }), new object[] { }));
            type.SetCustomAttribute(new CustomAttributeBuilder(typeof(ValidateModelAttribute).GetConstructor(new Type[] { }), new object[] { }));
            if (!string.IsNullOrEmpty(routePrefix))
            {
                type.SetCustomAttribute(new CustomAttributeBuilder(typeof(RouteAttribute).GetConstructor(new Type[] { typeof(string) }), new object[] { routePrefix }));
            }

            var callTargets = new Dictionary<object, FieldInfo>(actions.Length);

            for (int i = 0; i < actions.Length; i++)
            {
                var action = actions[i];
                var method = action.Implementation.Method;
                var parameters = method.GetParameters();
                var actionBuilder = type.DefineMethod(
                    "Find" + i,
                    MethodAttributes.Public,
                    CallingConventions.HasThis,
                    method.ReturnType,
                    parameters.Select(pi => pi.ParameterType).ToArray());

                actionBuilder.SetCustomAttribute(
                    new CustomAttributeBuilder(
                        action.HttpMethod.MapToAttribute().GetConstructors().Single(c => c.GetParameters().Length == 1 && c.GetParameters()[0].ParameterType == typeof(string)),
                        new object[] { action.Route }));
                
                var aIL = actionBuilder.GetILGenerator();
                
                var target = action.Implementation;

                FieldInfo field;
                if (callTargets.ContainsKey(target))
                {
                    field = callTargets[target];
                }
                else
                {
                    field = type.DefineField("str" + i, target.GetType(), FieldAttributes.Private | FieldAttributes.Static);
                    callTargets.Add(target, field);
                }

                aIL.Emit(OpCodes.Ldsfld, field);

                for (int j = 0; j < parameters.Length; j++)
                {
                    var newParameter = actionBuilder.DefineParameter(j + 1, ParameterAttributes.In, parameters[j].Name);
                    foreach (var attribute in parameters[j].CustomAttributes)
                    {
                        var namedArguments = attribute.NamedArguments.ToLookup(na => na.IsField);
                        var namedProperties = namedArguments[false];
                        var namedFields = namedArguments[true];

                        newParameter.SetCustomAttribute(
                            new CustomAttributeBuilder(
                                attribute.Constructor,
                                attribute.ConstructorArguments.Select(ca => ca.Value).ToArray(),
                                namedProperties.Select(np => (PropertyInfo)np.MemberInfo).ToArray(),
                                namedProperties.Select(np => np.TypedValue.Value).ToArray(),
                                namedFields.Select(na => (FieldInfo)na.MemberInfo).ToArray(),
                                namedFields.Select(na => na.TypedValue.Value).ToArray()
                            )
                        );
                    }
                    aIL.Emit(OpCodes.Ldarg, j + 1);
                }
                aIL.Emit(OpCodes.Callvirt, target.GetType().GetMethod("Invoke"));
                aIL.Emit(OpCodes.Ret);
            }

            var ti = type.CreateTypeInfo();

            var fields = ti.GetFields(BindingFlags.Static | BindingFlags.NonPublic);

            foreach (var t in callTargets)
            {
                var field = fields.First(f => f.Name == t.Value.Name);
                field.SetValue(null, t.Key);
            }

            return ti;
        }

        static Type MapToAttribute(this HttpMethod m)
            =>
                m == HttpMethod.Get ? typeof(HttpGetAttribute)
              : m == HttpMethod.Post ? typeof(HttpPostAttribute)
              : m == HttpMethod.Delete ? typeof(HttpDeleteAttribute)
              : throw new NotImplementedException();
    }
}
