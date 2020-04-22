using System;
using System.Drawing;
using Lin.ImageHelper;

namespace Lin.ImageHelper
{
    internal class Test
    {
        /// <summary>
        /// 
        /// </summary>
        public void main()
        {
            var img = Image.FromFile("Test.jpg");
            //图像转RAW
            var w = img.Width;
            var h = img.Height;
            var jpgbytes = img.ImgToBytes(ImageFormat.RAW_RGBA32);
            //RAW转回图像
            var nweimg = jpgbytes.ToImage(ImageFormat.RAW_RGBA32, w, h);
        }
    }
}
