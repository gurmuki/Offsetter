using System.Diagnostics;

namespace Offsetter.Entities
{
    public class GDNode
    {
        public GDNode() { }

        public void Append(GDNode post)
        {
            Debug.Assert((post != null), "GDNode.Append()");
            post.Prev = this;
            post.Next = this.Next;
            this.Next = post;
            if (post.Next != null)
                post.Next.Prev = post;
        }

        public void Prepend(GDNode pre)
        {
            Debug.Assert((pre != null), "GDNode.Prepend()");
            pre.Prev = this.Prev;
            pre.Next = this;
            this.Prev = pre;
            if (pre.Prev != null)
                pre.Prev.Next = pre;
        }

        public void Unlink()
        {
            if (this.Prev != null)
                this.Prev.Next = this.Next;

            if (this.Next != null)
                this.Next.Prev = this.Prev;

            this.Prev = null;
            this.Next = null;
        }

        public GDNode Prev { get; set; } = null;
        public GDNode Next { get; set; } = null;
    }
}
