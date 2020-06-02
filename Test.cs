using System;
using System.Drawing;
using System.IO;
using Lin.ImageHelper;

namespace Lin.ImageHelper
{
	internal class Test
	{
		/// <summary>
		/// 
		/// </summary>
		public static void Main()
		{
			var img = Image.FromFile("D:/test1.gif");//从文件读取gif

			//深复制gif
			var img2 = img.DeepCopyBitmap();

			//转字节数组再转回来
			var gifdata = img2.ImgToBytes(ImageDataFormat.GIF);
			img2.Dispose();
			img2 = gifdata.ToImage();

			//分割与合并
			var imgs = img2.Split();
			img2.Dispose();
			img2 = imgs.Merge();

			img2.Save("d:/test2.gif");//保存gif

		}
	}
}
