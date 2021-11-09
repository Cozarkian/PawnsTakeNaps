using RimWorld;
using Verse;

namespace PawnsTakeNaps
{
    [DefOf]
    public class DefOf_Napping
    {
        public static ThoughtDef RestfulNap;
        public static ThoughtDef UnsatisfyingNap;
        public static HediffDef RefreshingNap;
        public static JobDef PTN_TakeNap;

        static DefOf_Napping()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(DefOf_Napping));
        }

    }
}
