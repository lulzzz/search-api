﻿using MongoDB.Bson.Serialization.Conventions;
using System;
using System.Collections.Generic;
using System.Text;

namespace SearchV2.MongoDB
{
    class Init
    {
        static readonly HashSet<Type> _done;
        public void ForType<T>(string idPropName)
        {
            var t = typeof(T);
            if (_done.Add(t))
            {
                var cp = new ConventionPack();
                cp.AddClassMapConvention(t.Name + "ids", bcm => bcm.MapIdProperty(idPropName));
                ConventionRegistry.Register(t.Name + "idsPack", cp, type => type == t);
            }
        }
    }
}
