using Microsoft.Graphics.Canvas;
using Microsoft.Toolkit.Uwp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI;

namespace Live_Music.Helpers
{
    class ImageColors
    {
        public class ImageThemeBrush
        {
            CanvasDevice device = new CanvasDevice();

            /// <summary>
            /// 通过Uri获取主题色
            /// </summary>
            /// <param name="uri"></param>
            /// <returns></returns>
            public async Task<Color> GetPaletteImage(Uri uri)
            {
                //实例化资源
                var bimap = await CanvasBitmap.LoadAsync(device, uri);
                //取色
                Color[] colors = bimap.GetPixelColors();
                return await GetThemeColor(colors);
            }

            /// <summary>
            /// 通过stream获取主题色
            /// </summary>
            /// <param name="uri"></param>
            /// <returns></returns>
            public async Task<Color> GetPaletteImage(IRandomAccessStream stream)
            {
                //实例化资源
                var bimap = await CanvasBitmap.LoadAsync(device, stream);
                //取色
                Color[] colors = bimap.GetPixelColors();
                return await GetThemeColor(colors);
            }

            #region Methon：方法
            private async Task<Color> GetThemeColor(Color[] colors)
            {
                Color color = new Color();

                await Task.Run(() =>
                {
                    //饱和度 黑色多
                    double sumS = 0;
                    //明亮度 白色多
                    double sumV = 0;
                    double sumHue = 0;
                    //颜色中最大亮度
                    double maxV = 0;
                    //颜色中最大饱和度
                    double maxS = 0;
                    //颜色中最大色相
                    double maxH = 0;
                    double count = 0;
                    List<Color> notBlackWhite = new List<Color>();
                    foreach (var item in colors)
                    {
                        //将 rgb 转换成 hsv 对象
                        HsvColor hsv = Microsoft.Toolkit.Uwp.Helpers.ColorHelper.ToHsv(item);

                        //先将黑色和白色剔除掉
                        if (hsv.V < 0.3 || hsv.S < 0.2)
                        {
                            continue;
                        }
                        //找出最大饱和度
                        maxS = hsv.S > maxS ? hsv.S : maxS;
                        //找出最大亮度度
                        maxV = hsv.V > maxV ? hsv.V : maxV;
                        //找出最大色相
                        maxH = hsv.H > maxH ? hsv.H : maxH;
                        //色相总和
                        sumHue += hsv.H;
                        //亮度总和
                        sumS += hsv.S;
                        //饱和度总和
                        sumV += hsv.V;
                        count++;
                    }

                    double avgH = sumHue / count;
                    double avgV = sumV / count;
                    double avgS = sumS / count;
                    double maxAvgV = maxV / 2;
                    double maxAvgS = maxS / 2;
                    double maxAvgH = maxH / 2;

                    //计算各个值，用来做判断用
                    double h = Math.Max(maxAvgV, avgV);
                    double s = Math.Min(maxAvgS, avgS);
                    double hue = Math.Min(maxAvgH, avgH);

                    //aveS = aveS ;
                    double R = 0;
                    double G = 0;
                    double B = 0;
                    count = 0;

                    foreach (var item in notBlackWhite)
                    {
                        HsvColor hsv = Microsoft.Toolkit.Uwp.Helpers.ColorHelper.ToHsv(item);
                        //颜色大于平均色相 并且 饱和度大于平局饱和度 并且 亮度大于平局亮度 的符合条件 进行相加
                        if (hsv.H >= hue + 10 && hsv.V >= h && hsv.S >= s)
                        {
                            R += item.R;
                            G += item.G;
                            B += item.B;
                            count++;
                        }
                    }

                    double r = R / count;
                    double g = G / count;
                    double b = B / count;

                    color = Color.FromArgb(255, (byte)r, (byte)g, (byte)b);
                });

                colors = null;
                return color;
            }
            #endregion
        }
    }
}