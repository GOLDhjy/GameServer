using System;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using LogSystem;
using static LogSystem.LogSystem;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace server_socket
{
/// <summary>
/// 用来存储发送和接收。
/// </summary>
    public class StateObject
    {
        public Socket worksocket=null;
        public const int buffersize = 1024;
        public byte[] buffer = new byte[buffersize];
        public StringBuilder sb = new StringBuilder();
    }

    public class AsynchronousSocketListener
    {
        const string path_rank = "rank.json";
        const string path_login = "login.json";
        static long counter = 0;//已经注册的

        public static ManualResetEvent alldone = new ManualResetEvent(false);
    
        static void Main(string[] args)
        {
            StartListener();
            return;

        }

        public static void StartListener()
        {
            //IPHostEntry iphostinfo = Dns.GetHostEntry("localhost");
            //string host = "120.78.135.242";//IP地址
            int port = 2000;//端口
            //IPAddress ip=IPAddress.Parse(host);
            IPAddress ip = IPAddress.Any;
            IPEndPoint ED=new IPEndPoint(ip,port);
            Socket listener = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                listener.Bind(ED);
                listener.Listen(100);
                while(true)
                {
                    alldone.Reset();
                    Console.WriteLine("wait ...connect");
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                    alldone.WaitOne();//   listener: 包含此请求的状态信息的对象。
                }
            }
            catch(Exception e)
            {
                LOG(NOW, 2, e.Message, SERVER);
            }

        }


        public static void Send(Socket handler,string data)
        {
            StateObject state = new StateObject();
            state.worksocket = handler;
            byte[] bytedata = Encoding.UTF8.GetBytes(data);
            Array.Copy(bytedata, state.buffer, bytedata.Length);
            handler.BeginSend(bytedata, 0, bytedata.Length, 0, new AsyncCallback(SendCallback), state);

        }
        public static void SendCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;

            Socket hanlder = (Socket)state.worksocket;
            try
            {
                int sendnum = hanlder.EndSend(ar);
                if(sendnum>0)
                    LOG(NOW, 0, $"send {sendnum} byte info to ->", hanlder.RemoteEndPoint.ToString());
                else
                {
                    hanlder.BeginSend(state.buffer, 0, state.buffer.Length, 0, new AsyncCallback(SendCallback), state);
                }
            }
            catch(Exception e)
            {
                LOG(NOW, 2, "send a message is fail... to", hanlder.RemoteEndPoint.ToString());
            }
        }


        public static void AcceptCallback(IAsyncResult ar)
        {
            alldone.Set();
            Socket listener = (Socket)ar.AsyncState;//   AsyncState  一个用户定义的对象，限定或包含有关异步操作的信息。
            Socket handler = listener.EndAccept(ar);
            LOG(NOW, 0, "connection...success",handler.RemoteEndPoint.ToString());
            //while (true)
            //{
                Console.WriteLine("wait ...");
                StateObject state = new StateObject();
                state.worksocket = handler;
                handler.BeginReceive(state.buffer, 0, StateObject.buffersize, 0, new AsyncCallback(ReceiveCallback), state);
            //}
    }
        public static void ReceiveCallback(IAsyncResult ar)
        {

            string content = string.Empty;
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.worksocket;
            try
            {
                int receivenum = handler.EndReceive(ar);

                if (receivenum > 0)
                {
                    state.sb.Append(Encoding.UTF8.GetString(state.buffer, 0, receivenum));
                    Processrequest(handler, receivenum, state.sb);

                    StateObject statec = new StateObject();
                    statec.worksocket = state.worksocket;
                    handler.BeginReceive(statec.buffer, 0, StateObject.buffersize, 0, new AsyncCallback(ReceiveCallback), statec);
                    LOG(NOW, 0, $"receive {receivenum} byte :{content} from", handler.RemoteEndPoint.ToString());
                }
                else
                {
                    handler.Close();
                    //handler.Shutdown(SocketShutdown.Both);
                    LOG(NOW, 2, $" remotesocket has close", "...");
                    // handler.BeginReceive(state.buffer, 0, StateObject.buffersize, 0, new AsyncCallback(ReceiveCallback), state);
                }
            }
            catch (ObjectDisposedException e)
            {
                handler.Close();
                LOG(NOW, 2, $"{e.Message} remotesocket has close", "...");
            }

        }
    



        public static async void Processrequest(Socket handler, int receiveNumber, StringBuilder content)
        {
            char c = content[receiveNumber-1];
            content = content.Remove(receiveNumber - 1,1);
            //string jsonwrite = JsonConvert.SerializeObject(content);
            //byte[] bytecont= Encoding.UTF8.GetBytes(jsonwrite);
            byte[] byteread = new byte[1024];
            switch(c)
            {
                case '1'://注册
                    try
                    {
                        await FileOperate.WriteAsync(path_login, content.ToString()+'\"',false);
                        LOG(NOW, 0, "registration success", handler.RemoteEndPoint.ToString());
                        send_true(handler);
                    }
                    catch(Exception e)
                    {
                        LOG(NOW, 0, $"registration failed {e.Message}", handler.RemoteEndPoint.ToString());
                        Console.WriteLine(e.Message);
                    };
                    break;
                case '2'://登录
                    StringBuilder jsonread = await FileOperate.ReadAsync(path_login);
                    
                    string[] tmp=jsonread.ToString().Split('\"');
                    if(CheckeKey(tmp, content))
                    {
                        LOG(NOW, 0, "log in success", handler.RemoteEndPoint.ToString());
                        send_true(handler);
                    }
                    else
                    {
                        LOG(NOW, 0, "log in failed", handler.RemoteEndPoint.ToString());
                        send_false(handler);
                    }
                    break;
                case '3'://关闭socket
                    try
                    {
                        LOG(NOW, 0, "close socket from:", handler.RemoteEndPoint.ToString());//关闭Socket并释放资源
                        Console.WriteLine("连接已关闭");
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine("关闭失败");
                        LOG(NOW, 2, "close fail", handler.RemoteEndPoint.ToString());
                    }
                    break;
                case '4'://发送排行榜
                    try
                    {
                        StringBuilder jsonread2 = await FileOperate.ReadAsync(path_rank);
                        if (jsonread2.ToString() == "")
                        {
                            Send(handler, "null");
                            LOG(NOW, 2, "ranklist is empty at", SERVER);
                        }
                        else
                        {
                            mydata.md = JsonConvert.DeserializeObject<mydata>(jsonread2.ToString());
                            Send(handler, JsonConvert.SerializeObject(mydata.md));
                            LOG(NOW, 0, "send a ranklist to", handler.RemoteEndPoint.ToString());
                        }
                    }
                    catch(Exception e)
                    {
                        LOG(NOW, 2, $"{e.Message} fail to send ranklist to  ", handler.RemoteEndPoint.ToString());
                    }
                    break;
                case '5'://上传排行榜
                    try
                    {
                        StringBuilder jsonread3 = await FileOperate.ReadAsync(path_rank);
                        if (JsonConvert.DeserializeObject<mydata>(jsonread3.ToString()) != null)
                            mydata.md = JsonConvert.DeserializeObject<mydata>(jsonread3.ToString());
                        mydata tmpdata = JsonConvert.DeserializeObject<mydata>(content.ToString());

                        mydata.md.myrank.Add(tmpdata.uprank);
                        string json = JsonConvert.SerializeObject(mydata.md);
                        await FileOperate.WriteAsync(path_rank, json,true);
                        LOG(NOW, 0, "upload a rank_info from ", handler.RemoteEndPoint.ToString());
                    }
                    catch(Exception e)
                    {
                        LOG(NOW, 2, $"{e.Message} fail upload a rank_info from", handler.RemoteEndPoint.ToString());
                    }
                    break;
            }
        }




        public static bool CheckeKey(string[] tmp, StringBuilder content)
        {
            for(int i=0;i<tmp.Length;i++)
            {
                if (tmp[i] == content.ToString())
                    return true;
            }
            return false;
        }
        private static void send_true(Socket s)
        {
            string t = "true";
            byte[] tt = new byte[1024];
            tt = Encoding.UTF8.GetBytes(t);
            s.Send(tt);
        }
        private static void send_false(Socket s)
        {
            string t = "false";
            byte[] tt = new byte[1024];
            tt = Encoding.UTF8.GetBytes(t);
            s.Send(tt);
        }

    }




    //    class Program
    //    {
    //    const string path_rank = @"D:\rank.txt";
    //    const string path_login = @"D:\login.txt";
    //    static long counter = 0;//已经注册的



    //    //创建套接字  
    //    static Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


    //    public static void SocketServie()
    //    {
    //        Console.WriteLine("服务端已启动");

    //        Thread myThread = new Thread(ListenClientConnect);//通过多线程监听客户端连接  
    //        myThread.Start();
    //        Console.ReadLine();
    //    }
    //    // 监听客户端连接  
    //    private static void ListenClientConnect()
    //    {
    //        while (true)
    //        {
    //            Socket clientSocket = socket.Accept();//accept会创建一个新的socket为新创建的连接
    //            Thread receiveThread = new Thread(ReceiveMessage);
    //            receiveThread.Start(clientSocket);
    //        }
    //    }
    //    // 接收消息  

    //    private static void ReceiveMessage(object clientSocket)
    //    {

    //        Socket myClientSocket = (Socket)clientSocket;

    //        while (true)
    //        {
    //            byte[] result = new byte[1024];//接收的地方
    //            byte[] result2 = new byte[1024];//读入文件的地方
    //            try
    //            {
    //                //通过clientSocket接收数据  
    //                int receiveNumber = myClientSocket.Receive(result);//会返回收到的字节数
    //                if (receiveNumber == 0)
    //                {
    //                    Console.WriteLine("没收到信息");
    //                    continue;
    //                }
    //                else
    //                {
    //                    Console.WriteLine(result.Length);
    //                    Console.WriteLine(Encoding.UTF8.GetString(result));
    //                }

    //               string s = Encoding.UTF8.GetString(result);
    //                //Console.WriteLine(s[receiveNumber-1]);

    //                if(s[receiveNumber-1]=='2')//注册口令
    //                {
    //                    if (!File.Exists(path_login))
    //                    {
    //                        // Create a file to write to.
    //                        using (FileStream ft = new FileStream(path_login, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
    //                        {
    //                            ft.Write(result, 0, receiveNumber-1);
    //                            ft.Close();
    //                        }

    //                    }
    //                    else
    //                    {

    //                        using (FileStream ft = File.Open(path_login, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
    //                        {
    //                            ft.Write(result, 0, receiveNumber-1);
    //                            ft.Close();
    //                        }
    //                    }
    //                    counter++;
    //                    send_true(myClientSocket);
    //                    continue;
    //                }
    //                else if (s[receiveNumber-1]=='1')//验证口令
    //                {
    //                    byte[] b = new byte[1024];
    //                    using (FileStream ft = new FileStream(path_login, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
    //                    {
    //                        while (ft.Read(result2, 0, result2.Length) > 0);//读取到后文件末尾回返回0
    //                        { }
    //                        ft.Close();
    //                    }
    //                    string bb;
    //                    int i;
    //                    for (i = 0; i < counter; i++)
    //                    {
    //                         Array.Copy(result2, i * 6, b, 0, 6);
    //                         bb=Encoding.UTF8.GetString(b);
    //                         bb = bb.Substring(0,6);
    //                       if (bb == s.Substring(0,6))
    //                        {
    //                            send_true(myClientSocket);
    //                            break;
    //                        }
    //                    }
    //                    if (i == counter)
    //                        send_false(myClientSocket);
    //                    //byte[] tmp ={0};
    //                    //myClientSocket.Send(tmp);
    //                    continue;
    //                }

    //                else if (s[receiveNumber - 1] == '3')//关闭套接字
    //                {
    //                    myClientSocket.Shutdown(SocketShutdown.Both);//禁止发送和上传
    //                    myClientSocket.Close();//关闭Socket并释放资源
    //                    Console.WriteLine("连接已关闭");
    //                    break;
    //                }

    //                else if(s[receiveNumber-1]=='4')//发送排行榜
    //                {
    //                    if (!File.Exists(path_rank))
    //                    {
    //                        using (FileStream ft = new FileStream(path_rank, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
    //                        {
    //                            ft.Write(result, 0, 0);
    //                            ft.Close();
    //                        }
    //                    }
    //                        using (FileStream ft = new FileStream(path_rank, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
    //                        {
    //                            while (ft.Read(result2, 0, result2.Length) > 0) ;//读取到后文件末尾回返回0
    //                            { }
    //                            ft.Close();
    //                        }
    //                        try
    //                        {

    //                            myClientSocket.Send(result2);  //返回信息给客户端
    //                            Console.WriteLine("发送成功");
    //                        }
    //                        catch (Exception ex)
    //                        {
    //                            Console.WriteLine("发送失败");
    //                            Console.WriteLine(ex.Message);
    //                        }

    //                        continue;
    //                }
    //                else if (s[receiveNumber - 1] == '5')//写入排行榜
    //                {
    //                    if (!File.Exists(path_rank))
    //                    {
    //                        // Create a file to write to.
    //                        using (FileStream ft = new FileStream(path_rank, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
    //                        {
    //                            ft.Write(result, 0, receiveNumber - 1);
    //                            ft.Close();
    //                        }

    //                    }
    //                    else
    //                    {

    //                        using (FileStream ft = File.Open(path_rank, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
    //                        {
    //                            ft.Write(result, 0, receiveNumber - 1);
    //                            ft.Close();
    //                        }
    //                    }

    //                    continue;
    //                }

    //                Console.WriteLine("接收客户端{0} 的消息：{1}", myClientSocket.RemoteEndPoint.ToString(), Encoding.UTF8.GetString(result, 0, receiveNumber));

    //            }
    //            catch (Exception ex)
    //            {
    //                Console.WriteLine(ex.Message);
    //            }    
    //        }
    //    }
    //        //private static void send_rank(object myclientsocket)
    //        //{
    //        //    Socket myclient_socket = (Socket)myclientsocket;
    //        //    try
    //        //    {
    //        //        myclient_socket.Send(result2);  //返回信息给客户端
    //        //        Console.WriteLine("发送成功");
    //        //    }
    //        //    catch (Exception ex)
    //        //    {
    //        //        Console.WriteLine("发送失败");
    //        //        Console.WriteLine(ex.Message);
    //        //    }
    //        //}
    //        private static void send_true(Socket s)
    //        {
    //            string t = "true";
    //            byte[] tt=new byte[1024];
    //            tt=Encoding.UTF8.GetBytes(t);
    //            s.Send(tt);
    //        }
    //        private static void send_false(Socket s)
    //        {
    //            string t = "false";
    //            byte[] tt = new byte[1024];
    //            tt = Encoding.UTF8.GetBytes(t);
    //            s.Send(tt);
    //        }


    //    }
}
