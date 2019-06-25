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
    public class BookLink
    {
        public string Title;
        public string Author;
        public string IndexPage;
        public DateTime LastUpdateTime;
        public List<IssueLink> IssueLinks = new List<IssueLink>();
    }

    /// <summary>
    /// 每一冊的資料
    /// </summary>
    public class IssueLink
    {
        public string Title;
        public List<ChapterLink> ChapterLinks = new List<ChapterLink>();
    }

    /// <summary>
    /// 每一章節的資料
    /// </summary>
    public class ChapterLink
    {
        public string Title;
        public string Url;
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
