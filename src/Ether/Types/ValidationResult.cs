using System.Collections.Generic;
using System.Linq;

namespace Ether.Types
{
    public class ValidationResult
    {
        public ValidationResult(Dictionary<string, IEnumerable<string>> errors)
        {
            Errors = errors;
        }

        public bool IsValid => !Errors.Any();

        public Dictionary<string, IEnumerable<string>> Errors { get; private set; }
    }
}
