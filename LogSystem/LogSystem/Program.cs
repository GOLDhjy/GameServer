using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            while(true)
            LogSystem.NOW = System.DateTime.Now.ToString();
        }
    }
}
