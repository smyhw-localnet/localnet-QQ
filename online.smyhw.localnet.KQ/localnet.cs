using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace online.smyhw.localnet.KQ.Code
{

    public class KQlib
    {
        /**
         * 
         * 处理ID
         */
        public static String ID_re(String QQ)
        {
            string re = (String)Sdata.IDlist[QQ];
            if (re == null) { return QQ; }
            return re;
        }

        /**
         * 处理CQ码
         */
        public static String CQmsg_re(String msg,long QQ)
        {
            try
            {
                String before_str, after_str,cq_msg,cq_type;
                int find_head,find_end;
                while (msg.Contains("[CQ:"))
                {
                    find_head = msg.IndexOf("[CQ:");//找到CQ头
                    find_end = msg.IndexOf("]",find_head);//找到CQ尾
                    cq_msg = msg.Substring(find_head,find_end-find_head+1);//截取出CQ文本
                    //分离CQ类型
                    cq_type = cq_msg.Substring(4,cq_msg.IndexOf(",")-4);
                    //将详细信息输出至日志群
                    Sdata.log.Debug("CQ码处理","cq_msg="+cq_msg+";cq_type="+cq_type+";msg="+msg);
                    //匹配并替换CQ
                    String re_msg;//对CQ码进行的替换
                    switch (cq_type)
                    {
                        case "face":
                            re_msg = "[系统表情]";
                            break;
                        case "emoji":
                            re_msg = "[emoji表情]";
                            break;
                        case "bface":
                            re_msg = "[原创表情]";
                            break;
                        case "sface":
                            re_msg = "[小表情]";
                            break;
                        case "image":
                            re_msg = "[自定义图片]";
                            break;
                        case "record":
                            re_msg = "[语音]";
                            break;
                        case "at":
                            re_msg = "@" + Sdata.APIII.GetGroupMemberInfo(QQ, (long)Convert.ToInt64(cq_msg.Replace("[CQ:at,qq=", "").Replace("]", ""))).Card;
                            break;
                        case "rps":
                            re_msg = "[猜拳魔法表情]";
                            break;
                        case "dice":
                            re_msg = "[掷骰子魔法表情]";
                            break;
                        case "shake":
                            re_msg = "[戳一戳]";
                            break;
                        case "anonymous":
                            re_msg = "[匿名发消息]";
                            break;
                        case "location":
                            re_msg = "[地点]";
                            break;
                        case "sign":
                            re_msg = "[签到]";
                            break;
                        case "music":
                            re_msg = "[音乐]";
                            break;
                        case "share":
                            re_msg = "[链接分享]";
                            break;

                        default:
                            re_msg = "[未知特殊消息]";
                            break;

                    }

                    before_str = msg.Substring(0, find_head);//CQ码之前的信息
                    after_str = msg.Substring(find_end);//CQ码之后的信息
                    msg = before_str + re_msg + after_str;//拼接信息
                }
                return msg;
            }
            catch (Exception e)
            { 
                return "[信息处理错误]";
            }
        }

    }
    class localnet
    {

    }

    /**
     * localnet协议在C#的实现
     * 
     */
    public class TCPLK_QQ
    {

        Socket s=null;
        Thread thread = null;
        String IP;
        int Port;
        public String ID;
        long QQ;
        public TCPLK_QQ(String IP, int Port, String ID, long QQ)
        {
            this.QQ = QQ;
            conn(IP,Port,ID);
        }
        public void conn(String IP, int Port, String ID)
        {
            //初始化信息
            this.IP = IP;
            this.Port = Port;
            this.ID = ID;
            s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);//实例化Socket
            //初始化连接
            try
            {
                s.Connect(IP, Port);//建立连接
                Sdata.log.Warning("线程", "连接到localnet端：" + this.receive());//接受对方ID
                this.send("&" + ID);//发送自身ID
            }
            catch (Exception ee)
            {
                Sdata.log.Warning("线程","警告，与localnet尝试连接时出错！线程将被迭代，错误如下：\n" + ee.Message);
                this.died();
            }
            Thread.Sleep(5000);
            //初始化接收线程
            thread = new Thread(new ThreadStart(C_thread));//创建线程
            thread.Start();//开始线程
        }

        /**
         * 发送方法
         */
        public void send(String input)
        {
            Byte[] js = Encoding.UTF8.GetBytes(input);
            Byte[] f_msg = Encoding.UTF8.GetBytes(js.Length + "|" + input);
            s.Send(f_msg);
        }
        
        /**
         * 接收方法
         */
        public String receive()
        {
            String S_len = "";
            int len = 0;
            while (true)//读取长度位
            {
                Byte[] temp1 = new byte[1];
                this.s.Receive(temp1);
                String temp2 = Encoding.UTF8.GetString(temp1);
                if (temp2.Equals("|"))//匹配到标识符
                {
                    break;
                }
                else
                {
                    S_len = S_len + temp2;
                    continue;
                }
            }
            //读取数据
            int.TryParse(S_len, out len);
            if (len == 0)//当转换失败时异常处理 
            { Sdata.log.Warning("线程", "警告，读取报文长度时获取到0值！"); return ""; }
            String fin_data = "";//最终数据
            byte[] b_data = new byte[len];
            byte[] temp4 = new byte[1];
            for (int temp3 = 0; temp3 < len; temp3++)
            {
                this.s.Receive(temp4);
                b_data[temp3] = temp4[0];
            }
            fin_data = Encoding.UTF8.GetString(b_data);
            return fin_data;
        }


       /**
        * 接收线程
        */
        public void C_thread()
        {
            try
            {
                while (true)
                {
                    String redata = receive();
                    if (redata.StartsWith("#"))//分离心跳包
                    {
                        this.send("#xt");//返回心跳包
                        continue;
                    }
                    if (redata.StartsWith("*")) { redata = redata.Substring(1); }//除去标识符
                    Sdata.APIII.SendGroupMessage(this.QQ, redata);//发送消息至QQ群
                }
            }
            catch (Exception ee)
            {
                Sdata.log.Warning("线程", "警告，C_thread线程出错！\n" + ee.ToString() + "\n该线程将被迭代");
                this.died();
            }
        }



        //当线程出错时调用
        protected void died()
        {
            Sdata.log.Warning("线程", "警告，C_thread线程" +this.ID+"迭代！");
            Thread.Sleep(5000);
            this.thread.Abort();//强制停止线程
            this.s.Close();//关闭socket
            conn(IP, Port, ID);//迭代线程
        }
    }
}
