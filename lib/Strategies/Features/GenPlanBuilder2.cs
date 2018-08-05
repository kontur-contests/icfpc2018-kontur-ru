using System.Collections.Generic;
using System.Linq;

using lib.Models;
using lib.Utils;

namespace lib.Strategies.Features
{
    public class GenPlanBuilder2
    {
        private readonly State state;
        private int n;
        private Matrix<bool> used;

        public GenPlanBuilder2(State state)
        {
            this.state = state;
            this.n = state.R;
            used = new Matrix<bool>(n);
        }

        public List<Region> CreateGenPlan()
        {
            var result = new List<Region>();

            var vecs = CreateRegions();

            int done = 0;
            foreach (var vec in vecs)
            {
                Log.For("plan").Info($"{done++}/{vecs.Count}");

                for (int y = 0; y < n; y++)
                    for (int x = 0; x < n; x++)
                        for (int z = 0; z < n; z++)
                        {
                            var region = new Region(new Vec(x, y, z), new Vec(x, y, z) + vec);
                            if (!state.Matrix.IsInside(region.End))
                                continue;

                            var ok = true;
                            var target = state.TargetMatrix[region.First()];
                            var source = state.SourceMatrix[region.First()];
                            if (target == source)
                                continue;

                            foreach (var cell in region)
                            {
                                ok &= state.TargetMatrix[cell] == target && state.SourceMatrix[cell] == source && !used[cell];
                                if (!ok)
                                    break;
                            }

                            if (!ok)
                                continue;

                            region.ToGround = target;
                            result.Add(region);
                            foreach (var cell in region)
                                used[cell] = true;
                        }
            }

            return result;
        }

        private List<Vec> CreateRegions()
        {
            //return new List<Vec>() {Vec.Zero, };

            var result = new List<Vec>();

            var step = 1;
            var deltas = new List<int>() {0,1,2,3,4,5};
            for (int i = 6; i < 29; i += step)
                deltas.Add(i);
            if (deltas.Last() != 29)
                deltas.Add(29);

            //deltas = new List<int> {0, 10};

            foreach (var y in deltas)
                foreach (var x in deltas)
                    foreach (var z in deltas)
                        result.Add(new Vec(x, y, z));

            result = result.OrderByDescending(PerRobot)
                           .ThenBy(v => v.Y)
                           .ThenBy(v => v.X).ToList();

            return result;
        }

        private double PerRobot(Vec vec)
        {
            var area = (vec.X+1) * (vec.Y+1) * (vec.Z+1);
            var robots = new Region(Vec.Zero, vec).Vertices().Count();
            return 1.0 * area / robots / robots;
        }
    }
}