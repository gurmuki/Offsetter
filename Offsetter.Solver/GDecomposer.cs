using Offsetter.Entities;
using Offsetter.Math;
using System.Collections.Generic;

namespace Offsetter.Solver
{
    using GCurveList = List<GCurve>;
    using GChainList = List<GChain>;

    public class GDecomposer
    {
        public GDecomposer() { }

        public void Decompose(GChain shape, GChainList results)
        {
            GCurveList curves = shape.ToList();

            int count = curves.Count;
            if (count < 3)
                return;

            for (int indx = 0; indx < count; ++indx)
            {
                GPoint pt = curves[indx].ps;

                for (int jndx = 0; jndx < count; ++jndx)
                {
                    if ((jndx >= indx - 1) && (jndx <= indx + 1))
                        continue;

                    Closest closest = curves[jndx].Closest(pt)!;
                    if ((closest.Uparam <= 0) || (closest.Uparam >= 1))
                        continue;

                    GPoint nearest = closest.Point;
                    string key = string.Format("{0:0.000000},{1:0.000000}", nearest.x, nearest.y);
                    if (solns.ContainsKey(key))
                    {
                        if (closest.Distance < solns[key].Dist)
                            solns[key] = new CPair(jndx, indx, closest.Distance, closest.Point);
                    }
                    else
                    {
                        solns[key] = new CPair(jndx, indx, closest.Distance, closest.Point);
                    }
                }
            }

            foreach (var item in solns)
            {
                GChain chain = new GChain(GChainType.UNKNOWN);
                chain.Append(GLine.LineCreate(curves[item.Value.PointIndex].ps, item.Value.ClosestPt));
                results.Add(chain);
            }
        }

        private GCurveList curves = new GCurveList();
        private Dictionary<string, CPair> solns = new Dictionary<string, CPair>();
    }

    internal class CPair
    {
        public CPair(int curveIndex, int pointIndex, double dist, GPoint closestPt)
        {
            CurveIndex = curveIndex;
            PointIndex = pointIndex;
            Dist = dist;
            ClosestPt = closestPt;
        }

        public int CurveIndex { get; private set; }
        public int PointIndex { get; private set; }
        public double Dist { get; private set; }
        public GPoint ClosestPt { get; private set; }
    }
}
