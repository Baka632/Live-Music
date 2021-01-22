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
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using System.Threading;
using Id3;

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
        private TileHelper tileHelper = new TileHelper();
        /// <summary>
        /// 音乐信息的实例
        /// </summary>
        private MusicInfomation musicInfomation = App.musicInfomation;
        /// <summary>
        /// 音乐服务的实例
        /// </summary>
        private MusicService musicService = App.musicService;
        /// <summary>
        /// AppInfomation的实例
        /// </summary>
        private AppInfomation appInfomation = App.appInfomation;
        /// <summary>
        /// 取图片主题色的类的实例
        /// </summary>
        ImageColors.ImageThemeBrush imageThemeBrush = new ImageColors.ImageThemeBrush();
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
        /// 指示现在是否在添加音乐的值
        /// </summary>
        private bool IsAddingMusic = false;
        /// <summary>
        /// 声音图标状态的实例
        /// </summary>
        private VolumeGlyphState volumeGlyphState = App.volumeGlyphState;
        /// <summary>
        /// 当已更新完musicInfomation中的属性时发生
        /// </summary>
        public static event Action PropertiesLoadCompleteEvent;

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

        /// <summary>
        /// 支持的音频格式数组
        /// </summary>
        private static string[] supportedAudioFormats = new string[]
        {
            ".mp3", ".wav", ".wma", ".aac", ".adt", ".adts", ".ac3", ".ec3",
        };

        /// <summary>
        /// 初始化MainPage类的新实例
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();
            appInfomation.IsPlayerButtonEnabled = false;
            musicService.RepeatingMusicValueChanged += MusicService_RepeatingMusicValueChanged;
            Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated += Dispatcher_AcceleratorKeyActivated;
            musicService.mediaPlaybackList.CurrentItemChanged += MediaPlaybackList_CurrentItemChanged;
            musicService.mediaPlayer.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChanged;
            musicService.mediaPlayer.MediaFailed += MediaPlayer_MediaFailed;
            musicService.mediaPlayer.PlaybackSession.NaturalDurationChanged += PlaybackSession_NaturalDurationChanged;
            NowPlaying.NowPlayingDargOverEvent += NowPlaying_NowPlayingDargOverEvent;

            NavigationCacheMode = NavigationCacheMode.Required;

            mainContectFrame.Navigate(typeof(Views.FrameContect), null, new SuppressNavigationTransitionInfo());
            processSlider.IsEnabled = processSliderNarrow.IsEnabled = false;
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            processSlider.AddHandler(PointerReleasedEvent, new PointerEventHandler(UIElement_OnPointerReleased), true);
            processSlider.AddHandler(PointerPressedEvent, new PointerEventHandler(UIElement_EnterPressedReleased), true);
            processSliderNarrow.AddHandler(PointerReleasedEvent, new PointerEventHandler(UIElement_OnPointerReleased), true);
            processSliderNarrow.AddHandler(PointerPressedEvent, new PointerEventHandler(UIElement_EnterPressedReleased), true);
            if (musicService.mediaPlaybackList.CurrentItem == null)
            {
                appInfomation.IsMusicPlayingControlVisible = Visibility.Collapsed;
            }
            ChangeNavgationViewSelectItem();
        }

        private void PlaybackSession_NaturalDurationChanged(MediaPlaybackSession sender, object args)
        {
            musicInfomation.MusicLenthProperties = sender.NaturalDuration.ToString(@"m\:ss");
            musicInfomation.MusicDurationProperties = sender.NaturalDuration.TotalSeconds;
        }

        private void MusicService_RepeatingMusicValueChanged(RepeatingMusicValueChangedEventArgs e)
        {
            appInfomation.RepeatMusicButtonState = e.NewValue;
        }

        private void NowPlaying_NowPlayingDargOverEvent(NowPlayingDragOverEventArgs args)
        {
            appInfomation.IsMediaControlVisible = Visibility.Visible;
            PlayAndGetMusicProperites(args.Items);
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
            appInfomation.MusicNowPlayingTimeTextBlockText = musicService.mediaPlayer.PlaybackSession.Position.ToString(@"m\:ss");
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
                processSlider.Value = processSliderNarrow.Value = musicService.mediaPlayer.PlaybackSession.Position.TotalSeconds;
                appInfomation.MusicNowPlayingTimeTextBlockText = musicService.mediaPlayer.PlaybackSession.Position.ToString(@"m\:ss");
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
                appInfomation.MusicNowPlayingTimeTextBlockText = TimeSpan.FromSeconds(e.NewValue).ToString(@"m\:ss");
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
                appInfomation.ShuffleMusicButtonState = true;
            }
            else
            {
                ShufflingMusicProperties = "随机播放:关";
                appInfomation.ShuffleMusicButtonState = false;
            }
            SetTileSource();
        }

        /// <summary>
        /// 改变播放器"重复播放"的属性
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RepeatMusic(object sender, RoutedEventArgs e)
        {
            // ButtonOrder: true -> null -> false
            switch (musicService.IsRepeatingMusic)
            {
                case false:
                    musicService.RepeatMusic(true);
                    appInfomation.RepeatMusicButtonIconGlyph = "\uE1CD";
                    appInfomation.RepeatMusicButtonState = true;
                    RepeatingMusicProperties = "循环播放:全部循环";
                    break;
                case true:
                    musicService.RepeatMusic(null);
                    appInfomation.RepeatMusicButtonState = null;
                    appInfomation.RepeatMusicButtonIconGlyph = "\uE1CC";
                    RepeatingMusicProperties = "循环播放:单曲循环";
                    break;
                case null:
                    musicService.RepeatMusic(false);
                    appInfomation.RepeatMusicButtonIconGlyph = "\uE1CD";
                    appInfomation.RepeatMusicButtonState = false;
                    RepeatingMusicProperties = "循环播放:关闭循环";
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
                        appInfomation.NowPlayingButtonIconGlyph = "\uE103";
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
                        appInfomation.NowPlayingButtonIconGlyph = "\uE102";
                        NowPlayingProperties = "播放";
                        if (musicService.mediaPlaybackList.Items.Count == musicService.mediaPlaybackList.CurrentItemIndex + 1 && (int)processSlider.Value == (int)processSlider.Maximum)
                        {
                            dispatcherTimer.Stop();
                            appInfomation.MusicNowPlayingTimeTextBlockText = "0:00";
                            processSlider.Value = processSliderNarrow.Value = 0;
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
                Windows.Media.MusicDisplayProperties CurrentItemMusicDisplayProperties = sender.CurrentItem.GetDisplayProperties().MusicProperties;
                RandomAccessStreamReference CurrentItemMusicThumbnail = sender.CurrentItem.GetDisplayProperties().Thumbnail;
                IRandomAccessStream musicThumbnail = await CurrentItemMusicThumbnail.OpenReadAsync();
                BitmapImage musicImage = null;
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    musicImage = new BitmapImage();
                    await musicImage.SetSourceAsync(musicThumbnail);
                 });

                musicInfomation.MusicAlbumArtistProperties = CurrentItemMusicDisplayProperties.AlbumArtist;
                musicInfomation.MusicTitleProperties = CurrentItemMusicDisplayProperties.Title;
                musicInfomation.MusicImageProperties = musicImage;
                musicInfomation.MusicAlbumProperties = CurrentItemMusicDisplayProperties.AlbumTitle;

                PropertiesLoadCompleteEvent.Invoke();

                SetTileSource();

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    appInfomation.IsMusicPlayingControlVisible = Visibility.Visible;
                    processSlider.IsEnabled = processSliderNarrow.IsEnabled = true;
                    processSlider.Value = processSliderNarrow.Value = 0;
                    appInfomation.MusicNowPlayingTimeTextBlockText = "0:00";
                    dispatcherTimer.Start();
                });

                await Task.Run(async () =>
                {
                    string AlbumSaveName = musicInfomation.MusicAlbumProperties.Replace(":", string.Empty).Replace("/", string.Empty).Replace("\\", string.Empty).Replace("?", string.Empty).Replace("*", string.Empty).Replace("|", string.Empty).Replace("\"", string.Empty).Replace("<", string.Empty).Replace(">", string.Empty);
                    if (!File.Exists($"{ApplicationData.Current.TemporaryFolder.Path}\\{AlbumSaveName}.jpg"))
                    {
                        StorageFile storageFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync($"{AlbumSaveName}.jpg", CreationCollisionOption.OpenIfExists);
                        var fileStream = await storageFile.OpenStreamForWriteAsync();
                        await musicThumbnail.GetInputStreamAt(0).AsStreamForRead().CopyToAsync(fileStream);
                        fileStream.Dispose();
                    }
                });
            }
            else
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    dispatcherTimer.Stop();
                    appInfomation.IsMusicPlayingControlVisible = Visibility.Collapsed;
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
            IsAddingMusic = false;
            musicProcessGrid.Visibility = Visibility.Collapsed;
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
            musicInfomation.ResetAllMusicProperties();
        }

        /// <summary>
        /// 当播放器无法播放媒体时调用的方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void MediaPlayer_MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
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
                ResetMusicPropertiesList();
                if (musicService.mediaPlaybackList.Items != null)
                {
                    musicService.mediaPlaybackList.Items.Clear();
                }
                PlayAndGetMusicProperites(fileList);
                appInfomation.IsMediaControlVisible = Visibility.Visible;
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

            try
            {
                IReadOnlyList<StorageFile> fileList = await MusicPicker.PickMultipleFilesAsync();
                if (fileList.Count > 0)
                {
                    PlayAndGetMusicProperites(fileList);
                    appInfomation.IsMediaControlVisible = Visibility.Visible;
                }
            }
            catch (Exception ex) when (ex.HResult == -2147023170)
            {
                appInfomation.IsInfoBarButtonShow = Visibility.Collapsed;
                appInfomation.InfoBarMessage = "请重新启动应用";
                appInfomation.InfoBarTitle = "灾难性故障-打开文件夹失败";
                appInfomation.InfoBarSeverity = muxc.InfoBarSeverity.Warning;
                appInfomation.IsInfoBarOpen = true;
            }
        }

        /// <summary>
        /// 播放音乐,以及获取音乐的属性
        /// </summary>
        /// <param name="file">传入的音乐文件</param>
        private async void PlayAndGetMusicProperites(IReadOnlyList<StorageFile> fileList)
        {
            await Task.Run(() => GetProps(fileList));
            if (IsFirstTimeAddMusic == true)
            {
                musicService.mediaPlayer.Source = musicService.mediaPlaybackList;
                musicService.mediaPlayer.Play();
                ChangeMusicControlButtonsUsableState();
                IsFirstTimeAddMusic = false;
            }
            SetTileSource();

            async void GetProps(IReadOnlyList<StorageFile> fileList1)
            {
                IsAddingMusic = true;
                for (int i = 0; i < fileList1.Count; i++)
                {
                    if (IsAddingMusic == false)
                    {
                        ResetMusicPropertiesList();
                        musicService.StopMusic();
                        break;
                    }
                    StorageFile file = fileList1[i];

                    MusicProperties musicProperties = await file.Properties.GetMusicPropertiesAsync();

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


                    MediaPlaybackItem mediaPlaybackItem = new MediaPlaybackItem(MediaSource.CreateFromStorageFile(fileList1[i]));
                    MediaItemDisplayProperties props = mediaPlaybackItem.GetDisplayProperties();
                    props.Type = Windows.Media.MediaPlaybackType.Music;
                    props.MusicProperties.Title = musicProperties.Title;
                    props.MusicProperties.Artist = musicProperties.Artist;
                    props.MusicProperties.AlbumTitle = musicProperties.Album;
                    props.MusicProperties.TrackNumber = musicProperties.TrackNumber;
                    props.MusicProperties.AlbumArtist = musicProperties.AlbumArtist;
                    foreach (string item in musicProperties.Genre)
                    {
                        props.MusicProperties.Genres.Add(item);
                    }
                    #region Image
                    Mp3 mp3 = new Mp3(await file.OpenStreamForReadAsync());
                    var tag2x = mp3.GetTag(Id3TagFamily.Version2X);
                    var tag1x = mp3.GetTag(Id3TagFamily.Version1X);

                    if (tag2x.Pictures.Count < 1 && tag1x.Pictures.Count < 1)
                    {
                        var Thumbnail = await (await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/NullAlbum.png"))).GetScaledImageAsThumbnailAsync(ThumbnailMode.SingleItem);
                        props.Thumbnail = RandomAccessStreamReference.CreateFromStream(Thumbnail);
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            musicInfomation.GridAcrylicBrushColorProperties = (Color)Application.Current.Resources["SystemAccentColorDark2"];
                        });
                    }
                    else
                    {
                        StorageItemThumbnail Thumbnail = await file.GetScaledImageAsThumbnailAsync(ThumbnailMode.SingleItem);
                        props.Thumbnail = RandomAccessStreamReference.CreateFromStream(Thumbnail);
                        musicInfomation.GridAcrylicBrushColorProperties = await imageThemeBrush.GetPaletteImage(Thumbnail);
                    }
                    #endregion
                    mediaPlaybackItem.ApplyDisplayProperties(props);
                    musicService.mediaPlaybackList.Items.Add(mediaPlaybackItem);
                }
                IsAddingMusic = false;
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
            if (appInfomation.IsPlayerButtonEnabled == false)
            {
                appInfomation.IsPlayerButtonEnabled = true;
            }
            else
            {
                appInfomation.IsPlayerButtonEnabled = false;
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
            Frame.Navigate(typeof(NowPlaying), null);
        }

        /// <summary>
        /// 设置磁贴的源
        /// </summary>
        private async void SetTileSource()
        {
            string album = $"{musicInfomation.MusicAlbumProperties.Replace(":", string.Empty).Replace(" / ", string.Empty).Replace("\\", string.Empty).Replace(" ? ", string.Empty).Replace(" * ", string.Empty).Replace(" | ", string.Empty).Replace("\"", string.Empty).Replace("<", string.Empty).Replace(">", string.Empty)}";
            string imagePath = $"{ApplicationData.Current.TemporaryFolder.Path}\\{album}.jpg";
            if (album == "未知专辑")
            {
                imagePath = (await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/NullAlbum.png"))).Path;
            }

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (musicService.mediaPlaybackList.Items.Count > musicService.mediaPlaybackList.CurrentItemIndex + 1)
                {
                    int index = (int)(musicService.mediaPlaybackList.CurrentItemIndex + 1);
                    Windows.Media.MusicDisplayProperties MusicProps = musicService.mediaPlaybackList.Items[index].GetDisplayProperties().MusicProperties;
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
                                        Source = imagePath
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
                                        Source = imagePath
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
                                        },
                                        new AdaptiveText()
                                        {
                                            Text = ""
                                        },
                                        new AdaptiveText()
                                        {
                                            Text = MusicProps.Title,
                                            HintWrap = true,
                                            HintAlign = AdaptiveTextAlign.Left
                                        },
                                        new AdaptiveText()
                                        {
                                        Text = MusicProps.AlbumArtist,
                                        HintWrap = true
                                        },
                                        new AdaptiveText()
                                        {
                                            Text = MusicProps.AlbumTitle,
                                            HintWrap = true
                                        }
                                    },
                                    PeekImage = new TilePeekImage()
                                    {
                                        Source = imagePath
                                    }
                                }
                            }
                        }
                    };
                    tileHelper.ShowTitle(tileContent);
                }
                else
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
                                        Source = imagePath
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
                                        Source = imagePath
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
                                        Source = imagePath
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
                                        Source = imagePath
                                    }
                                }
                            }
                        }
                    };

                    tileHelper.ShowTitle(tileContent);
                }
            });
        }



        /// <summary>
        /// 重写的OnNavigatedTo方法
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                appInfomation.ShuffleMusicButtonState = musicService.mediaPlaybackList.ShuffleEnabled;
                appInfomation.RepeatMusicButtonState = musicService.IsRepeatingMusic;
                switch (musicService.IsRepeatingMusic)
                {
                    case true:
                        appInfomation.RepeatMusicButtonIconGlyph = "\uE1CD";
                        break;
                    case false:
                        appInfomation.RepeatMusicButtonIconGlyph = "\uE1CD";
                        break;
                    case null:
                        appInfomation.RepeatMusicButtonIconGlyph = "\uE1CC";
                        break;
                }
            }
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

        private void MusicDragOver(object sender, DragEventArgs e)
        {
            var deferral = e.GetDeferral();
            e.DragUIOverride.Caption = "播放";
            DataPackageView dataview = e.DataView;
            if (dataview.Contains(StandardDataFormats.StorageItems))
            {
                e.AcceptedOperation = DataPackageOperation.Copy;
            }
            else
            {
                e.AcceptedOperation = DataPackageOperation.None;
            }
            deferral.Complete();
        }

        private async void MusicDrop(object sender, DragEventArgs e)
        {
            var defer = e.GetDeferral();
            try
            {
                appInfomation.IsMediaControlVisible = Visibility.Visible;
                appInfomation.InfoBarTitle = "正在添加文件...";
                appInfomation.IsInfoBarButtonShow = Visibility.Collapsed;
                appInfomation.InfoBarSeverity = muxc.InfoBarSeverity.Informational;
                appInfomation.InfoBarMessage = "";
                appInfomation.IsInfoBarOpen = true;
                DataPackageView dpv = e.DataView;
                await Task.Run(() =>
                {
                    GetFiles(dpv);
                });
            }
            finally
            {
                defer.Complete();
            }

            async void GetFiles(DataPackageView dpv)
            {
                if (dpv.Contains(StandardDataFormats.StorageItems))
                {
                    IReadOnlyList<IStorageItem> files = await dpv.GetStorageItemsAsync();
                    List<StorageFile> musicList = new List<StorageFile>();
                    if (files.Count > 10)
                    {
                        foreach (IStorageItem file in files.AsParallel())
                        {
                            if (file.IsOfType(StorageItemTypes.File) && (file as StorageFile).ContentType == "audio/mpeg")
                            {
                                musicList.Add(file as StorageFile);
                            }
                        }
                    }
                    else
                    {
                        musicList = (from IStorageItem file in files.AsParallel() where file.IsOfType(StorageItemTypes.File) && (file as StorageFile).ContentType == "audio/mpeg" select file as StorageFile).ToList();
                    }

                    if (musicList.Count > 0)
                    {
                        PlayAndGetMusicProperites(musicList);
                        appInfomation.IsInfoBarOpen = false;
                    }
                    else
                    {
                        List<StorageFolder> musicFolderList = (from IStorageItem file in files where file.IsOfType(StorageItemTypes.Folder) select file as StorageFolder).ToList();
                        if (musicFolderList.Count > 0)
                        {
                            appInfomation.InfoBarTitle = "无法播放拖入的文件夹";
                            appInfomation.IsInfoBarButtonShow = Visibility.Collapsed;
                            appInfomation.InfoBarSeverity = muxc.InfoBarSeverity.Warning;
                            appInfomation.InfoBarMessage = "目前不支持文件夹拖入";
                            appInfomation.IsInfoBarOpen = true;
                        }
                        else
                        {
                            appInfomation.InfoBarTitle = "无法播放拖入的文件";
                            appInfomation.IsInfoBarButtonShow = Visibility.Collapsed;
                            appInfomation.InfoBarSeverity = muxc.InfoBarSeverity.Warning;
                            appInfomation.InfoBarMessage = "请检查你拖入的文件";
                            appInfomation.IsInfoBarOpen = true;
                        }
                    }
                }
            }
        }

        private void Search(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (sender.Text == "Debug")
            {

            }
        }
    }
}