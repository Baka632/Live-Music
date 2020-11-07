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

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Live_Music.FirstStartPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Settings : Page
    {
        Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        private enum ThemeSettings
        {
            Light, Dark, Default
        }
        public Settings()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
            themeComboBox.SelectedItem = "使用系统设置";
        }

        private void ThemeChanged(object sender, SelectionChangedEventArgs e)
        {
            string Theme = e.AddedItems[0].ToString();
            switch (Theme)
            {
                case "浅色":
                    App.settings.ThemeSettings = ThemeSettings.Light.ToString();
                    break;
                case "深色":
                    App.settings.ThemeSettings = ThemeSettings.Dark.ToString();
                    break;
                case "使用系统设置":
                    App.settings.ThemeSettings = ThemeSettings.Default.ToString();
                    break;
            }
        }
    }
}
