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
using Live_Music.Helpers;
using Live_Music.Services;
using Windows.UI.Xaml.Media.Animation;
using Windows.Media.Playback;
using Windows.UI.Core;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Live_Music.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class CompactPage : Page
    {
        MusicInfomation musicInfomation = App.musicInfomation;
        MusicService musicService = App.musicService;

        public CompactPage()
        {
            this.InitializeComponent();
            musicService.mediaPlayer.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChanged;
            NavigationCacheMode = NavigationCacheMode.Required;
            switch (musicService.mediaPlayer.PlaybackSession.PlaybackState)
            {
                case MediaPlaybackState.Playing:
                    musicPlayPauseButton.Content = new FontIcon { FontFamily = new FontFamily("Segoe MDL2 Assets"), Glyph = "\uE103", FontSize = 27 };
                    break;
                case MediaPlaybackState.Paused:
                    musicPlayPauseButton.Content = new FontIcon { FontFamily = new FontFamily("Segoe MDL2 Assets"), Glyph = "\uE102", FontSize = 27 };
                    break;
                default:
                    ChangeMusicPlayerVisibility(musicService.mediaPlayer.PlaybackSession.PlaybackState);
                    break;
            }
            MainPage.dispatcherTimer.Tick += DispatcherTimer_Tick;
            progressBar.Maximum = musicInfomation.MusicDurationProperties;
        }

        private void DispatcherTimer_Tick(object sender, object e)
        {
            progressBar.Value = musicService.mediaPlayer.PlaybackSession.Position.TotalSeconds;
        }


        private void ExitCompactPageMode(object sender, RoutedEventArgs e)
        {
            CompactPageService compactPageService = new CompactPageService();
            compactPageService.CompactOverlayMode();
            Frame.Navigate(typeof(MainPage), null, new SuppressNavigationTransitionInfo());
        }

        private void musicPlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            musicService.PlayPauseMusic();
        }

        private async void PlaybackSession_PlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            switch (musicService.mediaPlayer.PlaybackSession.PlaybackState)
            {
                case MediaPlaybackState.None:
                    ChangeMusicPlayerVisibility(MediaPlaybackState.None);
                    break;
                case MediaPlaybackState.Opening:
                    break;
                case MediaPlaybackState.Buffering:
                    break;
                case MediaPlaybackState.Playing:
                    ChangeMusicPlayerVisibility(MediaPlaybackState.Playing);
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        musicPlayPauseButton.Content = new FontIcon { FontFamily = new FontFamily("Segoe MDL2 Assets"), Glyph = "\uE103", FontSize = 27 };
                    });
                    break;
                case MediaPlaybackState.Paused:
                    ChangeMusicPlayerVisibility(MediaPlaybackState.Paused);
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        musicPlayPauseButton.Content = new FontIcon { FontFamily = new FontFamily("Segoe MDL2 Assets"), Glyph = "\uE102", FontSize = 27 };
                    });
                    break;
                default:
                    break;
            }
        }

        private async void ChangeMusicPlayerVisibility(MediaPlaybackState state)
        {
            if (state == MediaPlaybackState.None)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    musicInfoGrid.Visibility = Visibility.Collapsed;
                    noneMusicStackPanel.Visibility = Visibility.Visible;
                });
            }
            else
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    musicInfoGrid.Visibility = Visibility.Visible;
                    noneMusicStackPanel.Visibility = Visibility.Collapsed;
                });
            }
        }

        private void PreviousMusic(object sender, RoutedEventArgs e) => musicService.PreviousMusic();

        private void NextMusic(object sender, RoutedEventArgs e) => musicService.NextMusic();
    }
}
