using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI.Xaml;

namespace Live_Music.Services
{
    /// <summary>
    /// 为音乐播放及音乐播放列表提供类和方法
    /// </summary>
    public class MusicService : IDisposable
    {
        /// <summary>
        /// 音乐播放器的实例
        /// </summary>
        public MediaPlayer mediaPlayer = new MediaPlayer();
        /// <summary>
        /// 音乐播放列表的实例
        /// </summary>
        public MediaPlaybackList mediaPlaybackList = new MediaPlaybackList();
        /// <summary>
        /// 指示音乐服务是否被清理的值
        /// </summary>
        private bool disposedValue;

        /// <summary>
        /// 初始化MusicService类的新实例
        /// </summary>
        public MusicService()
        {
            mediaPlayer.AutoPlay = true;
            mediaPlayer.AudioCategory = MediaPlayerAudioCategory.Media;
            mediaPlayer.Volume = App.musicInfomation.MusicVolumeProperties;
        }

        #region DisposeMusicService
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                    mediaPlayer.Dispose();
                    mediaPlaybackList.Items.Clear();

                    mediaPlayer = null;
                    mediaPlaybackList = null;
                }

                // TODO: 释放未托管的资源(未托管的对象)并替代终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
                Debug.WriteLine("已清理音乐服务的资源。");
            }
        }

        // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        ~MusicService()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: false);
        }

        /// <summary>
        /// 清理音乐服务的所有资源,如果使用这个方法,则必须重新实例化这个类
        /// </summary>
        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion

        /// <summary>
        /// 终止音乐播放
        /// </summary>
        public void StopMusic()
        {
            mediaPlayer.Pause();
            mediaPlayer.PlaybackSession.Position = TimeSpan.Zero;
            mediaPlaybackList.Items.Clear();
        }

        /// <summary>
        /// 设置播放器的音量
        /// </summary>
        /// <param name="Volume">将要设置的音量大小,其值范围应在0和1之间,其余值将受到限制</param>
        public void SetMusicPlayerVolume(double Volume)
        {
            mediaPlayer.Volume = Volume;
        }

        /// <summary>
        /// 切换到上一个播放项
        /// </summary>
        public void PreviousMusic()
        {
            mediaPlaybackList.MovePrevious();
        }

        /// <summary>
        /// 切换到下一个播放项
        /// </summary>
        public void NextMusic()
        {
            mediaPlaybackList.MoveNext();
        }

        /// <summary>
        /// 改变播放状态,如果现在的状态为播放,则切换到暂停状态,反之亦然
        /// </summary>
        public void PlayPauseMusic()
        {
            switch (mediaPlayer.PlaybackSession.PlaybackState)
            {
                case MediaPlaybackState.None:
                    mediaPlayer.Play();
                    break;
                case MediaPlaybackState.Playing:
                    mediaPlayer.Pause();
                    break;
                case MediaPlaybackState.Paused:
                    mediaPlayer.Play();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 控制是否随机播放音乐,如果现在的状态为false,则改为true,反之亦然
        /// </summary>
        public void ShuffleMusic()
        {
            if (mediaPlaybackList.ShuffleEnabled == true)
            {
                mediaPlaybackList.ShuffleEnabled = false;
            }
            else
            {
                mediaPlaybackList.ShuffleEnabled = true;
            }
        }

        /// <summary>
        /// 控制是否循环播放
        /// </summary>
        /// <param name="IsRepeating">用于判断是否执行某些操作的值,若值为true,则关闭循环播放,若为false,则启用全部循环,若为null,则会单曲循环</param>
        public void RepeatMusic(bool? IsRepeating)
        {
            switch (IsRepeating)
            {
                case true:
                    mediaPlaybackList.AutoRepeatEnabled = false;
                    mediaPlayer.IsLoopingEnabled = false;
                    break;
                case false:
                    mediaPlaybackList.AutoRepeatEnabled = true;
                    break;
                case null:
                    mediaPlaybackList.AutoRepeatEnabled = true;
                    mediaPlayer.IsLoopingEnabled = true;
                    break;
                default:
                    break;
            }
        }
    }
}
