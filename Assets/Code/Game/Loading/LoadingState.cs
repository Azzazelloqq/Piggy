using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Code.Game.Loading.Window;
using LightDI.Runtime;
using Piggy.Code.StateMachine;
using ResourceLoader;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Code.Game.Loading
{
public sealed class LoadingState : GameState
{
    private const float FakeLoadingDelaySeconds = 1.5f;

    private readonly IResourceLoader _resourceLoader;
    private LoadingWindowPresenterBase _presenter;
    private LoadingWindowViewBase _viewInstance;

    public LoadingState([Inject] IResourceLoader resourceLoader) : base()
    {
        _resourceLoader = resourceLoader;
    }
    
    protected override async UniTask OnEnterAsync<T>(
        T gameStateContext,
        CancellationToken token)
    {
        await base.OnEnterAsync(gameStateContext, token);

        if (gameStateContext is not LoadingStateContext loadingStateContext)
        {
            return;
        }

        var viewResourceId = loadingStateContext.ViewResourceId;

        var parent = loadingStateContext.UIContext.OverlaysParent;
        
        _viewInstance = await _resourceLoader.LoadAndCreateAsync<LoadingWindowViewBase, Transform>(
            viewResourceId,
            parent,
            token);

        var model = new LoadingWindowModel();
        _presenter = new LoadingWindowPresenter(_viewInstance, model);

        try
        {
            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
            _presenter.Hide();
            
            await _presenter.InitializeAsync(token);
            _presenter.ReportProgress(0f);
            await _presenter.ShowAsync(token);

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
                await loadingStateContext.StepsOperation(stepReporter, token);
            }
            else if (loadingStateContext.LoadOperation != null)
            {
                var progress = new Progress<float>(value => _presenter.ReportProgress(value));
                await loadingStateContext.LoadOperation(progress, token);
            }
            else
            {
                var elapsed = 0f;
                while (elapsed < FakeLoadingDelaySeconds)
                {
                    var progress = Mathf.Clamp01(elapsed / FakeLoadingDelaySeconds);
                    _presenter.ReportProgress(progress);
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                    elapsed += Time.unscaledDeltaTime;
                }

                _presenter.ReportProgress(1f);
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
        return HideAndCleanupAsync(cancellationToken);
    }

    private UniTask HideAndCleanupAsync(CancellationToken cancellationToken)
    {
        var presenter = _presenter;
        var view = _viewInstance;
        _presenter = null;
        _viewInstance = null;

        if (presenter == null && view == null)
        {
            return UniTask.CompletedTask;
        }

        return HideAndCleanupAsync(presenter, view, cancellationToken);
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
                await presenter.DisposeAsync();
            }
            else if (view != null)
            {
                await view.DisposeAsync();
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