using System;
using System.Linq;

using lib.Commands;
using lib.Models;
using lib.Strategies.Features.Async;
using lib.Utils;

namespace lib.Strategies.Features
{
    public class ReachTarget : BotStrategy
    {
        private readonly Vec target;

        public ReachTarget(State state, Bot bot, Vec target)
            : base(state, bot)
        {
            this.target = target;
        }

        protected override sealed async StrategyTask<bool> Run()
        {
            if (Bot.Position == target)
                return true;

            if (state.IsVolatile(Bot, target))
                return false;

            while (true)
            {
                var path = new DrillerPathFinder(state, Bot, target).TryFindPath(state.GetOwner(Bot.Position) == Bot);
                if (path == null)
                    return false;

                while (path.Any())
                {
                    var step = path[0];
                    if (step.Type == DrillerPathFinder.StepType.Move)
                    {
                        if (step.MoveCommand.HasVolatileConflicts(Bot, state) || !step.MoveCommand.AllPositionsAreValid(state.Matrix, Bot))
                            break;
                        path.RemoveAt(0);
                        await Do(step.MoveCommand);
                        continue;
                    }

                    if (step.Type == DrillerPathFinder.StepType.Drill)
                    {
                        if (state.IsVolatile(Bot, step.Target) || !state.Matrix[step.Target])
                            break;

                        path.RemoveAt(0);

                        state.Own(Bot, step.Target);
                        await Do(new Voidd(step.Target - Bot.Position));

                        var prevPosition = Bot.Position;
                        await Do(step.MoveCommand);

                        if (state.GetOwner(prevPosition) == Bot)
                        {
                            state.Unown(Bot, prevPosition);
                            await Do(new Fill(prevPosition - Bot.Position));
                        }
                        continue;
                    }

                    if (step.Type == DrillerPathFinder.StepType.DrillOut)
                    {
                        if (state.IsVolatile(Bot, step.Target) || state.Matrix[step.Target])
                            break;

                        path.RemoveAt(0);

                        var prevPosition = Bot.Position;
                        await Do(step.MoveCommand);

                        if (state.GetOwner(prevPosition) == Bot)
                        {
                            state.Unown(Bot, prevPosition);
                            await Do(new Fill(prevPosition - Bot.Position));
                        }
                        else
                        {
                            throw new InvalidOperationException("WTF???");
                        }
                        continue;
                    }

                    throw new InvalidOperationException($"Strange step type {step.Type}");
                }

                if (!path.Any())
                    return true;
            }
        }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(target)}: {target}";
        }
    }
}