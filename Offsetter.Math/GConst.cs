using System;

namespace Offsetter.Math
{
    public static class GConst
    {
        public const int IUNDEFINED = int.MaxValue;

        public const double UNDEFINED = Double.MaxValue;
        public const double SMALL = 1e-6;
        public const double SMALL_SQRD = SMALL * SMALL;
        public const double VECTOR_SMALL = 1e-12;

        public const double PI = System.Math.PI;
        public const double HALF_PI = System.Math.PI / 2;
        public const double TWO_PI = System.Math.PI * 2;

        public const double DEG2RAD = System.Math.PI / 180.0;
        public const double RAD2DEG = 180.0 / System.Math.PI;

        public const int CW = -1;
        public const int NONE = 0;
        public const int CCW = 1;

        public const int LEFT = 1;    // interchangeable with CCW(?)
        public const int RIGHT = -1;  // interchangeable with CW(?)

        public const int UNIFORM = 1;
        public const int NONUNIFORM = 2;
        public const int NEST = 3;
    }

    public enum Layer
    {
        UNASSIGNED = 0,
        PART = 1,
        PATH = 2,
        TOOL = 3,
        INTERMEDIATE = 4
    }
}
