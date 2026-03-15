using UnityEngine;

namespace Code.Game.Exploration.Runtime
{
public sealed class ExplorationTimeTicker : MonoBehaviour
{
    private ExplorationTimeController _timeController;

    public void Bind(ExplorationTimeController timeController)
    {
        _timeController = timeController;
    }

    public void Clear()
    {
        _timeController = null;
    }

    private void Update()
    {
        if (_timeController == null || !_timeController.HasActiveFlow)
        {
            return;
        }

        _timeController.Tick(Time.deltaTime);
    }
}
}
