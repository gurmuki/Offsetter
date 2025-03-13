using Offsetter.Math;
using System.Collections.Generic;

namespace Offsetter.Entities
{
    // public struct GIntersection
    public class GIntersection
    {
        public GIntersection(GPoint ipt, double uparamA, double uparamB, bool isTangent)
        {
            pt.x = ipt.x;
            pt.y = ipt.y;
            uA = uparamA;
            uB = uparamB;
            tangent = isTangent;
        }

        public GPoint pt { get; private set; } = new GPoint(GPoint.UNDEFINED);
        public double uA { get; private set; }
        public double uB { get; private set; }
        public bool tangent { get; private set; }

        public void UparamsSwap()
        {
            double temp = uA;
            uA = uB;
            uB = temp;
        }
    }

    public class GIntersections : List<GIntersection> { }

    //=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=

    public class GIntersect
    {
        public static GIntersections CurveCurve(GCurve curveA, GCurve curveB, bool contiguous, bool onSegment)
        {
            GIntersections results = new GIntersections();

            if (contiguous)
            {
                if (curveA.pe == curveB.ps)
                    return results;
            }

            if (curveA.IsA(T.LINE))
            {
                if (curveB.IsA(T.LINE))
                    return LineLine((GLine)curveA, (GLine)curveB, onSegment);
                else if (curveB.IsA(T.ARC))
                    return LineArc((GLine)curveA, (GArc)curveB, onSegment);
            }
            else if (curveA.IsA(T.ARC))
            {
                if (curveB.IsA(T.LINE))
                    return ArcLine((GArc)curveA, (GLine)curveB, onSegment);
                else if (curveB.IsA(T.ARC))
                    return ArcArc((GArc)curveA, (GArc)curveB, onSegment, GConst.SMALL);
            }

            return results;
        }

        // Based on fab Int2dSegSeg()
        public static GIntersections LineLine(GLine lineA, GLine lineB, bool onSegment)
        {
            GIntersections results = new GIntersections();

            GPoint psA = lineA.ps;
            GPoint peA = lineA.pe;
            GPoint psB = lineB.ps;
            GPoint peB = lineB.pe;

            GVec vecA = new GVec(psA, peA);
            GVec vecB = new GVec(psB, peB);

            double det = (vecA.x * vecB.y) - (vecA.y * vecB.x);
            if (System.Math.Abs(det) < GConst.VECTOR_SMALL)
                return results;

            double numerator = vecB.x * (psA.y - psB.y) - vecB.y * (psA.x - psB.x);
            double numeratorB = vecA.x * (psB.y - psA.y) - vecA.y * (psB.x - psA.x);

            double uparam = numerator / det;
            double uparamB = -numeratorB / det;

            GPoint ipt = psA + (vecA * uparam)!;

            bool success = true;
            if (onSegment)
            {
                success = ((uparam > -GConst.VECTOR_SMALL && uparam < (1.0 + GConst.VECTOR_SMALL)) &&
                        (uparamB > -GConst.VECTOR_SMALL && uparamB < (1.0 + GConst.VECTOR_SMALL)));
            }

            if (success)
            {
                results.Add(new GIntersection(ipt, uparam, uparamB, false));
            }

            return results;
        }

        public static GIntersections LineArc(GLine lineA, GArc arcB, bool onSegment)
        {
            GIntersections results = new GIntersections();

            double radius = arcB.rad;

            double dist, u, v;
            Closest closest = lineA.Closest(arcB.pc);
            if (closest.Distance >= (radius + GConst.SMALL))
                return results;

            GPoint[] soln;
            bool tangent = false;
            if (System.Math.Abs(radius - closest.Distance) < GConst.SMALL)
            {
                tangent = true;

                soln = new GPoint[1];
                soln[0] = closest.Point;
            }
            else
            {
                double len = (radius * radius) - (closest.Distance * closest.Distance);
                len = ((len < GConst.SMALL_SQRD) ? 0 : System.Math.Sqrt(len));

                soln = new GPoint[2];
                GVec vec = lineA.TangentAt(0);
                soln[0] = closest.Point + (vec * len)!;
                soln[1] = closest.Point + (vec * -len)!;
            }

            foreach (GPoint ipt in soln)
            {
                Closest closestA = lineA.Closest(ipt);
                Closest closestB = arcB.Closest(ipt);

                if (GProperty.InRange(closestA.Uparam, 0, 1) && GProperty.InRange(closestB.Uparam, 0, 1))
                    results.Add(new GIntersection(ipt, closestA.Uparam, closestB.Uparam, tangent));
            }

            return results;
        }

        public static GIntersections ArcLine(GArc arcA, GLine lineB, bool onSegment)
        {
            GIntersections results = LineArc(lineB, arcA, onSegment);
            for(int indx = 0; indx < results.Count; ++indx)
            {
                results[indx].UparamsSwap();
                GPoint p0 = arcA.PointAtUparam(results[indx].uA);
                GPoint p1 = arcA.PointAtUparam(results[indx].uB);
                GPoint p2 = lineB.PointAtUparam(results[indx].uA);
                GPoint p3 = lineB.PointAtUparam(results[indx].uB);
            }

            return results;
        }

        // Based upon fab's -- void Int2dArcArc()
        public static GIntersections ArcArc(GArc arcA, GArc arcB, bool onSegment, double tangentTol )
        {
            GIntersections results = new GIntersections();

			double radA = arcA.rad;
			double radB = arcB.rad;
			if ((radA < GConst.SMALL) || (radB < GConst.SMALL))
				return results;  // trivial rejection

            GPoint pcA = arcA.pc;
			GPoint pcB = arcB.pc;
			GVec vec = pcB - pcA;
			double centerDist = vec.len;
			if (centerDist < GConst.SMALL)
				return results;  // trivial rejection (coincident centers)

			double delta = centerDist - (radA + radB);

            if (delta >= GConst.SMALL)
				return results;  // trivial rejection (externally separated)

            vec = vec.UnitVec()!;

            double dist, u, v;
            Closest closestA = new NotClosest();
            Closest closestB = new NotClosest();

            if (System.Math.Abs(delta) < tangentTol)
            {
                // Externally tangent.
                GPoint ipt = pcA + (vec! * radA)!;

                closestA = arcA.Closest(ipt);
                closestB = arcB.Closest(ipt);

                if (GProperty.InRange(closestB.Uparam, 0, 1) && GProperty.InRange(closestA.Uparam, 0, 1))
                    results.Add(new GIntersection(ipt, closestB.Uparam, closestA.Uparam, true));
            }
            else
            {
                if (radB > radA)
                    delta = radB - (centerDist + radA);
                else
                    delta = radA - (centerDist + radB);

                if (System.Math.Abs(delta) < tangentTol)
                {
                    // Internally tangent.
                    GPoint ipt = pcA + (vec! * radA)!;

                    closestA = arcA.Closest(ipt);
                    closestB = arcB.Closest(ipt);

                    if (GProperty.InRange(closestB.Uparam, 0, 1) && GProperty.InRange(closestA.Uparam, 0, 1))
                        results.Add(new GIntersection(ipt, closestB.Uparam, closestA.Uparam, true));

                    return results;
                }
                else
                {
                    if (delta > tangentTol)
                        return results;  // trivial rejection (internally separated)
                }

                // Local "x" for int pts; along center's line
                // TODO: Is the result affected by the order in which operations
                // are applied to the terms, as a function of term magnitudes?
                double dx = 0.5 * ((centerDist * centerDist) + (radA * radA) - (radB * radB)) / centerDist;

                // Local "y" for int pts; along normal to center line
                double dy = (radA * radA) - (dx * dx);

                if (dy < 0)
                {
                    // Imaginary roots, no intersection.
                    // Should never get this far.
                }
                else
                {
                    dy = System.Math.Sqrt(dy);

                    double ix = pcA.x + (dx * vec.x) - (dy * vec.y);
                    double iy = pcA.y + (dx * vec.y) + (dy * vec.x);
                    GPoint ipt = new GPoint(ix, iy);

                    double jx = pcA.x + (dx * vec.x) + (dy * vec.y);
                    double jy = pcA.y + (dx * vec.y) - (dy * vec.x);
                    GPoint jpt = new GPoint(jx, jy);

                    bool tangent = ipt.WithinTol(jpt);

                    closestA = arcA.Closest(ipt);
                    closestB = arcB.Closest(ipt);

                    if (GProperty.InRange(closestA.Uparam, 0, 1) && GProperty.InRange(closestB.Uparam, 0, 1))
                        results.Add(new GIntersection(ipt, closestA.Uparam, closestB.Uparam, tangent));

                    if (!tangent)
                    {
                        closestA = arcA.Closest(jpt);
                        closestB = arcB.Closest(jpt);

                        if (GProperty.InRange(closestA.Uparam, 0, 1) && GProperty.InRange(closestB.Uparam, 0, 1))
                            results.Add(new GIntersection(jpt, closestA.Uparam, closestB.Uparam, tangent));
                    }
                }
            }

            return results;
        }
    }
}
