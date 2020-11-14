using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
using Windows.UI.Notifications;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Live_Music.Helpers;
using Live_Music.Services;
using Windows.UI.Core.Preview;
using Windows.UI.Core;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Live_Music
{
    /// <summary>
    /// 提供特定于应用程序的行为，以补充默认的应用程序类。
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// 初始化单一实例应用程序对象。这是执行的创作代码的第一行，
        /// 已执行，逻辑上等同于 main() 或 WinMain()。
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;

            this.UnhandledException += AppExceptionHandler; //应用程序层级的异常处理

            //进入或退出后台时进行管理
            this.EnteredBackground += AppEnteredBackground;
            this.LeavingBackground += AppLeavingBackground;
            
            MemoryManager.AppMemoryUsageLimitChanging += AppMemoryUsageLimitChanging; //内存限制改变时的操作
            MemoryManager.AppMemoryUsageIncreased += AppMemoryUsageIncreased; //内存增加到上限值时的操作

            FocusVisualKind = FocusVisualKind.Reveal;

            string ThemeSettings = (string)localSettings.Values["ThemeSetting"];
            switch (ThemeSettings)
            {
                case "Light":
                    Current.RequestedTheme = ApplicationTheme.Light;
                    break;
                case "Dark":
                    Current.RequestedTheme = ApplicationTheme.Dark;
                    break;
                case "Default":
                    break;
                case null:
                    break;
            }
#if DEBUG
            Debug.WriteLine("[Info]现在的设置列表:");
            foreach (var item in localSettings.Values)
            {
                Debug.WriteLine(item.ToString());
            }
#endif
        }

        /// <summary>
        /// 应用程序设置的实例
        /// </summary>
        public static Settings settings = new Settings();
        /// <summary>
        /// 音乐信息的实例
        /// </summary>
        public static MusicInfomation musicInfomation = new MusicInfomation();
        /// <summary>
        /// 音乐服务的实例
        /// </summary>
        public static MusicService musicService = new MusicService();
        /// <summary>
        /// 声音图标状态的实例
        /// </summary>
        public static VolumeGlyphState volumeGlyphState = new VolumeGlyphState();

        /// <summary>
        /// 在用户代码中未处理的异常的出现数量
        /// </summary>
        private int UnhandledExceptionCount = 0;
        /// <summary>
        /// 异常的详细信息
        /// </summary>
        public static string[] UnhandledExceptionMessage = {"FullName","Message","StackTrace"};
        /// <summary>
        /// 此值指示未处理的异常是否出现了三次
        /// </summary>
        public static bool IsExceptionHappenedOverThree = false;
        /// <summary>
        /// 此值指示应用是否在后台模式
        /// </summary>
        bool IsInBackgroundMode;
        private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        /// <summary>
        /// 未在用户代码中处理的异常的异常处理程序
        /// </summary>
        /// <param name="sender">出现异常的源</param>
        /// <param name="e">有关异常的详细信息</param>
        private async void AppExceptionHandler(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            UnhandledExceptionCount++;
            UnhandledExceptionMessage[0] = e.Exception.GetType().FullName;
            UnhandledExceptionMessage[1] = e.Exception.ToString();
            var stackFrames = new StackTrace(e.Exception).GetFrames() ?? new StackFrame[0];
            var frames = stackFrames.Select(x => x.GetMethod()).Select(m =>
        $"{m.DeclaringType?.FullName ?? "null"}.{m.Name}({string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"))})");
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string text in frames.ToList())
            {
                stringBuilder.Append(text);
            }
            UnhandledExceptionMessage[2] = string.IsNullOrWhiteSpace(stringBuilder.ToString()) == true ? "null" : stringBuilder.ToString();
            ExceptionDetails.ExceptionDetailsDialog exceptionDetailsDialog = new ExceptionDetails.ExceptionDetailsDialog();
            TextBlock textBlock = new TextBlock();

            if (UnhandledExceptionCount > 3)
            {
                Debug.WriteLine("\n[Exception]出现了未在用户代码中进行处理的异常,且此异常出现了3次以上!");
                Debug.WriteLine($"[Exception]这种情况出现了{UnhandledExceptionCount}次。");
                Debug.WriteLine($"[Exception]更多信息:\n{e.Message}");
                exceptionDetailsDialog.Title = $"应用程序出现了一个异常";
                exceptionDetailsDialog.Content = textBlock.Text = $"(!)异常出现了三次以上,强烈建议向我们报告这个问题\n\n异常名称:{e.Exception.GetType().FullName}\n详细信息:{e.Message}";
                IsExceptionHappenedOverThree = true;
                try
                {
                    await exceptionDetailsDialog.ShowAsync();
                }
                catch (Exception)
                {
                    UnhandledExceptionCount = 4;
                }
                
            }
            else
            {
                Debug.WriteLine("\n[Exception]出现了未在用户代码中进行处理的异常!");
                Debug.WriteLine($"[Exception]这种情况出现了{UnhandledExceptionCount}次。");
                Debug.WriteLine($"[Exception]更多信息:\n{e.Message}");
                exceptionDetailsDialog.Title = $"应用程序出现了一个异常";
                exceptionDetailsDialog.Content = textBlock.Text = $"异常名称:{e.Exception.GetType().FullName}\n详细信息:{e.Message}";
                try
                {
                    await exceptionDetailsDialog.ShowAsync();
                }
                catch (Exception)
                {
                    UnhandledExceptionCount = 1;
                }
            }
        }

        /// <summary>
        /// 当应用进入后台时的操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AppEnteredBackground(object sender, EnteredBackgroundEventArgs e)
        {
            IsInBackgroundMode = true;
            Debug.WriteLine("[StateChanged]已进入后台");
        }

        /// <summary>
        /// 当应用离开后台时的操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AppLeavingBackground(object sender, LeavingBackgroundEventArgs e)
        {
            IsInBackgroundMode = false;
            if (Window.Current.Content == null)
            {
                CreateRootFrame(ApplicationExecutionState.Running, string.Empty);
            }
            Debug.WriteLine("[StateChanged]已离开后台");
        }

        /// <summary>
        /// 当应用的内存限制量改变时的操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AppMemoryUsageLimitChanging(object sender, AppMemoryUsageLimitChangingEventArgs e)
        {
            Debug.WriteLine("[MemoryUsage]内存限制量发生了改变");
            if (MemoryManager.AppMemoryUsage >= e.NewLimit)
            {
                ReduceMemoryUsage(e.NewLimit);
            }
        }

        /// <summary>
        /// 当应用的内存使用量增加到上限值时的操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AppMemoryUsageIncreased(object sender, object e)
        {
            var level = MemoryManager.AppMemoryUsageLevel;
            if (level == AppMemoryUsageLevel.OverLimit || level == AppMemoryUsageLevel.High)
            {
                Debug.WriteLine("[MemoryUsage]警告:内存使用量到达上限");
                ReduceMemoryUsage(MemoryManager.AppMemoryUsageLimit);
            }
        }

        /// <summary>
        /// 减少应用的内存使用量时使用的操作
        /// </summary>
        /// <param name="limit"></param>
        public void ReduceMemoryUsage(ulong limit)
        {
            Debug.WriteLine("[MemoryUsage]正在尝试减少应用内存使用量");
            if (IsInBackgroundMode==true && Window.Current.Content != null)
            {
                Debug.WriteLine("[MemoryUsage]正在卸载主页面内容");
                Window.Current.Content = null;
                Debug.WriteLine("[MemoryUsage]正在清理音乐及其信息服务的资源...");
                musicService.Dispose();
                musicInfomation.ResetAllMusicProperties();
                Debug.WriteLine("[MemoryUsage]完成。");
            }
            Debug.WriteLine("[MemoryUsage]正强制启动垃圾回收器");
            GC.Collect();
        }

        /// <summary>
        /// 当应用离开后台时发现主界面内容为空时的操作
        /// </summary>
        /// <param name="previousExecutionState"></param>
        /// <param name="arguments"></param>
        void CreateRootFrame(ApplicationExecutionState previousExecutionState, string arguments)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
            {
                rootFrame = new Frame();
                rootFrame.Language = Windows.Globalization.ApplicationLanguages.Languages[0];
                rootFrame.NavigationFailed += OnNavigationFailed;

                //重新实例化音乐及其信息服务
                musicInfomation = new MusicInfomation();
                musicService = new MusicService();

                if (previousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: 从之前挂起的应用程序加载状态
                    musicService.mediaPlayer.Volume = settings.MusicVolume;
                }
                Window.Current.Content = rootFrame;
            }
            if (rootFrame.Content == null)
            {
                rootFrame.Navigate(typeof(MainPage), arguments);
            }
            NotifyUserContectLost();
        }

        /// <summary>
        /// 通知用户内容可能已经丢失,这是通过Toast通知实现的
        /// </summary>
        private void NotifyUserContectLost()
        {
            var toastContent = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
            {
                new AdaptiveText()
                {
                    Text = "非常抱歉"
                },
                new AdaptiveText()
                {
                    Text = "因为内存限制,应用的某些内容可能已经丢失"
                }
            }
                    }
                }
            };

            // Create the toast notification
            var toastNotif = new ToastNotification(toastContent.GetXml());

            // And send the notification
            ToastNotificationManager.CreateToastNotifier().Show(toastNotif);
        }

        private void OnBackRequested(object sender, Windows.UI.Core.BackRequestedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame == null)
            {
                return; 
            }

            //当可以返回到上一个页面,且返回事件尚未被处理时发生
            if (rootFrame.CanGoBack && e.Handled == false)
            {
                e.Handled = true;
                rootFrame.GoBack();
                if (SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility == AppViewBackButtonVisibility.Visible)
                {
                    SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
                }
            }
        }

        /// <summary>
        /// 在应用程序由最终用户正常启动时进行调用。
        /// 将在启动应用程序以打开特定文件等情况下使用。
        /// </summary>
        /// <param name="e">有关启动请求和过程的详细信息。</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            Window.Current.Content = rootFrame;

            //当系统收到回退请求(BackRequested)的时候，就会调用方法OnBackRequested来处理该事件
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;

            // 隐藏标题栏
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            //标题栏按钮颜色
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Windows.UI.Colors.Transparent;

            #region 暂时不用的代码
            //var WhiteBlack = (Color)Application.Current.Resources["WhiteBlack"];
            //titleBar.ButtonForegroundColor = WhiteBlack;
            #endregion

            // 不要在窗口已包含内容时重复应用程序初始化，
            // 只需确保窗口处于活动状态
            if (rootFrame == null)
            {
                // 创建要充当导航上下文的框架，并导航到第一页
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;
                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: 从之前挂起的应用程序加载状态
                    Debug.WriteLine("[Resume]上次的状态:挂起后被关闭");
                    Debug.WriteLine("[Resume]已自动恢复以下信息:播放器音量");
                }

                // 将框架放在当前窗口中
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // 当导航堆栈尚未还原时，导航到第一页，
                    // 并通过将所需信息作为导航参数传入来配置参数
                    if (SystemInformation.IsFirstRun == true)
                    {
                        rootFrame.Navigate(typeof(FirstStart), null, new SuppressNavigationTransitionInfo());
                        settings.MusicVolume = 1d;
                    }
                    else
                    {
                        rootFrame.Navigate(typeof(MainPage), e.Arguments);
                    }
                }
                // 确保当前窗口处于活动状态
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// 导航到特定页失败时调用
        /// </summary>
        ///<param name="sender">导航失败的框架</param>
        ///<param name="e">有关导航失败的详细信息</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("加载页面失败! " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// 在将要挂起应用程序的执行时调用。 保存应用程序状态,
        /// 无需知道应用程序是要被终止
        /// 还是在内存内容原封未动时恢复
        /// </summary>
        /// <param name="sender">挂起的请求的源。</param>
        /// <param name="e">有关挂起请求的详细信息。</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: 保存应用程序状态并停止任何后台活动
            Debug.WriteLine("[Suspending]将要进入挂起态");

            settings.MusicVolume = musicService.mediaPlayer.Volume; //保存现在的音量
            Debug.WriteLine("[Suspending]已保存现在的音量");

            Debug.WriteLine("[Suspending]进入挂起态");
            deferral.Complete();
        }

        /// <summary>
        /// 当应用打开某个音乐文件时调用的方法
        /// </summary>
        /// <param name="args"></param>
        protected override void OnFileActivated(FileActivatedEventArgs args)
        {
            IStorageItem file = args.Files[0];
            Frame frame = Window.Current.Content as Frame;
            if (frame == null)
            {
                frame = new Frame();
                Window.Current.Content = frame;
            }
            frame.Navigate(typeof(MainPage),(StorageFile)file);
            Window.Current.Activate();
        }
    }
}
