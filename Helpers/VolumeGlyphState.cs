using Live_Music.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Live_Music.Helpers
{
    public class VolumeGlyphState : DependencyObject,INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly FontIcon MuteIcon = new FontIcon { FontFamily = new Windows.UI.Xaml.Media.FontFamily("Segoe MDL2 Assets"), Glyph = "\uE198",FontSize = 16 };
        private readonly FontIcon Volume0Icon = new FontIcon { FontFamily = new Windows.UI.Xaml.Media.FontFamily("Segoe MDL2 Assets"), Glyph = "\uE992", FontSize = 16 };
        private readonly FontIcon Volume1Icon = new FontIcon { FontFamily = new Windows.UI.Xaml.Media.FontFamily("Segoe MDL2 Assets"), Glyph = "\uE993", FontSize = 16 };
        private readonly FontIcon Volume2Icon = new FontIcon { FontFamily = new Windows.UI.Xaml.Media.FontFamily("Segoe MDL2 Assets"), Glyph = "\uE994", FontSize = 16 };
        private readonly FontIcon Volume3Icon = new FontIcon { FontFamily = new Windows.UI.Xaml.Media.FontFamily("Segoe MDL2 Assets"), Glyph = "\uE995", FontSize = 16 };
        private FontIcon _volumeGlyph;

        MusicInfomation musicInfomation = App.musicInfomation;
        MusicService musicService = App.musicService;

        public VolumeGlyphState()
        {
            ChangeVolumeGlyph();
            ChangeVolumeGlyph(true);
            musicService.mediaPlayer.VolumeChanged += MediaPlayer_VolumeChanged;
            musicService.mediaPlayer.IsMutedChanged += MediaPlayer_IsMutedChanged;
        }

        private void MediaPlayer_IsMutedChanged(Windows.Media.Playback.MediaPlayer sender, object args)
        {
            switch (musicService.mediaPlayer.IsMuted)
            {
                case true:
                    VolumeGlyph = MuteIcon;
                    break;
                case false:
                    ChangeVolumeGlyph();
                    break;
            }
        }

        private void MediaPlayer_VolumeChanged(Windows.Media.Playback.MediaPlayer sender, object args)
        {
            if (musicService.mediaPlayer.IsMuted != true)
            {
                ChangeVolumeGlyph();
            }
        }

        private void ChangeVolumeGlyph(bool IsInitialize)
        {
            if (IsInitialize == true)
            {
                double MediaPlayerVolume = musicInfomation.MusicVolumeProperties;
                if (MediaPlayerVolume > 0.6)
                {
                    _volumeGlyph = Volume3Icon;
                }
                else if (MediaPlayerVolume > 0.3 && MediaPlayerVolume < 0.6)
                {
                    _volumeGlyph = Volume2Icon;
                }
                else if (MediaPlayerVolume > 0 && MediaPlayerVolume < 0.3)
                {
                    _volumeGlyph = Volume1Icon;
                }
                else if (MediaPlayerVolume == 0)
                {
                    _volumeGlyph = Volume0Icon;
                }
            }
        }

        public void ChangeVolumeGlyph()
        {
            double MediaPlayerVolume = musicInfomation.MusicVolumeProperties;
            if (MediaPlayerVolume > 0.6)
            {
                VolumeGlyph = Volume3Icon;
            }
            else if (MediaPlayerVolume > 0.3 && MediaPlayerVolume < 0.6)
            {
                VolumeGlyph = Volume2Icon;
            }
            else if (MediaPlayerVolume > 0 && MediaPlayerVolume < 0.3)
            {
                VolumeGlyph = Volume1Icon;
            }
            else if (MediaPlayerVolume == 0)
            {
                VolumeGlyph = Volume0Icon;
            }
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

        public FontIcon VolumeGlyph
        {
            get => _volumeGlyph;
            set
            {
                _volumeGlyph = value;
                OnPropertiesChanged();
            }
        }
    }
}
