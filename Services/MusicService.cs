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
    public class MusicService
    {
        public MediaPlayer mediaPlayer = new MediaPlayer();
        public MediaPlaybackList mediaPlaybackList = new MediaPlaybackList();
        ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        bool IsSetStoredVolume = true;
        public MusicService()
        {
            mediaPlayer.AutoPlay = false;
            mediaPlayer.Volume = App.musicInfomation.MusicVolumeProperties;
        }

        public void StopMusic()
        {
            mediaPlayer.Pause();
            mediaPlaybackList.Items.Clear();
        }

        public void SetMusicPlayerVolume(double Volume)
        {
            if (IsSetStoredVolume == true)
            {
                IsSetStoredVolume = false;
                mediaPlayer.Volume = (double)localSettings.Values["MusicVolume"];
                App.musicInfomation.MusicVolumeProperties = (double)localSettings.Values["MusicVolume"];
            }
            else
            {
                mediaPlayer.Volume = Volume;
            }
        }

        public void PreviousMusic()
        {
            mediaPlaybackList.MovePrevious();
        }

        public void NextMusic()
        {
            mediaPlaybackList.MoveNext();
        }

        public void PlayPauseMusic()
        {
            switch (mediaPlayer.PlaybackSession.PlaybackState)
            {
                case MediaPlaybackState.None:
                    mediaPlayer.Play();
                    break;
                case MediaPlaybackState.Opening:
                    break;
                case MediaPlaybackState.Buffering:
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
