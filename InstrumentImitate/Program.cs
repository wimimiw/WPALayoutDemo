using System;
using System.Collections.Generic;
using System.Text;

namespace InstrumentImitate
{
    class Program
    {
        static void Main(string[] args)
        {
            ScpiCommand sc = new JcSpectrum("");

            //float tt = sc.CommandSetAnaylzer(":FRE 1800 MHZ");

            //tt = sc.CommandSetAnaylzer(":FRE -56.3 DBM");

            //sc.CommandPacketUnpack("*IDN?\n\r");

            sc.StartServer("127.0.0.1", 9999);

            while (true) ;
        }
    }
}
