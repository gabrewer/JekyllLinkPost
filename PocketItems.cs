using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JekyllLinkPost
{
    public class PocketItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string[] Tags { get; set; }
    }
}
