///////////////////////////////////////////////////////////
//  时  间:2018年3月9日19:05:26
//  作  者: Leeseett
///////////////////////////////////////////////////////////
using Common;
using Model;
using System;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace UI.Controllers
{
    /// <summary>
    /// 文件上传控制器
    /// </summary>
    /// <seealso cref="UI.Controllers.WebController" />
    public class UploadFileController : WebController
    {
        //
        // GET: /UploadFile/

        #region 上传图片
        /// <summary>
        /// 上传图片
        /// </summary>
        /// <param name="file">文件</param>
        /// <param name="width">缩略图宽度</param>
        /// <param name="height">缩略图高度</param>
        /// <param name="mode">生成缩略图的方式 
        /// HW 指定高宽缩放（可能变形）
        /// W 指定宽，高按比例  
        /// H 指定高，宽按比例
        /// Cut 指定高宽裁减（不变形）
        /// </param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UpImage(HttpPostedFileBase file, int width, int height, string mode, int sk = 0)
        {
            // 没有文件上传，直接返回
            if (file == null || string.IsNullOrEmpty(file.FileName) || file.ContentLength == 0)
            {
                return HttpNotFound();
            }

            //获取文件完整文件名(包含绝对路径)
            //文件存放路径格式：/files/upload/{日期}/{Guid+文件名}.{后缀名}
            //例如：/files/upload/20130913/43CA215D947F8C1F1DDFCED383C4D706.jpg
            //string str = Filedata.FileName.Substring(0, Filedata.FileName.LastIndexOf('.'));
            string fileName = Guid.NewGuid().ToString("N");
            string fileEextension = Path.GetExtension(file.FileName);
            string uploadDate = DateTime.Now.ToString("yyyyMMdd");

            if (file.ContentType.Contains("image"))
            {
                var virtualOriginalPath = string.Format("~/UploadFiles/images/{0}/Original/{1}{2}", uploadDate, fileName,
                    fileEextension);//原图片路径

                var virtualThumbnailPath = string.Format("~/UploadFiles/images/{0}/Thumbnail/{1}{2}", uploadDate, fileName,
                    fileEextension);//缩略图路径
                string fullOriginaFileName = this.Server.MapPath(virtualOriginalPath);
                string fullThumbnailFileName = this.Server.MapPath(virtualThumbnailPath);
                //创建文件夹，保存文件
                string path = Path.GetDirectoryName(fullOriginaFileName);
                if (path != null)
                {
                    Directory.CreateDirectory(path);
                    if (!System.IO.File.Exists(fullOriginaFileName))
                    {
                        file.SaveAs(fullOriginaFileName);
                    }
                }

                path = Path.GetDirectoryName(fullThumbnailFileName);
                if (path != null) Directory.CreateDirectory(path);
                ImageClass.MakeThumbnail(fullOriginaFileName, fullThumbnailFileName, width, height, mode);//生成缩略图
                if (sk != 0)
                {
                    Common.FileOperate.FileDel(fullOriginaFileName);
                }
                dynamic data = new
                {
                    OriginalPath = virtualOriginalPath.Remove(0, 1),
                    ThumbnailPath = virtualThumbnailPath.Remove(0, 1)
                };
                return Json(SysEnum.成功, data, "图片上传成功");
            }
            else
            {
                return Json(SysEnum.失败, "上传失败，格式错误");
            }
        }
        #endregion

        #region 上传音频或视频
        /// <summary>
        /// 上传音频或视频
        /// </summary>
        /// <param name="file">文件</param>
        /// <returns></returns>
        public ActionResult UpVideoOrAudio(HttpPostedFileBase file)
        {
            // 没有文件上传，直接返回
            if (file == null || string.IsNullOrEmpty(file.FileName) || file.ContentLength == 0)
            {
                return HttpNotFound();
            }
            string fileEextension = Path.GetExtension(file.FileName);
            if (fileEextension == ".mp3" || fileEextension == ".mp4" || fileEextension == ".wav" || fileEextension == ".wma" || fileEextension == ".aiff" ||
                fileEextension == ".amr" || fileEextension == ".m4a" || fileEextension == ".rm" ||
                fileEextension == ".rmvb" || fileEextension == ".avi" || fileEextension == ".3gp" ||
                fileEextension == ".ogg" || fileEextension == ".mpeg" || fileEextension == ".swf" ||
                fileEextension == ".flv" || fileEextension == ".vob" || fileEextension == ".mkv" ||
                fileEextension == ".raw")
            {
                //获取文件完整文件名(包含绝对路径)
                //文件存放路径格式：/files/upload/{日期}/{Guid+文件名}.{后缀名}
                //例如：/files/upload/20130913/43CA215D947F8C1F1DDFCED383C4D706.jpg
                //string str = Filedata.FileName.Substring(0, Filedata.FileName.LastIndexOf('.'));
                string fileName = Guid.NewGuid().ToString("N");

                string uploadDate = DateTime.Now.ToString("yyyyMMdd");
                var virtualOriginalPath = string.Format("~/UploadFiles/video/{0}/Original/{1}{2}", uploadDate, fileName, fileEextension); //原文件虚拟路径

                var virtualThumbnailPath = string.Format("~/UploadFiles/video/{0}/HandledPath/{1}{2}", uploadDate, fileName, fileEextension);
                //转换后的虚拟路径
                string fullOriginaFileName = this.Server.MapPath(virtualOriginalPath);
                //创建文件夹，保存文件
                string path = Path.GetDirectoryName(fullOriginaFileName);
                if (path != null)
                {
                    Directory.CreateDirectory(path);
                    if (!System.IO.File.Exists(fullOriginaFileName))
                    {
                        file.SaveAs(fullOriginaFileName);
                    }
                }
                //mp3,wma,aiff,arm,m4a
                if (fileEextension == ".mp3" || fileEextension == ".wav" || fileEextension == ".wma" || fileEextension == ".aiff" ||
                    fileEextension == ".amr" || fileEextension == ".m4a") //这些格式就是音频文件啦
                {
                    virtualThumbnailPath = ConvertFile.ConvertFileFormat("/bin/ffmpeg.exe", fullOriginaFileName,
                        virtualThumbnailPath + ".mp3"); //去转换格式，返回 新文件.mp3
                    if (virtualThumbnailPath == "格式转换失败！") //转换失败 
                    {
                        dynamic data = new
                        {
                            OriginalPath = virtualOriginalPath.Remove(0, 1),
                            ThumbnailPath = ""
                        };
                        return Json(SysEnum.其他错误, data, "上传成功，格式转换失败！");
                    }
                    else
                    {
                        dynamic data = new
                        {
                            OriginalPath = virtualOriginalPath.Remove(0, 1),
                            ThumbnailPath = ""
                        };
                        return Json(SysEnum.成功, data, "上传成功，格式转换成功！");
                    }
                }
                //rm,rmvb,avi,mp4,3gp,ogg,mpeg,swf,flv,vob,mkv,raw,mov
                if (fileEextension == ".mp4" || fileEextension == ".rm" || fileEextension == ".rmvb" || fileEextension == ".avi" ||
                    fileEextension == ".3gp" || fileEextension == ".ogg" || fileEextension == ".mpeg" ||
                    fileEextension == ".swf" || fileEextension == ".flv" || fileEextension == ".vob" ||
                    fileEextension == ".mkv" || fileEextension == ".raw") //视频格式文件
                {
                    virtualThumbnailPath = ConvertFile.ConvertFileFormat("/bin/ffmpeg.exe", fullOriginaFileName,
                        virtualThumbnailPath + ".mp4"); //返回 新文件.mp4
                    if (virtualThumbnailPath == "格式转换失败！") //转换失败 
                    {
                        dynamic data = new
                        {
                            OriginalPath = virtualOriginalPath.Remove(0, 1),
                            ThumbnailPath = ""
                        };
                        return Json(SysEnum.其他错误, data, "上传成功，格式转换失败！");
                    }
                    else
                    {
                        dynamic data = new
                        {
                            OriginalPath = virtualOriginalPath.Remove(0, 1),
                            ThumbnailPath = ""
                        };
                        return Json(SysEnum.成功, data, "上传成功，格式转换成功！");
                    }
                }
            }
            return Json(SysEnum.失败, "上传失败，文件格式错误！");
        }
        #endregion
        
        #region 通用
        /// <summary>
        /// 通用
        /// </summary>
        /// <param name="file">文件</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UpFile(HttpPostedFileBase file, int sk = -1)
        {
            // 没有文件上传，直接返回
            if (file == null || string.IsNullOrEmpty(file.FileName) || file.ContentLength == 0)
            {
                return HttpNotFound();
            }

            //获取文件完整文件名(包含绝对路径)
            //文件存放路径格式：/files/upload/{日期}/{Guid+文件名}.{后缀名}
            //例如：/files/upload/20130913/43CA215D947F8C1F1DDFCED383C4D706.jpg
            //string str = Filedata.FileName.Substring(0, Filedata.FileName.LastIndexOf('.'));
            string fileName = Guid.NewGuid().ToString("N");
            string fileEextension = Path.GetExtension(file.FileName);
            string uploadDate = DateTime.Now.ToString("yyyyMMdd");

            if (file.ContentType.Contains("image"))
            {
                var virtualOriginalPath = string.Format("~/UploadFiles/images/{0}/Original/{1}{2}", uploadDate, fileName,
                    fileEextension);//原图片路径

                var virtualThumbnailPath = string.Format("~/UploadFiles/images/{0}/Thumbnail/{1}{2}", uploadDate, fileName,
                    fileEextension);//缩略图路径
                string fullOriginaFileName = this.Server.MapPath(virtualOriginalPath);
                string fullThumbnailFileName = this.Server.MapPath(virtualThumbnailPath);
                //创建文件夹，保存文件
                string path = Path.GetDirectoryName(fullOriginaFileName);
                if (path != null)
                {
                    Directory.CreateDirectory(path);
                    if (!System.IO.File.Exists(fullOriginaFileName))
                    {
                        file.SaveAs(fullOriginaFileName);
                    }
                }

                path = Path.GetDirectoryName(fullThumbnailFileName);
                if (path != null) Directory.CreateDirectory(path);
                ImageClass.Compress(fullOriginaFileName, fullThumbnailFileName);//压缩图片
                if (sk != 0)
                {
                    Common.FileOperate.FileDel(fullOriginaFileName);
                }
                dynamic data = new
                {
                    OriginalPath = virtualOriginalPath.Remove(0, 1),
                    ThumbnailPath = virtualThumbnailPath.Remove(0, 1)
                };
                return Json(SysEnum.成功, data, "图片上传成功");
            }
            else if (fileEextension == ".mp3" || fileEextension == ".mp4" || fileEextension == ".wav" || fileEextension == ".wma" || fileEextension == ".aiff" ||
            fileEextension == ".amr" || fileEextension == ".m4a" || fileEextension == ".rm" ||
            fileEextension == ".rmvb" || fileEextension == ".avi" || fileEextension == ".3gp" ||
            fileEextension == ".ogg" || fileEextension == ".mpeg" || fileEextension == ".swf" ||
            fileEextension == ".flv" || fileEextension == ".vob" || fileEextension == ".mkv" ||
            fileEextension == ".raw")
            {
                var virtualOriginalPath = string.Format("~/UploadFiles/video/{0}/Original/{1}{2}", uploadDate, fileName,
                    fileEextension); //原文件虚拟路径

                var virtualThumbnailPath = string.Format("~/UploadFiles/video/{0}/HandledPath/{1}{2}", uploadDate, fileName, fileEextension);
                //转换后的虚拟路径
                string fullOriginaFileName = this.Server.MapPath(virtualOriginalPath);
                //创建文件夹，保存文件
                string path = Path.GetDirectoryName(fullOriginaFileName);
                if (path != null)
                {
                    Directory.CreateDirectory(path);
                    if (!System.IO.File.Exists(fullOriginaFileName))
                    {
                        file.SaveAs(fullOriginaFileName);
                    }
                }
                //mp3,wma,aiff,arm,m4a
                if (fileEextension == ".mp3" || fileEextension == ".wav" || fileEextension == ".wma" || fileEextension == ".aiff" ||
                    fileEextension == ".amr" || fileEextension == ".m4a") //这些格式就是音频文件啦
                {
                    virtualThumbnailPath = ConvertFile.ConvertFileFormat("/bin/ffmpeg.exe", fullOriginaFileName,
                        virtualThumbnailPath + ".mp3"); //去转换格式，返回 新文件.mp3
                    if (virtualThumbnailPath == "格式转换失败！") //转换失败 
                    {
                        dynamic data = new
                        {
                            OriginalPath = virtualOriginalPath.Remove(0, 1),
                            ThumbnailPath = ""
                        };
                        return Json(SysEnum.其他错误, data, "上传成功，格式转换失败！");
                    }
                    else
                    {
                        dynamic data = new
                        {
                            OriginalPath = virtualOriginalPath.Remove(0, 1),
                            ThumbnailPath = ""
                        };
                        return Json(SysEnum.成功, data, "上传成功，格式转换成功！");
                    }
                }
                //rm,rmvb,avi,mp4,3gp,ogg,mpeg,swf,flv,vob,mkv,raw,mov
                if (fileEextension == ".mp4" || fileEextension == ".rm" || fileEextension == ".rmvb" || fileEextension == ".avi" ||
                    fileEextension == ".3gp" || fileEextension == ".ogg" || fileEextension == ".mpeg" ||
                    fileEextension == ".swf" || fileEextension == ".flv" || fileEextension == ".vob" ||
                    fileEextension == ".mkv" || fileEextension == ".raw") //视频格式文件
                {
                    virtualThumbnailPath = ConvertFile.ConvertFileFormat("/bin/ffmpeg.exe", fullOriginaFileName,
                        virtualThumbnailPath + ".mp4"); //返回 新文件.mp4
                    if (virtualThumbnailPath == "格式转换失败！") //转换失败 
                    {
                        dynamic data = new
                        {
                            OriginalPath = virtualOriginalPath.Remove(0, 1),
                            ThumbnailPath = ""
                        };
                        return Json(SysEnum.其他错误, data, "上传成功，格式转换失败！");
                    }
                    else
                    {
                        dynamic data = new
                        {
                            OriginalPath = virtualOriginalPath.Remove(0, 1),
                            ThumbnailPath = ""
                        };
                        return Json(SysEnum.成功, data, "上传成功，格式转换成功！");
                    }
                }
            }
            else
            {
                var virtualOriginalPath = string.Format("~/UploadFiles/other/{0}/Original/{1}{2}", uploadDate, fileName,
                   fileEextension); //原文件虚拟路径
                //转换后的虚拟路径
                string fullOriginaFileName = this.Server.MapPath(virtualOriginalPath);
                //创建文件夹，保存文件
                string path = Path.GetDirectoryName(fullOriginaFileName);
                if (path != null)
                {
                    Directory.CreateDirectory(path);
                    if (!System.IO.File.Exists(fullOriginaFileName))
                    {
                        file.SaveAs(fullOriginaFileName);
                        dynamic data = new
                        {
                            OriginalPath = virtualOriginalPath.Remove(0, 1),
                            ThumbnailPath = virtualOriginalPath.Remove(0, 1)
                        };
                        return Json(SysEnum.成功, data, "上传成功！");
                    }
                }
            }
            return Json(SysEnum.失败, "上传失败！");
        }
		#endregion

		#region 删除文件
		/// <summary>
		/// 删除文件
		/// </summary>
		/// <param name="path">The path.</param>
		public void DeleteFile(string path)
		{
			FileAttributes attr = System.IO.File.GetAttributes(path);
			if (attr == FileAttributes.Directory)
			{
				Directory.Delete(path, true);
			}
			else
			{
				System.IO.File.Delete(path);
			}
		}

		/// <summary>
		/// 批量删除
		/// </summary>
		/// <param name="list">The list.</param>
		[HttpPost]
		public void DeleteFileList(string list)
		{
			var lists = list.Split(',');
			foreach (var path in lists)
			{
				var p = Server.MapPath(path);
				FileAttributes attr = System.IO.File.GetAttributes(p);
				if (attr == FileAttributes.Directory)
				{
					Directory.Delete(path, true);
				}
				else
				{
					System.IO.File.Delete(p);
				}
			}
		}
		#endregion
	}
}
