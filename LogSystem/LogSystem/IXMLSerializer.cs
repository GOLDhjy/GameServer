using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace LogSystem
{
    interface IXMLSerializer
    {
        void Serializer(string content);
        void DSerializer(out FileStream fs);
    }
}
