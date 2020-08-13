using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Live_Music
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class FirstStart : Page
    {
        int CountOfNavigate = 0;
        public FirstStart()
        {
            this.InitializeComponent();

            if (Windows.System.Power.PowerManager.EnergySaverStatus != Windows.System.Power.EnergySaverStatus.On)
            {
                EnterStoryboard.Begin();
            }
            
            navigationFrame.Navigate(typeof(FirstStartPages.MusicLibrary), null, new DrillInNavigationTransitionInfo());
            previousPageButton.IsEnabled = false;
        }

        private void BackToMainPage(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }

        private void NextPage(object sender, RoutedEventArgs e)
        {

            previousPageButton.IsEnabled = true;
            switch (CountOfNavigate)
            {
                case (0):
                    navigationFrame.Navigate(typeof(FirstStartPages.Settings), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                    CountOfNavigate++;
                    break;
                case (1):
                    navigationFrame.Navigate(typeof(FirstStartPages.Others), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                    CountOfNavigate++;
                    break;
                case (2):
                    navigationFrame.Navigate(typeof(FirstStartPages.End), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                    CountOfNavigate++;
                    nextPageButton.Content = "Done";
                    nextPageButton.Click += BackToMainPage;
                    break;
                default:
                    break;
            }
        }

        private void PreviousPage(object sender, RoutedEventArgs e)
        {
            switch (CountOfNavigate)
            {
                case (0):
                    //Do nothing
                    break;
                case (1):
                    navigationFrame.Navigate(typeof(FirstStartPages.MusicLibrary), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
                    previousPageButton.IsEnabled = false;
                    CountOfNavigate--;
                    break;
                case (2):
                    navigationFrame.Navigate(typeof(FirstStartPages.Settings), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
                    CountOfNavigate--;
                    break;
                case (3):
                    navigationFrame.Navigate(typeof(FirstStartPages.Others), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
                    nextPageButton.IsEnabled = true;
                    nextPageButton.Content = "Next";
                    nextPageButton.Click -= BackToMainPage;
                    CountOfNavigate--;
                    break;
                default:
                    break;
            }
        }
    }
}
