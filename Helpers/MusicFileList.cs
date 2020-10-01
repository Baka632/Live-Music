using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace Live_Music.Helpers
{
    class FolderList : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        MusicInfomation musicInfomation = App.musicInfomation;

        public string MusicTitle
        {
            get => musicInfomation.MusicTitleProperties;
            set
            {
                musicInfomation.MusicTitleProperties = value;
                OnPropertiesChanged();
            }
        }

        /// <summary>
        /// 通知系统属性已经发生更改
        /// </summary>
        /// <param name="propertyName">发生更改的属性名称,其填充是自动完成的</param>
        public void OnPropertiesChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
