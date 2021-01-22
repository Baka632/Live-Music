using Live_Music.Services;
using Live_Music.Views;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace Live_Music.Helpers
{
    /// <summary>
    /// 辅助音乐历史记录工作的类
    /// </summary>
    public class MusicHistoryHelper
    {
        const short MaxHistoryItemSaveCount = 50;
        MusicService musicService = App.musicService;
        MusicInfomation musicInfomation = App.musicInfomation;
        Dictionary<string, MusicHistoryTemplate> musicHistroyItems = new Dictionary<string, MusicHistoryTemplate>();

        public event Action<NewItemsAddedToMusicHistoryEventArgs> NewItemsAddedToMusicHistoryEvent;

        public MusicHistoryHelper()
        {
            MusicHistory.MusicHistoryLoadingEvent += MusicHistory_MusicHistoryLoadingEvent;
            MainPage.PropertiesLoadCompleteEvent += MainPage_PropertiesLoadCompleteEvent;
        }

        public Dictionary<string, MusicHistoryTemplate> MusicHistoryItemsProperty
        {
            get => musicHistroyItems;
            set
            {
                foreach (var item in value)
                {
                    musicHistroyItems.Add(item.Key,item.Value);
                }
            }
        }

        private void MainPage_PropertiesLoadCompleteEvent()
        {
            CheckIsNecessaryToAddToMusicHistory();
        }

        private void MusicHistory_MusicHistoryLoadingEvent()
        {
            CheckIsNecessaryToAddToMusicHistory();
        }

        private void CheckIsNecessaryToAddToMusicHistory()
        {
            int index = (int)musicService.mediaPlaybackList.CurrentItemIndex;
            string musicAlbumName = musicInfomation.MusicAlbumProperties;
            if (musicHistroyItems.ContainsKey(musicAlbumName) != true && string.IsNullOrWhiteSpace(musicAlbumName) != true)
            {
                musicHistroyItems.Add(musicAlbumName, new MusicHistoryTemplate(musicAlbumName, musicInfomation.MusicAlbumArtistProperties, MusicContentType.Album, musicInfomation.MusicImageProperties));
                NewItemsAddedToMusicHistoryEvent?.Invoke(new NewItemsAddedToMusicHistoryEventArgs() { ItemList = musicHistroyItems });
            }
        }
    }

    [Serializable]
    /// <summary>
    /// 播放内容的种类
    /// </summary>
    public enum MusicContentType
    {
        /// <summary>
        /// 专辑
        /// </summary>
        Album,
        /// <summary>
        /// 歌手
        /// </summary>
        Artist,
        /// <summary>
        /// 播放列表
        /// </summary>
        Playlist
    }

    [Serializable]
    public class MusicHistoryTemplate : INotifyPropertyChanged
    {
        private string _MusicAlbum;
        private string _MusicAlbumArtist;
        private MusicContentType _ContentType;
        [NonSerialized]
        private BitmapImage _ContentImage;
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 初始化MusicHistoryTemplate的新实例
        /// </summary>
        /// <param name="AlbumName">专辑名称</param>
        /// <param name="AlbumArtist">专辑艺术家</param>
        /// <param name="Type">播放类型</param>
        /// <param name="Image">代表播放内容的图片</param>
        public MusicHistoryTemplate(string AlbumName, string AlbumArtist, MusicContentType Type, BitmapImage Image)
        {
            _MusicAlbum = AlbumName;
            _MusicAlbumArtist = AlbumArtist;
            _ContentType = Type;
            _ContentImage = Image;
        }

        /// <summary>
        /// 通知系统属性已经发生更改
        /// </summary>
        /// <param name="propertyName">发生更改的属性名称,其填充是自动完成的</param>
        public void OnPropertiesChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string MusicAlbum
        {
            get => _MusicAlbum;
            private set
            {
                _MusicAlbum = value;
                OnPropertiesChanged();
            }
        }

        public string MusicAlbumArtist
        {
            get => _MusicAlbumArtist;
            private set
            {
                _MusicAlbumArtist = value;
                OnPropertiesChanged();
            }
        }

        public MusicContentType ContentType
        {
            get => _ContentType;
            private set
            {
                _ContentType = value;
                OnPropertiesChanged();
            }
        }

        public BitmapImage ContentImage
        {
            get => _ContentImage;
            private set 
            {
                _ContentImage = value;
                OnPropertiesChanged();
            }
        }
    }

    public class NewItemsAddedToMusicHistoryEventArgs : EventArgs
    {
        public Dictionary<string,MusicHistoryTemplate> ItemList { get; set; }
    }
}
