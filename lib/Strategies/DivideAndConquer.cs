using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using JetBrains.Annotations;

using lib.Commands;
using lib.Models;
using lib.Primitives;
using lib.Utils;

namespace lib.Strategies
{
    public class DivideAndConquer : IAmSolver
    {
        private readonly Matrix targetMatrix;
        private Matrix buildingMatrix;

        public DivideAndConquer([NotNull] Matrix targetMatrix)
        {
            this.targetMatrix = targetMatrix;
            this.buildingMatrix = new Matrix(targetMatrix.N);
        }

        private List<ICommand> Commands { get; } = new List<ICommand>();

        public IEnumerable<ICommand> Solve()
        {
            var state = new MutableState(targetMatrix);

            var blockSize = (state.BuildingMatrix.R + sqrtN - 1) / sqrtN;

            Commands.Clear();
            Clone(state);
            foreach (var command in Commands)
            {
                yield return command;
            }
            Commands.Clear();

            var a = GetColumns()
                .GroupBy(GetColumnBatchId)
                .OrderBy(x => x.Key).ToList();

            var queues = GetColumns()
                         .GroupBy(GetColumnBatchId)
                         .OrderBy(x => x.Key)
                         .Zip(state.GetOrderedBots(), (columns, bot) => (columns, bot))
                         .Select(x => (new Queue<ICommand>(GenerateCommandsForColumns(x.columns.Where(y => y.To.Y >= 0).ToList(), x.bot)), x.bot))
                         .ToList();
            while (queues.Any(x => x.Item1.Count > 0))
            {
                if (state.Bots.Select((x, i) => (x.Position.Z / blockSize, x.Position.X / blockSize, i)).Any(x => x.Item1 * sqrtN + x.Item2 != x.i))
                    throw new Exception("Wrong zone");
                var commands = new List<ICommand>();
                for (int i = 0; i < queues.Count; i++)
                {
                    if (queues[i].Item1.Count == 0)
                    {
                        commands.Add(new Wait());
                        continue;
                    }
                    if (queues[i].Item1.Peek() is Fill fillCommand)
                    {
                        if (CanFill(state, queues[i].bot.Position + fillCommand.Shift))
                        {
                            commands.Add(queues[i].Item1.Dequeue());
                        }
                        else
                        {
                            commands.Add(new Wait());
                        }
                    }
                    else
                    {
                        commands.Add(queues[i].Item1.Dequeue());
                    }
                }
                if (commands.All(x => x is Wait))
                    throw new Exception();
                foreach (var command in commands)
                {
                    yield return command;
                }
                state.Tick(new Queue<ICommand>(commands));
            }
            Commands.Clear();
            GoHome(state);
            ApplyCommand(state, new Halt());
            foreach (var command in Commands)
            {
                yield return command;
            }
            Commands.Clear();
            state.EnsureIsFinal();
        }

        private void Clone([NotNull] MutableState state)
        {
            var blockSize = (state.BuildingMatrix.R + sqrtN - 1) / sqrtN;
            for (int i = 0; i < n - 1; i++)
            {
                ApplyCommand(state, Enumerable.Repeat<ICommand>(new Wait(), i).Concat(new [] {new Fission(new NearDifference(new Vec(0, 0, 1)), n - i)}).ToArray());
            }
            for (int i = n - 1; i >= 0; i--)
            {
                var commands = GoToVerticalFirst(new Vec(0, 0, i), new Vec(i % sqrtN * blockSize, 0, i / sqrtN * blockSize));
                foreach (var command in commands)
                {
                    var toApply = Enumerable.Repeat<ICommand>(new Wait(), n).ToArray();
                    toApply[i] = command;
                    ApplyCommand(state, toApply);
                }
            }
        }

        private void GoHome([NotNull] MutableState state)
        {
            for (int i = 0; i < n; i++)
            {
                var currVec = state.GetOrderedBots()[i].Position;
                var commands = new List<ICommand>();
                if (currVec.Y == 0)
                {
                    commands.AddRange(GoToVerticalFirst(currVec, new Vec(currVec.X, state.BuildingMatrix.R - 1, currVec.Z)));
                    currVec = new Vec(currVec.X, state.BuildingMatrix.R - 1, currVec.Z);
                }
                commands.AddRange(GoToVerticalLast(currVec, new Vec(0, 0, i)));
                foreach (var command in commands)
                {
                    var toApply = Enumerable.Repeat<ICommand>(new Wait(), n).ToArray();
                    toApply[i] = command;
                    ApplyCommand(state, toApply);
                }
            }
            for (int i = n - 1; i > 0; i--)
            {
                ApplyCommand(state, Enumerable.Repeat<ICommand>(new Wait(), i - 1).Concat(new ICommand[] {new FusionP(new NearDifference(new Vec(0, 0, 1))), new FusionS(new NearDifference(new Vec(0, 0, -1)))}).ToArray());
            }
        }

        private void ApplyCommand([NotNull] MutableState state, [NotNull] params ICommand[] commands)
        {
            if (state.Bots.Count != commands.Length)
                throw new ArgumentException();
            state.Tick(new Queue<ICommand>(commands));
            Commands.AddRange(commands);
        }

        private bool CanFill([NotNull] MutableState state, [NotNull] Vec vec)
        {
            return vec.Y == 0 || vec.GetMNeighbours().Where(x => x.IsInCuboid(state.BuildingMatrix.R)).Any(x => !state.BuildingMatrix.IsVoidVoxel(x));
        }

        [NotNull]
        private List<ICommand> GenerateCommandsForColumns([NotNull] List<(Vec From, Vec To)> columns, [NotNull] Bot bot)
        {
            var result = new List<ICommand>();
            var pos = bot.Position;
            foreach (var column in columns)
            {
                result.AddRange(GoToVerticalLast(pos, column.From + new Vec(0, 1, 0)));
                pos = column.From + new Vec(0, 1, 0);
                result.Add(new Fill(new NearDifference(new Vec(0, -1, 0))));
                for (int i = 0; i < (column.To - column.From).MLen(); i++)
                {
                    result.Add(new SMove(new LongLinearDifference(new Vec(0, 1, 0))));
                    pos += new Vec(0, 1, 0);
                    result.Add(new Fill(new NearDifference(new Vec(0, -1, 0))));
                }
                result.AddRange(GoToVerticalFirst(pos, new Vec(column.From.X, targetMatrix.N - 1, column.From.Z)));
                pos = new Vec(column.From.X, targetMatrix.N - 1, column.From.Z);
            }

            return result;
        }

        [NotNull]
        private List<ICommand> GoToVerticalFirst([NotNull] Vec pos, [NotNull] Vec target)
        {
            var result = new List<ICommand>();
            result.AddRange(StraightGoTo(pos, new Vec(pos.X, target.Y, pos.Z)));
            pos = new Vec(pos.X, target.Y, pos.Z);
            result.AddRange(StraightGoTo(pos, new Vec(target.X, target.Y, pos.Z)));
            pos = new Vec(target.X, target.Y, pos.Z);
            result.AddRange(StraightGoTo(pos, new Vec(target.X, target.Y, target.Z)));
            return result;
        }

        [NotNull]
        private List<ICommand> GoToVerticalLast([NotNull] Vec pos, [NotNull] Vec target)
        {
            var result = new List<ICommand>();
            result.AddRange(StraightGoTo(pos, new Vec(target.X, pos.Y, pos.Z)));
            pos = new Vec(target.X, pos.Y, pos.Z);
            result.AddRange(StraightGoTo(pos, new Vec(target.X, pos.Y, target.Z)));
            pos = new Vec(target.X, pos.Y, target.Z);
            result.AddRange(StraightGoTo(pos, new Vec(target.X, target.Y, target.Z)));
            return result;
        }

        private List<ICommand> StraightGoTo(Vec botPosition, Vec target)
        {
            var res = new List<ICommand>();
            var step = (target - botPosition).Sign(); // todo (mpivko, 21.07.2018): constant
            var pos = botPosition;
            while (pos != target)
            {
                var delta = step * Math.Min(15, (target - pos).MLen());
                pos += delta;
                res.Add(new SMove(new LongLinearDifference(delta)));
            }
            return res;
        }

        private (int, int) GetColumnBatchId((Vec From, Vec To) column)
        {
            var blockSize = (targetMatrix.N + sqrtN - 1) / sqrtN;
            //var blockSize = targetMatrix.N; // todo (mpivko, 21.07.2018): 
            return (column.From.Z / blockSize, column.From.X / blockSize);
        }

        [NotNull]
        private List<(Vec From, Vec To)> GetColumns()
        {
            var columns = new List<(Vec From, Vec To)>();
            for (int x = 0; x < targetMatrix.N; x++)
            {
                for (int z = 0; z < targetMatrix.N; z++)
                {
                    columns.AddRange(GetColumnsAbove(x, z));
                }
            }

            columns = columns.OrderBy(x => x.From.Y).ToList();

            var realColumns = columns.Where(x => x.From.Y == 0).ToList();
            columns = columns.Where(x => x.From.Y != 0).ToList();
            for (int i = 0; i < realColumns.Count; i++)
            {
                var x = realColumns[i];
                bool Predicate((Vec From, Vec To) y) => (new Vec(y.From.X, x.From.Y, y.From.Z) - x.From).MLen() == 1 && x.From.Y <= y.From.Y && y.From.Y <= x.To.Y;
                realColumns.AddRange(columns.Where(Predicate));
                columns.RemoveAll(Predicate);
            }
            
            if (columns.Count > 0)
                throw new Exception("Not all columns are used");

            realColumns = realColumns.Select((x, i) => (x.From, x.To, i)).OrderBy(x => x, new Comparer()).Select(x => (x.From, x.To)).ToList();

            for (int x = 0; x < targetMatrix.N; x++)
            {
                for (int z = 0; z < targetMatrix.N; z++)
                {
                    realColumns.Add((new Vec(x, -1, z), new Vec(x, -1, z)));
                }
            }

            return realColumns;
        }

        private class Comparer : IComparer<(Vec From, Vec To, int Index)>
        {
            public int Compare((Vec From, Vec To, int Index) x, (Vec From, Vec To, int Index) y)
            {
                if (x.From.X == y.From.X && x.From.Z == y.From.Z)
                {
                    return x.From.Y.CompareTo(y.From.Y);
                }
                return x.Index.CompareTo(y.Index);
            }
        }

        [NotNull]
        private List<(Vec, Vec)> GetColumnsAbove(int x, int z)
        {
            var res = new List<(Vec, Vec)>();
            Vec currStart = null;
            for (int y = 0; y < targetMatrix.N; y++)
            {
                if (targetMatrix[x, y, z] && currStart == null)
                {
                    currStart = new Vec(x, y, z);
                }
                if (!targetMatrix[x, y, z] && currStart != null)
                {
                    res.Add((currStart, new Vec(x, y - 1, z)));
                    currStart = null;
                }
            }
            return res;
        }

        private static readonly int sqrtN = 4;
        private static readonly int n = sqrtN * sqrtN;
    }
}
