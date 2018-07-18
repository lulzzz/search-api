using MongoDB.Bson.Serialization.Conventions;
using System;
using System.Collections.Generic;

namespace SearchV2.MongoDB
{
    class Init
    {
        static readonly HashSet<Type> _done = new HashSet<Type>();
        public static void ForType<T>(string idPropName)
        {
            var t = typeof(T);
            lock (_done)
            {
                if (_done.Add(t))
                {
                    var cp = new ConventionPack();
                    cp.AddClassMapConvention(t.Name + "ids", bcm => bcm.MapIdProperty(idPropName));
                    ConventionRegistry.Register(t.Name + "idsPack", cp, type => type == t);
                }
            }
        }
    }
}
