using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;

namespace AnarchistMod
{
    public class JobDriver_BreachWall : JobDriver
    {
        // Наша цель - стена (Target A)
        private Thing TargetWall => job.GetTarget(TargetIndex.A).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(TargetWall, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            // Если стена исчезла или до нее не добраться - отменяем задание
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);

            // 1. Идем к стене
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

            // 2. Атакуем стену (подрывник будет бить её или кидать гранату, если она в руках)
            yield return Toils_Combat.FollowAndMeleeAttack(TargetIndex.A, () => { })
                .FailOnDespawnedNullOrForbidden(TargetIndex.A);
        }
    }
}