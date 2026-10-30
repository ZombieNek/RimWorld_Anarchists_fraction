using Verse;
using Verse.AI;
using RimWorld;

namespace AnarchistMod
{
    public class JobGiver_AnarchistBreach : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.kindDef.defName != "sapper") return null;

            Thing targetWall = GenClosest.ClosestThingReachable(
                pawn.Position,
                pawn.Map,
                ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial),
                PathEndMode.Touch,
                TraverseParms.For(pawn),
                9999f,
                (Thing t) => t.Faction == Faction.OfPlayer
                          && t.def.building != null
                          && t.def.building.isEdifice
                          && t.def.building.isWall
            );

            if (targetWall != null)
            {
                Log.Message($"[Anarchist] Найдена стена: {targetWall.Label}. Атакую!");
                return JobMaker.MakeJob(JobDefOf.AttackStatic, targetWall);
            }

            return null;
        }
    }
}