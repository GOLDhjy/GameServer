using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogSystem
{
    public class FilePathHelper
    {
        private static string serverpath = @"D:\Server.txt";
        private static string clientpath = @"D:\Client.txt";
        static string SelfPath = @"LogFile.txt";

        public static string Serverpath { get => serverpath; set => serverpath = value; }
        public static string Clientpath { get => clientpath; set => clientpath = value; }
        public static string SelfPath1 { get => SelfPath; set => SelfPath = value; }
    }
}
