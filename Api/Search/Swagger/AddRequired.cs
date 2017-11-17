using Microsoft.AspNetCore.Mvc.Controllers;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;

namespace Search.Swagger
{
    public class AddRequired : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            foreach(var parameter in context.ApiDescription.ActionDescriptor.Parameters)
            {
                if(parameter is ControllerParameterDescriptor cpd)
                {
                    Console.Write(cpd.ParameterInfo);
                }
            }
        }
    }
}