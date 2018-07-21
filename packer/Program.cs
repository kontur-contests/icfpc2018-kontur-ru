using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Nest;

namespace packer
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new ElasticClient(new ConnectionSettings(new Uri("http://efk2-elasticsearch9200.efk2.10.217.14.7.xip.io")).DefaultIndex("testruns"));
            
        }
    }
}
