
namespace Offsetter.Math
{
    public class GProperty
    {
        public static double ArcLen(GPoint ps, GPoint pe)
        {
            double dx = pe.x - ps.x;
            double dy = pe.y - ps.y;
            return System.Math.Sqrt(dx * dx + dy * dy);
        }

        public static double ArcLen(GPoint ps, GPoint pe, GPoint pc, int dir)
        {
            double sRadians = Radians((ps.x - pc.x), (ps.y - pc.y));
            double eRadians = Radians((pe.x - pc.x), (pe.y - pc.y));
            double radius = ArcLen(ps, pc);
            return (radius * System.Math.Abs(eRadians - sRadians));
        }

        public static int Winding(double radians)
        {
            return  ((System.Math.Abs(radians) < GConst.SMALL) ? 0 : System.Math.Sign(radians));
        }

        /// <summary>Returns a direction in the range [0, 2PI)</summary>
        public static double Radians(double dx, double dy)
        {
            double dval = System.Math.Atan2(dy, dx);
            return ((dval < 0) ? (dval + GConst.TWO_PI) : dval);
        }

        public static bool InRange(double dval, double dmin, double dmax)
        {
            return ((dmin <= dval) && (dval <= dmax));
        }
    }
}