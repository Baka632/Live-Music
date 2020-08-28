using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace Live_Music.Helpers
{
    /// <summary>
    /// 为音乐的信息提供属性
    /// </summary>
    public class MusicInfomation : DependencyObject, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// 访问本地设置的实例
        /// </summary>
        ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        /// <summary>
        /// 指示音量设置是否由用户控制的值
        /// </summary>
        public bool IsUserMode = false;

        /// <summary>
        /// 播放器音量,默认值为1
        /// </summary>
        double MusicVolume = 1;
        /// <summary>
        /// 专辑艺术家,默认值为空("")
        /// </summary>
        string MusicAlbumArtist = "";
        /// <summary>
        /// 音乐标题,默认值为空("")
        /// </summary>
        string MusicTitle = "";
        /// <summary>
        /// 音乐长度,默认值为空("")
        /// </summary>
        string MusicLenth = "";
        /// <summary>
        /// 音乐缩略图,默认值为null
        /// </summary>
        BitmapImage MusicImage = null;
        /// <summary>
        /// 音乐缩略图主题色,默认值为系统资源'SystemAccentColorDark2'所定义的值
        /// </summary>
        Color MusicNowPlayingGridAcrylicBrushColor = (Color)Application.Current.Resources["SystemAccentColorDark2"];

        /// <summary>
        /// 初始化MusicInfomation的新实例
        /// </summary>
        public MusicInfomation()
        {
            //这里什么都不会做,因为字段已经设置好了,显式写出这个构造器只是为了跟踪究竟有哪些语句使用了这个类,以及添加实例化类的注释
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
        /// 重置所有属性
        /// </summary>
        public void ResetAllMusicProperties()
        {
            MusicAlbumArtistProperties = "";
            MusicImageProperties = null;
            MusicTitleProperties = "";
            MusicLenthProperties = "0:00";
        }

        /// <summary>
        /// 播放器的音量属性
        /// </summary>
        public double MusicVolumeProperties
        {
            get 
            {
                if (localSettings.Values["MusicVolume"] != null && IsUserMode == false)
                {
                    IsUserMode = true;
                    MusicVolume = (double)localSettings.Values["MusicVolume"];
                    return (double)localSettings.Values["MusicVolume"];
                }
                else
                {
                    return MusicVolume;
                }
            }
            set
            {
                if (IsUserMode == false)
                {
                    IsUserMode = true;
                    MusicVolume = (double)localSettings.Values["MusicVolume"];
                }
                else
                {
                    MusicVolume = value;
                }
                App.musicService.SetMusicPlayerVolume(MusicVolume);
                OnPropertiesChanged();
            }
        }

        /// <summary>
        /// 音乐的专辑艺术家属性
        /// </summary>
        public string MusicAlbumArtistProperties
        {
            get => MusicAlbumArtist;
            set
            {
                MusicAlbumArtist = value;
                OnPropertiesChanged();
            }
        }

        /// <summary>
        /// 音乐的标题属性
        /// </summary>
        public string MusicTitleProperties
        {
            get => MusicTitle;
            set
            {
                MusicTitle = value;
                OnPropertiesChanged();
            }
        }

        /// <summary>
        /// 音乐的缩略图属性
        /// </summary>
        public BitmapImage MusicImageProperties
        {
            get => MusicImage;
            set
            {
                MusicImage = value;
                OnPropertiesChanged();
            }
        }

        /// <summary>
        /// 音乐缩略图的主题色属性,此属性将被musicNowPlayingGrid控件作为其亚克力画笔的颜色
        /// </summary>
        public Color GridAcrylicBrushColorProperties
        {
            get => MusicNowPlayingGridAcrylicBrushColor;
            set
            {
                MusicNowPlayingGridAcrylicBrushColor = value;
                OnPropertiesChanged();
            }
        }

        /// <summary>
        /// 音乐的时长属性
        /// </summary>
        public string MusicLenthProperties
        {
            get => MusicLenth;
            set
            {
                MusicLenth = value;
                OnPropertiesChanged();
            }
        }
    }
}
