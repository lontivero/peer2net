using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsyncTcpServer
{
    class DataArriveEventArg : EventArgs 
    {
        private string data;

        public DataArriveEventArg(string data)
        {
            this.data = data;
        }

        public string Data
        {
            get { return data; }
        }
    }
}
