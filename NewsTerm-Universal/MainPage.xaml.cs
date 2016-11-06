using NewsTerm_Universal.ViewModels;
using NextcloudNewsInterface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace NewsTerm_Universal
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ItemModel _lastSelectedItem;
        private ObservableCollection<ItemModel> _items;
        Windows.Storage.ApplicationDataContainer localSettings;
        NextcloudNewsInterface.NextcloudNewsInterface newsInterface;
        NextcloudNewsInterface.NextcloudNewsFeeds feeds;

        public MainPage()
        {
            this.InitializeComponent();
            localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            refreshButton.Click += RefreshButton_Click;
            _items = new ObservableCollection<ItemModel>();

            MasterListView.ItemsSource = _items;
        }

        private async void Refresh()
        {
            var backendItems = await NextcloudNewsInterface.NextcloudNewsInterface.getInstance().getItems();

            foreach (var item in backendItems.items)
            {
                var existingItem = (from ItemModel selitem in _items where selitem.id == item.id select selitem).FirstOrDefault<ItemModel>();
                if (existingItem == null)
                {
                    var newItem = ItemModel.FromItem(item);
                    var feed = feeds.getFeedForId(item.feedId);
                    if (feed != null)
                        newItem.favicon = feed.faviconLink;
                    else
                        newItem.favicon = "https://boingboing.net/favicon.ico";
                    _items.Insert(0, newItem);
                }
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (!(localSettings.Values.ContainsKey("username") &&
                  localSettings.Values.ContainsKey("password") &&
                  localSettings.Values.ContainsKey("host")))
            {
                //TODO: Pop up a dialog asking for login information
                var dialog = new ContentDialog()
                {
                    Title = "Nextcloud News Login",
                    MaxWidth = this.ActualWidth
                };

                var panel = new StackPanel();

                var hostBox = new TextBox()
                {
                    PlaceholderText = "https://yourdomain.tld"
                };
                var usernameBox = new TextBox()
                {
                    PlaceholderText = "username"
                };
                var passwordBox = new PasswordBox()
                {
                    PlaceholderText = "password"
                };

                panel.Children.Add(hostBox);
                panel.Children.Add(usernameBox);
                panel.Children.Add(passwordBox);

                dialog.Content = panel;
                dialog.PrimaryButtonText = "Log in";
                dialog.IsPrimaryButtonEnabled = true;
                dialog.PrimaryButtonClick += delegate
                {
                    localSettings.Values["host"] = hostBox.Text;
                    localSettings.Values["username"] = usernameBox.Text;
                    localSettings.Values["password"] = passwordBox.Password;
                };

                dialog.SecondaryButtonText = "Cancel";

                var result = await dialog.ShowAsync();
            }

            newsInterface = NextcloudNewsInterface.NextcloudNewsInterface.getInstance(localSettings.Values["host"] as String,
                                                                                      localSettings.Values["username"] as String,
                                                                                      localSettings.Values["password"] as String);

            var items = MasterListView.ItemsSource as List<ItemModel>;

            feeds = await newsInterface.getFeeds();

            if (items == null)
            {
                items = new List<ItemModel>();
                var backendItems = await newsInterface.getItems();

                foreach (var item in backendItems.items)
                {
                    var newItem = ItemModel.FromItem(item);
                    var feed = feeds.getFeedForId(item.feedId);
                    if (feed != null)
                        newItem.favicon = feed.faviconLink;
                    else
                        newItem.favicon = "https://boingboing.net/favicon.ico";
                    items.Add(newItem);
                }

                MasterListView.ItemsSource = items;
            }

            if (e.Parameter != null)
            {
                // Parameter is item ID
                var id = e.Parameter;
                //_lastSelectedItem =
                //    items.Where((item) => item.id == id).FirstOrDefault();
            }

            UpdateForVisualState(AdaptiveStates.CurrentState);

            // Don't play a content transition for first item load.
            // Sometimes, this content will be animated as part of the page transition.
            DisableContentTransitions();
        }

        private void UpdateForVisualState(VisualState newState, VisualState oldState = null)
        {
            var isNarrow = newState == NarrowState;

            if (isNarrow && oldState == DefaultState && _lastSelectedItem != null)
            {
                // Resize down to the detail item. Don't play a transition.
                Frame.Navigate(typeof(DetailPage), _lastSelectedItem.id, new SuppressNavigationTransitionInfo());
            }

            EntranceNavigationTransitionInfo.SetIsTargetElement(MasterListView, isNarrow);
            if (DetailContentPresenter != null)
            {
                EntranceNavigationTransitionInfo.SetIsTargetElement(DetailContentPresenter, !isNarrow);
            }
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            // Assure we are displaying the correct item. This is necessary in certain adaptive cases.
            MasterListView.SelectedItem = _lastSelectedItem;
        }

        private void AdaptiveStates_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            UpdateForVisualState(e.NewState, e.OldState);
        }

        private void MasterListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if(AdaptiveStates.CurrentState == NarrowState)
            {
                var itemid = 0;
                if (_lastSelectedItem == null)
                    itemid = ((ItemModel)e.ClickedItem).id;
                else
                    itemid = _lastSelectedItem.id;
                Frame.Navigate(typeof(DetailPage), itemid, new SuppressNavigationTransitionInfo());
            }
            else
            {
                EnableContentTransitions();
            }
        }

        private void EnableContentTransitions()
        {
            var transition = new EntranceThemeTransition();
            transition.FromHorizontalOffset = 1.0;
            DetailContentPresenter.ContentTransitions.Clear();
            DetailContentPresenter.ContentTransitions.Add(transition);
        }

        private void DisableContentTransitions()
        {
            if (DetailContentPresenter != null)
            {
                DetailContentPresenter.ContentTransitions.Clear();
            }
        }

        private void WebView_ScriptNotify(object sender, NotifyEventArgs e)
        {
            var type = e.Value;
            if (type == "left")
            {
                MasterListView.SelectedIndex++;
            }
            else if(type == "right")
            {
                if(MasterListView.SelectedIndex > 0)
                    MasterListView.SelectedIndex--;
            }
        }

        private void DetailContentPresenter_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            var item = args.NewValue as ItemModel;

            if(_lastSelectedItem != null && item != null)
            {
                var t = Task.Run(async delegate
                {
                    var newsif = NextcloudNewsInterface.NextcloudNewsInterface.getInstance();
                    newsif.markItemRead((await newsif.getItems()).getForId(_lastSelectedItem.id));
                });
                var items = MasterListView.ItemsSource as List<ItemModel>;
                items.Remove(_lastSelectedItem);
            }

            if(item != null)
            {
                _lastSelectedItem = item;
                //NextcloudNewsItem newsItem = newsInterface.getItems().getForId(item.id);
            }
        }

        private void MasterListView_RefreshRequested(object sender, RefreshRequestedEventArgs e)
        {
            Refresh();
        }
    }
}
