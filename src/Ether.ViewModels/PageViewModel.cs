using System.Collections.Generic;

namespace Ether.ViewModels
{
    public class PageViewModel<TModel>
    {
        public IEnumerable<TModel> Items { get; set; }

        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }
    }
}
