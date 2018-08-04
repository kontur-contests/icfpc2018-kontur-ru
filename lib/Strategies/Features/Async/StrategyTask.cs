using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace lib.Strategies.Features.Async
{
    public class StrategyTask
    {
        private readonly IStrategy[] strategies;
        private readonly WaitType waitType;

        public StrategyTask(IStrategy[] strategies, WaitType waitType)
        {
            this.strategies = strategies;
            this.waitType = waitType;
        }

        public StrategyAwaiter GetAwaiter()
        {
            return new StrategyAwaiter(strategies, waitType);
        }
    }

    [AsyncMethodBuilder(typeof(StrategyTaskBuilder<>))]
    public class StrategyTask<T>
    {
        private IAsyncStateMachine stateMachine;
        private StrategyAwaiter awaiter;

        public WaitType WaitType { get; private set; }

        public List<IStrategy> Strategies { get; private set; }

        public T Result { get; private set; }

        public bool IsComplete { get; private set; }

        public void SetResult(T result)
        {
            IsComplete = true;
            Result = result;
        }

        public void SetAwaiter(IAsyncStateMachine stateMachine, StrategyAwaiter awaiter)
        {
            this.stateMachine = stateMachine;
            this.awaiter = awaiter;
            Strategies = awaiter.Strategies?.ToList();
            WaitType = awaiter.WaitType;
        }

        public void Continue()
        {
            var aw = awaiter;
            var sm = stateMachine;
            awaiter = null;
            stateMachine = null;
            aw.OnCompleted(() => sm.MoveNext());
        }
    }
}