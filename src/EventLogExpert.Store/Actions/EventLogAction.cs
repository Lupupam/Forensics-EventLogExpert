﻿using EventLogExpert.Library.Models;
using EventLogExpert.Store.State;

namespace EventLogExpert.Store.Actions;

public record EventLogAction
{
    public record OpenLog(EventLogState.LogSpecifier LogSpecifier) : EventLogAction;

    public record ClearEvents : EventLogAction;

    public record LoadEvents(ICollection<DisplayEventModel> Events, IReadOnlyList<int> AllEventIds, IReadOnlyList<string> AllProviderNames, IReadOnlyList<string> AllTaskNames) : EventLogAction;

    public record FilterEvents(IList<Func<DisplayEventModel, bool>> Filter) : EventLogAction;
}
