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

        public void SetException(Exception exception)
        {
            ExceptionDispatchInfo.Capture(exception).Throw();
        }

        public void SetResult(T value)
        {
            Task.SetResult(value);
        }

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(
            ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            var strategyAwaiter = awaiter as StrategyAwaiter ?? throw new InvalidOperationException($"{GetType()} supports only awaiters of type {typeof(StrategyAwaiter)}");
            Task.SetAwaiter(stateMachine, strategyAwaiter);
        }

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
            ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            throw new NotSupportedException();
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
        }
    }
}