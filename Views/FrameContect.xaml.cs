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
using Live_Music.Views;
using Live_Music.Services;
using Live_Music.Helpers;
using Windows.Storage;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Live_Music.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class FrameContect : Page
    {
        /// <summary>
        /// 访问本地设置的实例
        /// </summary>
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        /// <summary>
        /// 音乐信息的实例
        /// </summary>
        MusicInfomation musicInfomation = App.musicInfomation;
        /// <summary>
        /// 音乐服务的实例
        /// </summary>
        MusicService musicService = App.musicService;

        public FrameContect()
        {
            this.InitializeComponent();
        }

        private void SaveVolume(object sender, RoutedEventArgs e)
        {
            localSettings.Values["MusicVolume"] = musicService.mediaPlayer.Volume;
        }

        private void LeadToException(object sender, RoutedEventArgs e) => throw new ArgumentException("Exception");

        private void ShowPopup(object sender, RoutedEventArgs e)
        {
            MainPage.popup.IsOpen = true;
        }
    }
}
