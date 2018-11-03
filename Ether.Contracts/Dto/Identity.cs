using System;

namespace Ether.Contracts.Dto
{
    public class Identity : BaseDto
    {
        public string Name { get; set; }

        public string Token { get; set; }

        public DateTime ExpirationDate { get; set; }
    }
}
