using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PublicTransportSimulator
{
    internal class BusStop
    {
        public int ID { get; set; }
        public double weight { get; set; }
        public string name { get; set; }
        public List<int> adjacentIdList { get; set; }
        public List<int> adjacentRoadsList { get; set; }
        public List<int> routeList { get; set; }
        public double coord_X { get; set; }
        public double coord_Y { get; set; }

        public BusStop()
        {
        }

        public BusStop(BusStop obj)
        {
            ID = obj.ID;
            weight = obj.weight;
            name = obj.name;
            adjacentIdList = obj.adjacentIdList;
            adjacentRoadsList = obj.adjacentRoadsList;
            routeList = obj.routeList;
            coord_X = obj.coord_X;
            coord_Y = obj.coord_Y;
        }
    }
}