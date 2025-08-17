using Matrix.Models;
using System.Collections.Generic;

namespace Matrix.ViewModels
{
    public class ArticleViewModel
    {
        public Article? Article { get; set; }
        public Person? Author { get; set; }
        public ArticleAttachment? Image { get; set; }
        public IEnumerable<ArticleAttachment> Attachments { get; set; }

        public ArticleViewModel()
        {
            Attachments = new List<ArticleAttachment>();
        }
    }
}
