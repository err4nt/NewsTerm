using System;
using System.Linq;
using System.Runtime.Serialization;

namespace NextcloudNewsInterface
{
    [DataContract(Name = "folders")]
    public class NextcloudNewsFolder
    {
        [DataMember(Name = "id")]
        public int id;
        [DataMember(Name = "name")]
        public String name;
    }

    [DataContract]
    public class NextcloudNewsFeed
    {
        [DataMember(Name = "id")]
        public int id;
        [DataMember(Name = "url")]
        public String url;
        [DataMember(Name = "title")]
        public String title;
        [DataMember(Name = "faviconLink")]
        public String faviconLink;
        [DataMember(Name = "added")]
        public long added;
        [DataMember(Name = "folderId")]
        public int folderId;
        [DataMember(Name = "unreadCount")]
        public int unreadCount;
        [DataMember(Name = "ordering")]
        public int ordering;
        [DataMember(Name = "link")]
        public String link;
        [DataMember(Name = "pinned")]
        public Boolean pinned;
        [DataMember(Name = "updateErrorCount")]
        public int updateErrorCount;
        [DataMember(Name = "lastUpdateError")]
        public string lastUpdateError;
    }

    [DataContract]
    public class NextcloudNewsFeeds
    {
        [DataMember(Name = "feeds")]
        public NextcloudNewsFeed[] feeds;
        [DataMember(Name = "starredCount")]
        public int starredCount;

        public NextcloudNewsFeed getFeedForId(int id)
        {
            var result = from NextcloudNewsFeed feed in this.feeds
                         where feed.id == id
                         select feed;
            return result.First<NextcloudNewsFeed>();
        }
    }

    public class NextcloudNewsItem
    {
        //NOTE: title, author, url, enclosureMime, and enclosureLink are not sanitized
        [DataMember(Name = "id")]
        public int id;
        [DataMember(Name = "guid")]
        public String guid;
        [DataMember(Name = "guidHash")]
        public String guidHash;
        [DataMember(Name = "url")]
        public String url;
        [DataMember(Name = "title")]
        public String title;
        [DataMember(Name = "author")]
        public String author;
        [DataMember(Name = "pubDate")]
        public long pubDate;
        [DataMember(Name = "body")]
        public String body;
        [DataMember(Name = "enclosureMime")]
        public String enclosureMime;
        [DataMember(Name = "enclosureLink")]
        public String enclosureLink;
        [DataMember(Name = "feedId")]
        public int feedId;
        [DataMember(Name = "unread")]
        public Boolean unread;
        [DataMember(Name = "starred")]
        public Boolean starred;
        [DataMember(Name = "lastModified")]
        public long lastModified;
        [DataMember(Name = "fingerprint")]
        public String fingerprint;

        //public async void markRead(NextcloudNewsInterface interface)
        //{
        //}
    }

    public class NextcloudNewsItems
    {
        [DataMember(Name = "items")]
        public NextcloudNewsItem[] items;

        public NextcloudNewsItem getForId(int id)
        {
            var result = from NextcloudNewsItem item in this.items
                         where item.id == id
                         select item;
            return result.First<NextcloudNewsItem>();
        }
    }

    public class NextcloudNewsUser
    {
    }
}
