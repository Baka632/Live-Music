using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Contacts;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.Email;
using System.Threading.Tasks;
using Windows.System;
using Windows.ApplicationModel;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace Live_Music.ExceptionDetails
{
    public sealed partial class ExceptionDetailsDialog : ContentDialog
    {
        public ExceptionDetailsDialog()
        {
            this.InitializeComponent();
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            string appVersion = string.Format("{0}.{1}.{2}.{3}",
                    Package.Current.Id.Version.Major,
                    Package.Current.Id.Version.Minor,
                    Package.Current.Id.Version.Build,
                    Package.Current.Id.Version.Revision);
            await UniversallyEmail(
                "livestudiohelp@outlook.com",
                $"Live Music异常跟踪",
                $@"===请在这里描述你做了什么===

===不要更改以下内容!===
异常名称:{App.UnhandledExceptionMessage[0]}
详细信息:{App.UnhandledExceptionMessage[1]} 
堆栈跟踪:{App.UnhandledExceptionMessage[2]}
应用版本:{appVersion} 
系统版本:{Environment.OSVersion}
是否出现三次以上:{App.IsExceptionHappenedOverThree}
===这是异常报告的末尾===");
        }

        private async Task UniversallyEmail(string email, string subject, string messageBody)
        {
            messageBody = Uri.EscapeDataString(messageBody);
            string url = $"mailto:{email}?subject={subject}&body={messageBody}";
            await Launcher.LaunchUriAsync(new Uri(url));
        }
    }
}
