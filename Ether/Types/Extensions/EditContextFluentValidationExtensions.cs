using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ether.ViewModels.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Components.Forms;

namespace Ether.Types.Extensions
{
    public static class EditContextFluentValidationExtensions
    {
        private static ConcurrentDictionary<(Type ModelType, string FieldName), PropertyInfo> _propertyInfoCache = new ConcurrentDictionary<(Type, string), PropertyInfo>();
        private static IEnumerable<IValidator> _validators;

        static EditContextFluentValidationExtensions()
        {
            var scanner = AssemblyScanner.FindValidatorsInAssemblyContaining<IdentityViewModelValidator>();
            _validators = scanner.Select(r => (IValidator)Activator.CreateInstance(r.ValidatorType)).ToArray();
        }

        public static EditContext AddFluentValidations(this EditContext editContext)
        {
            if (editContext == null)
            {
                throw new ArgumentNullException(nameof(editContext));
            }

            var messages = new ValidationMessageStore(editContext);

            editContext.OnValidationRequested +=
                (sender, eventArgs) => ValidateModel((EditContext)sender, messages);

            editContext.OnFieldChanged +=
                (sender, eventArgs) => ValidateField(editContext, messages, eventArgs.FieldIdentifier);

            return editContext;
        }

        private static void ValidateModel(EditContext editContext, ValidationMessageStore messages)
        {
            var validator = GetValidatorFor(editContext.Model);
            if (validator == null)
            {
                messages.Clear();
                return;
            }

            var result = validator.Validate(editContext.Model);
            messages.Clear();
            foreach (var validationError in result.Errors)
            {
                messages.Add(editContext.Field(validationError.PropertyName), validationError.ErrorMessage);
            }

            editContext.NotifyValidationStateChanged();
        }

        private static void ValidateField(EditContext editContext, ValidationMessageStore messages, in FieldIdentifier fieldIdentifier)
        {
            if (TryGetValidatableProperty(fieldIdentifier, out var propertyInfo))
            {
                var validator = GetValidatorFor(editContext.Model);
                if (validator == null)
                {
                    messages.Clear();
                    return;
                }

                var result = validator.Validate(editContext.Model);
                messages.Clear(fieldIdentifier);
                messages.AddRange(fieldIdentifier, result.Errors.Where(e => e.PropertyName == propertyInfo.Name).Select(e => e.ErrorMessage));

                editContext.NotifyValidationStateChanged();
            }
        }

        private static IValidator GetValidatorFor(object model)
        {
            return _validators.SingleOrDefault(v => v.CanValidateInstancesOfType(model.GetType()));
        }

        private static bool TryGetValidatableProperty(in FieldIdentifier fieldIdentifier, out PropertyInfo propertyInfo)
        {
            var cacheKey = (ModelType: fieldIdentifier.Model.GetType(), fieldIdentifier.FieldName);
            if (!_propertyInfoCache.TryGetValue(cacheKey, out propertyInfo))
            {
                propertyInfo = cacheKey.ModelType.GetProperty(cacheKey.FieldName);
                _propertyInfoCache[cacheKey] = propertyInfo;
            }

            return propertyInfo != null;
        }
    }
}
