/*----------------------------------------------------------------
    Copyright (C) 2017 Senparc

    文件名：CustomMessageHandler.cs
    文件功能描述：微信公众号自定义MessageHandler


    创建标识：Senparc - 20150312
----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using WXCommonService.Utilities;
using Senparc.Weixin;
using Senparc.Weixin.Exceptions;
using Senparc.Weixin.Helpers;
using Senparc.Weixin.MP;
using Senparc.Weixin.MP.Agent;
using Senparc.Weixin.MP.Entities;
using Senparc.Weixin.MP.Entities.Request;
using Senparc.Weixin.MP.Helpers;
using Senparc.Weixin.MP.MessageHandlers;

namespace WXCommonService.MessageHandlers.CustomMessageHandler
{
    /// <summary>
    /// 自定义MessageHandler
    /// 把MessageHandler作为基类，重写对应请求的处理方法
    /// </summary>
    public partial class CustomMessageHandler : MessageHandler<CustomMessageContext>
    {
        /*
         * 重要提示：v1.5起，MessageHandler提供了一个DefaultResponseMessage的抽象方法，
         * DefaultResponseMessage必须在子类中重写，用于返回没有处理过的消息类型（也可以用于默认消息，如帮助信息等）；
         * 其中所有原OnXX的抽象方法已经都改为虚方法，可以不必每个都重写。若不重写，默认返回DefaultResponseMessage方法中的结果。
         */


#if DEBUG
        //string agentUrl = "http://localhost:12222/App/Weixin/4";
        //string agentToken = "27C455F496044A87";
        //string wiweihiKey = "CNadjJuWzyX5bz5Gn+/XoyqiqMa5DjXQ";
#else
        //下面的Url和Token可以用其他平台的消息，或者到www.weiweihi.com注册微信用户，将自动在“微信营销工具”下得到
        private string agentUrl = WebConfigurationManager.AppSettings["WeixinAgentUrl"];//这里使用了www.weiweihi.com微信自动托管平台
        private string agentToken = WebConfigurationManager.AppSettings["WeixinAgentToken"];//Token
        private string wiweihiKey = WebConfigurationManager.AppSettings["WeixinAgentWeiweihiKey"];//WeiweihiKey专门用于对接www.Weiweihi.com平台，获取方式见：http://www.weiweihi.com/ApiDocuments/Item/25#51
#endif
        private string domainUrl = "http://" + WebConfigurationManager.AppSettings["Domain"];
        private string appId = WebConfigurationManager.AppSettings["WeixinAppId"];
        private string appSecret = WebConfigurationManager.AppSettings["WeixinAppSecret"];


        /// <summary>
        /// 模板消息集合（Key：checkCode，Value：OpenId）
        /// </summary>
        public static Dictionary<string, string> TemplateMessageCollection = new Dictionary<string, string>();

        public CustomMessageHandler(Stream inputStream, PostModel postModel, int maxRecordCount = 0)
            : base(inputStream, postModel, maxRecordCount)
        {
            //这里设置仅用于测试，实际开发可以在外部更全局的地方设置，
            //比如MessageHandler<MessageContext>.GlobalWeixinContext.ExpireMinutes = 3。
            WeixinContext.ExpireMinutes = 3;

            if (!string.IsNullOrEmpty(postModel.AppId))
            {
                appId = postModel.AppId;//通过第三方开放平台发送过来的请求
            }

            //在指定条件下，不使用消息去重
            base.OmitRepeatedMessageFunc = requestMessage =>
            {
                var textRequestMessage = requestMessage as RequestMessageText;
                if (textRequestMessage != null && textRequestMessage.Content == "容错")
                {
                    return false;
                }
                return true;
            };
        }

        public override void OnExecuting()
        {
            //测试MessageContext.StorageData
            if (CurrentMessageContext.StorageData == null)
            {
                CurrentMessageContext.StorageData = 0;
            }
            base.OnExecuting();
        }

        public override void OnExecuted()
        {
            base.OnExecuted();
            CurrentMessageContext.StorageData = ((int)CurrentMessageContext.StorageData) + 1;
        }

        /// <summary>
        /// 处理文字请求
        /// </summary>
        /// <returns></returns>
        public override IResponseMessageBase OnTextRequest(RequestMessageText requestMessage)
        {
            //TODO:这里的逻辑可以交给Service处理具体信息，参考OnLocationRequest方法或/Service/LocationSercice.cs

            //书中例子
            //if (requestMessage.Content == "你好")
            //{
            //    var responseMessage = base.CreateResponseMessage<ResponseMessageNews>();
            //    var title = "Title";
            //    var description = "Description";
            //    var picUrl = "PicUrl";
            //    var url = "Url";
            //    responseMessage.Articles.Add(new Article()
            //    {
            //        Title = title,
            //        Description = description,
            //        PicUrl = picUrl,
            //        Url = url
            //    });
            //    return responseMessage;
            //}
            //else if (requestMessage.Content == "Senparc")
            //{
            //    //相似处理逻辑
            //}
            //else
            //{
            //    //...
            //}



            //方法一（v0.1），此方法调用太过繁琐，已过时（但仍是所有方法的核心基础），建议使用方法二到四
            //var responseMessage =
            //    ResponseMessageBase.CreateFromRequestMessage(RequestMessage, ResponseMsgType.Text) as
            //    ResponseMessageText;

            //方法二（v0.4）
            //var responseMessage = ResponseMessageBase.CreateFromRequestMessage<ResponseMessageText>(RequestMessage);

            //方法三（v0.4），扩展方法，需要using Senparc.Weixin.MP.Helpers;
            //var responseMessage = RequestMessage.CreateResponseMessage<ResponseMessageText>();

            //方法四（v0.6+），仅适合在HandlerMessage内部使用，本质上是对方法三的封装
            //注意：下面泛型ResponseMessageText即返回给客户端的类型，可以根据自己的需要填写ResponseMessageNews等不同类型。

            return this.CreateResponseMessage<ResponseMessageTransfer_Customer_Service>();
        }

        /// <summary>
        /// 处理位置请求
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnLocationRequest(RequestMessageLocation requestMessage)
        {
            var responseMessage = this.CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = "您刚才发送的是位置信息：" + requestMessage.Location_X + " ," + requestMessage.Location_Y;
            return responseMessage;
        }

        public override IResponseMessageBase OnShortVideoRequest(RequestMessageShortVideo requestMessage)
        {
            var responseMessage = this.CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = "您刚才发送的是小视频";
            return responseMessage;
        }

        /// <summary>
        /// 处理图片请求
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnImageRequest(RequestMessageImage requestMessage)
        {
            var responseMessage = CreateResponseMessage<ResponseMessageNews>();
            responseMessage.Articles.Add(new Article()
            {
                Title = "您刚才发送了图片信息",
                Description = "您发送的图片将会显示在边上",
                PicUrl = requestMessage.PicUrl,
                Url = domainUrl
            });
            //responseMessage.Articles.Add(new Article()
            //{
            //    Title = "第二条",
            //    Description = "第二条带连接的内容",
            //    PicUrl = requestMessage.PicUrl,
            //    Url = "http://sdk.weixin.senparc.com"
            //});
            return responseMessage;
        }

        /// <summary>
        /// 处理语音请求
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnVoiceRequest(RequestMessageVoice requestMessage)
        {
            var responseMessage = this.CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = "您刚才发送的是语音";
            return responseMessage;
        }
        /// <summary>
        /// 处理视频请求
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnVideoRequest(RequestMessageVideo requestMessage)
        {
            var responseMessage = CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = "您发送了一条视频信息";
            return responseMessage;
        }

        /// <summary>
        /// 处理链接消息请求
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnLinkRequest(RequestMessageLink requestMessage)
        {
            var responseMessage = ResponseMessageBase.CreateFromRequestMessage<ResponseMessageText>(requestMessage);
            responseMessage.Content = string.Format(@"您发送了一条连接信息：
Title：{0}
Description:{1}
Url:{2}", requestMessage.Title, requestMessage.Description, requestMessage.Url);
            return responseMessage;
        }

        /// <summary>
        /// 处理事件请求（这个方法一般不用重写，这里仅作为示例出现。除非需要在判断具体Event类型以外对Event信息进行统一操作
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnEventRequest(IRequestMessageEventBase requestMessage)
        {
            var eventResponseMessage = base.OnEventRequest(requestMessage);//对于Event下属分类的重写方法，见：CustomerMessageHandler_Events.cs
            //TODO: 对Event信息进行统一操作
            return eventResponseMessage;
        }

        public override IResponseMessageBase DefaultResponseMessage(IRequestMessageBase requestMessage)
        {
            /* 所有没有被处理的消息会默认返回这里的结果，
            * 因此，如果想把整个微信请求委托出去（例如需要使用分布式或从其他服务器获取请求），
            * 只需要在这里统一发出委托请求，如：
            * var responseMessage = MessageAgent.RequestResponseMessage(agentUrl, agentToken, RequestDocument.ToString());
            * return responseMessage;
            */

            var responseMessage = this.CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = "这条消息来自DefaultResponseMessage。";
            return responseMessage;
        }
    }
}
