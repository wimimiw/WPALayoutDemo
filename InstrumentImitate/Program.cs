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
            ScpiCommand sg = new JcSigGener("");

            sc.StartServer("127.0.0.1", 9999);
            sg.StartServer("127.0.0.1", 8888);
            
            while (true) ;
        }
    }
}
