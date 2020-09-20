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
using Windows.Storage.Search;
using System.Collections.ObjectModel;
using Windows.Storage.FileProperties;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;

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
        IReadOnlyList<StorageFile> fileList;
        public ObservableCollection<string> musicTitleList = new ObservableCollection<string>();

        /// <summary>
        /// 初始化FrameContent类的新实例
        /// </summary>
        public FrameContect()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// 保存当前播放器的音量
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveVolume(object sender, RoutedEventArgs e)
        {
            localSettings.Values["MusicVolume"] = musicService.mediaPlayer.Volume;
        }

        /// <summary>
        /// 抛出一个异常,这仅仅是为了测试
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LeadToException(object sender, RoutedEventArgs e) => throw new ArgumentException("Exception");

        /// <summary>
        /// 显示通知横幅
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowPopup(object sender, RoutedEventArgs e)
        {
            MainPage.popup.IsOpen = true;
        }

        private async Task GetMusic()
        {
            StorageFolder musicFolder = KnownFolders.MusicLibrary;
            fileList = await musicFolder.GetFilesAsync(CommonFileQuery.OrderByMusicProperties);
            BitmapImage bitmapImage = new BitmapImage();
            InMemoryRandomAccessStream randomAccessStream = new InMemoryRandomAccessStream();
            foreach (StorageFile file in fileList)
            {
                MusicProperties musicProperties = await file.Properties.GetMusicPropertiesAsync();
                if (string.IsNullOrWhiteSpace(musicProperties.Title) != true && !musicTitleList.Contains(musicProperties.Album))
                {
                    musicTitleList.Add(musicProperties.Album);
                }
                else if(!musicTitleList.Contains("未知专辑"))
                {
                    musicTitleList.Add("未知专辑");
                }
            }
        }

        private async void GridView_Loaded(object sender, RoutedEventArgs e)
        {
            await GetMusic();
        }
    }
}
