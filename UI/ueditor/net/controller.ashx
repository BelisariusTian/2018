<%@ WebHandler Language="C#" Class="UEditorHandler" %>

using System;
using System.Web;
using Common;
using Model;
using System.Text.RegularExpressions;


public class UEditorHandler : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        //string r = "^[A-Za-z0-9]+&";
        string userGuid = context.Request.QueryString["token"];
        if (string.IsNullOrEmpty(userGuid))
        {
            //客户端没有cookie
            context.Response.Write("您没有权限操作");
            context.Response.End();
            return;
        }
        userGuid = userGuid.Substring(1,16);
        int userid;
        string userip;
        try
        {
            string[] arr = Common.EncryptHelper.Decrypt(userGuid).Split('|');
            userid = int.Parse(arr[0]);
            userip = arr[1];
        }
        catch (Exception)
        {
            context.Response.Write("您没有权限操作");
            context.Response.End();
            return;
        }
        var userInfo = Common.Cache.CacheHelper.GetCache(userid.ToString()) as administrator;
        if (userInfo == null)
        {
            //用户超过20分钟不操作，重新登陆
            context.Response.Write("您没有权限操作");
            context.Response.End();
            return;
        }
        //滑动窗口机制
        Common.Cache.CacheHelper.SetCache(userid.ToString(), userInfo, DateTime.Now.AddMinutes(60));
        Handler action = null;
        switch (context.Request["action"])
        {
            case "config":
                action = new ConfigHandler(context);
                break;
            case "uploadimage":
                action = new UploadHandler(context, new UploadConfig()
                {
                    AllowExtensions = Config.GetStringList("imageAllowFiles"),
                    PathFormat = Config.GetString("imagePathFormat"),
                    SizeLimit = Config.GetInt("imageMaxSize"),
                    UploadFieldName = Config.GetString("imageFieldName")
                });
                break;
            case "uploadscrawl":
                action = new UploadHandler(context, new UploadConfig()
                {
                    AllowExtensions = new string[] { ".png" },
                    PathFormat = Config.GetString("scrawlPathFormat"),
                    SizeLimit = Config.GetInt("scrawlMaxSize"),
                    UploadFieldName = Config.GetString("scrawlFieldName"),
                    Base64 = true,
                    Base64Filename = "scrawl.png"
                });
                break;
            case "uploadvideo":
                action = new UploadHandler(context, new UploadConfig()
                {
                    AllowExtensions = Config.GetStringList("videoAllowFiles"),
                    PathFormat = Config.GetString("videoPathFormat"),
                    SizeLimit = Config.GetInt("videoMaxSize"),
                    UploadFieldName = Config.GetString("videoFieldName")
                });
                break;
            case "uploadfile":
                action = new UploadHandler(context, new UploadConfig()
                {
                    AllowExtensions = Config.GetStringList("fileAllowFiles"),
                    PathFormat = Config.GetString("filePathFormat"),
                    SizeLimit = Config.GetInt("fileMaxSize"),
                    UploadFieldName = Config.GetString("fileFieldName")
                });
                break;
            case "listimage":
                action = new ListFileManager(context, Config.GetString("imageManagerListPath"), Config.GetStringList("imageManagerAllowFiles"));
                break;
            case "listfile":
                action = new ListFileManager(context, Config.GetString("fileManagerListPath"), Config.GetStringList("fileManagerAllowFiles"));
                break;
            case "catchimage":
                action = new CrawlerHandler(context);
                break;
            default:
                action = new NotSupportedHandler(context);
                break;
        }
        action.Process();
    }

    public bool IsReusable {
        get {
            return false;
        }
    }
}