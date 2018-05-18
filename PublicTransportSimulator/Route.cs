using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PublicTransportSimulator
{
    class Route
    {
        public int ID { get; set; }
        public List<int> way { get; set; }

        public Route()
        {

        }

        public Route(Route obj)
        {
            ID = obj.ID;
            way = obj.way;
        }
    }
}
