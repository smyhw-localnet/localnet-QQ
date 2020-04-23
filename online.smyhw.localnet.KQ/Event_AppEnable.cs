using Native.Sdk.Cqp.EventArgs;
using Native.Sdk.Cqp.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace online.smyhw.localnet.KQ.Code
{
    public class Event_AppEnable : IAppEnable
    {
        //这个方法将在程序运行时执行
        public void AppEnable(object sender, CQAppEnableEventArgs e)
        {
            e.CQLog.Info("localnet连接插件开始加载...");
            Sdata.APIII = e.CQApi;
            Sdata.log = e.CQLog;

            //加载配置文件
            if (!File.Exists("./LNconfig.txt"))
            {
                Sdata.log.Warning("配置文件", "未找到配置文件，创建之...");
                File.Create("./LNconfig.txt").Dispose();
                System.IO.StreamWriter config_file_ = new System.IO.StreamWriter("./LNconfig.txt");
                config_file_.WriteLine("#修改为你localnet的IP和端口");
                config_file_.WriteLine("IP=127.0.0.1");
                config_file_.WriteLine("Port=2333333");
                config_file_.WriteLine("#管理员QQ号");
                config_file_.WriteLine("adminQQ=123456789");
                config_file_.Close();
                Sdata.log.Warning("配置文件","配置文件创建完毕，请修改配置文件并重新启用应用");
                return;
            }


            if (!File.Exists("./LNid.txt"))
            {
                Sdata.log.Warning("配置文件", "未找到ID配置文件，创建之...");
                File.Create("./LNid.txt").Dispose();
                Sdata.log.Warning("配置文件", "ID配置文件创建完毕");
            }

            //读取配置文件
            System.IO.StreamReader config_file = new System.IO.StreamReader("./LNconfig.txt");
            while (true)
            {
                string line_text = config_file.ReadLine();
                if (line_text == null) { break; }
                string[] temp2 = line_text.Split('=');
                if (temp2.Length != 2) { continue; }
                switch (temp2[0])
                {
                    case "IP":
                        Sdata.lnIP = temp2[1];
                        break;
                    case "Port":
                        Sdata.lnPort = int.Parse(temp2[1]);
                        break;
                    case "adminQQ":
                        Sdata.adminQQ = temp2[1];
                        break;
                    default:
                        Sdata.log.Info("配置文件","未知配置项目:"+line_text);
                        break;
                }
            }
            config_file.Close();
            if (Sdata.lnIP == null) { Sdata.log.Error("配置文件", "未在配置文件中找到配置项目<IP>");return; }
            if (Sdata.lnPort == null) { Sdata.log.Error("配置文件", "未在配置文件中找到配置项目<Port>"); return; }
            if (Sdata.adminQQ == null) { Sdata.log.Error("配置文件", "未在配置文件中找到配置项目<adminQQ>"); return; }

            //读取ID对照表
            System.IO.StreamReader id_file = new System.IO.StreamReader("./LNid.txt");
            while (true)
            {
                string line_text = id_file.ReadLine();
                if (line_text == null) { break; }
                string[] temp2 = line_text.Split('=');
                if (temp2.Length != 2) { Sdata.log.Error("配置文件", "ID配置文件行<"+line_text+">无效"); continue; }
                Sdata.IDlist.Add(temp2[0],temp2[1]);
            }

            //载入群列表
            List<Native.Sdk.Cqp.Model.GroupInfo> temp1 = Sdata.APIII.GetGroupList();
            for (int temp2 = 0; temp2 < temp1.Count; temp2++)//批量向localnet注册群
            {
                Sdata.log.Info("初始化","加载群：" + temp1[temp2].Group.Id+"="+ KQlib.ID_re(temp1[temp2].Group.Id.ToString()));
                Sdata.GroupList.Add(temp1[temp2].Group.Id, new TCPLK_QQ(Sdata.lnIP, Sdata.lnPort, KQlib.ID_re(temp1[temp2].Group.Id.ToString()), temp1[temp2].Group.Id));
            }
            Sdata.isReady = true;
        }
    }
}
