using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Code.Game.Async
{
    public sealed class AsyncEvent
    {
        private readonly List<Func<UniTask>> _handlers = new List<Func<UniTask>>();

        public void Subscribe(Func<UniTask> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            _handlers.Add(handler);
        }

        public void Unsubscribe(Func<UniTask> handler)
        {
            if (handler == null)
            {
                return;
            }

            _handlers.Remove(handler);
        }

        public async UniTask InvokeAsync()
        {
            if (_handlers.Count == 0)
            {
                return;
            }

            var snapshot = _handlers.ToArray();
            foreach (var handler in snapshot)
            {
                await handler();
            }
        }

        public async UniTask InvokeSafeAsync(Action<Exception> onError)
        {
            if (_handlers.Count == 0)
            {
                return;
            }

            var snapshot = _handlers.ToArray();
            foreach (var handler in snapshot)
            {
                try
                {
                    await handler();
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception exception)
                {
                    onError?.Invoke(exception);
                }
            }
        }
    }
}
