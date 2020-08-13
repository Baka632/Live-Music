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

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板
//使用了HSVColorPickers,Win2D,Microsoft Toolkit和Windows UI

namespace Live_Music
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page , INotifyPropertyChanged
    {
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        public event PropertyChangedEventHandler PropertyChanged;

        MusicInfomation musicInfomation = new MusicInfomation();
        MusicService musicService = new MusicService();
        MediaPlaybackItem mediaPlaybackItem;

        bool IsFirstTimeAddMusic = true;
        bool IsSuperButtonShown = false;

        ObservableCollection<string> MusicArtistList = new ObservableCollection<string>();
        ObservableCollection<string> MusicTitleList = new ObservableCollection<string>();
        ObservableCollection<BitmapImage> MusicImageList = new ObservableCollection<BitmapImage>();
        ObservableCollection<Color> MusicGirdColorsList = new ObservableCollection<Color>();
        ObservableCollection<string> MusicLenthList = new ObservableCollection<string>();

        public MainPage()
        {
            this.InitializeComponent();
            pausePlayingButton.IsEnabled = false;
            stopPlayingButton.IsEnabled = false;
            fontIcon.FontFamily = fontFamily;
            Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated += Dispatcher_AcceleratorKeyActivated;
            mediaControlStackPanel.Visibility = Visibility.Collapsed;
            musicProcessStackPanel.Visibility = Visibility.Collapsed;
            musicService.mediaPlaybackList.CurrentItemChanged += MediaPlaybackList_CurrentItemChanged;
            musicService.mediaPlayer.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChanged;
            NavigationCacheMode = NavigationCacheMode.Required;
        }

        private async void PlaybackSession_PlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            switch (musicService.mediaPlayer.PlaybackSession.PlaybackState)
            {
                case MediaPlaybackState.None:
                    break;
                case MediaPlaybackState.Opening:
                    break;
                case MediaPlaybackState.Buffering:
                    break;
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

        public async void OnPropertiesChanged([CallerMemberName] string propertyName = "")
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }

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

        string NowPlayingState = "播放";
        string RepeatingMusicState = "循环播放:关";
        string ShufflingMusicState = "随机播放:关";
        private string NowPlayingProperties
        {
            get => NowPlayingState;
            set
            {
                NowPlayingState = value;
                OnPropertiesChanged();
            }
        }
        private string RepeatingMusicProperties
        {
            get => RepeatingMusicState;
            set
            {
                RepeatingMusicState = value;
                OnPropertiesChanged();
            }
        }
        private string ShufflingMusicProperties
        {
            get => ShufflingMusicState;
            set
            {
                ShufflingMusicState = value;
                OnPropertiesChanged();
            }
        }
        
        private void ResetMusicProperties()
        {
            musicInfomation.ResetAllMusicProperties();
            MusicGirdColorsList.Clear();
            MusicArtistList.Clear();
            MusicImageList.Clear();
            MusicTitleList.Clear();
        }
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
        private void HideSuperButton(object sender, RoutedEventArgs e)
        {
            superButtonGrid.Visibility = Visibility.Collapsed;
            IsSuperButtonShown = false;
        }

        private void Clear(object sender, RoutedEventArgs e) => localSettings.Values.Remove("FirstStart");

        private void LeadToException(object sender, RoutedEventArgs e) => throw new ArgumentException("Exception");

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
                //MusicAlbumArtistProperties = musicProperties.AlbumArtist;
                MusicArtistList.Add(musicProperties.AlbumArtist);
                //MusicTitleProperties = musicProperties.Title;
                MusicTitleList.Add(musicProperties.Title);
                var thumbnail = await file.GetScaledImageAsThumbnailAsync(ThumbnailMode.MusicView);
                await RandomAccessStream.CopyAsync(thumbnail, randomAccessStream);
                randomAccessStream.Seek(0);
                await bitmapImage.SetSourceAsync(randomAccessStream);
                //MusicImageProperties = bitmapImage;
                MusicImageList.Add(bitmapImage);

                ImageColors.ImageThemeBrush imageThemeBrush = new ImageColors.ImageThemeBrush();
                var color = await imageThemeBrush.GetPaletteImage(randomAccessStream);
                //GridAcrylicBrushColorProperties = color;
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

        private void StopMusic(object sender, RoutedEventArgs e)
        {
            musicProcessStackPanel.Visibility = Visibility.Collapsed;
            stopPlayingButton.IsEnabled = false;
            ChangeMusicControlButtonsUsableState();
            ResetMusicProperties();
            musicService.StopMusic();
        }

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
        private void PlayPauseMusic(object sender, RoutedEventArgs e)
        {
            musicService.PlayPauseMusic();
        }

        private void ShowPopup(object sender, RoutedEventArgs e)
        {
           PanePopup.IsOpen = true;
        }

        private void ClosePopup(object sender, RoutedEventArgs e)
        {
           PanePopup.IsOpen = false;
        }

        FontIcon fontIcon = new FontIcon();
        FontFamily fontFamily = new FontFamily("Segoe MDL2 Assets");

        private void VolumeChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            musicService.mediaPlayer.Volume = e.NewValue / 100;
            ChangeVolumeButtonGlyph(musicService.mediaPlayer.Volume);
            if (musicService.mediaPlayer.IsMuted == true)
            {
                musicService.mediaPlayer.IsMuted = false;
            }
        }

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

        private void PreviousMusic(object sender, RoutedEventArgs e) => musicService.PreviousMusic();

        private void NextMusic(object sender, RoutedEventArgs e) => musicService.NextMusic();

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

        private void EnterCompactOverlayMode(object sender, RoutedEventArgs e)
        {
            CompactPageService compactPageService = new CompactPageService();
            compactPageService.CompactOverlayMode();
            Frame.Navigate(typeof(Views.CompactPage), null, new SuppressNavigationTransitionInfo());
        }
    }
}