using NewsTerm_Universal.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsTerm_Universal
{
    class ItemList
    {
        private ObservableCollection<ItemModel> _list;
        private static ItemList _instance = null;
        private ItemModel _selectedItem = null;

        public delegate void RefreshEventHandler(object sender, Result resultCondition);

        public ObservableCollection<ItemModel> Items { get {return _list;} }
        public ItemModel SelectedItem { get { return _selectedItem; } set { _selectedItem = value; } }
        public event RefreshEventHandler RefreshComplete;

        public enum Result{
            NoError,
            ConnectionError,
        };

        public static ItemList getInstance()
        {
            if(_instance == null)
            {
                _instance = new ItemList();
            }
            return _instance;
        }

        public ItemList()
        {
            _list = new ObservableCollection<ItemModel>();
        }

        public async void RemoveRead()
        {
            try
            {
                var backendItems = await NextcloudNewsInterface.NextcloudNewsInterface.getInstance().getItems();

                foreach (var item in backendItems.items)
                {
                    var existingItem = (from ItemModel selitem in _list where selitem.id == item.id && item.unread == false select selitem).FirstOrDefault<ItemModel>();
                    if (existingItem != null)
                    {
                        _list.Remove(existingItem);
                    }
                }
            }
            catch
            {
                //DJ: Calling the RefreshComplete callback here is valid, since erroring out here means the refresh can not continue.
                //TODO: Catch the right exception
                RefreshComplete?.Invoke(this, Result.ConnectionError);
            }
        }

        public async void Refresh(bool showRead)
        {
            var errorCondition = Result.NoError; 
            try
            {
                var backendItems = await NextcloudNewsInterface.NextcloudNewsInterface.getInstance().getItems();
                var feeds = await NextcloudNewsInterface.NextcloudNewsInterface.getInstance().getFeeds();

                foreach (var item in backendItems.items)
                {
                    var existingItem = (from ItemModel selitem in _list where selitem.id == item.id select selitem).FirstOrDefault<ItemModel>();
                    if (existingItem == null)
                    {
                        if (showRead == false && item.unread == false)
                            continue;

                        var newItem = ItemModel.FromItem(item);
                        var feed = feeds.getFeedForId(item.feedId);

                        if (feed != null)
                        {
                            if (feed.faviconLink != null && feed.faviconLink != String.Empty)
                            {
                                Uri validatedUri;
                                //TODO: Need to also verify that the protocol part of the url is http or https
                                if (Uri.TryCreate(feed.faviconLink, UriKind.RelativeOrAbsolute, out validatedUri))
                                {
                                    newItem.favicon = feed.faviconLink;
                                }
                                else
                                {
                                    newItem.favicon = "https://boingboing.net/favicon.ico";
                                }
                            }
                            else
                            {
                                newItem.favicon = "https://boingboing.net/favicon.ico";
                            }
                        }
                        _list.Insert(0, newItem);
                    }
                }
            }
            catch
            {
                //TODO: Catch the right exception
                errorCondition = Result.ConnectionError;
            }
            finally
            {
                RefreshComplete?.Invoke(this, errorCondition);
            }
        }

        public ItemModel GetItemById(int id)
        {
            return (from ItemModel item in _list where item.id == id select item).FirstOrDefault<ItemModel>();
        }

        public async void MarkItemRead(ItemModel item)
        {
            var rawItem = (await NextcloudNewsInterface.NextcloudNewsInterface.getInstance().getItems()).getForId(item.id);
            item.unread = false;
            NextcloudNewsInterface.NextcloudNewsInterface.getInstance().markItemRead(rawItem);
        }

        public ItemModel GetNextItem()
        {
            var index = _list.IndexOf(_selectedItem)+1;
            if(index < _list.Count)
            {
                return _list[index];
            }
            return null;
        }

        public ItemModel GetPreviousItem()
        {
            var index = _list.IndexOf(_selectedItem) - 1;
            if (index >= 0)
            {
                return _list[index];
            }
            return null;
        }

        public bool HaveNextItem()
        {
            var index = _list.IndexOf(_selectedItem) + 1;
            if (index < _list.Count)
            {
                return true;
            }
            return false;
        }

        public bool HavePreviousItem()
        {
            var index = _list.IndexOf(_selectedItem) - 1;
            if (index >= 0)
            {
                return true;
            }
            return false;
        }
    }
}
