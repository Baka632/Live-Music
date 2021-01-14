using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Live_Music.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class AppSettings : Page, INotifyPropertyChanged
    {
        private AppInfomation appInfomation = App.appInfomation;
        private string _ThemeSettings = App.settings.ThemeSettings;
        private bool IsUserMode = false;
        public event PropertyChangedEventHandler PropertyChanged;

        public AppSettings()
        {
            this.InitializeComponent();
        }

        public string ThemeSettingProperty
        {
            get
            {
                switch (_ThemeSettings)
                {
                    case "Light":
                        _ThemeSettings = "浅色";
                        break;
                    case "Dark":
                        _ThemeSettings = "深色";
                        break;
                    case "Default":
                        _ThemeSettings = "使用系统设置";
                        break;
                    default:
                        _ThemeSettings = App.settings.ThemeSettings;
                        break;
                }
                return _ThemeSettings;
            }
            set
            {
                _ThemeSettings = value;
                OnPropertiesChanged();
            }
        }

        private string _TempFolderSize;

        public string TempFolderSize
        {
            get => _TempFolderSize;
            set
            {
                _TempFolderSize = value;
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

        private async void MailTo(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("mailto:stevemc123456@outlook.com"));
        }

        private async void GoToGithub(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("https://github.com/Baka632/Live-Music-Lite"));
        }

        private async void ThemeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsUserMode)
            {
                string Theme = e.AddedItems[0].ToString();
                switch (Theme)
                {
                    case "浅色":
                        App.settings.ThemeSettings = "Light";
                        ThemeSettingProperty = "浅色";
                        break;
                    case "深色":
                        App.settings.ThemeSettings = "Dark";
                        ThemeSettingProperty = "深色";
                        break;
                    case "使用系统设置":
                        App.settings.ThemeSettings = "Default";
                        ThemeSettingProperty = "使用系统设置";
                        break;
                }
                ContentDialog dialog = new ContentDialog();
                dialog.Title = "主题更改只有在重启应用后生效";
                dialog.Content = "要重启吗?";
                dialog.PrimaryButtonText = "是";
                dialog.CloseButtonText = "否";
                dialog.PrimaryButtonClick += Dialog_PrimaryButtonClick;
                IsUserMode = false;
                await dialog.ShowAsync();
            }
            else
            {
                IsUserMode = true;
            }

            async void Dialog_PrimaryButtonClick(ContentDialog sender1, ContentDialogButtonClickEventArgs args1)
            {
                AppRestartFailureReason result = await CoreApplication.RequestRestartAsync(string.Empty);
                if (result == AppRestartFailureReason.NotInForeground ||
                    result == AppRestartFailureReason.RestartPending ||
                    result == AppRestartFailureReason.Other)
                {
                    System.Diagnostics.Debug.WriteLine($"RequestRestartAsync failed: {result}");
                }
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            uint count = 0;
            await Task.Run(async () =>
            {
                count = await ApplicationData.Current.TemporaryFolder.CreateFileQuery().GetItemCountAsync();
            });
            TempFolderSize = $"缓存的图片数:{count}";
        }

        private async void ClearTempFolder(object sender, RoutedEventArgs e)
        {
            (sender as Button).IsEnabled = false;
            clearProgreeRing.IsActive = true;
            await Task.Run(async () =>
            {
                IReadOnlyList<StorageFile> files = await ApplicationData.Current.TemporaryFolder.GetFilesAsync();
                if (files.Count > 0)
                {
                    foreach (var item in files)
                    {
                        await item.DeleteAsync(StorageDeleteOption.PermanentDelete);
                    }
                }
            });
            (sender as Button).IsEnabled = true;
            clearProgreeRing.IsActive = false;
            uint count = 0;
            await Task.Run(async () =>
            {
                count = await ApplicationData.Current.TemporaryFolder.CreateFileQuery().GetItemCountAsync();
            });
            TempFolderSize = $"缓存的图片数:{count}";
        }
    }
}
