using NewsTerm_Universal.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public ObservableCollection<ItemModel> Items { get {return _list;} }
        public ItemModel SelectedItem { get { return _selectedItem; } set { _selectedItem = value; } }

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

        public async void Refresh()
        {
            var backendItems = await NextcloudNewsInterface.NextcloudNewsInterface.getInstance().getItems();
            var feeds = await NextcloudNewsInterface.NextcloudNewsInterface.getInstance().getFeeds();

            foreach (var item in backendItems.items)
            {
                var existingItem = (from ItemModel selitem in _list where selitem.id == item.id select selitem).FirstOrDefault<ItemModel>();
                if (existingItem == null)
                {
                    var newItem = ItemModel.FromItem(item);
                    var feed = feeds.getFeedForId(item.feedId);

                    if (feed != null)
                        if(feed.faviconLink != null && feed.faviconLink != String.Empty)
                        {
                            Uri validatedUri;
                            if(Uri.TryCreate(feed.faviconLink, UriKind.RelativeOrAbsolute, out validatedUri))
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
                    _list.Insert(0, newItem);
                }
            }
        }

        public ItemModel GetItemById(int id)
        {
            return (from ItemModel item in _list where item.id == id select item).FirstOrDefault<ItemModel>();
        }

        public async void MarkItemRead(ItemModel item)
        {
            var rawItem = (await NextcloudNewsInterface.NextcloudNewsInterface.getInstance().getItems()).getForId(item.id);
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
    }
}
