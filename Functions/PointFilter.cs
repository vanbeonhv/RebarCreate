using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tekla.Structures.ModelInternal;
using TSG = Tekla.Structures.Geometry3d;

namespace RebarCreate.Functions
{
    public class PointFilter
    {
        public PointFilter(List<TSG.Point> pointsList)
        {
            this._pointsList = pointsList;
        }

        private List<TSG.Point> _pointsList = new List<TSG.Point>();
        private List<double> pointValue = new List<double>();

        public double MaxX()
        {
            _pointsList.ForEach(point => pointValue.Add(point.X));
            return pointValue.Max();
        }

        public double MaxY()
        {
            _pointsList.ForEach(point => pointValue.Add(point.Y));
            return pointValue.Max();
        }

        public double MaxZ()
        {
            _pointsList.ForEach(point => pointValue.Add(point.Z));
            return pointValue.Max();
        }

        public double MinX()
        {
            _pointsList.ForEach(point => pointValue.Add(point.X));
            return pointValue.Min();
        }

        public double MinY()
        {
            _pointsList.ForEach(point => pointValue.Add(point.Y));
            return pointValue.Min();
        }

        public double MinZ()
        {
            _pointsList.ForEach(point => pointValue.Add(point.Z));
            return pointValue.Min();
        }
    }
}