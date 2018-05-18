using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PublicTransportSimulator
{
    internal class PublicTransport
    {
        public int ID { get; set; }
        public string transportId { get; set; }
        public string transportType { get; set; }
        public int last_stop { get;set; }
        public int next_stop { get; set; }
        public double progress { get; set; }

        public PublicTransport()
        {

        }

        public PublicTransport(PublicTransport obj)
        {
            ID = obj.ID;
            transportId = obj.transportId;
            transportType = obj.transportType;
            last_stop = obj.last_stop;
            next_stop = obj.next_stop;
            progress = obj.progress;
        }
    }
}