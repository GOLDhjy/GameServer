using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// json序列化与反序列化的存储
/// </summary>
namespace server_socket
{
    [Serializable]
    class mydata
    {
        public static mydata md = new mydata();
        public struct rank_struct
        {
            public string player_name;
            public string player_score;
        }
        public rank_struct uprank = new rank_struct();
        public List<rank_struct> myrank = new List<rank_struct>();
    }
}
