using System;
using System.Collections.Generic;
using System.Linq;
using Ether.ViewModels.Validators;
using FluentValidation;

namespace Ether.Types
{
    public class ModelValidationService
    {
        private readonly IEnumerable<IValidator> _validators;

        public ModelValidationService()
        {
            var s = new AssemblyScanner(new[] { typeof(IdentityViewModelValidator), typeof(GenerateReportViewModelValidator) });
            _validators = s.Select(r => (IValidator)Activator.CreateInstance(r.ValidatorType)).ToArray();

            Console.WriteLine($"ModelValidationService - found {_validators.Count()} validators.  {string.Join(",", _validators)}");
        }

        public Dictionary<string, IEnumerable<string>> Validate<T>(T model)
        {
            var validator = _validators.SingleOrDefault(v => v.CanValidateInstancesOfType(model.GetType()));
            if (validator == null)
            {
                return new Dictionary<string, IEnumerable<string>>(0);
            }

            var validationResult = validator.Validate(model);
            var properties = string.Join(",", validationResult.Errors.Select(f => f.PropertyName));
            return validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(k => k.Key, v => v.Select(e => e.ErrorMessage));
        }
    }
}
