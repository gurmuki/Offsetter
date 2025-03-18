using Offsetter.Math;
using System.Diagnostics;

namespace Offsetter.Entities
{
    public abstract class GCurve : GEntity
    {
        public GChain Owner { get; set; } = null;

        public abstract bool CanIgnore { get; }
        public virtual bool IsBlend { get; set; } = false;

        public abstract GCurve Clone(GPoint at);
        public abstract GCurve SplitAt(GPoint pt);
        public abstract GPoint PointAtUparam(double uparam);
        public abstract string PropertiesForm();

        public GCurve PrevCurve() { return (GCurve)Prev; }
        public GCurve NextCurve() { return (GCurve)Next;  }

        public void Append(GCurve post)
        {
            base.Append(post);
            post.Owner = this.Owner;
        }

        public void Prepend(GCurve pre)
        {
            base.Prepend(pre);
            pre.Owner = this.Owner;
        }

        public void Unlink()
        {
            base.Unlink();
            this.Owner = null;
        }

        public abstract void Reverse();
    }
}
