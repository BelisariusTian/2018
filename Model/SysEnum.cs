using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{

    public enum SysEnum : int
    {
        成功 = 0,
        失败 = -1,
        系统错误 = -2,
        用户名或密码错误 = -3,
        参数错误 = -4,
        其他错误 = -5,
        登录超时 = -6,
        IP不匹配 = -7,
        未授权登录 = -8,
        权限不足 = -9,
    }

    /// <summary>
    /// 请求接口种类
    /// </summary>
    public enum RequestCategory : int
    {
        微信公众号 = 0,
        微信小程序 = 1,
        APP = 2,
        WEB = 3
    }

    /// <summary>
    /// 微信用户状态
    /// </summary>
    public enum WXUserState
    {
        未关注 = 0,
        已关注 = 1,
        取消关注 = 2
    }

    /// <summary>
    /// 逻辑删除状态
    /// </summary>
    public enum DelFlag
    {
        正常 = 0,
        逻辑删除状态 = 1
    }

    /// <summary>
    /// 资源类别
    /// </summary>
    public enum ResourceType
    {
        图片 = 0,
        视频 = 1,
        音频 = 2,
        其他 = 3
    }

    /// <summary>
    /// 审核状态
    /// </summary>
    public enum CheckState
    {
        等待审核 = 0,
        通过 = 1,
        拒绝 = 2
    }
    /// <summary>
    /// 系统记录日志类别
    /// </summary>
    public enum SysLogType
    {
        后台日志 = 0,
        前台日志 = 1
    }
    /// <summary>
    /// 特殊权限状态
    /// </summary>
    public enum SpecialActionState
    {
        通过 = 0,
        不通过 = 1
    }
    /// <summary>
    /// 权限状态
    /// </summary>
    public enum ActionState
    {
        启用 = 0,
        禁用 = 1
    }
    /// <summary>
    /// 权限类别
    /// </summary>
    public enum ActionType
    {
        普通权限 = 0,
        菜单权限 = 1
    }
    /// <summary>
    /// 性别
    /// </summary>
    public enum Sex
    {
        女 = 0,
        男 = 1,
        未知 = 2
    }

    /// <summary>
    /// 用户特殊权限中间表  是否通过
    /// </summary>
    public enum Admin_actionIspass
    {
        通过 = 0,
        不通过 = 1
    }


	/// <summary>
	/// 微信消息回复
	/// </summary>
	public enum WXRequestType
	{
		文本 = 1,
		图片 = 2,
		图文 = 3,
		语音 = 4
	}

	/// <summary>
	/// 微信状态回复
	/// </summary>
	public enum WXRequestState
	{
		关注时回复 = 1,
		关键字回复 = 2
	}

}
