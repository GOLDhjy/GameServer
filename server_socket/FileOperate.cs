using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
namespace server_socket
{
    public static class FileOperate
    {
        /// <summary>
        /// path-路径 text-内容   cover-是否覆盖原文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="text"></param>
        /// <param name="cover"></param>
        /// <returns></returns>
        public static async Task WriteAsync(string path, string text,bool cover)
        {
            byte[] encode = Encoding.UTF8.GetBytes(text);
            switch (cover)
            {
                case false:
                    
                    using (FileStream ss = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite, 1024, useAsync: true))
                    {
                        await ss.WriteAsync(encode, 0, encode.Length);
                    }
                    break;
                case true:
                    using (FileStream ss = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite, 1024, useAsync: true))
                    {
                        await ss.WriteAsync(encode, 0, encode.Length);
                    }
                    break;
            }
        }
        public static async Task<StringBuilder> ReadAsync(string path)
        {
            using (FileStream ss = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite, 1024, useAsync: true))
            {
                StringBuilder sb = new StringBuilder();
                byte[] buffer = new byte[10240];
                int readnum;
                while ((readnum = await ss.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    sb.Append(Encoding.UTF8.GetString(buffer, 0, readnum));
                }
                return sb;
            }
        }
    }
}
