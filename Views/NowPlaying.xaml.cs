﻿using Live_Music.Helpers;
using Live_Music.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Live_Music.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class NowPlaying : Page
    {
        /// <summary>
        /// 音乐信息的实例
        /// </summary>
        MusicInfomation musicInfomation = App.musicInfomation;
        /// <summary>
        /// 音乐服务的实例
        /// </summary>
        MusicService musicService = App.musicService;
        /// <summary>
        /// fontIcon的实例
        /// </summary>
        FontIcon fontIcon = new FontIcon();
        /// <summary>
        /// 页面上按钮中的FontIcon字号的大小,该字段为常量
        /// </summary>
        private const int PlayPauseButtonFontSize = 20;
        /// <summary>
        /// 指示是否第一次进入页面
        /// </summary>
        bool IsFirstTimeEnterPage = true;

        /// <summary>
        /// 初始化NowPlaying类的新实例
        /// </summary>
        public NowPlaying()
        {
            this.InitializeComponent();
            musicService.mediaPlayer.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChanged;
            NavigationCacheMode = NavigationCacheMode.Enabled;
            switch (musicService.mediaPlayer.PlaybackSession.PlaybackState)
            {
                case MediaPlaybackState.Playing:
                    musicPlayPauseButton.Content = new FontIcon { FontFamily = new FontFamily("Segoe MDL2 Assets"), Glyph = "\uE103", FontSize = PlayPauseButtonFontSize };
                    break;
                case MediaPlaybackState.Paused:
                    musicPlayPauseButton.Content = new FontIcon { FontFamily = new FontFamily("Segoe MDL2 Assets"), Glyph = "\uE102", FontSize = PlayPauseButtonFontSize };
                    break;
                default:
                    ChangeMusicPlayerVisibility(musicService.mediaPlayer.PlaybackSession.PlaybackState);
                    break;
            }
            volumeSlider.Value = musicInfomation.MusicVolumeProperties * 100;
            muteButton.Loaded += MuteButton_Loaded;
            musicService.mediaPlaybackList.CurrentItemChanged += MediaPlaybackList_CurrentItemChanged;
            ChangeVolumeButtonGlyph(musicInfomation.MusicVolumeProperties);
            MainPage.dispatcherTimer.Tick += DispatcherTimer_Tick; ;
        }

        private void DispatcherTimer_Tick(object sender, object e)
        {
            this.processSlider.Value = musicService.mediaPlayer.PlaybackSession.Position.TotalSeconds;
            this.musicNowPlayingTimeTextBlock.Text = musicService.mediaPlayer.PlaybackSession.Position.ToString(@"m\:ss");
        }

        private async void MediaPlaybackList_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                processSlider.Maximum = musicInfomation.MusicDurationProperties;
            });
        }

        /// <summary>
        /// 当MuteButton被加载时调用的方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MuteButton_Loaded(object sender, RoutedEventArgs e)
        {
            ChangeVolumeButtonGlyph(musicInfomation.MusicVolumeProperties);
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
                fontIcon.FontSize = PlayPauseButtonFontSize;
                volumeButton.Content = fontIcon;
                if (muteButton.IsLoaded == true)
                {
                    muteButton.Content = "\uE995";
                }
            }
            else if (MediaPlayerVolume > 0.3 && MediaPlayerVolume < 0.6)
            {
                fontIcon.Glyph = "\uE994";
                fontIcon.FontSize = PlayPauseButtonFontSize;
                volumeButton.Content = fontIcon;
                if (muteButton.IsLoaded == true)
                {
                    muteButton.Content = "\uE994";
                }
            }
            else if (MediaPlayerVolume > 0 && MediaPlayerVolume < 0.3)
            {
                fontIcon.Glyph = "\uE993";
                fontIcon.FontSize = PlayPauseButtonFontSize;
                volumeButton.Content = fontIcon;
                if (muteButton.IsLoaded == true)
                {
                    muteButton.Content = "\uE993";
                }
            }
            else if (MediaPlayerVolume == 0)
            {
                fontIcon.Glyph = "\uE992";
                fontIcon.FontSize = PlayPauseButtonFontSize;
                volumeButton.Content = fontIcon;
                if (muteButton.IsLoaded == true)
                {
                    fontIcon.FontSize = 16;
                    muteButton.Content = "\uE992";
                }
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
                volumeButton.Content = fontIcon;
                if (muteButton.IsLoaded == true)
                {
                    muteButton.Content = "\uE198";
                }
            }
            else
            {
                musicService.mediaPlayer.IsMuted = false;
                ChangeVolumeButtonGlyph(musicService.mediaPlayer.Volume);
            }
        }

        /// <summary>
        /// 改变播放器的播放状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void musicPlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            musicService.PlayPauseMusic();
        }

        /// <summary>
        /// 当播放器的播放状态改变时调用的方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
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
                        musicPlayPauseButton.Content = new FontIcon { FontFamily = new FontFamily("Segoe MDL2 Assets"), Glyph = "\uE103", FontSize = PlayPauseButtonFontSize };
                    });
                    break;
                case MediaPlaybackState.Paused:
                    ChangeMusicPlayerVisibility(MediaPlaybackState.Paused);
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        musicPlayPauseButton.Content = new FontIcon { FontFamily = new FontFamily("Segoe MDL2 Assets"), Glyph = "\uE102", FontSize = PlayPauseButtonFontSize };
                    });
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 更改音乐播放控件的显示状态
        /// </summary>
        /// <param name="state">播放器的播放状态</param>
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

        /// <summary>
        /// 切换到上一首音乐
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PreviousMusic(object sender, RoutedEventArgs e) => musicService.PreviousMusic();

        /// <summary>
        /// 切换到下一首音乐
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NextMusic(object sender, RoutedEventArgs e) => musicService.NextMusic();

        /// <summary>
        /// 改变播放器的"随机播放"属性
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShuffleMusic(object sender, RoutedEventArgs e) => musicService.ShuffleMusic();

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
                    break;
                case false:
                    musicService.RepeatMusic(true);
                    repeatMusicButton.Content = new FontIcon { FontFamily = new FontFamily("Segoe MDL2 Assets"), Glyph = "\uE1CD", FontSize = PlayPauseButtonFontSize };
                    break;
                case null:
                    musicService.RepeatMusic(null);
                    repeatMusicButton.Content = new FontIcon { FontFamily = new FontFamily("Segoe MDL2 Assets"), Glyph = "\uE1CC", FontSize = PlayPauseButtonFontSize };
                    break;
                default:
                    break;
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

            ChangeVolumeButtonGlyph(musicService.mediaPlayer.Volume);
            if (musicService.mediaPlayer.IsMuted == true)
            {
                musicService.mediaPlayer.IsMuted = false;
            }
        }

        /// <summary>
        /// 当页面被加载好时调用的方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            if (IsFirstTimeEnterPage)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    processSlider.Maximum = musicInfomation.MusicDurationProperties;
                });
                IsFirstTimeEnterPage = false;
            }
        }

        private void processSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            musicService.mediaPlayer.PlaybackSession.Position = TimeSpan.FromSeconds(e.NewValue);
            musicNowPlayingTimeTextBlock.Text = musicService.mediaPlayer.PlaybackSession.Position.ToString(@"m\:ss");
        }
    }
}
