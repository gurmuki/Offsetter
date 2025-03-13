using Offsetter.Math;

namespace Offsetter.Entities
{
    public class Closest
    {
        public Closest(GCurve curve, GPoint pt, double dist, double uparam)
        {
            Curve = curve;
            Point = pt;
            Distance = dist;
            Uparam = uparam;
        }

        protected Closest() { }

        public GCurve Curve { get; private set; } = null;
        public GPoint Point { get; private set; } = GPoint.UNDEFINED;
        public double Distance { get; private set; } = double.MaxValue;
        public double Uparam { get; private set; } = double.MaxValue;
    }

    public class NotClosest : Closest
    {
        public NotClosest() : base() { }
    }
}
