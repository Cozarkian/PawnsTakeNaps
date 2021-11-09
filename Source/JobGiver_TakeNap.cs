using System;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace PawnsTakeNaps
{
	public class JobGiver_TakeNap : ThinkNode_JobGiver
	{
		public override float GetPriority(Pawn pawn)
		{
			/*if (pawn.RaceProps.Animal)
			{
				Log.Warning("An animal tried to take a nap.");
			}*/
			int hour = GenLocalDate.HourOfDay(pawn);
			if (hour < 10 || hour > 17)
			{
				//Log.Message("It's not a good time for napping");
				return 0f;
			}
			Need_Rest rest = pawn.needs.rest;
			if (rest == null || rest.CurLevelPercentage > RestUtility.FallAsleepMaxLevel(pawn) - .10f)
            {
				return 0f;
            }
			if (pawn.needs.mood?.thoughts?.memories?.GetFirstMemoryOfDef(DefOf_Napping.RestfulNap) != null)
			{ 
				//Log.Message(pawn.Label + " already had a good nap.");
				return 0f;
			}
			Lord lord = pawn.GetLord();
			if (lord?.CurLordToil != null && !lord.CurLordToil.AllowRestingInBed)
            {
				return 0f;
            }
			TimeAssignmentDef timeAssignmentDef = (pawn.timetable == null) ? TimeAssignmentDefOf.Anything : pawn.timetable.CurrentAssignment;
			if (timeAssignmentDef == TimeAssignmentDefOf.Sleep)
            {
				return 0f;
            }
			if (timeAssignmentDef == TimeAssignmentDefOf.Meditate)
            {
				return 0f;
            }
			float num = Rand.ValueSeeded(pawn.thingIDNumber ^ 125) * 10f;
			//Log.Message(pawn.Label + " has a starting value of " + num.ToString());
			num -= rest.CurLevelPercentage;
			if (pawn.needs.joy != null) num += pawn.needs.joy.CurLevel;
			if (pawn.needs.comfort != null) num -= pawn.needs.comfort.CurLevel;
			if (pawn.health?.hediffSet?.PainTotal != null) num += pawn.health.hediffSet.PainTotal;
			num = (int)(num * 10) / 10f;
			//Log.Message(pawn.Label + " has a final value of " + num.ToString());
			return Math.Max(0f, num);
        }

        protected override Job TryGiveJob(Pawn pawn)
		{
			Job job;
			//Log.Message("Assigning " + pawn.Label + " a job of " + DefOf_Napping.PTN_TakeNap.defName);
			
			//Lord lord = pawn.GetLord();
			Building_Bed building_Bed = null;
			if (!pawn.IsWildMan() && (!pawn.InMentalState || pawn.MentalState.AllowRestingInBed))
			{
				Pawn_RopeTracker roping = pawn.roping;
				if (roping == null || !roping.IsRoped)
				{
					building_Bed = RestUtility.FindBedFor(pawn);
				}
			}
			if (building_Bed != null)
			{
				job = JobMaker.MakeJob(DefOf_Napping.PTN_TakeNap, building_Bed);
			}
			else
			{
				job = JobMaker.MakeJob(DefOf_Napping.PTN_TakeNap, FindGroundSleepSpotFor(pawn));
			}
			return job;
		}

		public IntVec3 FindGroundSleepSpotFor(Pawn pawn)
		{
			Map map = pawn.Map;
			IntVec3 position = pawn.Position;
			if (pawn.RaceProps.Dryad && pawn.connections != null)
			{
				foreach (Thing thing in pawn.connections.ConnectedThings)
				{
					if (pawn.CanReach(thing, PathEndMode.Touch, Danger.Deadly, false, false, TraverseMode.ByPawn))
					{
						position = thing.Position;
						break;
					}
				}
			}
			Predicate<IntVec3> extraValidator = (IntVec3 x) => !x.IsForbidden(pawn) && !x.GetTerrain(map).avoidWander;
			IntVec3 result;
			if (CellFinder.TryRandomClosewalkCellNear(position, map, 4, out result, extraValidator))
			{
				return result;
			}
			if (CellFinder.TryRandomClosewalkCellNear(position, map, 12, out result, extraValidator))
			{
				return result;
			}
			return CellFinder.RandomClosewalkCellNearNotForbidden(pawn, 4);
		}
	}
}
