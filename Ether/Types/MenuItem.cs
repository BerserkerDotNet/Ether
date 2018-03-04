using Ether.Core.Filters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ether.Types
{
    public class MenuItem 
    {
        public MenuItem(Type pageType)
        {
            PageType = pageType;
            SubItems = new List<MenuItem>();
            SetTitle();
        }

        public void AddSubItem(MenuItem item)
        {
            item.Parent = this;
            SubItems.Add(item);
        }

        public MenuItem Find(Type type)
        {
            if (PageType == type)
                return this;

            foreach (var subItem in SubItems)
            {
                var menu = subItem.Find(type);
                if (menu != null)
                    return menu;
            }

            return null;
        }

        private void SetTitle()
        {
            if (PageType == null)
                return;

            var pageTitleAttribute = PageType.CustomAttributes.SingleOrDefault(a => a.AttributeType == typeof(PageTitleAttribute));
            if (pageTitleAttribute != null)
                Title = pageTitleAttribute.ConstructorArguments.First().Value.ToString();
        }

        public string Title { get; protected set; }

        public string Path
        {
            get
            {
                if (PageType == null)
                    return string.Empty;

                var pageName = PageType.Name.Replace("Model", string.Empty);
                var path = PageType.FullName
                    .Replace("Ether.Pages.", string.Empty)
                    .Replace(".", "/")
                    .Replace(PageType.Name, pageName);
                return $"/{path}";
            }
        }

        public Type PageType { get; }

        public MenuItem Parent { get; private set; }

        public IList<MenuItem> SubItems { get; set; }
    }
}
