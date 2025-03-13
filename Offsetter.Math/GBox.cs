
namespace Offsetter.Math
{
    public class GBox
    {
        public GBox()
        {
            Invalidate();
        }

        public GBox(double xmin, double ymin, double xmax, double ymax)
        {
            this.xmin = xmin;
            this.ymin = ymin;
            this.xmax = xmax;
            this.ymax = ymax;
        }

        public GBox(GBox rhs)
        {
            xmin = rhs.xmin;
            ymin = rhs.ymin;
            xmax = rhs.xmax;
            ymax = rhs.ymax;
        }

        public GBox(GPoint ptA, GPoint ptB)
        {
            xmin = System.Math.Min(ptA.x, ptB.x);
            ymin = System.Math.Min(ptA.y, ptB.y);
            xmax = System.Math.Max(ptA.x, ptB.x);
            ymax = System.Math.Max(ptA.y, ptB.y);
        }

        public void Invalidate()
        {
            // Clearly invalid
            xmin = GConst.UNDEFINED;
            ymin = GConst.UNDEFINED;
            xmax = -GConst.UNDEFINED;
            ymax = -GConst.UNDEFINED;
        }

        public bool IsValid()
        {
            return ((xmin <= xmax) && (ymin <= ymax));
        }

        // TODO: Might want to check for overlap within some tolerance.
        public bool Overlaps(GBox rhs)
        {
            if (!this.IsValid() || !rhs.IsValid())
                return false;

            if (xmin >= rhs.xmax)
                return false;

            if (ymin >= rhs.ymax)
                return false;

            if (xmax <= rhs.xmin)
                return false;

            if (ymax <= rhs.ymin)
                return false;

            return true;
        }

        public bool Contains(GPoint pt)
        {
            bool contains = (GProperty.InRange(pt.x, xmin, xmax)
                && GProperty.InRange(pt.y, ymin, ymax));

            return contains;
        }

        public double xmin { get; set; }
        public double ymin { get; set; }
        public double xmax { get; set; }
        public double ymax { get; set; }

        public double dx { get { return xmax - xmin; } }
        public double dy { get { return ymax - ymin; } }
        public double area { get { return dx * dy; } }

        public double xc { get { return (xmin + xmax) / 2; } }
        public double yc { get { return (ymin + ymax) / 2; } }
        public GPoint pc { get { return new GPoint(xc, yc); } }

        // Union
        static public GBox operator+(GBox lhs, GBox rhs)
        {
            return new GBox(
                System.Math.Min(lhs.xmin, rhs.xmin),
                System.Math.Min(lhs.ymin, rhs.ymin),
                System.Math.Max(lhs.xmax, rhs.xmax),
                System.Math.Max(lhs.ymax, rhs.ymax));
        }

        static public GBox operator +(GBox lhs, GPoint pt)
        {
            return new GBox(
                System.Math.Min(lhs.xmin, pt.x),
                System.Math.Min(lhs.ymin, pt.y),
                System.Math.Max(lhs.xmax, pt.x),
                System.Math.Max(lhs.ymax, pt.y));
        }

        public GBox Resize(double delta)
        {
            GBox resized = new GBox(this);

            double ds = ((this.dx > this.dy) ? this.dx : this.dy) * (delta - 1.0);
            resized.xmin -= ds;
            resized.ymin -= ds;
            resized.xmax += ds;
            resized.ymax += ds;

            return resized;
        }

        public GBox ScaleAbout(double xp, double yp, double scale)
        {
            GBox scaled = new GBox(this);

            scaled.Shift(-xp, -yp);

            scaled.xmin *= scale;
            scaled.ymin *= scale;
            scaled.xmax *= scale;
            scaled.ymax *= scale;

            scaled.Shift(xp, yp);

            return scaled;
        }

        public void MoveTo(GPoint pt)
        {
            GVec delta = pt - pc;
            Shift(delta.x, delta.y);
        }

        private void Shift(double dx, double dy)
        {
            xmin += dx;
            ymin += dy;
            xmax += dx;
            ymax += dy;
        }
    }
}
