using Live_Music.Helpers;
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
        /// 页面上按钮中的FontIcon字号的大小,该字段为常量
        /// </summary>
        private const int PlayPauseButtonFontSize = 20;
        /// <summary>
        /// 指示是否第一次进入页面
        /// </summary>
        bool IsFirstTimeEnterPage = true;
        VolumeGlyphState volumeGlyphState = App.volumeGlyphState;

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
            musicService.mediaPlaybackList.CurrentItemChanged += MediaPlaybackList_CurrentItemChanged;
            MainPage.dispatcherTimer.Tick += DispatcherTimer_Tick;
            processSlider.AddHandler(UIElement.PointerReleasedEvent /*哪个事件*/, new PointerEventHandler(UIElement_OnPointerReleased) /*使用哪个方法处理*/, true /*如果在之前处理，是否还使用函数*/);
            processSlider.AddHandler(UIElement.PointerPressedEvent /*哪个事件*/, new PointerEventHandler(UIElement_EnterPressedReleased) /*使用哪个方法处理*/, true /*如果在之前处理，是否还使用函数*/);
        }

        /// <summary>
        /// 开始拖拽进度条时调用的方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UIElement_EnterPressedReleased(object sender, PointerRoutedEventArgs e)
        {
            MainPage.IsPointerEntered = true;
        }

        /// <summary>
        /// 结束拖拽进度条时调用的方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void UIElement_OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            musicService.mediaPlayer.PlaybackSession.Position = TimeSpan.FromSeconds(MainPage.SliderNewValue);
            musicNowPlayingTimeTextBlock.Text = musicService.mediaPlayer.PlaybackSession.Position.ToString(@"m\:ss");
            MainPage.IsPointerEntered = false;
        }

        /// <summary>
        /// 当超过计时器间隔时调用的方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DispatcherTimer_Tick(object sender, object e)
        {
            if (MainPage.IsPointerEntered == false)
            {
                processSlider.Value = musicService.mediaPlayer.PlaybackSession.Position.TotalSeconds;
                musicNowPlayingTimeTextBlock.Text = musicService.mediaPlayer.PlaybackSession.Position.ToString(@"m\:ss");
            }
        }

        /// <summary>
        /// 当媒体播放列表的当前播放项目发生更改时调用的方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void MediaPlaybackList_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                processSlider.Maximum = musicInfomation.MusicDurationProperties;
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
                if (muteButton.IsLoaded == true)
                {
                    muteButton.Content = "\uE198";
                }
            }
            else
            {
                musicService.mediaPlayer.IsMuted = false;
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
                        MainPage.dispatcherTimer.Start();
                    });
                    break;
                case MediaPlaybackState.Paused:
                    ChangeMusicPlayerVisibility(MediaPlaybackState.Paused);
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        musicPlayPauseButton.Content = new FontIcon { FontFamily = new FontFamily("Segoe MDL2 Assets"), Glyph = "\uE102", FontSize = PlayPauseButtonFontSize };
                        if (musicService.mediaPlaybackList.Items.Count == musicService.mediaPlaybackList.CurrentItemIndex + 1 && (int)processSlider.Value == (int)processSlider.Maximum)
                        {
                            MainPage.dispatcherTimer.Stop();
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

        /// <summary>
        /// 当进度条被拖动时调用的方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void processSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            MainPage.SliderNewValue = e.NewValue;
            if (MainPage.IsPointerEntered)
            {
                musicNowPlayingTimeTextBlock.Text = TimeSpan.FromSeconds(e.NewValue).ToString(@"m\:ss");
            }
        }
    }
}
