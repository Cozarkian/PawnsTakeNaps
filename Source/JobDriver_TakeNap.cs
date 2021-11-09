using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace PawnsTakeNaps
{
	public class JobDriver_TakeNap : JobDriver_LayDown
	{
		public override bool LookForOtherJobs
		{
			get
			{
				return false;
			}
		}

		public override Toil LayDownToil(bool hasBed)
		{
			Toil toil = Toils_LayDown.LayDown(TargetIndex.A, hasBed, this.LookForOtherJobs, this.CanSleep, this.CanRest, PawnPosture.LayingOnGroundNormal);
			toil.defaultCompleteMode = ToilCompleteMode.Delay;
			toil.defaultDuration = Rand.Range(2000, 6000);
			toil.AddFinishAction(delegate
			{
				if (ticksLeftThisToil > 0 || toil.defaultDuration % 10 == 0)
				{
					//Log.Message(pawn.LabelShort + "'s nap was interrupted!");
					if (pawn.needs?.mood?.thoughts?.memories != null)
					{
						pawn.needs.mood.thoughts.memories.TryGainMemory(DefOf_Napping.UnsatisfyingNap, null);
					} 
				}
				else
				{
					//Log.Message(pawn.LabelShort + " successfully napped for " + toil.defaultDuration + " ticks.");
					if (pawn.health?.hediffSet != null)
                    {
						pawn.health.hediffSet.AddDirect(HediffMaker.MakeHediff(DefOf_Napping.RefreshingNap, pawn, null));
                    }
					if (pawn.needs?.mood?.thoughts?.memories != null)
					{
						pawn.needs.mood.thoughts.memories.TryGainMemory(DefOf_Napping.RestfulNap, null);
					}
				}
			});
			//Log.Message(pawn.LabelShort + " is sleeping for " + toil.defaultDuration + " ticks.");
			return toil;
		}

        protected override IEnumerable<Toil> MakeNewToils()
		{
			job.startTick = Find.TickManager.TicksGame;
			bool hasBed = Bed != null;
			if (hasBed)
			{
				yield return Toils_Bed.ClaimBedIfNonMedical(TargetIndex.A, TargetIndex.None);
				yield return Toils_Bed.GotoBed(TargetIndex.A);
			}
			else
			{
				yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
			}
			yield return LayDownToil(hasBed);
			yield break;
		}
	}
}