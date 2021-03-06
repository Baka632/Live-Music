﻿using Live_Music.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Live_Music.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MusicHistory : Page
    {
        MusicInfomation musicInfomation = App.musicInfomation;
        MusicHistoryHelper musicHistoryHelper = App.musicHistoryHelper;
        public static event Action MusicHistoryLoadingEvent;
        ObservableCollection<MusicHistoryTemplate> musicHistoryTemplates = new ObservableCollection<MusicHistoryTemplate>();

        public MusicHistory()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
            musicHistoryHelper.NewItemsAddedToMusicHistoryEvent += MusicHistoryHelper_NewItemsAddedToMusicHistoryEvent;
        }

        private async void MusicHistoryHelper_NewItemsAddedToMusicHistoryEvent(NewItemsAddedToMusicHistoryEventArgs e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                () => 
                {
                    musicHistoryTemplates.Add(e.ItemList.GetValueOrDefault(musicInfomation.MusicAlbumProperties));
                });
        }

        private void GridView_Loading(FrameworkElement sender, object args)
        {
            MusicHistoryLoadingEvent.Invoke();
        }
    }
}
