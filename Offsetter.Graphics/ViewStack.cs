using Offsetter.Math;
using System.Collections.Generic;

namespace Offsetter.Graphics
{
    public class ViewStack
    {
        private Stack<GBox> stack = new Stack<GBox>();

        public ViewStack() { }

        public void Clear() { stack.Clear(); }

        public GBox Push(GBox model)
        {
            stack.Push(model);
            return model;
        }

        public GBox Pop() { return stack.Pop(); }

        public GBox Peek() { return stack.Peek(); }

        public int Count { get { return stack.Count; } }
    }
}
