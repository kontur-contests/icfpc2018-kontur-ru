using System.Linq;

using lib.Models;
using lib.Strategies.Features.Async;
using lib.Utils;

namespace lib.Strategies.Features
{
    public class ReachFinalState : Strategy
    {
        public ReachFinalState(State state)
            : base(state)
        {
        }

        protected override async StrategyTask<bool> Run()
        {
            await WhenAll(state.Bots.Select(x => new ReachTarget(state, x, Vec.Zero)));

            var master = state.Bots.SingleOrDefault(b => b.Position == Vec.Zero);
            if (master == null)
                return false;

            var botsLeft = state.Bots.Except(new[] { master }).ToList();
            var fusionPositions = Vec.Zero.GetNears().Where(n => n.IsInCuboid(state.Matrix.R)).ToList();
            while (botsLeft.Count > 0)
            {
                await WhenAll(botsLeft.Select((x, i) => new ReachTarget(state, x, fusionPositions[i % fusionPositions.Count])));
                var slave = botsLeft.FirstOrDefault(b => fusionPositions.Contains(b.Position));
                if (slave == null)
                    return false;
                await MergeNears(master, slave);
                botsLeft.Remove(slave);
            }
            return await Halt();
        }
    }
}