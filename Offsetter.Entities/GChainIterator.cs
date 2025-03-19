
namespace Offsetter.Entities
{
    public class GChainIterator
    {
        private GChain chain = null!;

        public GChainIterator(GChain chain)
        {
            this.chain = chain;
            FirstCurve();
        }

        public GCurve Curve { get; private set; } = null!;

        public GCurve FirstCurve()
        {
            Curve = chain.FirstCurve();
            return Curve;
        }

        public GCurve NextCurve()
        {
            GEntity iter = Curve;

            while (iter != null)
            {
                iter = (GEntity)iter.Next;
                if ((iter == null) || iter.IsA(T.SUBCHN))
                    continue;

                Curve = (GCurve)iter;
                return Curve;
            }

            return null!;
        }

        public GCurve TerminalCurve()
        {
            Curve = chain.TerminalCurve();
            return Curve;
        }

        public GCurve PrevCurve()
        {
            GEntity iter = Curve;

            while (iter != null)
            {
                iter = (GEntity)iter.Prev;
                if ((iter == null) || iter.IsA(T.SUBCHN))
                    continue; ;

                Curve = (GCurve)iter;
                return Curve;
            }

            return null!;
        }
    }
}
