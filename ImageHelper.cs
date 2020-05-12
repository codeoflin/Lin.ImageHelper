using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;

namespace Lin.ImageHelper
{
	/// <summary>
	/// Image图像与byte数组之间的转换关系
	/// </summary>
	/// <value></value>
	public enum ImageFormat : byte
	{
		///<summary>
		/// 作为输出的时候:根据其他参数决定格式
		/// 作为输入的时候:自动识别文件数据格式(支持:JPG,PNG,BMP等常见格式)
		///</summary>
		FILE = 0,
		///<summary>
		/// RAW_RGB24
		///</summary>
		RAW_RGB24 = 1,
		///<summary>
		/// RAW_ARGB32
		///</summary>
		RAW_ARGB32 = 2,
		///<summary>
		/// RAW_BGR24
		///</summary>
		RAW_BGR24 = 3,
		///<summary>
		/// RAW_RGBA32
		///</summary>
		RAW_RGBA32 = 4,
	}

	/// <summary>
	/// 
	/// </summary>
	public static class ImageHelper
	{
		#region 图像与base64之间互转
		/// <summary>
		///  将Base64字符串转换为图片
		/// </summary>
		/// <param name="base64">字串</param>
		/// <param name="format"></param>
		/// <param name="width">如果是RAW格式,需要填入宽高</param>
		/// <param name="height">如果是RAW格式,需要填入宽高</param>
		/// <returns>图片</returns>
		public static Image Base64ToImg(this string base64, ImageFormat format = ImageFormat.FILE, int width = 0, int height = 0)
		{
			var heads = new string[] { "png", "jgp", "jpg", "jpeg" };
			foreach (var head in heads) base64 = base64.Replace($"data:image/{head};base64,", "");
			var bytes = Convert.FromBase64String(base64);
			return bytes.ToImage(format, width, height);
		}

		/// <summary>
		/// 图片转base64字符串
		/// </summary>
		/// <param name="img"></param>
		/// <param name="format">如果为FILE,以fileformat决定格式</param>
		/// <param name="fileformat">默认为JPG格式</param>
		/// <returns></returns>
		public static string ImgToBase64(this Bitmap img, ImageFormat format = ImageFormat.FILE, System.Drawing.Imaging.ImageFormat fileformat = null) => Convert.ToBase64String(img.ImgToBytes(format, fileformat));

		/// <summary>
		/// 图片转base64字符串
		/// </summary>
		/// <param name="img"></param>
		/// <param name="format">如果为FILE,以fileformat决定格式</param>
		/// <param name="fileformat">默认为JPG格式</param>
		/// <returns></returns>
		public static string ImgToBase64(this Image img, ImageFormat format = ImageFormat.FILE, System.Drawing.Imaging.ImageFormat fileformat = null) => Convert.ToBase64String(img.ImgToBytes(format, fileformat));
		#endregion

		#region 图像与Bytes之间互转
		/// <summary>
		/// 图片转byte数组
		/// </summary>
		/// <param name="img"></param>
		/// <param name="format">如果为FILE,以fileformat决定格式</param>
		/// <param name="fileformat">默认为JPG格式</param>
		/// <returns></returns>
		public static byte[] ImgToBytes(this Image img, ImageFormat format = ImageFormat.FILE, System.Drawing.Imaging.ImageFormat fileformat = null) => ImgToBytes((Bitmap)img, format, fileformat);

		/// <summary>
		/// 图片转byte数组
		/// </summary>
		/// <param name="img"></param>
		/// <param name="format">如果为FILE,以fileformat决定格式</param>
		/// <param name="fileformat">默认为JPG格式</param>
		/// <returns></returns>
		public static byte[] ImgToBytes(this Bitmap img, ImageFormat format = ImageFormat.FILE, System.Drawing.Imaging.ImageFormat fileformat = null)
		{
			byte[] buff = null;
			switch (format)
			{
				case ImageFormat.FILE:
					{
						using (var ms = new MemoryStream())
						{
							img.Save(ms, fileformat == null ? System.Drawing.Imaging.ImageFormat.Jpeg : fileformat);
							buff = ms.ToArray();
						}
						break;
					}
				case ImageFormat.RAW_RGB24:
					{
						buff = new byte[img.Width * img.Height * 3];
						var bgrabuff = new byte[img.Width * img.Height * 4];
						var data = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
						Marshal.Copy(data.Scan0, bgrabuff, 0, bgrabuff.Length);
						img.UnlockBits(data);
						for (int i = 0; i < img.Width * img.Height; i++)
						{
							buff[i * 3 + 0] = bgrabuff[i * 4 + 2];
							buff[i * 3 + 1] = bgrabuff[i * 4 + 1];
							buff[i * 3 + 2] = bgrabuff[i * 4 + 0];
						}
						break;
					}
				case ImageFormat.RAW_ARGB32:
					{
						buff = new byte[img.Width * img.Height * 4];
						var bgrabuff = new byte[img.Width * img.Height * 4];
						var data = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
						Marshal.Copy(data.Scan0, bgrabuff, 0, bgrabuff.Length);
						img.UnlockBits(data);
						for (int i = 0; i < img.Width * img.Height; i++)
						{
							buff[i * 4 + 0] = bgrabuff[i * 4 + 3];
							buff[i * 4 + 1] = bgrabuff[i * 4 + 2];
							buff[i * 4 + 2] = bgrabuff[i * 4 + 1];
							buff[i * 4 + 3] = bgrabuff[i * 4 + 0];
						}
						break;
					}
				case ImageFormat.RAW_BGR24:
					{
						buff = new byte[img.Width * img.Height * 3];
						var bgrabuff = new byte[img.Width * img.Height * 4];
						var data = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
						Marshal.Copy(data.Scan0, bgrabuff, 0, bgrabuff.Length);
						img.UnlockBits(data);
						for (int i = 0; i < img.Width * img.Height; i++)
						{
							buff[i * 3 + 0] = bgrabuff[i * 4 + 0];
							buff[i * 3 + 1] = bgrabuff[i * 4 + 1];
							buff[i * 3 + 2] = bgrabuff[i * 4 + 2];
						}
						break;
					}
				case ImageFormat.RAW_RGBA32:
					{
						buff = new byte[img.Width * img.Height * 4];
						var bgrabuff = new byte[img.Width * img.Height * 4];
						var data = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
						Marshal.Copy(data.Scan0, bgrabuff, 0, bgrabuff.Length);
						img.UnlockBits(data);
						for (int i = 0; i < img.Width * img.Height; i++)
						{
							buff[i * 4 + 0] = bgrabuff[i * 4 + 2];
							buff[i * 4 + 1] = bgrabuff[i * 4 + 1];
							buff[i * 4 + 2] = bgrabuff[i * 4 + 0];
							buff[i * 4 + 3] = bgrabuff[i * 4 + 3];
						}
						break;
					}
				default: break;
			}
			return buff;
		}

		/// <summary>
		/// Byte数组转Image
		/// </summary>
		/// <param name="data"></param>
		/// <param name="format">如果数据格式为常见文件格式(JPG,BMP,PNG,...)无需改此参数,会自动识别.</param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns></returns>
		public static Image ToImage(this byte[] data, ImageFormat format = ImageFormat.FILE, int width = 0, int height = 0)
		{
			if (data == null) return null;
			Bitmap img = null;
			switch (format)
			{
				case ImageFormat.FILE:
					{
						using (var ms = new MemoryStream(data)) using (var tmp = Bitmap.FromStream(ms)) img = (Bitmap)tmp.DeepCopyBitmap();
						break;
					}
				case ImageFormat.RAW_RGB24:
					{
						img = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
						var imglocker = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);
						var bgrabuff = new byte[img.Width * img.Height * 4];
						for (int i = 0; i < img.Width * img.Height; i++)
						{
							bgrabuff[i * 4 + 0] = data[i * 3 + 2];
							bgrabuff[i * 4 + 1] = data[i * 3 + 1];
							bgrabuff[i * 4 + 2] = data[i * 3 + 0];
							bgrabuff[i * 4 + 3] = 0xFF;
						}
						Marshal.Copy(bgrabuff, 0, imglocker.Scan0, bgrabuff.Length);
						img.UnlockBits(imglocker);
						break;
					}
				case ImageFormat.RAW_ARGB32:
					{
						img = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
						var imglocker = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);
						var bgrabuff = new byte[img.Width * img.Height * 4];
						for (int i = 0; i < img.Width * img.Height; i++)
						{
							bgrabuff[i * 4 + 0] = data[i * 4 + 3];
							bgrabuff[i * 4 + 1] = data[i * 4 + 2];
							bgrabuff[i * 4 + 2] = data[i * 4 + 1];
							bgrabuff[i * 4 + 3] = data[i * 4 + 0];
						}
						Marshal.Copy(bgrabuff, 0, imglocker.Scan0, bgrabuff.Length);
						img.UnlockBits(imglocker);
						break;
					}
				case ImageFormat.RAW_BGR24:
					{
						img = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
						var imglocker = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);
						var bgrabuff = new byte[img.Width * img.Height * 4];
						for (int i = 0; i < img.Width * img.Height; i++)
						{
							bgrabuff[i * 4 + 0] = data[i * 3 + 0];
							bgrabuff[i * 4 + 1] = data[i * 3 + 1];
							bgrabuff[i * 4 + 2] = data[i * 3 + 2];
							bgrabuff[i * 4 + 3] = 0xFF;
						}
						Marshal.Copy(bgrabuff, 0, imglocker.Scan0, bgrabuff.Length);
						img.UnlockBits(imglocker);
						break;
					}
				case ImageFormat.RAW_RGBA32:
					{
						img = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
						var imglocker = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);
						var bgrabuff = new byte[img.Width * img.Height * 4];
						for (int i = 0; i < img.Width * img.Height; i++)
						{
							bgrabuff[i * 4 + 0] = data[i * 4 + 2];
							bgrabuff[i * 4 + 1] = data[i * 4 + 1];
							bgrabuff[i * 4 + 2] = data[i * 4 + 0];
							bgrabuff[i * 4 + 3] = data[i * 4 + 3];
						}
						Marshal.Copy(bgrabuff, 0, imglocker.Scan0, bgrabuff.Length);
						img.UnlockBits(imglocker);
						break;
					}
				default: break;
			}
			return img;
		}
		#endregion

		/// <summary>
		/// 
		/// </summary>
		/// <param name="bitmap"></param>
		/// <param name="insteadtransparent">用来顶替透明区的颜色,默认为null则不顶替</param>
		/// <returns></returns>
		public static Bitmap DeepCopyBitmap(this Bitmap bitmap, Brush insteadtransparent = null) => (Bitmap)DeepCopyBitmap((Image)bitmap, insteadtransparent);
		/// <summary>
		/// 
		/// </summary>
		/// <param name="bitmap"></param>
		/// <param name="insteadtransparent">用来顶替透明区的颜色,默认为null则不顶替</param>
		/// <returns></returns>
		public static Image DeepCopyBitmap(this Image bitmap, Brush insteadtransparent = null)
		{
			try
			{
				var img = new Bitmap(bitmap.Width, bitmap.Height);
				var g = Graphics.FromImage(img);
				if (insteadtransparent != null) g.FillRectangle(insteadtransparent, 0, 0, img.Width, img.Height);
				g.DrawImage(bitmap, 0, 0);
				g.Flush();
				g.Dispose();
				return img;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error : {0}", ex.Message);
				return null;
			}
		}

		/// <summary>
		/// 图片剪切
		/// </summary>
		/// <param name="img"></param>
		/// <param name="rect"></param>
		/// <returns></returns>
		public static Bitmap Cut(this Image img, Rectangle rect) => Cut((Bitmap)img, rect);

		/// <summary>
		/// 图片剪切
		/// </summary>
		/// <param name="b"></param>
		/// <param name="rect"></param>
		/// <returns></returns>
		public static Bitmap Cut(this Bitmap b, Rectangle rect)
		{
			if (b == null) return null;
			int w = b.Width;
			int h = b.Height;
			if (rect.Left >= w || rect.Top >= h) return null;
			//if (rect.Left + rect.Width > w) iWidth = w - StartX;
			//if (StartY + iHeight > h) iHeight = h - StartY;
			try
			{
				var imgout = new Bitmap(rect.Width, rect.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
				using (var g = Graphics.FromImage(imgout)) g.DrawImage(b, new Rectangle(0, 0, rect.Width, rect.Height), rect, GraphicsUnit.Pixel);
				return imgout;
			}
			catch
			{
				return null;
			}
		}

		/// <summary>
		/// 拉伸图片
		/// </summary>
		/// <param name="img"></param>
		/// <param name="size"></param>
		/// <returns></returns>
		public static Bitmap Resize(this Bitmap img, System.Drawing.Size size)
		{
			var newimg = new Bitmap(size.Width, size.Height);
			using (var gp = Graphics.FromImage(newimg))
			{
				gp.DrawImage(img, new Rectangle(0, 0, size.Width, size.Height), new Rectangle(0, 0, img.Width, img.Height), GraphicsUnit.Pixel);
				gp.Flush();
			}
			return newimg;
		}

		/// <summary>
		/// 拉伸图片
		/// </summary>
		/// <param name="img"></param>
		/// <param name="size"></param>
		/// <returns></returns>
		public static Bitmap Resize(this Image img, System.Drawing.Size size) => Resize((Bitmap)img, size);

		/// <summary>
		/// 反色
		/// </summary>
		/// <param name="img"></param>
		/// <param name="reversetransparent">是否包括透明</param>
		/// <returns></returns>
		public static Bitmap Reverse(this Bitmap img, bool reversetransparent = false)
		{
			var buff = img.ImgToBytes(ImageFormat.RAW_ARGB32);
			//这样算法快些
			if (reversetransparent)
			{
				for (int i = 0; i < buff.Length; i += 4)
				{
					buff[i + 0] = (byte)(255 - buff[i + 0]);
					buff[i + 1] = (byte)(255 - buff[i + 1]);
					buff[i + 2] = (byte)(255 - buff[i + 2]);
					buff[i + 3] = (byte)(255 - buff[i + 3]);
				}
			}
			else
			{
				for (int i = 0; i < buff.Length; i += 4)
				{
					buff[i + 1] = (byte)(255 - buff[i + 1]);
					buff[i + 2] = (byte)(255 - buff[i + 2]);
					buff[i + 3] = (byte)(255 - buff[i + 3]);
				}
			}
			return (Bitmap)buff.ToImage(ImageFormat.RAW_ARGB32, img.Width, img.Height);
		}

		/// <summary>
		/// 反色
		/// </summary>
		/// <param name="img"></param>
		/// <param name="reversetransparent">是否包括透明</param>
		/// <returns></returns>
		public static Bitmap Reverse(this Image img, bool reversetransparent = false) => Reverse((Bitmap)img, reversetransparent);

		/// <summary>
		/// 本方法用于将多张图像叠加,需要确保传入的每张图片大小相同,本方法不负责调整图像大小
		/// </summary>
		/// <param name="imgs">图片数组</param>
		/// <param name="ratios">比例数组,该数组表示图片数组里面每张图片的可见度比例,最小为0,最大为1,该参数可不传,若不传,将自动按图片数量均分可见度.</param>
		public static Bitmap Superposition(this Image[] imgs, double[] ratios = null) => Superposition((from img in imgs select (Bitmap)img).ToArray(), ratios);

		/// <summary>
		/// 本方法用于将多张图像叠加,需要确保传入的每张图片大小相同,本方法不负责调整图像大小
		/// </summary>
		/// <param name="imgs">图片数组</param>
		/// <param name="ratios">比例数组,该数组表示图片数组里面每张图片的可见度比例,最小为0,最大为1,该参数可不传,若不传,将自动按图片数量均分可见度.</param>
		public static Bitmap Superposition(this Bitmap[] imgs, double[] ratios = null)
		{
			#region 参数验证
			if (imgs == null) throw new ArgumentException("请传入要叠加的图片");
			if (imgs.Length <= 0) return null;
			if (ratios != null)
			{
				if (ratios.Length != imgs.Length) throw new ArgumentException("ratios的长度必须与imgs相同!");
				foreach (var r in ratios) if (r < 0 || r > 1) throw new ArgumentException("ratio的范围必须在0~1之间!");
			}
			var size = imgs[0].Size;
			foreach (var img in imgs) if (img.Width != size.Width || img.Height != size.Height) throw new ArgumentException("检测到图片大小不同,请传入相同大小的图片!");
			#endregion
			var newbuff = new byte[size.Width * size.Height * 4];
			var buffs = new byte[imgs.Length][];
			for (var i = 0; i < imgs.Length; i++) buffs[i] = imgs[i].ImgToBytes(ImageFormat.RAW_ARGB32);
			#region 考虑到效率直接分为2个算法了
			if (ratios == null)
			{
				for (var i = 0; i < newbuff.Length; i += 4)
				{
					newbuff[i + 0] = 0xff;
					int tmpr = 0;
					int tmpg = 0;
					int tmpb = 0;
					foreach (var buff in buffs)
					{
						tmpr += buff[i + 1];
						tmpg += buff[i + 2];
						tmpb += buff[i + 3];
					}
					newbuff[i + 1] = (byte)(tmpr / imgs.Length);
					newbuff[i + 2] = (byte)(tmpg / imgs.Length);
					newbuff[i + 3] = (byte)(tmpb / imgs.Length);
				}
			}
			else
			{
				for (var i = 0; i < newbuff.Length; i += 4)
				{
					newbuff[i + 0] = 0xff;
					int tmpr = 0;
					int tmpg = 0;
					int tmpb = 0;
					for (int j = 0; j < buffs.Length; j++)
					{
						var buff = buffs[j];
						tmpr += (byte)(buff[i + 1] * ratios[j]);
						tmpg += (byte)(buff[i + 2] * ratios[j]);
						tmpb += (byte)(buff[i + 3] * ratios[j]);
					}
					newbuff[i + 1] = (byte)(tmpr < 0x100 ? tmpr : 0xff);
					newbuff[i + 2] = (byte)(tmpg < 0x100 ? tmpg : 0xff);
					newbuff[i + 3] = (byte)(tmpb < 0x100 ? tmpb : 0xff);
				}
			}
			#endregion
			return (Bitmap)newbuff.ToImage(ImageFormat.RAW_ARGB32, size.Width, size.Height);
		}

		/// <summary>
		/// 编辑图片
		/// </summary>
		/// <param name="img"></param>
		/// <param name="action"></param>
		public static void Edit(this Bitmap img, Action<Graphics> action)
		{
			var g = Graphics.FromImage(img);
			action(g);
			g.Flush();
			g.Dispose();
		}

		/// <summary>
		/// 编辑图片
		/// </summary>
		/// <param name="img"></param>
		/// <param name="action"></param>
		public static void Edit(this Image img, Action<Graphics> action) => Edit((Bitmap)img, action);

	}//End Class
}