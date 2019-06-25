using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using HtmlAgilityPack;

namespace NovelSiteParser
{
    public abstract class BaseNovelSiteParser : IDisposable
    {
        protected HttpClient httpClient;
        protected HttpClientHandler handler;
        protected CookieContainer cookieContainer;
        private const int SLEEP_MS = 20;

        #region Abstract functions
        protected abstract Task<BookLink> FindBookIndexPageAsync(HtmlDocument htmlDoc);
        public abstract Task<List<BookshelfLink>> GetBooksFromBookshelf();
        protected abstract Chapter GetChapterContent(HtmlDocument htmlDoc);
        protected abstract BookLink GetChapterLinks(HtmlDocument htmlDoc, string baseUrl);
        public abstract bool SaveCookie();
        protected abstract CookieContainer LoadCookie();
        #endregion

        //protected BaseNovelSiteParser()
        //{
        //
        //}

        protected void Init(string baseLoginAddress, CookieContainer lastCookie = null)
        {
            cookieContainer = lastCookie ?? new CookieContainer();
            handler = new HttpClientHandler() { CookieContainer = cookieContainer };
            var baseUri = new Uri(baseLoginAddress);
            httpClient = new HttpClient(handler) { BaseAddress = baseUri };
        }

        public void Dispose()
        {
            httpClient?.Dispose();
            handler?.Dispose();
        }

        /// <summary>
        /// 找到第一個包含相同字串內容的超連結
        /// </summary>
        public string FindTargetHref(HtmlDocument htmlDoc, string hrefName)
        {
            HtmlNode link = htmlDoc.DocumentNode.SelectNodes("//a").FirstOrDefault(x => x.InnerHtml == hrefName);
            if (link != null)
            {
                string s = link.Attributes["href"]?.Value;
                return s;
            }
            return string.Empty;
        }

        /// <summary>
        /// 下載指定網頁的 HTML 內容至 string
        /// </summary>
        public async Task<string> GetPageBodyAsync(string uri, CodePage codePage)
        {
            try
            {
                var httpResponseMessage = await httpClient.GetAsync(uri);
                httpResponseMessage.EnsureSuccessStatusCode();
                byte[] buffer = await httpResponseMessage.Content.ReadAsByteArrayAsync();
                string htmlString = Encoding.GetEncoding((int)codePage).GetString(buffer);
                return htmlString;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        /// <summary>
        /// 從正文網頁內取得章節標題和內文
        /// </summary>
        public async Task<Chapter> GetChapterContent(string uri)
        {
            string htmlString = await GetPageBodyAsync(uri, CodePage.Gb2312);
            HtmlDocument bookDocument = new HtmlDocument();
            bookDocument.LoadHtml(htmlString);
            return GetChapterContent(bookDocument);
        }

        /// <summary>
        /// 從書一本的主頁尋找目錄頁的超連結，並從目錄頁記下此書所有章節的連結
        /// </summary>
        public async Task<BookLink> FindBookIndexPageAsync(string mainPageUrl)
        {
            string htmlString = await GetPageBodyAsync(mainPageUrl, CodePage.Gb2312);
            HtmlDocument bookDocument = new HtmlDocument();
            bookDocument.LoadHtml(htmlString);
            return await FindBookIndexPageAsync(bookDocument);
        }

        /// <summary>
        /// 下載一冊書
        /// </summary>
        public async Task<Issue> GetIssueAsync(IssueLink issueLink)
        {
            if (issueLink == null || issueLink.ChapterLinks.Count == 0)
                return null;

            Issue issue = new Issue();
            issue.Title = issueLink.Title;
            foreach (var chapterLink in issueLink.ChapterLinks)
            {
                issue.Chapters.Add(await GetChapterContent(chapterLink.Url));
                await Task.Delay(SLEEP_MS);
            }

            return issue;
        }

        /// <summary>
        /// 下載整套書
        /// </summary>
        public async Task<Book> GetBookAsync(BookLink bookLink)
        {
            if (bookLink == null || bookLink.IssueLinks.Count == 0)
                return null;

            Book book = new Book();
            book.Title = bookLink.Title;
            foreach (var issue in bookLink.IssueLinks)
            {
                book.Issues.Add(await GetIssueAsync(issue));
                await Task.Delay(SLEEP_MS);
            }

            return book;
        }

        protected static bool WriteCookiesToDisk(string file, CookieContainer cookieJar)
        {
            using (Stream stream = File.Create(file))
            {
                try
                {
                    Console.Out.Write("Writing cookies to disk... ");
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, cookieJar);
                    return true;
                }
                catch (Exception e)
                {
                    Console.Out.WriteLine("Problem writing cookies to disk: " + e);
                }
            }
            return false;
        }

        protected static CookieContainer ReadCookiesFromDisk(string file)
        {

            try
            {
                using (Stream stream = File.Open(file, FileMode.Open))
                {
                    Console.Out.Write("Reading cookies from disk... ");
                    BinaryFormatter formatter = new BinaryFormatter();
                    Console.Out.WriteLine("Done.");
                    return (CookieContainer)formatter.Deserialize(stream);
                }
            }
            catch (Exception e)
            {
                Console.Out.WriteLine("Problem reading cookies from disk: " + e);
                return null;
            }
        }
    }
}
