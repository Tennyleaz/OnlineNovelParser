using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace NovelSiteParser
{
    interface INovelSiteParser
    {
        /// <summary>
        /// 下載指定網頁的 HTML 內容至 string
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="codePage"></param>
        /// <returns></returns>
        Task<string> GetPageBodyAsync(string uri, CodePage codePage);

        /// <summary>
        /// 找到第一個包含相同字串內容的超連結
        /// </summary>
        /// <param name="htmlDoc"></param>
        /// <param name="hrefName"></param>
        /// <returns></returns>
        string FindTargetHref(HtmlDocument htmlDoc, string hrefName);

        /// <summary>
        /// 讀取預設書架上所有書的連結
        /// </summary>
        /// <param name="htmlDoc"></param>
        /// <returns></returns>
        Task<List<BookshelfLink>> GetBooksFromBookshelf();

        /// <summary>
        /// 從書一本的主頁尋找目錄頁的超連結，並從目錄頁記下此書所有章節的連結
        /// </summary>
        Task<BookLink> FindBookIndexPageAsync(string mainPageUrl);

        /// <summary>
        /// 從正文網頁內取得章節標題和內文
        /// </summary>
        /// <param name="htmlDoc"></param>
        /// <returns></returns>
        Chapter GetChapterContent(HtmlDocument htmlDoc);

        /// <summary>
        /// 從目錄頁記下此書所有章節的連結
        /// </summary>
        /// <param name="htmlDoc"></param>
        /// <param name="baseUrl"></param>
        /// <returns></returns>
        BookLink GetChapterLinks(HtmlDocument htmlDoc, string baseUrl);
    }
}
