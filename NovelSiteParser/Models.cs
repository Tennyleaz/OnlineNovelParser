using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovelSiteParser
{
    /// <summary>
    /// 書的主頁
    /// </summary>
    public class BookshelfLink
    {
        public string Title;
        public string MainPage;
    }

    /// <summary>
    /// 目錄的頁面
    /// </summary>
    public class BookLink : IEquatable<BookLink>
    {
        public string Title;
        public string Author;
        public string IndexPage;
        public DateTime LastUpdateTime;
        public List<IssueLink> IssueLinks = new List<IssueLink>();

        public bool Equals(BookLink book)
        {
            if (book == null)
                return false;
            if (Object.ReferenceEquals(this, book))
                return true;
            if (IndexPage != book.IndexPage)
                return false;
            if (IssueLinks == null && book.IssueLinks == null)
                return (IndexPage == book.IndexPage) && (Title == book.Title);
            if (IssueLinks?.Count != book.IssueLinks?.Count)
                return false;
            for (int i = 0; i < IssueLinks.Count; i++)
            {
                if (!IssueLinks[i].Equals(book.IssueLinks[i]))
                    return false;
            }
            return true;
        }
    }

    /// <summary>
    /// 每一冊的資料
    /// </summary>
    public class IssueLink : IEquatable<IssueLink>
    {
        public string Title;
        public List<ChapterLink> ChapterLinks = new List<ChapterLink>();

        public bool Equals(IssueLink issue)
        {
            if (issue == null)
                return false;
            if (Object.ReferenceEquals(this, issue))
                return true;
            if (ChapterLinks == null && issue.ChapterLinks == null)
                return Title == issue.Title;
            if (ChapterLinks?.Count != issue.ChapterLinks?.Count)
                return false;
            for (int i=0; i<ChapterLinks.Count; i++)
            {
                if (!ChapterLinks[i].Equals(issue.ChapterLinks[i]))
                    return false;
            }
            return true;
        }
    }

    /// <summary>
    /// 每一章節的資料
    /// </summary>
    public class ChapterLink : IEquatable<ChapterLink>
    {
        public string Title;
        public string Url;
        public bool Downloaded;

        public bool Equals(ChapterLink c)
        {
            if (c == null)
                return false;
            if (Object.ReferenceEquals(this, c))
                return true;
            return Url == c.Url;
        }
    }

    public class Book
    {
        public string Title;
        public List<Issue> Issues = new List<Issue>();

        public bool SaveToTxt(string parentPath)
        {
            try
            {
                if (!Directory.Exists(parentPath))
                    Directory.CreateDirectory(parentPath);
                string newFolder = parentPath + "/" + Utilities.TrimIllegalPath(Title);
                Directory.CreateDirectory(newFolder);
                bool success = true;
                for (int i = 0; i < Issues.Count; i++)
                {
                    success = Issues[i].SaveToTxt(newFolder, i);
                    if (!success)
                        break;
                }
                return success;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }
    }

    public class Issue
    {
        public string Title;
        public List<Chapter> Chapters = new List<Chapter>();

        public bool SaveToTxt(string parentPath, int index)
        {
            string newFolder = parentPath + "/" + index + " - " + Utilities.TrimIllegalPath(Title);
            try
            {
                if (!Directory.Exists(parentPath))
                    Directory.CreateDirectory(parentPath);
                Directory.CreateDirectory(newFolder);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            bool success = true;
            for (int i=0; i<Chapters.Count; i++)
            {
                success = Chapters[i].SaveToTxt(newFolder, i);
                if (!success)
                    break;
            }

            return success;
        }
    }

    public class Chapter
    {
        public string Title;
        public string Content;

        public Chapter(string title, string content)
        {
            Title = title;
            Content = content;
        }

        public bool SaveToTxt(string parentPath, int index)
        {
            string fileName = parentPath + "/" + index + " - " + Utilities.TrimIllegalPath(Title) + ".txt";
            try
            {
                if (!Directory.Exists(parentPath))
                    Directory.CreateDirectory(parentPath);
                File.WriteAllText(fileName, Content);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;
        }
    }
}
