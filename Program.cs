using System;
using System.Collections.Generic;
using System.Configuration;
using JekyllLinkPost.Properties;

namespace JekyllLinkPost
{
    class Program
    {

        static void Main(string[] args)
        {
            // login
            PocketApi api = new PocketApi();
            bool result = api.LoginToPocket();
            if (result)
            {
                IEnumerable<PocketItem> items = GetBlogItems(api);
                CreatePostFile(items);
                ArchiveBlogItems(api, items);
            }
        }

        private static IEnumerable<PocketItem> GetBlogItems(PocketApi api)
        {
            IEnumerable<PocketItem> items = null;

            string tagToRetreive = Settings.Default.TagToRetreive;
            int numberOfPostsToRetreive = Settings.Default.NumberOfPostsToRetreive;

            if (tagToRetreive != string.Empty)
            {
                items = api.RetreiveItems(tagToRetreive);
            }
            else
            {
                items = api.RetreiveItems(numberOfPostsToRetreive);
            }

            return items;
        }

        private static void CreatePostFile(IEnumerable<PocketItem> items)
        {
            DateTime postDate = DateTime.Now;
            string fileName = GetFileName(postDate);
            LinkPost post = LinkPost.CreateNewPost(p =>
            {
                p.Items = items;
                p.PostDate = postDate;
                p.FileName = fileName;
            });
            post.CreateOutputFile();  
        }

        private static void ArchiveBlogItems(PocketApi api, IEnumerable<PocketItem> items)
        {
            api.ArchivePocketItems(items);
        }

        private static string GetFileName(DateTime postDate)
        {
            string postPath = Settings.Default.PostPath;

            string result = string.Format(
                @"{0}\{1}-links-{2}-slash-{3}-slash-{4}.markdown",
                postPath, postDate.ToString("yyyy-MM-dd"), postDate.Month, postDate.Day, postDate.Year);
            return result;
        }
    }
}
