using System.Collections.Generic;
using System.Linq;

namespace JekyllLinkPost
{
    public partial class LinkPost
    {
        private partial class LinkCategory
        {
            public string CategoryName { get; set; }
            public List<PocketItem> Items { get; private set; }
            public LinkCategory()
            {
                CategoryName = "";
                Items = new List<PocketItem>();
            }

            public LinkCategory(string categoryName)
            {
                CategoryName = categoryName;
                Items = new List<PocketItem>();
            }
        }

        private partial class CategoryMap : IEnumerable<LinkCategory>
        {
            private Dictionary<string, LinkCategory> _categoryMap;
            private List<LinkCategory> _categoryList;

            public CategoryMap()
            {
                _categoryMap = new Dictionary<string, LinkCategory>();
                _categoryList = new List<LinkCategory>();
            }

            public void Add(string pocketTag, LinkCategory linkCategory)
            {
                _categoryMap.Add(pocketTag, linkCategory);
                _categoryList.Add(linkCategory);
            }

            public void Add(PocketItem item)
            {
                LinkCategory link = null;
                if (item.Tags.Length > 0)
                {
                    link = Lookup(item.Tags[0]); 
                }

                if (link == null)
                {
                    link = Lookup("unknown");
                }

                link.Items.Add(item);
            }

            IEnumerator<LinkCategory> IEnumerable<LinkCategory>.GetEnumerator()
            {
                return _categoryList.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return _categoryList.GetEnumerator();
            }

            public LinkCategory Lookup(string category)
            {
                return _categoryMap.SingleOrDefault(c => c.Key == category).Value;
            }
        }
    }
}
