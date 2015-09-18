using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;

//4.3.1 IEEE488.2 共同命令
//*CLS
//*ESE
//*ESE?
//*ESR?
//*IDN?
//*OPC
//*OPC?
//*PSC
//*PSC?
//*RST
//*SRE
//*SRE?
//*STB?
//*SAV
//*RCL
 
//4.3.2 SCPI 标准命令
//SYSTem
//:ERRor?
//:VERSion?
//:BEEPer[:IMMediate]
//:ADDRess?
//STATus
//:QUEStionable
//:ENABle <enable value>
//:ENABle?
//[:EVENt]?
//:CONDition?
//:OPERation
//:ENABle <enable value>
//:ENABle?
//[:EVENt]?
//:CONDition?
//:INSTrumenu
//[:EVENt]?
//:ENABle <value>
//:ENABle?
//CONDition?
//INSTrument
//[:SELect] {FIRst|SECOnd|THIrd}
//[:SELect]?
//NSELect {1|2|3}
//NSELect?
//OUTPut
//[:STATe] {0|1}
//[:STATe]?
//[SOURce:]
//CURRent[:LEVel][:IMMediate][:AMPLitude] {<current>|MIN|MAX}
//CURRent[:LEVel][:IMMediate][:AMPLitude]? {MIN|MAX}
//VOLTage[:LEVel][:IMMediate][:AMPLitude] {<voltage>|MIN|MAX}
//VOLTage[:LEVel][:IMMediate][:AMPLitude]? {MIN|MAX}
//VOLTage:PROTection[:LEVel][:IMMediate][:AMPLitude]
//VOLTage:PROTection[:LEVel][:IMMediate][:AMPLitude]?

namespace InstrumentImitate
{
    class ScpiCommand
    {
        protected string __idn = "ScpiCommand";
        private Byte __sre;
        protected List<string> __errList = new List<string>();
        private StringBuilder __revCmdStr = new StringBuilder(512);
        private AsyncTcpServer __server;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public bool StartServer(string ip,int port)
        {
            __server = new AsyncTcpServer(System.Net.IPAddress.Parse(ip), port);
            __server.PlaintextReceived += __server_PlaintextReceived;
            __server.Start();

            return true;
        }

        void __server_PlaintextReceived(object sender, TcpDatagramReceivedEventArgs<string> e)
        {
            this.CommandPacketUnpack(e.remoteEndPoint,e.Datagram);
            //throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public float CommandSetAnaylzer(string cmd)
        {  
            //^[+-]?\d+(\.\d+)?$    匹配任意有可选符号的实数。
            //"^\d+$"　　//非负整数（正整数 + 0） 
            //"^[0-9]*[1-9][0-9]*$"　　//正整数 
            //"^((-\d+)|(0+))$"　　//非正整数（负整数 + 0） 
            //"^-[0-9]*[1-9][0-9]*$"　　//负整数 
            //"^-?\d+$"　　　　//整数 
            //"^\d+(\.\d+)?$"　　//非负浮点数（正浮点数 + 0） 
            //"^(([0-9]+\.[0-9]*[1-9][0-9]*)|([0-9]*[1-9][0-9]*\.[0-9]+)|([0-9]*[1-9][0-9]*))$"　　//正浮点数 
            //"^((-\d+(\.\d+)?)|(0+(\.0+)?))$"　　//非正浮点数（负浮点数 + 0） 
            //"^(-(([0-9]+\.[0-9]*[1-9][0-9]*)|([0-9]*[1-9][0-9]*\.[0-9]+)|([0-9]*[1-9][0-9]*)))$"　　//负浮点数 
            //"^(-?\d+)(\.\d+)?$"　　//浮点数 
            //"^[A-Za-z]+$"　　//由26个英文字母组成的字符串 
            //"^[A-Z]+$"　　//由26个英文字母的大写组成的字符串 
            //"^[a-z]+$"　　//由26个英文字母的小写组成的字符串 
            //"^[A-Za-z0-9]+$"　　//由数字和26个英文字母组成的字符串 
            //"^\w+$"　　//由数字、26个英文字母或者下划线组成的字符串 
            //"^[\w-]+(\.[\w-]+)*@[\w-]+(\.[\w-]+)+$"　　　　//email地址 
            //"^[a-zA-z]+://(\w+(-\w+)*)(\.(\w+(-\w+)*))*(\?\S*)?$"　　//url 
            ///^(d{2}|d{4})-((0([1-9]{1}))|(1[1|2]))-(([0-2]([1-9]{1}))|(3[0|1]))$/   //  年-月-日 
            ///^((0([1-9]{1}))|(1[1|2]))/(([0-2]([1-9]{1}))|(3[0|1]))/(d{2}|d{4})$/   // 月/日/年 
            //"^([w-.]+)@(([[0-9]{1,3}.[0-9]{1,3}.[0-9]{1,3}.)|(([w-]+.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(]?)$"   //Emil 
            //"(d+-)?(d{4}-?d{7}|d{3}-?d{8}|^d{7,8})(-d+)?"     //电话号码 
            //"^(d{1,2}|1dd|2[0-4]d|25[0-5]).(d{1,2}|1dd|2[0-4]d|25[0-5]).(d{1,2}|1dd|2[0-4]d|25[0-5]).(d{1,2}|1dd|2[0-4]d|25[0-5])$"   //IP地址 

            //^([0-9A-F]{2})(-[0-9A-F]{2}){5}$   //MAC地址的正则表达式 
            //^[-+]?\d+(\.\d+)?$  //值类型正则表达式  
       
            int factor = 0;

            if (cmd.ToLower().Contains("khz"))
                factor = 1;
            else
                if (cmd.ToLower().Contains("mhz"))
                    factor = 1000;
                else
                    if (cmd.ToLower().Contains("dbm"))
                        factor = 1;

            NumberFormatInfo nfi = NumberFormatInfo.CurrentInfo;

            // Define the regular expression pattern.
            string pattern = @"[+-]?\d+(\.\d*)?";//匹配任意有可选符号的实数。

            Regex rgx = new Regex(pattern);
            Match match = rgx.Match(cmd);

            return match.Success ? float.Parse(match.Value)*factor :float.MaxValue;
        }

        private string Scpi_Idn()
        {
            return this.__idn;
        }

        private string Scpi_Sre()
        {
            return this.__sre.ToString();
        }

        private string Scpi_Cls()
        {
            this.__errList.Clear();
            this.__sre = 0;

            return "+1";
        }

        protected string Scpi_Opc()
        {
            return "+1";
        }

        protected virtual string Scpi_OpcWait()
        {
            return "+1";
        }

        protected virtual string Scpi_Rst()
        {
            return "+1";
        }

        protected virtual string User_CommandPcs(string cmd)
        {
            return "+1";
        }

        protected void SetSRE()
        {
            this.__sre = 0x01;
        }

        protected void ClsSRE()
        {
            this.__sre = 0x00;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        public void CommandPacketSend(string endPoint,string str)
        {
            __server.Send(endPoint, str);
        }
        /// <summary>
        /// //
        /// </summary>
        /// <param name="cmd"></param>
        public void CommandPacketUnpack(string endPoint,string cmd)
        {
            if (!cmd.Contains("\r\n"))
            {
                this.__revCmdStr.Append(cmd);
                return;
            }

            this.__revCmdStr.Append(cmd);

            string cmdStr = this.__revCmdStr.ToString().TrimEnd(new char[]{'\r','\n'}).ToUpper();

            this.__revCmdStr.Remove(0, this.__revCmdStr.Length);

            List<string> cmdList = new List<string>();

            cmdList.AddRange(cmdStr.Split(';'));

            string returnValue = "-1\r\n";

            foreach (var item in cmdList)
            {
                if (item.Contains("*IDN?")) returnValue = Scpi_Idn();
                else
                    if (item.Contains("*OPC?")) returnValue = Scpi_OpcWait();
                    else
                        if (item.Contains("*SRE?")) returnValue = Scpi_Sre();
                        else
                            if (item.Contains("*OPC")) returnValue = Scpi_Opc();
                            else
                                if (item.Contains("*CLS")) returnValue = Scpi_Cls();
                                else
                                    if (item.Contains("*RST")) returnValue = Scpi_Rst();
                                    else
                                        returnValue = User_CommandPcs(item);
            }

            CommandPacketSend(endPoint,returnValue+"\r\n");
        }
    }


    class JcSpectrum : ScpiCommand
    {
        public JcSpectrum(string idn)
        {
 
        }

        protected override string Scpi_OpcWait()
        {
            return base.Scpi_OpcWait();
        }

        protected override string Scpi_Rst()
        {
            return base.Scpi_Rst();
        }

        protected override string User_CommandPcs(string cmd)
        {
            //:CENT
            //:SPAN
            //:BWID
            //:RES
            //:VID
            //:STAT
            //:OPER
            //:CALC
            //:MARK
            //:MAX

            return "";
            //return base.User_CommandPcs(cmd);
        }
    }

    class JcSigGener : ScpiCommand
    {
        public JcSigGener()
        {
 
        }

        protected override string Scpi_OpcWait()
        {
            return base.Scpi_OpcWait();
        }

        protected override string Scpi_Rst()
        {
            return base.Scpi_Rst();
        }

        protected override string User_CommandPcs(string cmd)
        {
            //:POW
            //:FREQ
            //:REF

            return "";
            //return base.User_CommandPcs(cmd);
        }
    }
}
