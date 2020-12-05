﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.UI.Notifications;
using Windows.UI.Xaml.Media.Animation;
using muxc = Microsoft.UI.Xaml.Controls;
using System.ComponentModel;
using System.Globalization;
using Live_Music.Services;
using Windows.UI.Core;
using Windows.System;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using Windows.Storage.FileProperties;
using Windows.UI;
using Live_Music.Helpers;
using Microsoft.Toolkit.Extensions;
using System.Collections.ObjectModel;
using Windows.UI.ViewManagement;
using Windows.ApplicationModel.Core;
using Windows.Devices.Custom;
using Live_Music.Views;
using System.Threading.Tasks;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板
//使用了Win2D,Microsoft Toolkit和Windows UI

namespace Live_Music
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// 磁贴助手的实例
        /// </summary>
        TileHelper tileHelper = new TileHelper();
        /// <summary>
        /// 音乐信息的实例
        /// </summary>
        MusicInfomation musicInfomation = App.musicInfomation;
        /// <summary>
        /// 音乐服务的实例
        /// </summary>
        MusicService musicService = App.musicService;
        /// <summary>
        /// 单个媒体播放项
        /// </summary>
        MediaPlaybackItem mediaPlaybackItem;
        AppInfomation appInfomation = App.appInfomation;
        /// <summary>
        /// 指示内嵌的Frame是否能够返回
        /// </summary>
        bool _ContentFrameCanGoBack = false;
        /// <summary>
        /// 计时器DispatcherTimer的实例
        /// </summary>
        public static DispatcherTimer dispatcherTimer;
        /// <summary>
        /// 新的进度条值
        /// </summary>
        public static double SliderNewValue;
        /// <summary>
        /// 指示鼠标指针是否在拖动进度条的值
        /// </summary>
        public static bool IsPointerEntered = false;
        /// <summary>
        /// SMTC显示属性的实例
        /// </summary>
        MediaItemDisplayProperties props;
        /// <summary>
        /// 声音图标状态的实例
        /// </summary>
        VolumeGlyphState volumeGlyphState = App.volumeGlyphState;

        /// <summary>
        /// 指示是否从启动以来第一次添加音乐
        /// </summary>
        bool IsFirstTimeAddMusic = true;
        /// <summary>
        /// 指示"超级按钮"是否已显示
        /// </summary>
        bool IsSuperButtonShown = false;
        /// <summary>
        /// 指示是否导航到"正在播放"的值
        /// </summary>
        bool IsEnteringNowPlaying = false;
        /// <summary>
        /// 正在播放的状态
        /// </summary>
        string NowPlayingState = "播放";
        /// <summary>
        /// 循环播放的状态
        /// </summary>
        string RepeatingMusicState = "循环播放:关";
        /// <summary>
        /// 随机播放的状态
        /// </summary>
        string ShufflingMusicState = "随机播放:关";
        int TotalIndex = 0;

        /// <summary>
        /// 音乐缩略图的列表
        /// </summary>
        Dictionary<int, BitmapImage> MusicImageList = new Dictionary<int, BitmapImage>();
        /// <summary>
        /// 音乐缩略图主题色的列表
        /// </summary>
        Dictionary<int, Color> MusicGirdColorsList = new Dictionary<int, Color>();
        /// <summary>
        /// 音乐属性的列表
        /// </summary>
        Dictionary<int, MusicProperties> MusicPropertiesList = new Dictionary<int, MusicProperties>();
        /// <summary>
        /// 支持的音频格式数组
        /// </summary>
        private static string[] supportedAudioFormats = new string[]
        {
            ".mp3", ".wav", ".wma",".3g2", ".3gp2", ".3gp", ".3gpp", ".m4a", ".asf", ".aac", ".adt", ".adts", ".ac3", ".ec3",
        };

        /// <summary>
        /// 初始化MainPage类的新实例
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();
            pausePlayingButton.IsEnabled = false;
            stopPlayingButton.IsEnabled = false;

            Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated += Dispatcher_AcceleratorKeyActivated;
            musicService.mediaPlaybackList.CurrentItemChanged += MediaPlaybackList_CurrentItemChanged;
            musicService.mediaPlayer.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChanged;
            musicService.mediaPlayer.MediaFailed += MediaPlayer_MediaFailed;
            mediaControlStackPanel.Visibility = Visibility.Collapsed;
            musicProcessStackPanel.Visibility = Visibility.Collapsed;

            NavigationCacheMode = NavigationCacheMode.Required;

            mainContectFrame.Navigate(typeof(Views.FrameContect), null, new SuppressNavigationTransitionInfo());
            processSlider.IsEnabled = false;
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            processSlider.AddHandler(PointerReleasedEvent /*哪个事件*/, new PointerEventHandler(UIElement_OnPointerReleased) /*使用哪个函数处理*/, true /*如果在之前处理，是否还使用函数*/);
            processSlider.AddHandler(PointerPressedEvent /*哪个事件*/, new PointerEventHandler(UIElement_EnterPressedReleased) /*使用哪个函数处理*/, true /*如果在之前处理，是否还使用函数*/);
            if (musicService.mediaPlaybackList.CurrentItem == null)
            {
                musicInfomationButton.Visibility = Visibility.Collapsed;
            }
            ChangeNavgationViewSelectItem();
        }

        /// <summary>
        /// 开始拖拽进度条时调用的方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UIElement_EnterPressedReleased(object sender, PointerRoutedEventArgs e)
        {
            IsPointerEntered = true;
        }

        /// <summary>
        /// 结束拖拽进度条时调用的方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void UIElement_OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            musicService.mediaPlayer.PlaybackSession.Position = TimeSpan.FromSeconds(SliderNewValue);
            musicNowPlayingTimeTextBlock.Text = musicService.mediaPlayer.PlaybackSession.Position.ToString(@"m\:ss");
            IsPointerEntered = false;
        }

        /// <summary>
        /// 当超过计时器间隔时调用的方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dispatcherTimer_Tick(object sender, object e)
        {
            if (IsPointerEntered == false)
            {
                processSlider.Value = musicService.mediaPlayer.PlaybackSession.Position.TotalSeconds;
                musicNowPlayingTimeTextBlock.Text = musicService.mediaPlayer.PlaybackSession.Position.ToString(@"m\:ss");
            }
        }

        /// <summary>
        /// 通知系统属性已经发生更改
        /// </summary>
        /// <param name="propertyName">发生更改的属性名称,其填充是自动完成的</param>
        public async void OnPropertiesChanged([CallerMemberName] string propertyName = "")
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }

        /// <summary>
        /// 改变播放器的静音状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MuteMusic(object sender, RoutedEventArgs e)
        {
            if (musicService.mediaPlayer.IsMuted == false)
            {
                musicService.mediaPlayer.IsMuted = true;
            }
            else
            {
                musicService.mediaPlayer.IsMuted = false;
            }
        }

        /// <summary>
        /// 当进度条被拖动时调用的方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void processSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            SliderNewValue = e.NewValue;
            if (IsPointerEntered)
            {
                musicNowPlayingTimeTextBlock.Text = TimeSpan.FromSeconds(e.NewValue).ToString(@"m\:ss");
            }
        }

        /// <summary>
        /// 切换到上一个音乐
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PreviousMusic(object sender, RoutedEventArgs e)
        {
            musicService.PreviousMusic();
        }

        /// <summary>
        /// 切换到下一个音乐
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NextMusic(object sender, RoutedEventArgs e)
        {
            musicService.NextMusic();
        }

        /// <summary>
        /// 改变播放器的"随机播放"属性
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShuffleMusic(object sender, RoutedEventArgs e)
        {
            musicService.ShuffleMusic();
            if (musicService.mediaPlaybackList.ShuffleEnabled == true)
            {
                ShufflingMusicProperties = "随机播放:开";
            }
            else
            {
                ShufflingMusicProperties = "随机播放:关";
            }
        }

        /// <summary>
        /// 改变播放器"重复播放"的属性
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RepeatMusic(object sender, RoutedEventArgs e)
        {
            switch (repeatMusicButton.IsChecked)
            {
                case true:
                    musicService.RepeatMusic(false);
                    RepeatingMusicProperties = "循环播放:全部循环";
                    break;
                case false:
                    musicService.RepeatMusic(true);
                    repeatMusicButton.Content = new FontIcon { FontFamily = new FontFamily("Segoe MDL2 Assets"), Glyph = "\uE1CD", FontSize = 16 };
                    RepeatingMusicProperties = "循环播放:关闭循环";
                    break;
                case null:
                    musicService.RepeatMusic(null);
                    repeatMusicButton.Content = new FontIcon { FontFamily = new FontFamily("Segoe MDL2 Assets"), Glyph = "\uE1CC", FontSize = 16 };
                    RepeatingMusicProperties = "循环播放:单曲循环";
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 当播放器播放状态发生改变时调用的方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void PlaybackSession_PlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            switch (musicService.mediaPlayer.PlaybackSession.PlaybackState)
            {
                case MediaPlaybackState.Playing:
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        pausePlayingButton.Content = new FontIcon { FontFamily = new FontFamily("Segoe MDL2 Assets"), Glyph = "\uE103", FontSize = 27 };
                        NowPlayingProperties = "暂停";
                        if (dispatcherTimer.IsEnabled == false)
                        {
                            dispatcherTimer.Start();
                        }
                    });
                    break;
                case MediaPlaybackState.Paused:
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        pausePlayingButton.Content = new FontIcon { FontFamily = new FontFamily("Segoe MDL2 Assets"), Glyph = "\uE102", FontSize = 27 };
                        NowPlayingProperties = "播放";
                        if (musicService.mediaPlaybackList.Items.Count == musicService.mediaPlaybackList.CurrentItemIndex + 1 && (int)processSlider.Value == (int)processSlider.Maximum)
                        {
                            dispatcherTimer.Stop();
                            musicNowPlayingTimeTextBlock.Text = "0:00";
                            processSlider.Value = 0;
                        }
                    });
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 改变播放器播放的状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayPauseMusic(object sender, RoutedEventArgs e)
        {
            musicService.PlayPauseMusic();
        }

        /// <summary>
        /// 当媒体播放列表的当前播放项目发生更改时调用的方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void MediaPlaybackList_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            if (musicService.mediaPlaybackList.CurrentItem != null)
            {
                int CurrentItemIndex = (int)musicService.mediaPlaybackList.CurrentItemIndex;

                musicInfomation.MusicAlbumArtistProperties = MusicPropertiesList[CurrentItemIndex].AlbumArtist;
                musicInfomation.MusicTitleProperties = MusicPropertiesList[CurrentItemIndex].Title;
                musicInfomation.MusicImageProperties = MusicImageList[CurrentItemIndex];
                musicInfomation.GridAcrylicBrushColorProperties = MusicGirdColorsList[CurrentItemIndex];
                musicInfomation.MusicLenthProperties = MusicPropertiesList[CurrentItemIndex].Duration.ToString(@"m\:ss");
                musicInfomation.MusicDurationProperties = MusicPropertiesList[CurrentItemIndex].Duration.TotalSeconds;
                musicInfomation.MusicAlbumProperties = MusicPropertiesList[CurrentItemIndex].Album;

                StorageFile storageFile = await ApplicationData.Current.TemporaryFolder.GetFileAsync($"{musicInfomation.MusicAlbumProperties.Replace(":", string.Empty).Replace("/", string.Empty).Replace("\\", string.Empty).Replace("?", string.Empty).Replace("*", string.Empty).Replace("|", string.Empty).Replace("\"", string.Empty).Replace("<", string.Empty).Replace(">", string.Empty)}.jpg");
                props = mediaPlaybackItem.GetDisplayProperties();
                props.Type = Windows.Media.MediaPlaybackType.Music;
                props.MusicProperties.Title = musicInfomation.MusicTitleProperties;
                props.MusicProperties.Artist = musicInfomation.MusicAlbumArtistProperties;
                props.Thumbnail = RandomAccessStreamReference.CreateFromFile(storageFile);
                props.MusicProperties.AlbumTitle = musicInfomation.MusicAlbumProperties;
                mediaPlaybackItem.ApplyDisplayProperties(props);

                SetTileSource();

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    musicInfomationButton.Visibility = Visibility.Visible;
                    processSlider.Maximum = musicInfomation.MusicDurationProperties;
                    processSlider.IsEnabled = true;
                    processSlider.Value = 0;
                    musicNowPlayingTimeTextBlock.Text = "0:00";
                    dispatcherTimer.Start();
                });
            }
            else
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    dispatcherTimer.Stop();
                    musicInfomationButton.Visibility = Visibility.Collapsed;
                });
                tileHelper.DeleteTile();
            }
        }

        /// <summary>
        /// 终止播放音乐
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StopMusic(object sender, RoutedEventArgs e)
        {
            musicProcessStackPanel.Visibility = Visibility.Collapsed;
            stopPlayingButton.IsEnabled = false;
            ChangeMusicControlButtonsUsableState();
            ResetMusicPropertiesList();
            musicService.StopMusic();
        }

        /// <summary>
        /// 播放器"正在播放"状态的属性
        /// </summary>
        private string NowPlayingProperties
        {
            get => NowPlayingState;
            set
            {
                NowPlayingState = value;
                OnPropertiesChanged();
            }
        }

        /// <summary>
        /// 播放器"重复播放"状态的属性
        /// </summary>
        private string RepeatingMusicProperties
        {
            get => RepeatingMusicState;
            set
            {
                RepeatingMusicState = value;
                OnPropertiesChanged();
            }
        }

        /// <summary>
        /// 播放器"循环播放"状态的属性
        /// </summary>
        private string ShufflingMusicProperties
        {
            get => ShufflingMusicState;
            set
            {
                ShufflingMusicState = value;
                OnPropertiesChanged();
            }
        }

        /// <summary>
        /// 重置音乐属性的列表
        /// </summary>
        private void ResetMusicPropertiesList()
        {
            TotalIndex = 0;
            musicInfomation.ResetAllMusicProperties();
            MusicPropertiesList.Clear();
            MusicGirdColorsList.Clear();
            MusicImageList.Clear();
        }

        /// <summary>
        /// 当播放器无法播放媒体时调用的方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void MediaPlayer_MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,async () =>
            {
                ExceptionDetails.ExceptionDetailsDialog exceptionDetailsDialog = new ExceptionDetails.ExceptionDetailsDialog();
                TextBlock textBlock = new TextBlock();
                exceptionDetailsDialog.Title = "无法播放音乐";
                exceptionDetailsDialog.Content = textBlock.Text = "这可能是因为文件已损坏,或者音乐文件的格式不受本应用支持。";
                await exceptionDetailsDialog.ShowAsync();
            });
        }

        /// <summary>
        /// 直接打开音乐文件来播放音乐(应用外)
        /// </summary>
        /// <param name="fileList">传入的文件</param>
        public void OpenMusicFile(IReadOnlyList<StorageFile> fileList)
        {
            if (fileList.Count > 0)
            {
                TotalIndex = 0;
                ResetMusicPropertiesList();
                if (musicService.mediaPlaybackList.Items != null)
                {
                    musicService.mediaPlaybackList.Items.Clear();
                }
                PlayAndGetMusicProperites(fileList);
                mediaControlStackPanel.Visibility = Visibility.Visible;
                musicProcessStackPanel.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// 手动打开音乐文件(应用内)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OpenMusicFile(object sender, RoutedEventArgs e)
        {
            var MusicPicker = new Windows.Storage.Pickers.FileOpenPicker();
            MusicPicker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            MusicPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.MusicLibrary;
            foreach (string fileExtension in supportedAudioFormats)
            {
                MusicPicker.FileTypeFilter.Add(fileExtension);
            }

            IReadOnlyList<StorageFile> fileList = await MusicPicker.PickMultipleFilesAsync();

            if (fileList.Count > 0)
            {
                PlayAndGetMusicProperites(fileList);
                mediaControlStackPanel.Visibility = Visibility.Visible;
                musicProcessStackPanel.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// 播放音乐,以及获取音乐的属性
        /// </summary>
        /// <param name="file">传入的音乐文件</param>
        private async void PlayAndGetMusicProperites(IReadOnlyList<StorageFile> fileList)
        {
            for (int i = 0; i < fileList.Count; i++)
            {
                mediaPlaybackItem = new MediaPlaybackItem(MediaSource.CreateFromStorageFile(fileList[i]));
                musicService.mediaPlaybackList.Items.Add(mediaPlaybackItem);
            }
            
            if (IsFirstTimeAddMusic == true)
            {
                musicService.mediaPlayer.Source = musicService.mediaPlaybackList;
                musicService.mediaPlayer.Play();
                ChangeMusicControlButtonsUsableState();
                IsFirstTimeAddMusic = false;
            }
            if (musicService.mediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.None || musicService.mediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Paused)
            {
                musicService.mediaPlayer.Play();
            }

            for (int i = 0; i < fileList.Count; i++)
            {
                StorageFile file = fileList[i];

                Task<MusicProperties> musicPropertiesTask = file.Properties.GetMusicPropertiesAsync().AsTask();
                musicPropertiesTask.Wait();
                MusicProperties musicProperties = musicPropertiesTask.Result;

                if (string.IsNullOrWhiteSpace(musicProperties.Artist) == true)
                {
                    musicProperties.AlbumArtist = "未知艺术家";
                }

                if (string.IsNullOrWhiteSpace(musicProperties.Title) == true)
                {
                    musicProperties.Title = file.Name;
                }

                if (string.IsNullOrWhiteSpace(musicProperties.Album) == true)
                {
                    musicProperties.Album = "未知专辑";
                }

                MusicPropertiesList.Add(TotalIndex, musicProperties);

                Task<StorageItemThumbnail> musicThumbnailTask = file.GetScaledImageAsThumbnailAsync(ThumbnailMode.SingleItem).AsTask();
                musicThumbnailTask.Wait();

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.SetSource(musicThumbnailTask.Result);

                MusicImageList.Add(TotalIndex, bitmapImage);

                ImageColors.ImageThemeBrush imageThemeBrush = new ImageColors.ImageThemeBrush();
                Task<Color> imageColorsTask = Task.Run(() => imageThemeBrush.GetPaletteImage(musicThumbnailTask.Result));
                imageColorsTask.Wait();
                MusicGirdColorsList.Add(TotalIndex, imageColorsTask.Result);

                string AlbumSaveName = musicProperties.Album;
                AlbumSaveName = AlbumSaveName.Replace(":", string.Empty).Replace("/", string.Empty).Replace("\\", string.Empty).Replace("?", string.Empty).Replace("*", string.Empty).Replace("|", string.Empty).Replace("\"", string.Empty).Replace("<", string.Empty).Replace(">", string.Empty);

                using (var fileStream = File.Create($"{ApplicationData.Current.TemporaryFolder.Path}\\{AlbumSaveName}.jpg"))
                {
                    await WindowsRuntimeStreamExtensions.AsStreamForRead(musicThumbnailTask.Result.GetInputStreamAt(0)).CopyToAsync(fileStream);
                }
                TotalIndex++;
            }
        }

        /// <summary>
        /// 当按下某个键时调用的方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Dispatcher_AcceleratorKeyActivated(CoreDispatcher sender, AcceleratorKeyEventArgs args)
        {
            if (args.EventType.ToString().Contains("Down"))
            {
                var alt = Window.Current.CoreWindow.GetKeyState(VirtualKey.Menu);
                if (alt.HasFlag(CoreVirtualKeyStates.Down))
                {
                    switch (args.VirtualKey)
                    {
                        case VirtualKey.C:
                            superButtonGrid.Visibility = Visibility.Visible;
                            if (IsSuperButtonShown == false)
                            {
                                IsSuperButtonShown = true;
                            }
                            else
                            {
                                HideSuperButton(new object(), new RoutedEventArgs());
                            }
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// 隐藏"超级按钮"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HideSuperButton(object sender, RoutedEventArgs e)
        {
            superButtonGrid.Visibility = Visibility.Collapsed;
            IsSuperButtonShown = false;
        }

        /// <summary>
        /// 改变控制音乐播放的按钮的可用性
        /// </summary>
        private void ChangeMusicControlButtonsUsableState()
        {
            if (pausePlayingButton.IsEnabled == nextMusicButton.IsEnabled == previousMusicButton.IsEnabled == false)
            {
                pausePlayingButton.IsEnabled = true;
                nextMusicButton.IsEnabled = true;
                previousMusicButton.IsEnabled = true;
                stopPlayingButton.IsEnabled = true;
            }
            else
            {
                stopPlayingButton.IsEnabled = false;
                pausePlayingButton.IsEnabled = false;
                nextMusicButton.IsEnabled = false;
                previousMusicButton.IsEnabled = false;
                IsFirstTimeAddMusic = true;
            }
        }

        /// <summary>
        /// 当volumeSlider的值发生改变时调用的方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VolumeChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            musicInfomation.MusicVolumeProperties = e.NewValue / 100;
            if (musicService.mediaPlayer.IsMuted == true)
            {
                musicService.mediaPlayer.IsMuted = false;
            }
        }

        /// <summary>
        /// 进入"最小模式"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EnterCompactOverlayMode(object sender, RoutedEventArgs e)
        {
            CompactPageService compactPageService = new CompactPageService();
            compactPageService.CompactOverlayMode();
            Frame.Navigate(typeof(Views.CompactPage), null, new DrillInNavigationTransitionInfo());
        }

        /// <summary>
        /// 当主页面的显示状态发生改变时调用的方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void mainPageNavigtationView_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
        {
            if (args.DisplayMode == NavigationViewDisplayMode.Compact)
            {
                titleTextBlock.Visibility = Visibility.Collapsed;
            }
            else
            {
                titleTextBlock.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// 当导航视图上的元素被点击时调用的方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void mainPageNavigtationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            switch (args.InvokedItem)
            {
                case "我的音乐":
                    if (mainContectFrame.CurrentSourcePageType != typeof(FrameContect))
                    {
                        mainContectFrame.Navigate(typeof(FrameContect), null);
                    }
                    break;
                case "播放历史":
                    if (mainContectFrame.CurrentSourcePageType != typeof(MusicHistory))
                    {
                        mainContectFrame.Navigate(typeof(MusicHistory), null);
                    }
                    break;
                case "正在播放":
                    if (mainContectFrame.CurrentSourcePageType != typeof(NowPlaying))
                    {
                        EnterNowPlaying();
                    }
                    break;
            }
            if (args.IsSettingsInvoked == true)
            {
                mainContectFrame.Navigate(typeof(AppSettings), null);
            }
            ContentFrameCanGoBack = mainContectFrame.CanGoBack;
        }

        /// <summary>
        /// 当导航视图上的返回按钮被点击时调用的方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void mainPageNavigtationView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (mainContectFrame.CanGoBack)
            {
                mainContectFrame.GoBack();
                ChangeNavgationViewSelectItem();
            }
            ContentFrameCanGoBack = mainContectFrame.CanGoBack;
        }

        /// <summary>
        /// 改变导航视图的选择项
        /// </summary>
        private void ChangeNavgationViewSelectItem()
        {
            if (mainContectFrame.CurrentSourcePageType == typeof(FrameContect))
            {
                mainPageNavigtationView.SelectedItem = myMusic;
            }
            else if (mainContectFrame.CurrentSourcePageType == typeof(MusicHistory))
            {
                mainPageNavigtationView.SelectedItem = musicPlayedHistory;
            }
            else if (mainContectFrame.CurrentSourcePageType == typeof(NowPlaying))
            {
                mainPageNavigtationView.SelectedItem = nowPlaying;
            }
            else if (mainContectFrame.CurrentSourcePageType == typeof(AppSettings))
            {
                mainPageNavigtationView.SelectedItem = mainPageNavigtationView.SettingsItem;
            }
        }

        /// <summary>
        /// 指示内嵌的Frame是否能够返回的属性
        /// </summary>
        private bool ContentFrameCanGoBack
        {
            get => _ContentFrameCanGoBack;
            set
            {
                _ContentFrameCanGoBack = value;
                OnPropertiesChanged();
            }
        }

        /// <summary>
        /// 进入"正在播放"页面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EnterNowPlaying(object sender = null, RoutedEventArgs e = null)
        {
            IsEnteringNowPlaying = true;
            Frame.Navigate(typeof(NowPlaying), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom });
        }

        /// <summary>
        /// 设置磁贴的源
        /// </summary>
        private async void SetTileSource()
        {
            string album = $"{musicInfomation.MusicAlbumProperties.Replace(":", string.Empty).Replace(" / ", string.Empty).Replace("\\", string.Empty).Replace(" ? ", string.Empty).Replace(" * ", string.Empty).Replace(" | ", string.Empty).Replace("\"", string.Empty).Replace("<", string.Empty).Replace(">", string.Empty)}";
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var tileContent = new TileContent()
                {
                    Visual = new TileVisual()
                    {
                        TileSmall = new TileBinding()
                        {
                            Content = new TileBindingContentAdaptive()
                            {
                                BackgroundImage = new TileBackgroundImage()
                                {
                                    Source = $"{ApplicationData.Current.TemporaryFolder.Path}\\{album}.jpg"
                                }
                            }
                        },
                        TileMedium = new TileBinding()
                        {
                            Content = new TileBindingContentAdaptive()
                            {
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = musicInfomation.MusicTitleProperties,
                                        HintMaxLines = 2,
                                        HintWrap = true,
                                        HintAlign = AdaptiveTextAlign.Left
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = musicInfomation.MusicAlbumArtistProperties,
                                        HintMaxLines = 1,
                                        HintWrap = true
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = musicInfomation.MusicAlbumProperties,
                                        HintMaxLines = 1,
                                        HintWrap = true
                                    }
                                },
                                PeekImage = new TilePeekImage()
                                {
                                    Source = $"{ApplicationData.Current.TemporaryFolder.Path}\\{album}.jpg"
                                }
                            }
                        },
                        TileWide = new TileBinding()
                        {
                            Content = new TileBindingContentAdaptive()
                            {
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = musicInfomation.MusicTitleProperties,
                                        HintStyle = AdaptiveTextStyle.Subtitle,
                                        HintMaxLines = 1,
                                        HintWrap = true,
                                        HintAlign = AdaptiveTextAlign.Left
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = musicInfomation.MusicAlbumArtistProperties,
                                        HintMaxLines = 1,
                                        HintWrap = true
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = musicInfomation.MusicAlbumProperties,
                                        HintMaxLines = 1,
                                        HintWrap = true
                                    }
                                },
                                PeekImage = new TilePeekImage()
                                {
                                    Source = $"{ApplicationData.Current.TemporaryFolder.Path}\\{album}.jpg"
                                }
                            }
                        },
                        TileLarge = new TileBinding()
                        {
                            Content = new TileBindingContentAdaptive()
                            {
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = musicInfomation.MusicTitleProperties,
                                        HintStyle = AdaptiveTextStyle.Title,
                                        HintMaxLines = 3,
                                        HintWrap = true,
                                        HintAlign = AdaptiveTextAlign.Left
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = musicInfomation.MusicAlbumArtistProperties,
                                        HintWrap = true
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = musicInfomation.MusicAlbumProperties,
                                        HintWrap = true
                                    }
                                },
                                PeekImage = new TilePeekImage()
                                {
                                    Source = $"{ApplicationData.Current.TemporaryFolder.Path}\\{album}.jpg"
                                }
                            }
                        }
                    }
                };

                tileHelper.ShowTitle(tileContent);
            });
        }


        /// <summary>
        /// 重写的OnNavigatedTo方法
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is IReadOnlyList<StorageFile> file && e.Parameter != null && IsEnteringNowPlaying == false)
            {
                OpenMusicFile(file);
            }
            else if (IsEnteringNowPlaying == true)
            {
                IsEnteringNowPlaying = false;
                ChangeNavgationViewSelectItem();
            }
            else
            {
                //Do nothing
            }
            base.OnNavigatedTo(e);
        }
    }
}