using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace Live_Music.Helpers
{
    public class MusicInfomation : DependencyObject, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        string MusicAlbumArtist = "";
        string MusicTitle = "";
        string MusicLenth = "";
        BitmapImage MusicImage = null;
        Color MusicNowPlayingGridAcrylicBrushColor = (Color)Application.Current.Resources["SystemAccentColorDark2"];

        public async void OnPropertiesChanged([CallerMemberName] string propertyName = "")
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }
        public void ResetAllMusicProperties()
        {
            MusicAlbumArtistProperties = "";
            MusicImageProperties = null;
            MusicTitleProperties = "";
            MusicLenthProperties = "0:00";
        }

        public string MusicAlbumArtistProperties
        {
            get => MusicAlbumArtist;
            set
            {
                MusicAlbumArtist = value;
                OnPropertiesChanged();
            }
        }
        public string MusicTitleProperties
        {
            get => MusicTitle;
            set
            {
                MusicTitle = value;
                OnPropertiesChanged();
            }
        }
        public BitmapImage MusicImageProperties
        {
            get => MusicImage;
            set
            {
                MusicImage = value;
                OnPropertiesChanged();
            }
        }
        public Color GridAcrylicBrushColorProperties
        {
            get => MusicNowPlayingGridAcrylicBrushColor;
            set
            {
                MusicNowPlayingGridAcrylicBrushColor = value;
                OnPropertiesChanged();
            }
        }
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
