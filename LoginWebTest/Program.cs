using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using HtmlAgilityPack;
using NovelSiteParser;

namespace LoginWebTest
{
    class Program
    {
        private static HttpClient httpClient;
        private const string INDEX_PAGE = "https://www.wenku8.net/index.php";
        private const string LOGIN_PAGE = "https://www.wenku8.net/login.php";

        static async Task Main(string[] args)
        {
            Test();
            string username = File.ReadAllText(@"D:\Test Dir\LoginWebTest\username.txt");
            string password = File.ReadAllText(@"D:\Test Dir\LoginWebTest\password.txt");
            Wenku8Parser parser = new Wenku8Parser(username, password, Wenku8Parser.LoginDuration.OneDay);
            parser.Init();
            if (await parser.TryLogin())
            {
                parser.SaveCookie();
                List<BookshelfLink> myBooks = await parser.GetBooksFromBookshelf();
                foreach (var bookshelfLink in myBooks)
                {
                    BookLink bookLink = await parser.FindBookIndexPageAsync(bookshelfLink.MainPage);
                    Book book = await parser.GetBookAsync(bookLink);
                    book.SaveToTxt(AppDomain.CurrentDomain.BaseDirectory);
                }
            }
            else
            {
                Console.WriteLine("Login failed!");
            }

            Console.ReadLine();
            return;

            var baseAddress = new Uri(LOGIN_PAGE);
            var cookieContainer = new CookieContainer();
            using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer, AllowAutoRedirect = false })
            {
                using (httpClient = new HttpClient(handler) { BaseAddress = baseAddress })
                {
                    try
                    {
                        /*
                        //usually i make a standard request without authentication, eg: to the home page.
                        //by doing this request you store some initial cookie values, that might be used in the subsequent login request and checked by the server
                        var homePageResult = await httpClient.GetAsync("/");
                        homePageResult.EnsureSuccessStatusCode();
                        Console.WriteLine("Get / success.");

                        // username=a45312&password=a45312&usecookie=0&action=login&submit=%26nbsp%3B%B5%C7%26nbsp%3B%26nbsp%3B%C2%BC%26nbsp%3B
                        var content = new FormUrlEncodedContent(new[]
                        {
                            //the name of the form values must be the name of <input /> tags of the login form, in this case the tag is <input type="text" name="username">
                            new KeyValuePair<string, string>("username", "a45312"),
                            new KeyValuePair<string, string>("password", "a45312"),
                            new KeyValuePair<string, string>("usecookie", "0"),
                            new KeyValuePair<string, string>("action", "login")
                        });
                        var loginResult = await httpClient.PostAsync("https://www.wenku8.net/login.php?do=submit", content);
                        loginResult.EnsureSuccessStatusCode();
                        Console.WriteLine("Login success.");

                        //make the subsequent web requests using the same HttpClient object
                        // get index page after login
                        //string index = "https://www.wenku8.net/index.php";
                        string bookCase = "https://www.wenku8.net/modules/article/bookcase.php";
                        string htmlString = await GetPageBodyAsync(bookCase, CodePage.Gb2312);

                        var htmlDoc = new HtmlDocument();
                        htmlDoc.LoadHtml(htmlString);
                        List<BookshelfLink> myBooks = GetBooksFromBookshelf(htmlDoc);
                        foreach (var bookshelfLink in myBooks)
                        {
                            await FindBookIndexPageAsync(bookshelfLink.Title);
                        }*/

                        // select a node from bookcase
                        /*var checkFormNode = htmlDoc.DocumentNode.SelectSingleNode("//form[@name='checkform']");
                        var tableNode = checkFormNode.SelectSingleNode("//table");
                        if (tableNode != null)
                        {
                            foreach (HtmlNode row in tableNode.SelectNodes("tr"))
                            {
                                var columns = row.SelectNodes("th|td");
                                if (columns != null && columns.Count >= 7)
                                {
                                    var secondColumn = columns[1];
                                    var link = secondColumn.Descendants("a").FirstOrDefault();
                                    if (link != null)
                                    {
                                        string bookHref = link.Attributes["href"]?.Value;

                                        string bookPage = await GetPageBodyAsync(bookHref, CodePage.Gb2312);
                                        HtmlDocument bookDocument = new HtmlDocument();
                                        bookDocument.LoadHtml(bookPage);
                                        string indexHref = FindTargetHref(bookDocument, "小说目录");

                                        bookPage = await GetPageBodyAsync(indexHref, CodePage.Gb2312);
                                        HtmlDocument bookPageDocument = new HtmlDocument();
                                        bookPageDocument.LoadHtml(bookPage);
                                        GetChapterLinks(bookPageDocument, indexHref);
                                    }
                                }
                            }
                        }*/
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                }
            }

            Console.ReadLine();
        }

        /*
        /// <summary>
        /// 下載指定網頁的 HTML 內容至 string
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="codePage"></param>
        /// <returns></returns>
        static async Task<string> GetPageBodyAsync(string uri, CodePage codePage)
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
        /// 讀取 wenku8 預設書架上所有書的連結
        /// </summary>
        /// <param name="htmlDoc"></param>
        /// <returns></returns>
        private static List<BookshelfLink> GetBooksFromBookshelf(HtmlDocument htmlDoc)
        {
            List<BookshelfLink> bookshelfLinks = new List<BookshelfLink>();

            // select a node from bookcase
            var checkFormNode = htmlDoc.DocumentNode.SelectSingleNode("//form[@name='checkform']");
            var tableNode = checkFormNode.SelectSingleNode("//table");
            if (tableNode != null)
            {
                foreach (HtmlNode row in tableNode.SelectNodes("tr"))
                {
                    var columns = row.SelectNodes("th|td");
                    if (columns != null && columns.Count >= 7)
                    {
                        var secondColumn = columns[1];
                        var link = secondColumn.Descendants("a").FirstOrDefault();
                        if (link != null)
                        {
                            BookshelfLink bookshelfLink = new BookshelfLink
                            {
                                Title = link.InnerText,
                                MainPage = link.Attributes["href"]?.Value
                            };
                            bookshelfLinks.Add(bookshelfLink);
                        }
                    }
                }
            }

            return bookshelfLinks;
        }

        private static string FindTargetHref(HtmlDocument htmlDoc, string hrefName)
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
        /// 從一本書的主頁尋找目錄頁的超連結，並從目錄頁記下此書所有章節的連結
        /// </summary>
        private static async Task<BookLink> FindBookIndexPageAsync(string mainPageUrl)
        {
            string htmlString = await GetPageBodyAsync(mainPageUrl, CodePage.Gb2312);
            HtmlDocument bookDocument = new HtmlDocument();
            bookDocument.LoadHtml(htmlString);
            return await FindBookIndexPageAsync(bookDocument);
        }

        /// <summary>
        /// 從書一本的主頁尋找目錄頁的超連結，並從目錄頁記下此書所有章節的連結
        /// </summary>
        private static async Task<BookLink> FindBookIndexPageAsync(HtmlDocument bookDocument)
        {
            // get book status
            var tdStatusNodes = bookDocument.DocumentNode.SelectNodes("//td[@width='20%']");
            string lastUpdateTimeString = tdStatusNodes.FirstOrDefault(x => x.InnerText.StartsWith("最后更新："))?.InnerText;
            lastUpdateTimeString = TrimStart(lastUpdateTimeString, "最后更新：");
            // get index page link
            string indexHref = FindTargetHref(bookDocument, "小说目录");
            // get index page body
            string htmlString2 = await GetPageBodyAsync(indexHref, CodePage.Gb2312);
            HtmlDocument bookPageDocument = new HtmlDocument();
            bookPageDocument.LoadHtml(htmlString2);
            // parse the index page
            BookLink bookLink = GetChapterLinks(bookPageDocument, indexHref);
            if (DateTime.TryParse(lastUpdateTimeString, out DateTime lastTime))
                bookLink.LastUpdateTime = lastTime;
            return bookLink;
        }

        private static Chapter GetChapterContent(HtmlDocument htmlDoc)
        {
            string title = string.Empty;
            HtmlNode titleDiv = htmlDoc.DocumentNode.SelectSingleNode("//div[@id='title']");
            if (titleDiv != null)
                title = titleDiv.InnerText;

            var contents = htmlDoc.DocumentNode.SelectNodes("//div[@id='content']");
            if (contents != null && contents.Count > 0)
            {
                string text = contents.FirstOrDefault()?.InnerText;
                if (!string.IsNullOrEmpty(text))
                {
                    text = WebUtility.HtmlDecode(text);
                    Chapter chapter = new Chapter(title,text);
                    return chapter;
                }
            }
            return null;
        }

        private static BookLink GetChapterLinks(HtmlDocument htmlDoc, string baseUrl)
        {
            var titleNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@id='title']");
            var authorNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@id='info']");

            BookLink bookLink = new BookLink
            {
                IndexPage = baseUrl,
                Title = titleNode?.InnerText,
                Author = authorNode?.InnerText
            };

            baseUrl = TrimEnd(baseUrl.ToLower(), "index.htm");

            List<HtmlNode> tableRows = new List<HtmlNode>();
            var tables = htmlDoc.DocumentNode.SelectNodes("//table[@class='css']");
            foreach (var tableNode in tables)
            {
                tableRows.AddRange(tableNode.Descendants("tr"));
            }

            List<IssueLink> issueLinks = new List<IssueLink>();
            foreach (var node in tableRows)
            {
                var titleRow = node.SelectSingleNode(".//td[@class='vcss']");
                if (titleRow != null)
                {
                    IssueLink b = new IssueLink
                    {
                        Title = titleRow.InnerText
                    };
                    issueLinks.Add(b);
                    continue;
                }

                // 一個章節的 tr 裡面會有一個以上的 ccss 文章超連結
                var chapterRows = node.SelectNodes(".//td[@class='ccss']");
                foreach (var chapterRow in chapterRows)
                {
                    var link = chapterRow.Descendants("a").FirstOrDefault();
                    if (link != null)
                    {
                        ChapterLink chapterLink = new ChapterLink();
                        chapterLink.Title = link.InnerText;
                        chapterLink.Url = baseUrl + link.Attributes["href"]?.Value;
                        issueLinks.Last()?.ChapterLinks.Add(chapterLink);
                    }
                }
            }

            bookLink.IssueLinks = issueLinks;
            return bookLink;
        }

        public static string TrimEnd(string source, string trimChars)
        {
            return source.EndsWith(trimChars) ? source.Remove(source.LastIndexOf(trimChars, StringComparison.Ordinal)) : source;
        }

        public static string TrimStart(string source, string trimChars)
        {
            return source.TrimStart(trimChars.ToCharArray());
        }*/

        private static void Test()
        {
            string f = File.ReadAllText("bookPage.html");
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(f);
            //FindBookIndexPageAsync(htmlDoc);
            //GetChapterLinks(htmlDoc, "https://www.wenku8.net/novel/1/1435/index.htm");

        }
    }
}
