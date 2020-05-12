# 图像处理辅助类

## 简介

对.Net自带的两个图像类Image和Bitmap进行了一些扩展,实现了以下功能:
一些基本的图像处理函数(切割,拉伸,深复制)
与Bytes数组互相转换(实现对接OpenCVSharp和Dlib等视觉处理库,可互相灵活转换)

## 使用方法

nuget安装这个包之后,在需要使用扩展方法的文件里,引用如下命名空间:
```SHARP
using Lin.ImageHelper;
```
### 切割图片

可将图片对象的一部分切割下来,生成一个新的图片对象.

```SHARP
var img = Image.FromFile("Test.jpg");
var newimg = img.Cut(new Rectangle(0, 0, 32, 32));
```

### 拉伸图片

改变图片对象的大小.
```SHARP
var img = Image.FromFile("Test.jpg");
var newimg = img.Resize(new Size(32, 32));
```

### 深复制

众所周知,.Net的图像复制后其实的数据其实是同一块,对图像进行修改,会互相影响.
这个方法可以完全复制一个图像,复制出来的图像与之前的图像不会互相影响.
另外:在C#中,图像如果是使用FromStream从Stream里面读取的,如果事后Dispose了Stream会导致图像对象也无法使用,在这之前可以深复制一下.

```SHARP
var img = Image.FromFile("Test.jpg");
var newimg=img.DeepCopyBitmap();
```

### 反色

```SHARP
var img = Image.FromFile("Test.jpg");
var newimg=img.Reverse();
```

### 图像转Byte[]

使用这个方法可以在图像与byte[]之间灵活转换,并且对接其他图像处理库.
参数是选择格式,第一个参数是选 "文件格式" 或者 "RAW格式"
文件格式表示常见的JPG,BMP,PNG这些.
RAW格式表示直接把图像的像素值数据提取到byte[],有多种排列方式选择

第二个参数是选文件格式,只有第一个参数填FILE的时候,才会用到这个参数,不填也行,这个参数默认是JPG.
```SHARP
var img = Image.FromFile("Test.jpg");
var jpgbytes = img.ImgToBytes(ImageFormat.FILE,System.Drawing.Imaging.ImageFormat.Jpeg);
```

### Byte[]转图像

上面接口的反向操作,参数同理,如果是FILE,无需填写w,h 如果是RAW,需要填写

```SHARP
var img = Image.FromFile("Test.jpg");
//图像转RAW
var w = img.Width;
var h = img.Height;
var jpgbytes = img.ImgToBytes(ImageFormat.RAW_RGBA32);
//RAW转回图像
var nweimg = jpgbytes.ToImage(ImageFormat.RAW_RGBA32, w, h);
```

### 编辑图像

偶尔你可能需要在图像上做些小修改.

```SHARP
var img = Image.FromFile("Test.jpg");
img.Edit(g => g.DrawRectangle(Pens.Black, new Rectangle(0, 0, 10, 10)));
```

### 图片叠加

可将多张图片叠加在一起.
后面参数传入的是源图像的可见度比例(每张图片的范围为0~1).
按照例子这样传入,可让第一张图片的可见度更大一些(70%)第二张图片可见度低些(30%)
传入多张图片的情况下,需要为每张图片指定一个可见度.
可见度可以不传,函数内部会自动根据图片数量,按比例生成一组可见度.比如传4张图片,内部会自动生成 [0.25,0.25,0.25,0.25]

```SHARP
// 加载两张图片
var img1 = Image.FromFile("Test1.jpg");
var img2 = Image.FromFile("Test2.jpg");
// 这样就能将两张图片叠加在一起了,图片可以不止2张,但要求图片必须全部一样大小
var newimg = new Image[]{img1,img2}.Superposition(new double[]{0.7,0.3});
```