﻿using NewsTerm_Universal.ViewModels;
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
        Windows.Storage.ApplicationDataContainer localSettings;

        public MainPage()
        {
            this.InitializeComponent();
            localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            ItemList.getInstance().RefreshComplete += MainPage_RefreshComplete;
            MasterListView.ItemsSource = ItemList.getInstance().Items;
        }

        private void MainPage_SelectedItemChanged(object sender, ItemModel selectedItem)
        {
            nextButton.IsEnabled = ItemList.getInstance().HaveNextItem();
            prevButton.IsEnabled = ItemList.getInstance().HavePreviousItem();

            shareButton.IsEnabled = (selectedItem != null);

            if(selectedItem != null)
            {
                MasterListView.SelectedItem = selectedItem;
                if (AdaptiveStates.CurrentState == NarrowState)
                {
                    Frame.Navigate(typeof(DetailPage), null, new SuppressNavigationTransitionInfo());
                }
                else
                {
                    EnableContentTransitions();
                }
                ItemList.getInstance().MarkItemRead(selectedItem);
            }
        }

        private void MainPage_RefreshComplete(object sender, ItemList.Result resultCondition)
        {
            //TODO: ErrorListText needs to go, we should have a popup notification (not a dialog)
            //to incicate error. Problem if we try to show ErrorListText and there are elements in
            //list

            LoadingProcessProgressRing.IsActive = false;

            if (resultCondition == ItemList.Result.NoError)
            {
                ErrorListText.Visibility = Visibility.Collapsed;
                if (ItemList.getInstance().Items.Count == 0)
                {
                    EmptyListText.Visibility = Visibility.Visible;
                }
                else
                {
                    EmptyListText.Visibility = Visibility.Collapsed;
                    //TODO: select first item
                }
            }
            else
            {
                ErrorListText.Visibility = Visibility.Visible;
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            EmptyListText.Visibility = Visibility.Collapsed;
            ErrorListText.Visibility = Visibility.Collapsed;
            LoadingProcessProgressRing.IsActive = true;
            ItemList.getInstance().RemoveRead();
            ItemList.getInstance().SelectedItem = null;
            NextcloudNewsInterface.NextcloudNewsInterface.getInstance().invalidateCache();
            ItemList.getInstance().Refresh(false);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            ItemList.getInstance().SelectedItemChanged += MainPage_SelectedItemChanged;

            if (localSettings.Values.ContainsKey("darkTheme") && ((bool)localSettings.Values["darkTheme"]) == true)
                RequestedTheme = ElementTheme.Dark;
            else
                RequestedTheme = ElementTheme.Light;

            if (e.Parameter != null && e.Parameter as String != String.Empty)
            {
                var selectedItem = ItemList.getInstance().GetItemById((int)e.Parameter);
                if (selectedItem != null)
                {
                    MasterListView.SelectedItem = selectedItem;
                }
            }
            else
            {
                if (!(localSettings.Values.ContainsKey("username") &&
                      localSettings.Values.ContainsKey("password") &&
                      localSettings.Values.ContainsKey("host")))
                {
                    this.Frame.Navigate(typeof(SettingsPage));
                }

                NextcloudNewsInterface.NextcloudNewsInterface.getInstance(localSettings.Values["host"] as String,
                                                                          localSettings.Values["username"] as String,
                                                                          localSettings.Values["password"] as String);

                LoadingProcessProgressRing.IsActive = true;
                ItemList.getInstance().Refresh(false);
            }

            UpdateForVisualState(AdaptiveStates.CurrentState);

            // Don't play a content transition for first item load.
            // Sometimes, this content will be animated as part of the page transition.
            DisableContentTransitions();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            ItemList.getInstance().SelectedItemChanged -= MainPage_SelectedItemChanged;
        }

        private void UpdateForVisualState(VisualState newState, VisualState oldState = null)
        {
            var isNarrow = newState == NarrowState;

            if (isNarrow && oldState == DefaultState && ItemList.getInstance().SelectedItem != null)
            {
                // Resize down to the detail item. Don't play a transition.
                Frame.Navigate(typeof(DetailPage), null, new SuppressNavigationTransitionInfo());
            } 

            EntranceNavigationTransitionInfo.SetIsTargetElement(MasterListView, isNarrow);
            if (DetailContentPresenter != null)
            {
                EntranceNavigationTransitionInfo.SetIsTargetElement(DetailContentPresenter, !isNarrow);
            }

            if(isNarrow)
            {
                shareButton.Visibility = Visibility.Collapsed;
                nextButton.Visibility = Visibility.Collapsed;
                prevButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                shareButton.Visibility = Visibility.Visible;
                nextButton.Visibility = Visibility.Visible;
                prevButton.Visibility = Visibility.Visible;
            }
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            // Assure we are displaying the correct item. This is necessary in certain adaptive cases.
            if(ItemList.getInstance().SelectedItem != null)
                MasterListView.SelectedItem = ItemList.getInstance().SelectedItem;
        }

        private void AdaptiveStates_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            UpdateForVisualState(e.NewState, e.OldState);
        }

        private void MasterListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as ItemModel;
            if (item != null)
                ItemList.getInstance().SelectedItem = item;
            //if(AdaptiveStates.CurrentState == NarrowState)
            //{
            //    Frame.Navigate(typeof(DetailPage), null, new SuppressNavigationTransitionInfo());
            //}
            //else
            //{
            //    EnableContentTransitions();
            //}
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
                var nextItem = ItemList.getInstance().GetNextItem();
                if(nextItem != null)
                    ItemList.getInstance().SelectedItem = nextItem;
            }
            else if(type == "right")
            {
                var prevItem = ItemList.getInstance().GetPreviousItem();
                if (prevItem != null)
                    ItemList.getInstance().SelectedItem = prevItem;
            }
        }

        private void DetailContentPresenter_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            //TODO: Clear this out if we aren't using it
            var newItem = args.NewValue as ItemModel;

            if (newItem != null)
            {
            }
        }

        private void MasterListView_RefreshRequested(object sender, RefreshRequestedEventArgs e)
        {
        }

        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SettingsPage));
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            var nextItem = ItemList.getInstance().GetNextItem();
            if (nextItem != null)
            {
                ItemList.getInstance().SelectedItem = nextItem;
                DetailContentPresenter.Content = nextItem;
            }
        }

        private void prevButton_Click(object sender, RoutedEventArgs e)
        {
            var prevItem = ItemList.getInstance().GetPreviousItem();
            if (prevItem != null)
            {
                ItemList.getInstance().SelectedItem = prevItem;
                DetailContentPresenter.Content = prevItem;
            }
        }
    }
}
