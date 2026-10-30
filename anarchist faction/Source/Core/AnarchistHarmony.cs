using HarmonyLib;
using Verse;
using Verse.AI;
using RimWorld;

namespace AnarchistMod
{
    [StaticConstructorOnStartup]
    public static class AnarchistHarmony
    {
        static AnarchistHarmony()
        {
            Log.Message("[Anarchist Faction] Harmony инициализирован.");
            var harmony = new Harmony("ZombieNek.AnarchistFaction");
            harmony.PatchAll();
        }
    }

    // Патчим публичный метод JobTrackerTick
    [HarmonyPatch(typeof(Pawn_JobTracker), "JobTrackerTick")]
    public static class Patch_Pawn_JobTracker_JobTrackerTick
    {
        // Postfix вызывается ПОСЛЕ того, как игра проверила текущие задания
        static void Postfix(Pawn_JobTracker __instance)
        {
            // Получаем пешку из трекера через рефлексию (так как поле pawn закрытое)
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();

            if (pawn == null || pawn.kindDef == null || pawn.kindDef.defName != "sapper") return;
            if (pawn.Dead || pawn.Downed || !pawn.Spawned) return;

            // Если у пешки уже есть задание - ничего не делаем
            if (pawn.jobs.curJob != null) return;
            if (pawn.InMentalState) return;

            // Ищем стену игрока
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
                Log.Message($"[Anarchist] Принудительно даю задание подрывнику {pawn.LabelShort} атаковать {targetWall.Label}");

                // Создаем задание AttackStatic (пешка будет атаковать стену)
                Job job = JobMaker.MakeJob(JobDefOf.AttackStatic, targetWall);

                // Начинаем выполнение. Прерываем текущее, если оно было.
                pawn.jobs.StartJob(job, JobCondition.InterruptForced);
            }
        }
    }
}