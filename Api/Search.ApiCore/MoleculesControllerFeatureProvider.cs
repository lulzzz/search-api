using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Controllers;
using System;

namespace Search.ApiCore
{
    public class MoleculesControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        readonly Type _tId;
        readonly Type _tData;
        readonly Type _tFilterQuery;

        public MoleculesControllerFeatureProvider(Type tId, Type tFilterQuery, Type tData)
        {
            _tId = tId;
            _tData = tData;
            _tFilterQuery = tFilterQuery;
        }

        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            feature.Controllers.Add(typeof(MoleculesControllerBase<,,>).MakeGenericType(_tId, _tFilterQuery, _tData).GetTypeInfo());
        }
    }
}
