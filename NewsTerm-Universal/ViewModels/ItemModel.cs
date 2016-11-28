using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsTerm_Universal.ViewModels
{
    public class ItemModel : INotifyPropertyChanged
    {
        private bool _unread;

        public int id { get; set; }
        public String title { get; set; }
        public String body { get; set; }
        public String author { get; set; }
        public String favicon { get; set; }
        public bool unread { get { return _unread; }
            set
            {
                _unread = value;
                OnPropertyChanged("unread");
            }
        }

        public ItemModel()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public static ItemModel FromItem(NextcloudNewsInterface.NextcloudNewsItem backendItem)
        {
            var newItem = new ItemModel();

            newItem.id = backendItem.id;
            newItem.title = backendItem.title;
            newItem.author = backendItem.author;
            newItem.body = backendItem.body;
            newItem.unread = backendItem.unread;

            return newItem;
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
