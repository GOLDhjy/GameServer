using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using static System.Console;

namespace LogSystem
{
    public class LogSystem: IXMLSerializer
    {
        public static string NOW = System.DateTime.Now.ToString();
        public static string SERVER = "server";
        public static string CLIENT = "client";
        public static string SELFPATH = "";
        /// <summary>
        /// 时间，等级，消息，主角。等级为0/1/2,0是info，1是警告,2是错误。文件保存路径默认为selfpath。
        /// </summary>
        /// <param name="time"></param>
        /// <param name="level"></param>
        /// <param name="msg"></param>
        /// <param name="p1"></param>
        public static async void LOG(string time, int level, string msg, string p1)
        {

            Message oo = new Message(time,level,msg,p1);
            WriteLine(oo.Result);
            await WriteAsync(FilePathHelper.SelfPath1, oo.Result);
        }
        /// <summary>
        /// 时间，等级，消息，主角,事件。等级为0/1/2,0是info，1是警告,2是错误。文件保存路径默认为selfpath
        /// </summary>
        /// <param name="time"></param>
        /// <param name="level"></param>
        /// <param name="msg"></param>
        /// <param name="p1"></param>
        /// <param name="done"></param>
        public static async void LOG(string time, int level, string msg, string p1,string done)
        {

            Message oo = new Message(time, level, msg, p1,done);
            await WriteAsync(FilePathHelper.SelfPath1, oo.Result);
        }
        //public static async void LOG<T1,T2>(T1 op1,T2 op2,T1 op3,T1 op4,T1 op5)
        //{
        //    Message oo = new Message(op1, op2, op3, op4, op5);
        //}




        /// <summary>
        /// 修改selfpath。
        /// </summary>
        /// <param name="path"></param>
        public static void modify(string path)
        {
            FilePathHelper.SelfPath1 = path;
        }



        /// <summary>
        /// op == 0，path 是client。op==1，path是server.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="op"></param>
        public static void modify(string path,int op)
        {
            switch(op)
            {
                case 0:
                    FilePathHelper.Clientpath = path;
                    break;
                case 1:
                    FilePathHelper.Serverpath = path;
                    break;
                default:
                    Console.WriteLine("fail modify path");
                    break;
            }
        }

        void IXMLSerializer.Serializer(string content)
        {
            throw new NotImplementedException();
        }

        void IXMLSerializer.DSerializer(out FileStream fs)
        {
            throw new NotImplementedException();
        }

        private static async Task WriteAsync(string path,string text)
        {
            byte[] encode = Encoding.UTF8.GetBytes(text);
            using (FileStream ss = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite, 1024, useAsync: true))
            {
                await ss.WriteAsync(encode, 0, encode.Length);
            }
        }
        private static async Task<string> ReadAsync(string path)
        {
            using (FileStream ss = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1024, useAsync: true))
            {
                StringBuilder sb = new StringBuilder();
                byte[] buffer = new byte[10240];
                int readnum;
                while ((readnum =await ss.ReadAsync(buffer, 0, buffer.Length))!=0)
                {
                    sb.Append(Encoding.UTF8.GetString(buffer,0,readnum));
                }
                return sb.ToString();
            }
        }


    }
}
