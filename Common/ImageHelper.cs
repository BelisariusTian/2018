using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Encoder = System.Text.Encoder;

namespace Common
{
    /// <summary>
    /// 图片处理
    /// </summary>
    public static class ImageHelper
    {
        public static string CropImage(byte[] content, int x, int y, int cropWidth, int cropHeight, string file)
        {
            using (MemoryStream stream = new MemoryStream(content))
            {
                return CropImage(stream, x, y, cropWidth, cropHeight,file);
            }
        }
        public static string CropImage(Stream content, int x, int y, int cropWidth, int cropHeight, string file)
        {
            using (Bitmap sourceBitmap = new Bitmap(content))
            {
                // 将选择好的图片缩放
                Bitmap bitSource = new Bitmap(sourceBitmap, sourceBitmap.Width, sourceBitmap.Height);
                Rectangle cropRect = new Rectangle(x, y, cropWidth, cropHeight);
                using (Bitmap newBitMap = new Bitmap(cropWidth, cropHeight))
                {
                    newBitMap.SetResolution(sourceBitmap.HorizontalResolution, sourceBitmap.VerticalResolution);
                    using (Graphics g = Graphics.FromImage(newBitMap))
                    {
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        g.CompositingQuality = CompositingQuality.HighQuality;
                        g.DrawImage(bitSource, new Rectangle(0, 0, newBitMap.Width, newBitMap.Height), cropRect, GraphicsUnit.Pixel);
                        return CropImageFilePath(newBitMap,file);
                        //return GetBitmapBytes(newBitMap);
                    }
                }
            }
        }

        public static string CropImageFilePath(Bitmap source, string file)
        {
            ImageCodecInfo codec = ImageCodecInfo.GetImageEncoders()[4];
            EncoderParameters parameters = new EncoderParameters(1);
            parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);
            using (MemoryStream tmpStream = new MemoryStream())
            {
                source.Save(tmpStream, codec, parameters);
                byte[] data = new byte[tmpStream.Length];
                tmpStream.Seek(0, SeekOrigin.Begin);
                tmpStream.Read(data, 0, (int)tmpStream.Length);
                File.WriteAllBytes(file, data);
                return file;
            }
        }

        public static string GetBitmapBytes(Bitmap source)
        {
            ImageCodecInfo codec = ImageCodecInfo.GetImageEncoders()[4];
            EncoderParameters parameters = new EncoderParameters(1);
            parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);
            using (MemoryStream tmpStream = new MemoryStream())
            {
                source.Save(tmpStream, codec, parameters);
                byte[] data = new byte[tmpStream.Length];
                tmpStream.Seek(0, SeekOrigin.Begin);
                tmpStream.Read(data, 0, (int)tmpStream.Length);
                string result = Convert.ToBase64String(data);
                return result;
            }
        }
        /// <summary>
        /// 图片转换Base64
        /// </summary>
        /// <param name="urlOrPath">图片网络地址或本地路径</param>
        /// <returns></returns>
        public static string ImageToBase64(string urlOrPath)
        {
            try
            {
                if (urlOrPath.Contains("http"))
                {
                    Uri url = new Uri(urlOrPath);
                    using (var webClient = new WebClient())
                    {
                        var imgData = webClient.DownloadData(url);
                        using (var ms = new MemoryStream(imgData))
                        {
                            byte[] data = new byte[ms.Length];
                            ms.Seek(0, SeekOrigin.Begin);
                            ms.Read(data, 0, Convert.ToInt32(ms.Length));
                            string netImage = Convert.ToBase64String(data);
                            return netImage;
                        }
                    }
                }
                else
                {
                    FileStream fileStream = new FileStream(urlOrPath, FileMode.Open);
                    Stream stream = fileStream as Stream;
                    byte[] data = new byte[stream.Length];
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.Read(data, 0, Convert.ToInt32(stream.Length));
                    string netImage = Convert.ToBase64String(data);
                    return netImage;
                }
            }
            catch (Exception)
            {
                return null;
            }

        }
        /// <summary>
        /// 按比例缩放图片
        /// </summary>
        /// <param name="content"></param>
        /// <param name="resizeWidth"></param>
        /// <returns></returns>
        public static string ResizeImage(Stream content, int resizeWidth, int resizeHeight)
        {
            using (Bitmap sourceBitmap = new Bitmap(content))
            {
                int width = sourceBitmap.Width,
                    height = sourceBitmap.Height;
                if (height > resizeHeight || width > resizeWidth)
                {
                    if ((width * resizeHeight) > (height * resizeWidth))
                    {
                        resizeHeight = (resizeWidth * height) / width;
                    }
                    else
                    {
                        resizeWidth = (width * resizeHeight) / height;
                    }
                }
                else
                {
                    resizeWidth = width;
                    resizeHeight = height;
                }
                // 将选择好的图片缩放
                Bitmap bitSource = new Bitmap(sourceBitmap, resizeWidth, resizeHeight);
                bitSource.SetResolution(sourceBitmap.HorizontalResolution, sourceBitmap.VerticalResolution);
                using (MemoryStream stream = new MemoryStream())
                {
                    bitSource.Save(stream, ImageFormat.Jpeg);
                    byte[] data = new byte[stream.Length];
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.Read(data, 0, Convert.ToInt32(stream.Length));
                    string newImage = Convert.ToBase64String(data);
                    return newImage;
                }
            }
        }

        /// <summary>
        /// Convert Image to Byte[]
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static byte[] ImageToBytes(Image image)
        {
            ImageFormat format = image.RawFormat;
            using (MemoryStream ms = new MemoryStream())
            {
                if (format.Equals(ImageFormat.Jpeg))
                {
                    image.Save(ms, ImageFormat.Jpeg);
                }
                else if (format.Equals(ImageFormat.Png))
                {
                    image.Save(ms, ImageFormat.Png);
                }
                else if (format.Equals(ImageFormat.Bmp))
                {
                    image.Save(ms, ImageFormat.Bmp);
                }
                else if (format.Equals(ImageFormat.Gif))
                {
                    image.Save(ms, ImageFormat.Gif);
                }
                else if (format.Equals(ImageFormat.Icon))
                {
                    image.Save(ms, ImageFormat.Icon);
                }
                byte[] buffer = new byte[ms.Length];
                //Image.Save()会改变MemoryStream的Position，需要重新Seek到Begin
                ms.Seek(0, SeekOrigin.Begin);
                ms.Read(buffer, 0, buffer.Length);
                return buffer;
            }
        }

        /// <summary>
        /// Convert Byte[] to Image
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static Image BytesToImage(byte[] buffer)
        {
            MemoryStream ms = new MemoryStream(buffer);
            Image image = System.Drawing.Image.FromStream(ms);
            return image;
        }

        /// <summary>
        /// Convert Byte[] to a picture and Store it in file
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static string CreateImageFromBytes(string fileName, byte[] buffer)
        {
            string file = fileName;
            //Image image = BytesToImage(buffer);
            //ImageFormat format = image.RawFormat;
            //if (format.Equals(ImageFormat.Jpeg))
            //{
            //    file += ".jpeg";
            //}
            //else if (format.Equals(ImageFormat.Png))
            //{
            //    file += ".png";
            //}
            //else if (format.Equals(ImageFormat.Bmp))
            //{
            //    file += ".bmp";
            //}
            //else if (format.Equals(ImageFormat.Gif))
            //{
            //    file += ".gif";
            //}
            //else if (format.Equals(ImageFormat.Icon))
            //{
            //    file += ".icon";
            //}
            //System.IO.FileInfo info = new System.IO.FileInfo(file);
            //System.IO.Directory.CreateDirectory(info.Directory.FullName);
            File.WriteAllBytes(file, buffer);
            return file;
        }
    }
}
