﻿using Live_Music.Helpers;
using Live_Music.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Live_Music.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MyMusic : Page
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
        /// <summary>
        /// 音乐文件的列表
        /// </summary>
        IReadOnlyList<StorageFile> fileList;
        /// <summary>
        /// 音乐标题的列表
        /// </summary>
        public ObservableCollection<string> musicTitleList = new ObservableCollection<string>();
        public MyMusic()
        {
            this.InitializeComponent();
            if (localSettings.Values["IsLoadMusicOnStartUp"] != null)
            {
                IsLoadMusicToggleSwitch.IsOn = (bool)localSettings.Values["IsLoadMusicOnStartUp"];
            }
            else
            {
                IsLoadMusicToggleSwitch.IsOn = false;
                localSettings.Values["IsLoadMusicOnStartUp"] = false;
            }

            if ((bool)localSettings.Values["IsLoadMusicOnStartUp"])
            {
                Task getMusicFile = Task.Run(() =>
                {
                    GetMusic();
                });
            }
        }

        private async void GetMusic()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                StorageFolder musicFolder = KnownFolders.MusicLibrary;
                fileList = await musicFolder.GetFilesAsync(CommonFileQuery.OrderByMusicProperties);
                BitmapImage bitmapImage = new BitmapImage();
                InMemoryRandomAccessStream randomAccessStream = new InMemoryRandomAccessStream();
                foreach (StorageFile file in fileList)
                {
                    MusicProperties musicProperties = await file.Properties.GetMusicPropertiesAsync();
                    if (!musicTitleList.Contains(musicProperties.Album) && string.IsNullOrWhiteSpace(musicProperties.Title) != true)
                    {
                        musicTitleList.Add(musicProperties.Album);
                    }
                    else if (!musicTitleList.Contains("未知专辑"))
                    {
                        musicTitleList.Add("未知专辑");
                    }
                }
            });
        }

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleSwitch toggleSwitch)
            {
                if (toggleSwitch.IsOn == true)
                {
                    localSettings.Values["IsLoadMusicOnStartUp"] = true;
                    Task getMusicFile = Task.Run(() => GetMusic());
                }
                else
                {
                    localSettings.Values["IsLoadMusicOnStartUp"] = false;
                }
            }
        }
    }
}
