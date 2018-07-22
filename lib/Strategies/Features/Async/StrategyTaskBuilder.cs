using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace lib.Strategies.Features.Async
{
    public class StrategyTaskBuilder<T>
    {
        public static StrategyTaskBuilder<T> Create() => new StrategyTaskBuilder<T>();

        public StrategyTask<T> Task { get; } = new StrategyTask<T>();

        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
        }

        public void SetException(Exception exception)
        {
            ExceptionDispatchInfo.Capture(exception).Throw();
        }

        public void SetResult(T value)
        {
            Task.Result = value;
            Task.IsComplete = true;
        }

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(
            ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            var aw = awaiter;
            var sm = stateMachine;
            Task.Strategies = (awaiter as StrategyAwaiter)?.Strategies;
            Task.Continue = () =>
                {
                    Task.Continue = null;
                    Task.Strategies = null;
                    aw.OnCompleted(() => sm.MoveNext());
                };
        }

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
            ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            throw new NotSupportedException();
        }
    }
}