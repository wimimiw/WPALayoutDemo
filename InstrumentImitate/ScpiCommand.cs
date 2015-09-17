using System;
using System.Collections.Generic;
using System.Text;

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
        public string idn;
        protected List<string> __errList = new List<string>();
        protected StringBuilder __revCmdStr = new StringBuilder(512);

//*IDN?
// *ESE
// *OPC
// *SRE?
 
//*RST
// *ESE?
// *OPC?
// *STB
 
//*CLS
// *ESR
// *SRE
// *TST?
 
//*WAI
//:SENSe
//:FREQ
//:CENT
//:SPAN
//:BWID
//:RES
//:VID
//:STATus
//:OPERation
//:CALCulate
//:MARKer
//:MAXimum

        public void CommandPacketUnpack(string cmd)
        {
            if (!cmd.Contains("\n\r"))
            {
                this.__revCmdStr.Append(cmd);
            }

            string cmdStr = this.__revCmdStr.ToString().TrimEnd(new char[]{'\n','\r'}).ToUpper();

            this.__revCmdStr.Remove(0, this.__revCmdStr.Length);

            List<string> cmdList = new List<string>();

            cmdList.AddRange(cmdStr.Split(';'));

            foreach (var item in cmdList)
            {
                
            }
        }

        public virtual void abc()
        {

        }

    }
}
