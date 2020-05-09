using Native.Sdk.Cqp;
using Native.Sdk.Cqp.EventArgs;
using Native.Sdk.Cqp.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace online.smyhw.localnet.KQ.Code
{

    public class Sdata//静态资源池(划掉)
    {
        public static String lnIP;//localnet的IP
        public static int lnPort;//localnet的端口
//        public static long infoGroup;//调试信息输出的QQ群
        public static CQApi APIII;//当前KQ_API实例
        public static CQLog log;//当前KQ_API的日志实例
        public static Hashtable GroupList = new Hashtable();//QQ群与对应localnet连接的对应map
        public static string adminQQ;//管理员QQ号
        public static Hashtable IDlist = new Hashtable();//QQ号与ID的对应map
        public static Boolean isReady = false;//是否初始化完毕
    }
    public class Event_GroupMessage : IGroupMessage
    {
        public void GroupMessage(object sender, CQGroupMessageEventArgs e)
        {
            if (!Sdata.isReady) { return; }//如果配置没有成功加载，则不处理消息

            if (e.Message.Text.StartsWith("#"))
            {
                if (e.FromQQ.Id.ToString() != Sdata.adminQQ)
                {
                    Sdata.log.Warning("鉴权","权限不足的用户在尝试使用#命令<"+ e.FromQQ.ToString()+"!="+ Sdata.adminQQ);
                    Sdata.APIII.SendGroupMessage(e.FromGroup,"抱歉，权限不足！");
                    return; 
                }
                String command_msg = e.Message.Text.Substring(1);
//                command_msg = "/" + command_msg;
                TCPLK_QQ temp2 = (TCPLK_QQ)Sdata.GroupList[e.FromGroup.Id];
                temp2.sendData("command",command_msg);
                return;
            }

            //处理普通消息（转换CQ码，处理@的信息，加上用户名）
            String sendMSG;
            //获取群名片
            String frome_name = Sdata.APIII.GetGroupMemberInfo(e.FromGroup, e.FromQQ).Card;
            if (frome_name.Equals("")) //如果没有获取到群名片，就拿QQ昵称替代
            {
                frome_name = Sdata.APIII.GetGroupMemberInfo(e.FromGroup, e.FromQQ).Nick;
            }
            String text = e.Message.Text;
            text = text.Replace("\n", " | ");//处理换行
            text = KQlib.CQmsg_re(text,e.FromGroup.Id);//处理CQ码
            sendMSG = "["+frome_name + "]:" + text;//拼接消息
            //发送消息
            TCPLK_QQ temp1 = (TCPLK_QQ)Sdata.GroupList[e.FromGroup.Id];
            temp1.send(sendMSG);
            return;
        }


    }
}
