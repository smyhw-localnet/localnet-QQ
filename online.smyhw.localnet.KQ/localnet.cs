using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;

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
                this.sendData("auth" ,ID);//发送自身ID
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
            sendData("message", input);
        }

        public void sendData(String type, String input)
        {
            input = Json.Encoded(input);
            switch(type)
            {
                case "message":
                    input = "{\"type\":\"message\",\"message\":\"" + input + "\"}";
                    break;
                case "cmd":
                    input = "{\"type\":\"command\",\"CmdText\":\"" + input + "\"}";
                    break;
                case "connect":
                    input = "{\"type\":\"connect\",\"operation\":\"" + input + "\"}";
                    break;
                case "auth":
                    input = "{\"type\":\"auth\",\"ID\":\"" + input + "\"}";
                    break;
            }
            
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

                    if (redata.Equals("{type:connect,operation:xt}"))//分离心跳包
                    {
                        this.sendData("connect", "xt");//返回心跳包
                        continue;
                    }
                    Hashtable data = Json.Parse(redata);
                    if (data["type"].Equals("message"))
                    {
                        Sdata.APIII.SendGroupMessage(this.QQ, "[核心]:" + data["message"]);//发送消息至QQ群
                        continue;
                    }
                    if (data["type"].Equals("forward_message"))
                    {
                        Sdata.APIII.SendGroupMessage(this.QQ, "["+data["From"]+"]:" + data["message"]);//发送消息至QQ群
                        continue;
                    }
                    
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


/**
 * 该类被设计为处理标准Json信息</br>
 * 
 * @author smyhw
 */
public class Json
{

    /**
	 * 解析JSON字符串
	 * @param input JSON字符串
	 * @return
	 */
    public static Hashtable Parse(String input)
    {
        Hashtable re = new Hashtable();
        //		if(!input.startsWith("{")) {return null;};
        //		input = input.substring(1);
        //		input = input.substring(0, input.length()-1);
        char[] str = input.ToCharArray();
        String key = "", value = "";
        int type = 0;//type==0#键;type==1#值
        int stru = 0;//stru==0#构造字符;stru==1#数据字符
        for (int i = 0; i < str.Length; i++)
        {
            if (i > 0 && str[i] == '"' && str[i - 1] != '\\')//加前置条件i>0是为了防止检测第0个字符的前一位(i-1)导致异常
            {//如果检测到有效的双引号，则切换stru
                if (stru == 1) { stru = 0; }
                else { stru = 1; }
                continue;
            }
            if (stru == 0)
            {//如果读取的是构造字符...
             //需要考虑逗号问题
                if (str[i] == '{') { continue; }
                if (str[i] == '}')
                {//处于构造字符的大括号代表字符串结束
                 //注意,这里别忘了保存最后一个键值对
                    type = 0;
                    key = Decoded(key);
                    value = Decoded(value);
                    re.Add(key, value);
                    key = "";
                    value = "";
                    return re;
                }
                if (str[i] == ':') { type = 1; continue; }//表示接下来读取的是值
                if (str[i] == ',')
                {//表示一个键值对已经完成，提交到Hashtable
                    type = 0;
                    key = Decoded(key);
                    value = Decoded(value);
                    re.Add(key, value);
                    key = "";
                    value = "";
                    continue;
                }

                //能处理到这，说明这个构造字符是tm非法的,直接返回null,表示错误数据
                return null;
            }
            else
            {//如果读取的是数据
                if (type == 0)
                {//如果读取的是键
                    key = key + str[i];
                    continue;
                }
                else
                {//如果读取的是值
                    value = value + str[i];
                    continue;
                }
            }
        }
        key = Decoded(key);
        value = Decoded(value);
        re.Add(key, value);
        //		message.show(re.toString());
        return re;
    }

    /**
	 * 根据Hashtable构造JSON字符串
	 * @param input 
	 * @return
	 */
    public static String Create(Hashtable input)
    {
        String re = "{";
        foreach (string key1 in input.Keys)
        {
            string value = (string)input[key1];
            string key = key1;
            key = Encoded(key);
            value = Encoded(value);
            re = re + "\"" + key + "\":\"" + value + "\",";
        }
        re = re.Substring(0, re.Length - 1);
        re = re + "}";
        return re;
    }



    /**
	 * 用于转义特殊字符</br>
	 * <\>(反斜杠)</br>
	 * <">(双引号)</br>
	 * 都会被转义</br>
	 * @param input 未转义的字符串
	 * @return 转义后的字符串
	 */
    public static String Encoded(String input)
    {
        //		message.info("en++"+input);
        char[] str = input.ToCharArray();
        ArrayList out_str = new ArrayList();
        ArrayList key_word = new ArrayList();
        key_word.Add('\\');
        key_word.Add('"');
        for (int i = 0; i < str.Length; i++)
        {
            if (key_word.Contains(str[i]))
            {
                out_str.Add('\\');
            }
            out_str.Add(str[i]);
        }
        String re = "";
        for (int i = 0; i < out_str.Count; i++)
        {
            re = re+out_str[i];
        }
        //		message.info("en--"+re);
        return re;
    }

    /**
	 * 反转义特殊字符
	 * @param input
	 * @return
	 * @see public static String Encoded(String input)
	 */
    public static String Decoded(String input)
    {
        //		message.info("de++"+input);
        char[] str = input.ToCharArray();
        ArrayList out_str = new ArrayList();
        ArrayList key_word = new ArrayList();
        key_word.Add('\\');
        key_word.Add('"');
        for (int i = 0; i < str.Length; i++)
        {
            if (str[i] == '\\' && key_word.Contains(str[i + 1]))
            {
                i = i + 1;
            }
            out_str.Add(str[i]);
        }
        String re = "";
        for (int i = 0; i < out_str.Count; i++)
        {
            re = re+out_str[i];
        }
        //		message.info("de--"+re);
        return re;
    }
}
