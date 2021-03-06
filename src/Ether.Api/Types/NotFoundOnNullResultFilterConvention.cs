﻿using System.Linq;
using Ether.Api.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Ether.Api.Types
{
    public class NotFoundOnNullResultFilterConvention : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            if (IsApiController(controller))
            {
                controller.Filters.Add(new NotFoundOnNullResultFilterAttribute());
            }
        }

        private bool IsApiController(ControllerModel controller)
        {
            if (controller.Attributes.OfType<ApiControllerAttribute>().Any())
            {
                return true;
            }

            return controller
                .ControllerType
                .GetCustomAttributes(inherit: true)
                .OfType<ApiControllerAttribute>()
                .Any();
        }
    }
}
