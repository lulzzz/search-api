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

        public MoleculesControllerFeatureProvider(Type tId, Type tData)
        {
            _tId = tId;
            _tData = tData;
        }

        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            feature.Controllers.Add(typeof(MoleculesControllerBase<,>).MakeGenericType(_tId, _tData).GetTypeInfo());
        }
    }
}
