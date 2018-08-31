using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Common
{
    public class ConvertFile
    {
        /// <summary>
        /// 文件格式转换(PS： .* => .mp3 )
        /// </summary>
        /// <param name="ffmpegVirtualPath">ffmpeg.exe 虚拟路径</param>
        /// <param name="sourceFile">源文件物理路径</param>
        /// <param name="fileVirtualPath">目标文件虚拟路径</param>
        /// <returns></returns>

        public static string ConvertFileFormat(string ffmpegVirtualPath, string sourceFile, string fileVirtualPath)
        {
            //取得ffmpeg.exe的物理路径
            string ffmpeg = HttpContext.Current.Server.MapPath(ffmpegVirtualPath);
            if (!System.IO.File.Exists(ffmpeg))
            {
                throw new Exception("找不到格式转换程序");
            }
            if (!System.IO.File.Exists(sourceFile))
            {
                throw new Exception("找不到源文件");
            }
            string destFile = HttpContext.Current.Server.MapPath(fileVirtualPath);//要保存的新文件

            try
            {
                string Command = " -i " + sourceFile + " " + destFile; //cmd命令
                System.Diagnostics.Process p = new System.Diagnostics.Process(); //创建进程
                p.StartInfo.FileName = ffmpeg;//要启动的应用程序
                p.StartInfo.Arguments = Command;//设置启动时执行命令
                p.StartInfo.UseShellExecute = false;//是否使用 shell 执行
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.CreateNoWindow = false;//是否在新窗口打开应用程序
                p.Start();//开始执行
                p.BeginErrorReadLine();
                p.WaitForExit();//等待应用程序执行完成
                p.Close();//关闭所有与这个应用程序有关联的资源
                p.Dispose();//释放
            }
            catch
            {
                return "格式转换失败！";
            }
            if (!System.IO.File.Exists(destFile))//如果转换后的文件不存在，那么代表转换失败了
            {
                return "格式转换失败！";
            }
            return fileVirtualPath;
        }
    }
}
