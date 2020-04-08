using System;
using System.Text;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace Edu.Service
{
    /// <summary>     
    /// 生成验证码的类     
    /// </summary>     
    public class ValidateCode
    {
        public ValidateCode()
        {
            this.Width = 80;
            this.Height = 34;
            this.Length = 5;
        }

        public ValidateCode(int width, int height)
        {
            this.Width = width;
            this.Height = height;
            this.Length = 5;
        }

        public ValidateCode(int width, int height, int length)
        {
            this.Width = width;
            this.Height = height;
            this.Length = length;
        }
        /// <summary>     
        /// 验证码的宽度  
        /// </summary>     
        public int Width
        {
            get; set;
        }
        /// <summary>     
        /// 验证码的高度     
        /// </summary>     
        public int Height
        {
            get; set;
        }
        /// <summary>     
        /// 验证码的最大长度     
        /// </summary>     
        public int Length
        {
            get; set;
        }

        public byte[] CreateIdentityCode(string iCode)
        {
            using (Image img = new Bitmap(Width, Height))//背景图片
            {
                using (Image fontImg = new Bitmap(Width, Height))//预先画验证码的临时图片
                {
                    //先将验证码画到临时图片fontImg上
                    using (Graphics fontGraphic = Graphics.FromImage(fontImg))
                    {
                        fontGraphic.Clear(Color.White);//将画布涂白
                        //画干扰线
                        //生成随机生成器     
                        Random random = new Random();
                        //画图片的干扰线     
                        for (int i = 0; i < 25; i++)
                        {
                            int x1 = random.Next(img.Width);
                            int x2 = random.Next(img.Width);
                            int y1 = random.Next(img.Height);
                            int y2 = random.Next(img.Height);
                            fontGraphic.DrawLine(new Pen(Color.Silver), x1, y1, x2, y2);
                        }
                        //画验证吗
                        fontGraphic.DrawString(iCode, new Font("宋体", 18, FontStyle.Italic | FontStyle.Bold), Brushes.Black, new PointF(2, 2));

                    }
                    //将临时验证码图片fontImg分成若干份画到背景图片img上
                    using (Graphics g = Graphics.FromImage(img))
                    {
                        g.Clear(Color.White);//将画布涂白                       
                        int sum = 50;//分成60份
                        int niu = 2;//上下最多偏移10像素
                        float x = 0, y = 1;//从临时图片fontImg选取矩形区域的起始坐标
                        //从临时图片fontImg选取矩形区域的宽度，由于有背景图片映衬，这里只确定宽度                        
                        float width = ((float)fontImg.Width) / sum;
                        //依次把临时图片fontImg各部分画到背景图片上                       
                        for (int i = 0; i < sum; i++)
                        {
                            //获取当前部分的正弦值，计算上下偏移量。但这样会使验证码的扭曲单一
                            float rate = (float)Math.Sin(360 * 1.0 / sum * i / 180 * Math.PI) * niu;
                            //如果使用随机数,会有不同的变化
                            //Random r = new Random(DateTime.Now.Millisecond);
                            //float rate = (float)Math.Sin(360 * 1.0 / sum * i / 180 * Math.PI) * r.Next(1,niu);

                            //把临时图片fontImg当前部分画到背景图片上
                            g.DrawImage(fontImg, new Rectangle(x == 0 ? 1 : (int)x, (int)y, img.Width, img.Height), x, y * rate, width, fontImg.Height, GraphicsUnit.Pixel);
                            //依次改变横坐标的下一个位置
                            x += width;

                        }
                        //为背景图片画边框
                        g.DrawRectangle(new Pen(Brushes.Black), new Rectangle(0, 0, img.Width - 1, img.Height - 1));
                    }
                }
                //将背景图片保存到输出流
                //img.Save(Response.OutputStream, ImageFormat.Jpeg);

                MemoryStream stream = new MemoryStream();
                img.Save(stream, ImageFormat.Jpeg);
                return stream.ToArray();
            }

        }

        public string GetIdentityCode()
        {
            Random r = new Random();
            StringBuilder passWord = new StringBuilder();
            string strCache = "ABCD1EF2GH3IJ4KL5MN6P7QR8ST9UVWXYZ";
            for (int i = 0; i < Length; i++)
            {
                passWord.Append(strCache[r.Next(0, strCache.Length)]);
            }
            return passWord.ToString();
        }
    }
}
