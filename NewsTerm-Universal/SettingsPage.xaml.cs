using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace NewsTerm_Universal
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        private ApplicationDataContainer localSettings;

        public SettingsPage()
        {
            this.InitializeComponent();

            localSettings = ApplicationData.Current.LocalSettings;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (localSettings.Values.ContainsKey("darkTheme") && ((bool)localSettings.Values["darkTheme"]) == true)
                RequestedTheme = ElementTheme.Dark;
            else
                RequestedTheme = ElementTheme.Light;

            if (localSettings.Values.ContainsKey("username"))
            {
                usernameTextBox.Text = localSettings.Values["username"] as String;
            }
            if (localSettings.Values.ContainsKey("host"))
            {
                urlTextBox.Text = localSettings.Values["host"] as String;
            }
            if (localSettings.Values.ContainsKey("password"))
            {
                passwordBox.Password = localSettings.Values["password"] as String;
            }
            if (localSettings.Values.ContainsKey("darkTheme"))
            {
                darkThemeCheckBox.IsChecked = ((bool)localSettings.Values["darkTheme"]);
            }

            if (localSettings.Values.ContainsKey("refresh"))
            {
                switch((int)localSettings.Values["refresh"])
                {
                    case 1:
                        comboBox.SelectedItem = OneMinuteItem;
                        break;
                    case 5:
                        comboBox.SelectedItem = FiveMinuteItem;
                        break;
                    case 10:
                        comboBox.SelectedItem = TenMinuteItem;
                        break;
                    case 15:
                        comboBox.SelectedItem = FifteenMinuteItem;
                        break;
                    case 30:
                        comboBox.SelectedItem = ThirtyMinuteItem;
                        break;
                    case 60:
                        comboBox.SelectedItem = OneHourItem;
                        break;
                    case 480:
                        comboBox.SelectedItem = EightHourItem;
                        break;
                    case 1440:
                        comboBox.SelectedItem = TwentyFourHourItem;
                        break;
                    default:
                        comboBox.SelectedItem = NoRefreshItem;
                        break;
                }
            }
            else
            {
                comboBox.SelectedItem = NoRefreshItem;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            localSettings.Values["host"] = urlTextBox.Text;
            localSettings.Values["username"] = usernameTextBox.Text;
            localSettings.Values["password"] = passwordBox.Password;
            localSettings.Values["darkTheme"] = darkThemeCheckBox.IsChecked;

            var item = comboBox.SelectedItem as ComboBoxItem;

            switch (item.Name)
            {
                case "OneMinuteItem":
                    localSettings.Values["refresh"] = 1;
                    break;
                case "FiveMinuteItem":
                    localSettings.Values["refresh"] = 5;
                    break;
                case "TenMinuteItem":
                    localSettings.Values["refresh"] = 10;
                    break;
                case "FifteenMinuteItem":
                    localSettings.Values["refresh"] = 15;
                    break;
                case "ThirtyMinuteItem":
                    localSettings.Values["refresh"] = 30;
                    break;
                case "OneHourItem":
                    localSettings.Values["refresh"] = 60;
                    break;
                case "EightHourItem":
                    localSettings.Values["refresh"] = 480;
                    break;
                case "TwnetyFourHourItem":
                    localSettings.Values["refresh"] = 1440;
                    break;
                default:
                    localSettings.Values["refresh"] = 0;
                    break;
            }

            ((Frame)Window.Current.Content).GoBack();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ((Frame)Window.Current.Content).GoBack();
        }

        private void darkThemeCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (darkThemeCheckBox.IsChecked == true)
            {
                RequestedTheme = ElementTheme.Dark;
            }
            else
            {
                RequestedTheme = ElementTheme.Light;
            }
        }
    }
}
