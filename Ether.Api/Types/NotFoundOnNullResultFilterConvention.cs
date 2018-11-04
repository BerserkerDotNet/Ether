using System.Linq;
using Ether.Api.Attributes;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Internal;

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
            if (controller.Attributes.OfType<IApiBehaviorMetadata>().Any())
            {
                return true;
            }

            return controller
                .ControllerType
                .GetCustomAttributes(inherit: true)
                .OfType<IApiBehaviorMetadata>()
                .Any();
        }
    }
}
