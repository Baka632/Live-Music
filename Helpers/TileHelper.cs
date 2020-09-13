using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml;

namespace Live_Music.Helpers
{
    /// <summary>
    /// 为显示和删除磁贴提供方法
    /// </summary>
    class TileHelper
    {
        /// <summary>
        /// 显示磁贴
        /// </summary>
        /// <param name="tileContent">磁贴源</param>
        public void ShowTitle(TileContent tileContent)
        {
            // Create the tile notification
            var tileNotif = new TileNotification(tileContent.GetXml());

            // And send the notification to the primary tile
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotif);
        }

        /// <summary>
        /// 删除磁贴
        /// </summary>
        public void DeleteTile() => TileUpdateManager.CreateTileUpdaterForApplication().Clear();
    }
}
