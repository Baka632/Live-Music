﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        /// 播放器音量,默认值为1
        /// </summary>
        double MusicVolume = App.settings.MusicVolume;
        /// <summary>
        /// 专辑艺术家,默认值为空("")
        /// </summary>
        string MusicAlbumArtist = "";
        /// <summary>
        /// 音乐标题,默认值为空("")
        /// </summary>
        string MusicTitle = "";
        /// <summary>
        /// 音乐专辑名称,默认值为空("")
        /// </summary>
        string MusicAlbum = "";
        /// <summary>
        /// 音乐长度,默认值为空("")
        /// </summary>
        string MusicLenth = "";
        /// <summary>
        /// 音乐实际长度(未被转换为string),默认值为0
        /// </summary>
        double MusicDuration = 0;
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
            MusicDurationProperties = 0;
            MusicAlbumProperties = "";
        }

        /// <summary>
        /// 播放器的音量属性
        /// </summary>
        public double MusicVolumeProperties
        {
            get => MusicVolume;
            set
            {
                MusicVolume = value;
                VolumeInSliderProperties = value * 100;
                App.musicService.SetMusicPlayerVolume(MusicVolume);
                OnPropertiesChanged();
            }
        }

        public double VolumeInSliderProperties
        {
            get => MusicVolumeProperties * 100;
            set
            {
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
        
        /// <summary>
        /// 音乐的实际时长属性(未被转换为string)
        /// </summary>
        public double MusicDurationProperties
        {
            get => MusicDuration;
            set
            {
                MusicDuration = value;
                OnPropertiesChanged();
            }
        }

        /// <summary>
        /// 音乐的专辑名称属性
        /// </summary>
        public string MusicAlbumProperties
        {
            get => MusicAlbum;
            set
            {
                MusicAlbum = value;
                OnPropertiesChanged();
            }
        }
    }
}
