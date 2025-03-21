
namespace Offsetter.Math
{
    public class GVec
    {
        public GVec(double dx, double dy)
        {
            this.x = dx;
            this.y = dy;
        }

        public GVec(GPoint ps, GPoint pe)
            : this((pe.x - ps.x), (pe.y - ps.y))
        {
        }

        public GVec(double radians)
            : this(System.Math.Cos(radians), System.Math.Sin(radians))
        {
            Normalize();
        }

        public static GVec UnitVec(double dx, double dy, bool normalize)
        {
            GVec vec = new GVec(dx, dy);

            vec.Unitize();

            if (normalize)
            {
                if (System.Math.Abs(vec.x) == 1)
                    vec.y = 0;

                else if (System.Math.Abs(vec.y) == 1)
                    vec.x = 0;
            }

            return vec;
        }

        public static GVec UnitVec(GPoint ps, GPoint pe)
        {
            return UnitVec((pe.x - ps.x), (pe.y - ps.y));
        }

        public double x { get; set; }
        public double y { get; set; }

        public double len
        {
            get { return System.Math.Sqrt(x * x + y * y); }
            private set { }
        }

        public void Unitize()
        {
            double len = this.len;
            if (len > 0)
            {
                this.x /= len;
                this.y /= len;
            }
        }

        public void Normalize()
        {
            if (System.Math.Abs(this.x) == 1)
                this.y = 0;
            else if (System.Math.Abs(this.y) == 1)
                this.x = 0;
        }

        public GVec UnitVec()
        {
            return UnitVec(this.x, this.y);
        }

        public static GVec UnitVec(double dx, double dy)
        {
            double dist = System.Math.Sqrt(dx * dx + dy * dy);
            if (dist < GConst.SMALL)
                return null!;

            return new GVec(dx / dist, dy / dist);
        }

        /// <summary>Return a scaled vector.</summary>
        public static GVec operator *(GVec vec, double len)
        {
            // if (System.Math.Abs(len) < GConst.SMALL)
            //    return null;

            return new GVec(vec.x* len, vec.y * len);
        }

        /// <summary>Perp dot product.</summary>
        public static double operator *(GVec vecA, GVec vecB)
        {
            return ((vecA.x * vecB.x) + (vecA.y * vecB.y));
        }

        /// <summary>Cross product.</summary>
        public static double operator ^(GVec vecA, GVec vecB)
        {
            return ((vecA.x * vecB.y) - (vecA.y * vecB.x));
        }

        /// <summary>Return a rotated vector.</summary>
        public static GVec operator +(GVec vec, double radians)
        {
            double ca = System.Math.Cos(radians);
            double sa = System.Math.Sin(radians);
            GVec uvec = new GVec(ca*vec.x - sa*vec.y, sa*vec.x + ca*vec.y);

            uvec.Normalize();
            return uvec;
        }

        public static GVec operator +(GVec vecA, GVec vecB)
        {
            return new GVec(vecA.x + vecB.x, vecA.y + vecB.y);
        }

        public static GVec operator -(GVec vecA, GVec vecB)
        {
            return new GVec(vecA.x - vecB.x, vecA.y - vecB.y);
        }

        /// <summary>Be careful to use a unit vector (as necessary).</summary>
        public double Radians()
        {
            return GProperty.Radians(x, y);
        }

        public bool IsValid
        {
            // At least one component must be non-zero.
            get
            {
                if (System.Math.Abs(x) > 0)
                    return true;

                if (System.Math.Abs(y) > 0)
                    return true;

                return false;
            }
        }
    }
}
