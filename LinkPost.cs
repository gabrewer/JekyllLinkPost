using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace JekyllLinkPost
{
    public partial class LinkPost
    {
        private partial class LinkCategory { }
        private partial class CategoryMap { }

        public IEnumerable<PocketItem> Items { get; set; }
        public string FileName { get; set; }
        public DateTime PostDate { get; set; }

        CategoryMap _categoryMap = null;
        
        private LinkPost() 
        {
            _categoryMap = new CategoryMap();
            AddLinkCategories(_categoryMap);
        }

        public static LinkPost CreateNewPost(Action<LinkPost> initializer)
        {
            LinkPost post = new LinkPost();
            initializer(post);
            return post;
        }

        public void CreateOutputFile()
        {
            CategorizeItems(_categoryMap, Items);

            StreamWriter file = CreateNewFile(FileName);
            WriteYamlHeader(file, PostDate);

            WriteHeader(file);
            WriteItems(file, _categoryMap);
            WriteFooter(file);

            file.Close();
        }

        private StreamWriter CreateNewFile(string fileName)
        {
            StreamWriter result = File.CreateText(fileName);
            return result;
        }
        private void WriteYamlHeader(StreamWriter file, DateTime postDate)
        {
            file.WriteLine("---");
            file.WriteLine(@"title: ""Links - {0}""", postDate.ToString("MM/dd/yyyy"));
            file.WriteLine(@"date: {0}", postDate.ToString("yyyy-MM-dd hh:mm:ss zzz"));
            file.WriteLine("comments: true");
            file.WriteLine("categories: [Links]");
            file.WriteLine("---");
            file.WriteLine("");
        }

        private static void WriteHeader(StreamWriter file)
        {
            WriteTemplate(file, @"header.txt");
        }
        private static void WriteFooter(StreamWriter file)
        {
            WriteTemplate(file, @"footer.txt");
        }

        private static void WriteTemplate(StreamWriter file, string templateFile)
        {
            if (File.Exists(templateFile))
            {
                string footerContent = File.ReadAllText(templateFile);
                file.Write(footerContent);
            }
        }
        private void WriteItems(StreamWriter file, CategoryMap categoryMap)
        {
            foreach (LinkCategory category in categoryMap)
            {
                file.WriteLine("####{0}", category.CategoryName);
                foreach(PocketItem item in category.Items)
                {
                    file.WriteLine("[{0}]({1})", item.Title, item.Url);
                    file.WriteLine("");
                }
            }
        }

        private void AddLinkCategories(CategoryMap categoryMap)
        {
            categoryMap.Add("dont know", new LinkCategory("I dont know where this goes but it is good"));
            categoryMap.Add("everyone has one", new LinkCategory("Everyones got one..."));
            categoryMap.Add("testing", new LinkCategory("Testing"));
            categoryMap.Add("security", new LinkCategory("Security"));
            categoryMap.Add("virtual", new LinkCategory("Virtual"));
            categoryMap.Add("networking", new LinkCategory("Networking"));
            categoryMap.Add("ides/editors", new LinkCategory("IDEs/Editors"));
            categoryMap.Add("builds", new LinkCategory("Builds"));
            categoryMap.Add("business", new LinkCategory("Business"));
            categoryMap.Add("leadership/management", new LinkCategory("Leadership/Management"));
            categoryMap.Add("web development", new LinkCategory("Web Development"));
            categoryMap.Add("web services", new LinkCategory("Web Services (REST, HyperMedia, WebAPI, SignalR)"));
            categoryMap.Add("javascript", new LinkCategory("JavaScript"));
            categoryMap.Add("angular", new LinkCategory("Angular"));
            categoryMap.Add("software development", new LinkCategory("Software Development"));
            categoryMap.Add("game development", new LinkCategory("Game Development"));
            categoryMap.Add("web design", new LinkCategory("Web Design"));
            categoryMap.Add("azure", new LinkCategory("Windows Azure"));
            categoryMap.Add("database", new LinkCategory("Database"));
            categoryMap.Add(".net", new LinkCategory(".Net"));
            categoryMap.Add("mobile development", new LinkCategory("Mobile Development"));
            categoryMap.Add("software", new LinkCategory("Software"));
            categoryMap.Add("gadgets", new LinkCategory("Gadgets"));
            categoryMap.Add("events and videos", new LinkCategory("Podcasts, Conferences, Events"));

            categoryMap.Add("unknown", new LinkCategory("Unknown"));
        }

        private void CategorizeItems(CategoryMap categoryMap, IEnumerable<PocketItem> items)
        {
            Parallel.ForEach(items, item =>
            {
                categoryMap.Add(item);
            });
        }

    }
}
