using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.BulkAccess;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Live_Music.FirstStartPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MusicLibrary : Page
    {
        ObservableCollection<FolderList> folderList = new ObservableCollection<FolderList>();
        StorageFolder Folder;
        ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

        public MusicLibrary()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
        }

        private async void ChooseMusicFolder(object sender, RoutedEventArgs e)
        {
            var FolderPicker = new FolderPicker();
            FolderPicker.ViewMode = PickerViewMode.Thumbnail;
            FolderPicker.FileTypeFilter.Add("*");
            FolderPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            Folder = await FolderPicker.PickSingleFolderAsync();
            
            folderListView.ItemsSource = folderList;
            if (Folder != null)
            {
                var futureAccessList = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList;
                folderList.Add(new FolderList() { FolderName = $"{Folder.Path}" });
                string falToken = futureAccessList.Add(Folder, "PickedMusicFolder");
                //ApplicationDataCompositeValue FutureAccessListToken = new ApplicationDataCompositeValue { };

                //FutureAccessListToken.Add("Token", falToken);
                //localSettings.Values["falToken"]=FutureAccessListToken; //HACK: 
                //var openedfolder = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFolderAsync(falToken);
                //warnTextBlock.Text += "\n"+FutureAccessListToken;
            }
            else
            {
                warnTextBlock.Text = "似乎你已取消操作";
            }
        }

        private void ClearFolderAccess(object sender, RoutedEventArgs e)
        {
            try
            {
                warnTextBlock.Text = "";
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Clear();
                folderList.Clear();
            }
            catch (ArgumentException)
            {
                warnTextBlock.Text = "目前还没有可以移除的文件夹";
            }
        }
    }
}
