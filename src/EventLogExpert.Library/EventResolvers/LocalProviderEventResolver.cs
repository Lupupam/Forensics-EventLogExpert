﻿// // Copyright (c) Microsoft Corporation.
// // Licensed under the MIT License.

using EventLogExpert.Library.Helpers;
using EventLogExpert.Library.Models;
using EventLogExpert.Library.Providers;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;

namespace EventLogExpert.Library.EventResolvers;

/// <summary>
///     Resolves event descriptions by using our own logic to look up
///     message strings in the providers available on the local machine.
/// </summary>
public class LocalProviderEventResolver : EventResolverBase, IEventResolver
{
    private Dictionary<string, ProviderDetails?> _providerDetails = new();

    public LocalProviderEventResolver() : base(s => Debug.WriteLine(s)) { }

    public LocalProviderEventResolver(Action<string> tracer) : base(tracer) { }

    public DisplayEventModel Resolve(EventRecord eventRecord)
    {
        if (!_providerDetails.ContainsKey(eventRecord.ProviderName))
        {
            var provider = new EventMessageProvider(eventRecord.ProviderName, _tracer);
            _providerDetails.Add(eventRecord.ProviderName, provider.LoadProviderDetails());
        }

        _providerDetails.TryGetValue(eventRecord.ProviderName, out var providerDetails);

        if (providerDetails == null)
        {
            return new DisplayEventModel(
                eventRecord.RecordId,
                eventRecord.TimeCreated?.ToUniversalTime(),
                eventRecord.Id,
                eventRecord.MachineName,
                (SeverityLevel?)eventRecord.Level,
                eventRecord.ProviderName,
                "",
                "Description not found. No provider available.",
                "");
        }

        // The Properties getter is expensive, so we only call the getter once,
        // and we pass this value separately from the eventRecord so it can be reused.
        var eventProperties = eventRecord.Properties;

        var result = ResolveFromProviderDetails(eventRecord, eventProperties, providerDetails);

        if (result.Description == null)
        {
            result = result with { Description = "" };
        }

        return result;
    }
}
