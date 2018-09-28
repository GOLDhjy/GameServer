using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogSystem
{

    class Message
    {

        enum ll:int
        {
            LOG_INFO,
            LOG_WARNING,
            LOG_ERR
        }
        enum place:int
        {
            client,
            server,
        }

        place pl;
        string _time;
        ll _level;
        string _done;
        string _msg;
        string _p1;
        string _p2;
        long amount;
        string result;

        public string Result { get => result; set => result = value; }

        public Message(string time, int level,string msg,string p1)
        {

            if (level == 0)
                _level = ll.LOG_INFO;
            else if (level == 1)
                _level = ll.LOG_WARNING;
            else if (level == 2)
                _level = ll.LOG_ERR;

            _time = time;
            _msg = msg;
            _p1 = p1;
            result = _time + ":" + _level.ToString() + ": " + _msg + "   " + _p1+"\r\n";
        }
        public Message(string time, int level, string msg, string p1, string done)
        {
            if (level == 0)
                _level = ll.LOG_INFO;
            else if (level == 1)
                _level = ll.LOG_WARNING;
            else if (level == 2)
                _level = ll.LOG_ERR;
            _time = time;
            _msg = msg;
            _p1 = p1;
            _done = done;
            result = _time + ":" + _level.ToString() + ": " + _msg + "   " + _p1 + "  "+_done+"\r\n";
        }
    }
}
