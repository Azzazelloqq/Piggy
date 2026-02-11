using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Code.Game.Loading.Window;
using Piggy.Code.StateMachine;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Code.Game.Loading
{
    public sealed class LoadingState : GameState
    {
        private LoadingWindowPresenterBase _presenter;
        private LoadingWindowViewBase _viewInstance;

        protected override async UniTask OnEnterAsync<T>(
            T gameStateContext,
            CancellationToken cancellationToken)
        {
            await base.OnEnterAsync(gameStateContext, cancellationToken);

            if (gameStateContext is not LoadingStateContext loadingStateContext)
            {
                return;
            }

            var uiContext = loadingStateContext.UIContext;
            if (uiContext.LoadingWindowPrefab == null)
            {
                Debug.LogWarning("LoadingState: LoadingWindowPrefab is not set in UIContext.");
                return;
            }

            var parent = uiContext.OverlaysParent != null
                ? uiContext.OverlaysParent
                : uiContext.MainUIParent;

            _viewInstance = Object.Instantiate(uiContext.LoadingWindowPrefab, parent, false);
            _presenter = new LoadingWindowPresenter(_viewInstance, new LoadingWindowModel());

            try
            {
                _presenter.Initialize();
                _presenter.ReportProgress(0f);
                await _presenter.ShowAsync(cancellationToken);

                if (loadingStateContext.StepsOperation != null)
                {
                    if (loadingStateContext.TotalSteps <= 0)
                    {
                        Debug.LogWarning("LoadingState: TotalSteps must be greater than 0 for steps mode.");
                    }
                    else
                    {
                        _presenter.BeginSteps(loadingStateContext.TotalSteps);
                    }

                    var stepReporter = new LoadingStepReporter(_presenter);
                    await loadingStateContext.StepsOperation(stepReporter, cancellationToken);
                }
                else if (loadingStateContext.LoadOperation != null)
                {
                    var progress = new Progress<float>(value => _presenter.ReportProgress(value));
                    await loadingStateContext.LoadOperation(progress, cancellationToken);
                }
                else
                {
                    await UniTask.Yield(cancellationToken);
                }
            }
            catch (Exception)
            {
                CleanupImmediate();
                throw;
            }
        }

        protected override UniTask OnExitAsync(CancellationToken cancellationToken)
        {
            BeginHideAndCleanup(cancellationToken);
            return UniTask.CompletedTask;
        }

        private void BeginHideAndCleanup(CancellationToken cancellationToken)
        {
            var presenter = _presenter;
            var view = _viewInstance;
            _presenter = null;
            _viewInstance = null;

            if (presenter == null && view == null)
            {
                return;
            }

            _ = HideAndCleanupAsync(presenter, view, cancellationToken);
        }

        private async UniTask HideAndCleanupAsync(
            LoadingWindowPresenterBase presenter,
            LoadingWindowViewBase view,
            CancellationToken cancellationToken)
        {
            try
            {
                if (presenter != null)
                {
                    await presenter.HideAsync(cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                if (presenter != null)
                {
                    presenter.Dispose();
                }
                else if (view != null)
                {
                    view.Dispose();
                }
            }
        }

        private void CleanupImmediate()
        {
            if (_presenter != null)
            {
                _presenter.Hide();
                _presenter.Dispose();
                _presenter = null;
            }
            else if (_viewInstance != null)
            {
                _viewInstance.Dispose();
            }

            _viewInstance = null;
        }

        private sealed class LoadingStepReporter : ILoadingStepReporter
        {
            private readonly LoadingWindowPresenterBase _presenter;

            public LoadingStepReporter(LoadingWindowPresenterBase presenter)
            {
                _presenter = presenter;
            }

            public void CompleteStep()
            {
                _presenter.CompleteStep();
            }
        }
    }
}
