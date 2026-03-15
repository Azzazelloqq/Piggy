namespace Code.Game.Exploration.Domain
{
    public enum MapEntityType
    {
        Activity = 0,
        Item = 1,
        Transition = 2,
        Character = 3,
        Trigger = 4,
        Decoration = 5
    }

    public enum KnowledgeState
    {
        Unknown = 0,
        Hinted = 1,
        Revealed = 2,
        Resolved = 3
    }

    public enum DiscoveryMode
    {
        Passive = 0,
        Active = 1,
        Conditional = 2,
        Scripted = 3
    }

    public enum EventTriggerType
    {
        Travel = 0,
        Interaction = 1,
        Enter = 2,
        Time = 3,
        Flag = 4
    }

    public enum InteractionResult
    {
        CriticalSuccess = 0,
        Success = 1,
        PartialSuccess = 2,
        Failure = 3,
        CriticalFailure = 4
    }

    public enum ConditionType
    {
        HasFlag = 0,
        TimeRange = 1,
        EntityKnowledgeState = 2
    }

    public enum ActionType
    {
        SpendTime = 0,
        SetFlag = 1,
        ClearFlag = 2,
        RevealEntity = 3,
        HideEntity = 4,
        ResolveEntity = 5,
        MoveToLocation = 6,
        MoveToNode = 7,
        ChangeSuspicion = 8
    }

    public enum TimeAdvanceMode
    {
        None = 0,
        Instant = 1,
        Flow = 2
    }
}
