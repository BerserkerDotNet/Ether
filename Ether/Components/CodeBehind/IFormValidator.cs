using System;
using Ether.Types;

namespace Ether.Components.CodeBehind
{
    public interface IFormValidator
    {
        event EventHandler<ValidationResult> OnValidated;

        bool Validate<T>(T model);
    }
}
