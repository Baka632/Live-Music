using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace Live_Music
{
    /// <summary>
    /// 为应用程序提供各种信息
    /// </summary>
    public class AppInfomation : DependencyObject , INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// 指示InfoBar是否打开的值
        /// </summary>
        private bool _IsInfoBarOpen = false;
        /// <summary>
        /// InfoBar标题栏的值
        /// </summary>
        private string _InfoBarTitle; 
        /// <summary>
        /// InfoBar信息的值
        /// </summary>
        private string _InfoBarMessage;
        /// <summary>
        /// InfoBar严重性的值
        /// </summary>
        private Microsoft.UI.Xaml.Controls.InfoBarSeverity _InfoBarSeverity;
        /// <summary>
        /// 用以显示详细信息的委托的值
        /// </summary>
        private RoutedEventHandler _InfoBarButtonClick;
        /// <summary>
        /// 指示InfoBar的按钮是否显示的值
        /// </summary>
        private Visibility _IsInfoBarButtonShow;
        /// <summary>
        /// 有关正在播放的控件是否显示的值
        /// </summary>
        private Visibility _IsMusicPlayingControlVisible;
        /// <summary>
        /// 指示播放器的按钮是否显示的值
        /// </summary>
        private bool _IsPlayerButtonEnabled;
        /// <summary>
        /// 指示媒体控件是否显示的值
        /// </summary>
        private Visibility _IsMediaControlVisible;
        /// <summary>
        /// 正在播放音乐的进度的值
        /// </summary>
        private string _MusicNowPlayingTimeTextBlockText;
        /// <summary>
        /// "正在播放"按钮的图标的值
        /// </summary>
        private string _NowPlayingButtonIconGlyph = "\uE102";
        /// <summary>
        /// "循环播放"按钮的图标的值
        /// </summary>
        private string _RepeatMusicButtonIconGlyph = "\uE1CD";
        /// <summary>
        /// "循环播放"按钮的状态
        /// </summary>
        private bool? _RepeatMusicButtonState;
        /// <summary>
        /// "随机播放"按钮的状态
        /// </summary>
        private bool _ShuffleMusicButtonState;

        public AppInfomation()
        {
            RepeatMusicButtonState = App.musicService.IsRepeatingMusic;
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
        /// 指示InfoBar是否打开的属性
        /// </summary>
        public bool IsInfoBarOpen
        {
            get => _IsInfoBarOpen;
            set 
            { 
                _IsInfoBarOpen = value;
                OnPropertiesChanged();
            }
        }

        /// <summary>
        /// InfoBar的标题
        /// </summary>
        public string InfoBarTitle
        {
            get => _InfoBarTitle;
            set 
            { 
                _InfoBarTitle = value;
                OnPropertiesChanged();
            }
        }
        
        /// <summary>
        /// InfoBar的消息
        /// </summary>
        public string InfoBarMessage
        {
            get => _InfoBarMessage;
            set 
            {
                _InfoBarMessage = value;
                OnPropertiesChanged();
            }
        }

        /// <summary>
        /// InfoBar严重性的属性
        /// </summary>
        public Microsoft.UI.Xaml.Controls.InfoBarSeverity InfoBarSeverity
        {
            get => _InfoBarSeverity;
            set 
            {
                _InfoBarSeverity = value;
                OnPropertiesChanged();
            }
        }

        /// <summary>
        /// InfoBar打开详细信息所用委托的属性
        /// </summary>
        public RoutedEventHandler InfoBarButtonClick
        {
            get => _InfoBarButtonClick;
            set 
            { 
                _InfoBarButtonClick = value;
                OnPropertiesChanged();
            }
        }

        /// <summary>
        /// InfoBar的按钮是否显示的属性
        /// </summary>
        public Visibility IsInfoBarButtonShow
        {
            get => _IsInfoBarButtonShow;
            set 
            { 
                _IsInfoBarButtonShow = value;
                OnPropertiesChanged();
            }
        }

        /// <summary>
        /// 有关正在播放的控件是否显示的属性
        /// </summary>
        public Visibility IsMusicPlayingControlVisible
        {
            get => _IsMusicPlayingControlVisible;
            set 
            { 
                _IsMusicPlayingControlVisible = value;
                OnPropertiesChanged();
            }
        }

        /// <summary>
        /// 播放器按钮是否显示的属性
        /// </summary>
        public bool IsPlayerButtonEnabled
        {
            get => _IsPlayerButtonEnabled;
            set 
            {
                _IsPlayerButtonEnabled = value;
                OnPropertiesChanged();
            }
        }

        /// <summary>
        /// 媒体控件是否显示的属性
        /// </summary>
        public Visibility IsMediaControlVisible
        {
            get => _IsMediaControlVisible;
            set 
            { 
                _IsMediaControlVisible = value;
                OnPropertiesChanged();
            }
        }

        /// <summary>
        /// 正在播放音乐的进度的属性
        /// </summary>
        public string MusicNowPlayingTimeTextBlockText
        {
            get => _MusicNowPlayingTimeTextBlockText;
            set 
            { 
                _MusicNowPlayingTimeTextBlockText = value;
                OnPropertiesChanged();
            }
        }

        /// <summary>
        /// "正在播放"按钮的图标的属性
        /// </summary>
        public string NowPlayingButtonIconGlyph
        {
            get => _NowPlayingButtonIconGlyph;
            set 
            { 
                _NowPlayingButtonIconGlyph = value;
                OnPropertiesChanged();
            }
        }
        
        /// <summary>
        /// "重复播放"按钮的图标的属性
        /// </summary>
        public string RepeatMusicButtonIconGlyph
        {
            get => _RepeatMusicButtonIconGlyph;
            set 
            {
                _RepeatMusicButtonIconGlyph = value;
                OnPropertiesChanged();
            }
        }

        /// <summary>
        /// "循环播放"按钮的状态的属性
        /// </summary>
        public bool? RepeatMusicButtonState
        {
            get => _RepeatMusicButtonState;
            set 
            { 
                _RepeatMusicButtonState = value;
                OnPropertiesChanged();
            }
        }

        /// <summary>
        /// "随机播放"按钮的状态的属性
        /// </summary>
        public bool ShuffleMusicButtonState
        {
            get => _ShuffleMusicButtonState;
            set 
            { 
                _ShuffleMusicButtonState = value;
                OnPropertiesChanged();
            }
        }
    }
}