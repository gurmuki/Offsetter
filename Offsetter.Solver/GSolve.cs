using Offsetter.Entities;
using Offsetter.Math;

namespace Offsetter.Solver
{
    public class GSolve
    {
        public static bool Intersects(GChain chainA, GChain chainB)
        {
            GLogger.Assert((chainA != chainB), "GSolver.Intersect(chainA, chainB)");
            if (chainA == chainB)
                return false;

            GCurve pcurve = chainA.FirstSubchain();
            while (pcurve != null)
            {
                if (pcurve == null)
                    break;

                if (pcurve.CanIgnore)
                {
                    if (pcurve == chainA.TerminalSubchain())
                        break;

                    pcurve = pcurve.NextCurve();
                    continue;
                }

                GCurve icurve = chainB.TerminalCurve();
                while (icurve != null)
                {
                    if (icurve.CanIgnore)
                    {
                        if (icurve == chainB.FirstSubchain())
                            break;

                        icurve = icurve.PrevCurve();  // we've encounter a blend or subchn
                        continue;
                    }

                    bool contiguous = false;
                    GIntersections results = GIntersect.CurveCurve(pcurve, icurve, contiguous, true);
                    if (results.Count > 0)
                        return true;

                    icurve = icurve.PrevCurve();
                }

                pcurve = pcurve.NextCurve();
            }

            return false;
        }
    }
}
