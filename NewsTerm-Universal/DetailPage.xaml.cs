﻿using NewsTerm_Universal.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace NewsTerm_Universal
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DetailPage : Page
    {
        private static DependencyProperty s_itemProperty
            = DependencyProperty.Register("Item", typeof(ItemModel), typeof(DetailPage), new PropertyMetadata(null));

        public static DependencyProperty ItemProperty
        {
            get { return s_itemProperty; }
        }

        public ItemModel Item
        {
            get { return (ItemModel)GetValue(s_itemProperty); }
            set { SetValue(s_itemProperty, value); }
        }

        public DetailPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("darkTheme") && ((bool)ApplicationData.Current.LocalSettings.Values["darkTheme"]) == true)
                RequestedTheme = ElementTheme.Dark;
            else
                RequestedTheme = ElementTheme.Light;

            ItemList.getInstance().SelectedItemChanged += DetailPage_SelectedItemChanged;

            var backStack = Frame.BackStack;
            var backStackCount = backStack.Count;

            if (backStackCount > 0)
            {
                var masterPageEntry = backStack[backStackCount - 1];
                backStack.RemoveAt(backStackCount - 1);

                // Doctor the navigation parameter for the master page so it
                // will show the correct item in the side-by-side view.
                var modifiedEntry = new PageStackEntry(
                    masterPageEntry.SourcePageType,
                    ItemList.getInstance().SelectedItem.id,
                    masterPageEntry.NavigationTransitionInfo
                    );
                backStack.Add(modifiedEntry);
            }

            // Register for hardware and software back request from the system
            SystemNavigationManager systemNavigationManager = SystemNavigationManager.GetForCurrentView();
            systemNavigationManager.BackRequested += DetailPage_BackRequested;
            systemNavigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
        }

        private void DetailPage_SelectedItemChanged(object sender, ItemModel selectedItem)
        {
            nextButton.IsEnabled = ItemList.getInstance().HaveNextItem();
            prevButton.IsEnabled = ItemList.getInstance().HavePreviousItem();

            shareButton.IsEnabled = (selectedItem != null);

            if (selectedItem != null)
            {
                Item = selectedItem;
                ItemList.getInstance().MarkItemRead(selectedItem);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            ItemList.getInstance().SelectedItemChanged -= DetailPage_SelectedItemChanged;
            SystemNavigationManager systemNavigationManager = SystemNavigationManager.GetForCurrentView();
            systemNavigationManager.BackRequested -= DetailPage_BackRequested;
            systemNavigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }

        void NavigateBackForWideState(bool useTransition)
        {
            // Evict this page from the cache as we may not need it again.
            NavigationCacheMode = NavigationCacheMode.Disabled;

            if (useTransition)
            {
                Frame.GoBack(new EntranceNavigationTransitionInfo());
            }
            else
            {
                Frame.GoBack(new SuppressNavigationTransitionInfo());
            }
        }

        private bool ShouldGoToWideState()
        {
            return Window.Current.Bounds.Width >= 720;
        }

        private void OnBackRequested()
        {
            // Page above us will be our master view.
            // Make sure we are using the "drill out" animation in this transition.

            Frame.GoBack(new DrillInNavigationTransitionInfo());
        }

        private void PageRoot_Loaded(object sender, RoutedEventArgs e)
        {
            if (ShouldGoToWideState())
            {
                // We shouldn't see this page since we are in "wide master-detail" mode.
                // Play a transition as we are navigating from a separate page.
                NavigateBackForWideState(useTransition: true);
            }
            else
            {
                // Realize the main page content.
                FindName("RootPanel");
            }

            Window.Current.SizeChanged += Window_SizeChanged;

            if (ItemList.getInstance().SelectedItem != null)
            {
                //TODO: I don't like setting Item and doing MarkItemRead here
                Item = ItemList.getInstance().SelectedItem;
                ItemList.getInstance().MarkItemRead(Item);
                nextButton.IsEnabled = ItemList.getInstance().HaveNextItem();
                prevButton.IsEnabled = ItemList.getInstance().HavePreviousItem();
            } 
        }

        private void PageRoot_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged -= Window_SizeChanged;
        }

        private void Window_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            if (ShouldGoToWideState())
            {
                // Make sure we are no longer listening to window change events.
                Window.Current.SizeChanged -= Window_SizeChanged;

                // We shouldn't see this page since we are in "wide master-detail" mode.
                NavigateBackForWideState(useTransition: false);
            }
        }

        private void WebView_ScriptNotify(object sender, NotifyEventArgs e)
        {
            var type = e.Value;
            if (type == "left")
            {
                var newItem = ItemList.getInstance().GetNextItem();
                if (newItem != null)
                {
                    ItemList.getInstance().SelectedItem = newItem;
                }
            }
            else if (type == "right")
            {
                var newItem = ItemList.getInstance().GetPreviousItem();
                if (newItem != null)
                {
                    ItemList.getInstance().SelectedItem = newItem;
                }
            }
        }

        private void DetailPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            // Mark event as handled so we don't get bounced out of the app.
            e.Handled = true;

            OnBackRequested();
        }

        private void shareButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void prevButton_Click(object sender, RoutedEventArgs e)
        {
            var newItem = ItemList.getInstance().GetPreviousItem();
            if (newItem != null)
            {
                ItemList.getInstance().SelectedItem = newItem;
            }
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            var newItem = ItemList.getInstance().GetNextItem();
            if (newItem != null)
            {
                ItemList.getInstance().SelectedItem = newItem;
            }
        }
    }
}
