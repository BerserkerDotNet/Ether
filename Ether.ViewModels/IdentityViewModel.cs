using System;

namespace Ether.ViewModels
{
    public class IdentityViewModel : ViewModelWithId
    {
        public string Name { get; set; }

        public string Token { get; set; }

        public DateTime ExpirationDate { get; set; }
    }
}
