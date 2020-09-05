using System;
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

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板
//使用了Win2D,Microsoft Toolkit和Windows UI

namespace Live_Music
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page , INotifyPropertyChanged
    {
        /// <summary>
        /// 访问本地设置的实例
        /// </summary>
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        public event PropertyChangedEventHandler PropertyChanged;

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
        /// <summary>
        /// 通知横幅
        /// </summary>
        public static Popup popup;

        /// <summary>
        /// 指示是否从启动以来第一次添加音乐
        /// </summary>
        bool IsFirstTimeAddMusic = true;
        /// <summary>
        /// 指示"超级按钮"是否已显示
        /// </summary>
        bool IsSuperButtonShown = false;

        /// <summary>
        /// 音乐艺术家的列表
        /// </summary>
        ObservableCollection<string> MusicArtistList = new ObservableCollection<string>();
        /// <summary>
        /// 音乐标题的列表
        /// </summary>
        ObservableCollection<string> MusicTitleList = new ObservableCollection<string>();
        /// <summary>
        /// 音乐缩略图的列表
        /// </summary>
        ObservableCollection<BitmapImage> MusicImageList = new ObservableCollection<BitmapImage>();
        /// <summary>
        /// 音乐缩略图主题色的列表
        /// </summary>
        ObservableCollection<Color> MusicGirdColorsList = new ObservableCollection<Color>();
        /// <summary>
        /// 音乐长度的列表
        /// </summary>
        ObservableCollection<string> MusicLenthList = new ObservableCollection<string>();

        /// <summary>
        /// 初始化MainPage类的新实例
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();
            pausePlayingButton.IsEnabled = false;
            stopPlayingButton.IsEnabled = false;

            popup = panePopup;
            fontIcon.FontFamily = fontFamily;

            Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated += Dispatcher_AcceleratorKeyActivated;
            musicService.mediaPlaybackList.CurrentItemChanged += MediaPlaybackList_CurrentItemChanged;
            musicService.mediaPlayer.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChanged;

            mediaControlStackPanel.Visibility = Visibility.Collapsed;
            musicProcessStackPanel.Visibility = Visibility.Collapsed;

            NavigationCacheMode = NavigationCacheMode.Required;

            volumeSlider.Value = musicInfomation.MusicVolumeProperties * 100;
            mainContectFrame.Navigate(typeof(Views.FrameContect), null, new SuppressNavigationTransitionInfo());
            ChangeVolumeButtonGlyph(musicInfomation.MusicVolumeProperties);
            //SetTitleBar();
        }

        /// <summary>
        /// 设置自定义的标题栏可拖动区域
        /// </summary>
        private void SetTitleBar()
        {
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;
            // Set XAML element as a draggable region.
            Window.Current.SetTitleBar(null);
        }

        /// <summary>
        /// 当标题栏要响应大小更改时调用的方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            mainPageGrid.Height = sender.Height;
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
                     });
                    break;
                case MediaPlaybackState.Paused:
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        pausePlayingButton.Content = new FontIcon { FontFamily = new FontFamily("Segoe MDL2 Assets"), Glyph = "\uE102", FontSize = 27 };
                        NowPlayingProperties = "播放";
                    });
                    break;
                default:
                    break;
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
        /// 当媒体播放列表的当前播放项目发生更改时调用的方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void MediaPlaybackList_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            if (musicService.mediaPlaybackList.CurrentItem != null)
            {
                musicInfomation.MusicAlbumArtistProperties = MusicArtistList[(int)musicService.mediaPlaybackList.CurrentItemIndex];
                musicInfomation.MusicTitleProperties = MusicTitleList[(int)musicService.mediaPlaybackList.CurrentItemIndex];
                musicInfomation.MusicImageProperties = MusicImageList[(int)musicService.mediaPlaybackList.CurrentItemIndex];
                musicInfomation.GridAcrylicBrushColorProperties = MusicGirdColorsList[(int)musicService.mediaPlaybackList.CurrentItemIndex];
                musicInfomation.MusicLenthProperties = MusicLenthList[(int)musicService.mediaPlaybackList.CurrentItemIndex];
            }
        }

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
            musicInfomation.ResetAllMusicProperties();
            MusicGirdColorsList.Clear();
            MusicArtistList.Clear();
            MusicImageList.Clear();
            MusicTitleList.Clear();
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
        /// 手动打开音乐文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OpenMusicFile(object sender, RoutedEventArgs e)
        {
            var MusicPicker = new Windows.Storage.Pickers.FileOpenPicker();
            MusicPicker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            MusicPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.MusicLibrary;
            MusicPicker.FileTypeFilter.Add(".mp3");
            MusicPicker.FileTypeFilter.Add(".wav"); // TODO: Add more file type

            StorageFile file = await MusicPicker.PickSingleFileAsync();

            if (file != null)
            {
                BitmapImage bitmapImage = new BitmapImage();
                InMemoryRandomAccessStream randomAccessStream = new InMemoryRandomAccessStream();

                MusicProperties musicProperties = await file.Properties.GetMusicPropertiesAsync();
                MusicArtistList.Add(musicProperties.AlbumArtist);
                MusicTitleList.Add(musicProperties.Title);
                var thumbnail = await file.GetScaledImageAsThumbnailAsync(ThumbnailMode.MusicView);
                await RandomAccessStream.CopyAsync(thumbnail, randomAccessStream);
                randomAccessStream.Seek(0);
                await bitmapImage.SetSourceAsync(randomAccessStream);
                MusicImageList.Add(bitmapImage);

                ImageColors.ImageThemeBrush imageThemeBrush = new ImageColors.ImageThemeBrush();
                var color = await imageThemeBrush.GetPaletteImage(randomAccessStream);
                
                MusicGirdColorsList.Add(color);

                MusicLenthList.Add(musicProperties.Duration.ToString(@"m\:ss"));

                mediaPlaybackItem = new MediaPlaybackItem(MediaSource.CreateFromStorageFile(file));
                musicService.mediaPlaybackList.Items.Add(mediaPlaybackItem);
                if (IsFirstTimeAddMusic == true)
                {
                    musicService.mediaPlayer.Source = musicService.mediaPlaybackList;
                    musicService.mediaPlayer.Play();
                    ChangeMusicControlButtonsUsableState();
                    IsFirstTimeAddMusic = false;
                }
                pausePlayingButton.IsEnabled = true;
                stopPlayingButton.IsEnabled = true;
                mediaControlStackPanel.Visibility = Visibility.Visible;
                musicProcessStackPanel.Visibility = Visibility.Visible;
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
        /// 改变控制音乐播放的按钮的可用性
        /// </summary>
        private void ChangeMusicControlButtonsUsableState()
        {
            if (pausePlayingButton.IsEnabled == nextMusicButton.IsEnabled == previousMusicButton.IsEnabled == false)
            {
                pausePlayingButton.IsEnabled = true;
                nextMusicButton.IsEnabled = true;
                previousMusicButton.IsEnabled = true;
            }
            else
            {
                pausePlayingButton.IsEnabled = false;
                nextMusicButton.IsEnabled = false;
                previousMusicButton.IsEnabled = false;
                IsFirstTimeAddMusic = true;
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
        /// 关闭弹出横幅
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClosePopup(object sender, RoutedEventArgs e)
        {
           panePopup.IsOpen = false;
        }

        /// <summary>
        /// FontIcon的实例
        /// </summary>
        FontIcon fontIcon = new FontIcon();
        /// <summary>
        /// FontFamily的实例
        /// </summary>
        FontFamily fontFamily = new FontFamily("Segoe MDL2 Assets");

        /// <summary>
        /// 当音量发生改变时调用的方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VolumeChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            musicInfomation.MusicVolumeProperties = e.NewValue / 100;

            ChangeVolumeButtonGlyph(musicService.mediaPlayer.Volume);
            if (musicService.mediaPlayer.IsMuted == true)
            {
                musicService.mediaPlayer.IsMuted = false;
            }
        }

        /// <summary>
        /// 改变音量按钮的显示图标
        /// </summary>
        /// <param name="MediaPlayerVolume"></param>
        private void ChangeVolumeButtonGlyph(double MediaPlayerVolume)
        {
            if (MediaPlayerVolume > 0.6 && muteButton != null)
            {
                fontIcon.Glyph = "\uE995";
                fontIcon.FontSize = 17;
                muteButton.Content = fontIcon;
            }
            else if (MediaPlayerVolume > 0.3 && MediaPlayerVolume < 0.6)
            {
                fontIcon.Glyph = "\uE994";
                fontIcon.FontSize = 17;
                muteButton.Content = fontIcon;
            }
            else if (MediaPlayerVolume > 0 && MediaPlayerVolume < 0.3)
            {
                fontIcon.Glyph = "\uE993";
                fontIcon.FontSize = 17;
                muteButton.Content = fontIcon;
            }
            else if (MediaPlayerVolume == 0)
            {
                fontIcon.Glyph = "\uE992";
                fontIcon.FontSize = 17;
                muteButton.Content = fontIcon;
            }
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
                fontIcon.Glyph = "\uE198";
                muteButton.Content = fontIcon;
            }
            else
            {
                musicService.mediaPlayer.IsMuted = false;
                ChangeVolumeButtonGlyph(musicService.mediaPlayer.Volume);
            }
        }

        /// <summary>
        /// 切换到上一个音乐
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PreviousMusic(object sender, RoutedEventArgs e) => musicService.PreviousMusic();

        /// <summary>
        /// 切换到下一个音乐
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NextMusic(object sender, RoutedEventArgs e) => musicService.NextMusic();

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
                    repeatMusicButton.Content = new FontIcon { FontFamily = new FontFamily("Segoe MDL2 Assets"), Glyph = "\uE1CD" , FontSize = 16 };
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
                    mainContectFrame.Navigate(typeof(MusicHistory), null);
                    break;
                case "正在播放":
                    break;
            }
            if (args.IsSettingsInvoked == true)
            {
                mainContectFrame.Navigate(typeof(AppSettings), null);
            }
        }
    }
}