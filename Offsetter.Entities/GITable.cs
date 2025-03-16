using Offsetter.Math;
using System.Collections.Generic;
using System.Diagnostics;

namespace Offsetter.Entities  // TODO: This class doesn't really belong to GEntities.
{
    public class IntList : List<GSubchn>
    {
        void Dump()
        {
            for (int indx = 0; indx < Count; ++indx)
            {
                GSubchn item = this[indx];
                Debug.Write(string.Format("{0} ({1})", item.id, item.iindex));
                if (indx < Count - 1)
                    Debug.Write(", ");
            }
            Debug.WriteLine("");
        }
    }

    public class GITable : Dictionary<string, IntList>
    {
        public GITable() { IsDirty = false; }

        public bool IsDirty { get; set; }

        public void Record(GSubchn subchn)
        {
            if (subchn == null)
                return;

            string key = string.Format("{0:0.000000},{1:0.000000}", subchn.ipt.x, subchn.ipt.y);
            IntList list = (this.ContainsKey(key) ? this[key] : new IntList());

            if (!list.Exists(x => x.id == subchn.id))
                list.Add(subchn);

            this[key] = list;

            IsDirty = true;
        }

        public IntList Value(GPoint pt)
        {
            string key = string.Format("{0:0.000000},{1:0.000000}", pt.x, pt.y);
            return (this.ContainsKey(key) ? this[key] : null);
        }

        public GSubchn Sister(GChain parent, GSubchn subchn)
        {
            GSubchn next = parent.NextSubchain(subchn);
            if (next == parent.TerminalSubchain())
                next = parent.FirstSubchain();

            GLogger.Assert((next != null), "GITable.Sister() - subchn not in parent.");
            if (next == null)
                return null;

            IntList list = Value(next.ipt);
            if (list == null)
                return null;

            if (list.Count == 3)
            {
                // Special case where the tool fits down a blind slot.
                // file:///C:/_sandbox/Offsetter/docs/SpecialCaseSlot.png
                for (int jndx = 0; jndx < 3; ++jndx)
                {
                    GSubchn candidate = list[jndx];
                    if ((candidate.id != subchn.id) && (candidate.id != next.id) && (candidate.iindex == subchn.iindex))
                        return candidate;
                }

                // GLogger.Assert(false, "GITable.Sister() - list count == 3.");
                return null;
            }

            GLogger.Assert((list.Count == 2), "GITable.Sister() - list count != 2.");
            if (list.Count != 2)
                return null;

            int indx = list.FindIndex(x => x.id == next.id);
            if (indx < 0)
            {
                if (next == parent.TerminalSubchain())
                {
                    // ASSUMPTION: chains are closed.
                    indx = list.FindIndex(x => x.id == parent.FirstSubchain().id);
                    GLogger.Assert((indx >= 0), "GITable.Sister() - #3");
                    if (indx < 0)
                        return null;
                }
            }

            GSubchn sister = ((indx == 0) ? list[1] : list[0]);
            return ((sister.IsValid && (sister.iindex == subchn.iindex)) ? sister : null);
        }

        public GSubchn NextMinSubchain(GSubchn curr)
        {
            int iindex = curr.iindex;

            GCurve next = curr.NextCurve();
            if (next == null)
                return null;  // ASSUMPTION: We've encountered the terminal subchain.

            if (!next.IsA(T.SUBCHN))
                GLogger.Assert(false, "GITable.NextMinSubchain() - The subchain elements of 'curr' should have been moved to another chain.");

            IntList list = Value(next.ps);
            if (list == null)
                return null;

            foreach (GSubchn subchn in list)
            {
                if ((subchn != next) && (subchn.iindex == iindex))
                    return subchn;
            }

            return null;
        }

        public void Dump()
        {
            List<string> log = new List<string>();
            Log(log);

            foreach (string str in log)
            { Debug.WriteLine(str); }
        }

        public void Log(List<string>? log)
        {
            string header = "=-=-=-= itable =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=";
            if (GLogger.Active)
                GLogger.Log(header);
            else
                log.Add(header);

            foreach (var entry in this)
            {
                string tmp = ("{" + entry.Key + "} ");
                for (int indx = 0; indx < entry.Value.Count; ++indx)
                {
                    if (indx > 0)
                        tmp += (", ");

                    tmp += string.Format("{0}", entry.Value[indx].id);
                }

                if (GLogger.Active)
                    GLogger.Log(tmp);
                else
                    log.Add(tmp);
            }

            if (GLogger.Active)
                GLogger.Log("");
            else
                log.Add("");
        }
    }
}
