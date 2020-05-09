# localnet-QQ
localnet的QQ对接插件

使用 <a href="https://cqp.cc/">酷Q</a> 以及 <a href="https://github.com/Jie2GG/Native.Framework">Native.Framework</a>

#

第一次启动插件时，会生成配置文件（LNconfig.txt/LNid.txt）  
在LNconfig.txt里配置localnet相关信息（IP，端口）以及管理员QQ号。  
在LNid.txt里配置QQ群与ID的对应关系，例如  
123456789=awa  
987654321=QAQ  
这样，在localnet里，123456789这个群内的消息在转发时，前缀就会显示为awa，  
如果发出消息的群不在这个配置文件里，那么前缀将会是QQ群号。  
  
使用<#>开头可以直接向localnet端发送指令，例如#list  
注意，仅管理员可以使用指令，其他QQ账号发出#开头的内容时，会返回权限不足的提示  
  
插件会把QQ账号内每一个QQ群都向localnet单独建立一个连接，  
例如，你的QQ号内有10个群，那么你在localnet里就可以看到10个连接上的终端，每一个终端的ID就是LNid.txt中所对应的ID，如果LNid.txt中没有对应ID，那么ID则是QQ群号  
  
插件启用时会逐一将QQ群和localnet的连接，在连接建立完成前，本插件不会处理消息，可以在酷Q的日志内查看建立进度  

#
插件出现任何问题，先看酷Q的日志，看看插件有没有输出报错信息  
