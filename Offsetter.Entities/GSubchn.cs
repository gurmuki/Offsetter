using Offsetter.Graphics;
using Offsetter.Math;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Offsetter.Entities
{
    public class GSubchn : GCurve
    {
        private GPoint intersection = null;

        public GSubchn(GPoint pt, GChain owner)
        {
            this.intersection = new GPoint(pt);
            this.Owner = owner;
        }

        public int iindex { get; set; } = GConst.IUNDEFINED;
        public bool IsAssigned() { return (iindex != GConst.IUNDEFINED); }

        public override GPoint ps
        {
            get { Debug.Assert(false); return ipt; }
            set { Debug.Assert(false); }
        }

        public override GPoint pe
        {
            get { Debug.Assert(false); return ipt; }
            set { Debug.Assert(false); }
        }

        public GPoint ipt
        {
            get { return intersection; }
            set { intersection.x = value.x; intersection.y = value.y; }
        }

        public GSubchn PrevSubchain()
        {
            GChain owner = this.Owner;
            if (owner == null)
                return null;

            GSubchn prev = owner.PrevSubchain(this);

            return prev;
        }


        public GSubchn NextSubchain()
        {
            GChain owner = this.Owner;
            if (owner == null)
                return null;

            GSubchn next = owner.NextSubchain(this);

            return next;
        }

        //=-=-=-= GCurve =-=-=-=

        public sealed override bool IsValid { get; set; } = true;

        public sealed override double area { get { return 0; } }

        public sealed override GBox box { get { return new GBox(); } }

        public sealed override GVec TangentAt(double uparam) { return new GVec(0, 0); }

        public sealed override Closest Closest(GPoint pt, double pickTol)
        {
            // throw new NotSupportedException();
            return new NotClosest();
        }

        public sealed override void Reverse() { }

        //=-=-=-= GCurve =-=-=-=

        public sealed override bool CanIgnore { get; } = true;
        public sealed override GCurve Clone(GPoint at) { return null; }
        public sealed override GCurve SplitAt(GPoint pt) { return null; }
        public sealed override GPoint PointAtUparam(double uparam) { return null; }
        public sealed override string CanonicalForm()
        {
            string istr = (IsAssigned() ? iindex.ToString() : "UNDEFINED");
            return string.Format("--sub: id{{{0}}} iindex{{{1}}} {{{2}}}", this.id, istr, ipt.Format());
        }

        public override void Tabulate(VertexList verts, double chordalTol)
        { return; }
    }
}
