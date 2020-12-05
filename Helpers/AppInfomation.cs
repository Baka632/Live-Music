using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace Live_Music
{
    public class AppInfomation : DependencyObject , INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

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
        /// 指示InfoBar是否打开的值
        /// </summary>
        private bool _IsInfoBarOpen = false;
        /// <summary>
        /// InfoBar标题栏的值
        /// </summary>
        private string _InfoBarTitle; 
        /// <summary>
        /// InfoBar信息的值
        /// </summary>
        private string _InfoBarMessage;
        /// <summary>
        /// InfoBar严重性的值
        /// </summary>
        private Microsoft.UI.Xaml.Controls.InfoBarSeverity _InfoBarSeverity;

        /// <summary>
        /// 指示InfoBar是否打开的属性
        /// </summary>
        public bool IsInfoBarOpen
        {
            get => _IsInfoBarOpen;
            set 
            { 
                _IsInfoBarOpen = value;
                OnPropertiesChanged();
            }
        }

        /// <summary>
        /// InfoBar的标题
        /// </summary>
        public string InfoBarTitle
        {
            get => _InfoBarTitle;
            set 
            { 
                _InfoBarTitle = value;
                OnPropertiesChanged();
            }
        }
        
        /// <summary>
        /// InfoBar的消息
        /// </summary>
        public string InfoBarMessage
        {
            get => _InfoBarMessage;
            set 
            {
                _InfoBarMessage = value;
                OnPropertiesChanged();
            }
        }

        /// <summary>
        /// InfoBar严重性的属性
        /// </summary>
        public Microsoft.UI.Xaml.Controls.InfoBarSeverity InfoBarSeverity
        {
            get => _InfoBarSeverity;
            set 
            {
                _InfoBarSeverity = value;
                OnPropertiesChanged();
            }
        }

        private RoutedEventHandler _InfoBarButtonClick;

        public RoutedEventHandler InfoBarButtonClick
        {
            get => _InfoBarButtonClick;
            set 
            { 
                _InfoBarButtonClick = value;
                OnPropertiesChanged();
            }
        }

    }
}