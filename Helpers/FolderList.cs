using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Live_Music.FirstStartPages
{
    class FolderList : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string folderName;
        public string FolderName
        {
            get => folderName;
            set
            {
                folderName = value;
                if (PropertyChanged!=null)
                {
                    PropertyChanged.Invoke(this,new PropertyChangedEventArgs("FolderName"));
                }
            }
        }
    }
}
