using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Override to create events that pass back specific data to listeners of the event
public class CallbackEvent
{
    public CallbackEvent(){ }
}

struct TriggeredEvent
{
    public string eventName;
    public CallbackEvent eventData;

    public TriggeredEvent(string aEventName, CallbackEvent aEventData)
    {
        eventName = aEventName;
        eventData = aEventData;
    }
}


public class EventManager : BaseManager<EventManager>
{
    private Dictionary<string, Dictionary<string, Action<CallbackEvent>>> eventDictionary;
    private List<TriggeredEvent> triggeredEvents;

    // prevent multiple copies of this manager
    protected EventManager()
    {
        eventDictionary = new Dictionary<string, Dictionary<string, Action<CallbackEvent>>>();
        triggeredEvents = new List<TriggeredEvent>();
    }

    public override void OnRestartLevel()
    {
        triggeredEvents.Clear();
        foreach(KeyValuePair<string, Dictionary<string, Action<CallbackEvent>>> keyPair in eventDictionary)
        {
            keyPair.Value.Clear();
        }
        eventDictionary.Clear();
    }

    // Update is called once per frame
    void Update()
    {
        // We want to fire off the events all at once in succession the order they came in.
        // This allows for a more accurate sequence of events when we have lots of events being triggered all at the same time.
        if(triggeredEvents.Count > 0)
        {
            for (int i = 0; i <  triggeredEvents.Count; ++i)
            {
                string eventName = triggeredEvents[i].eventName;
                Dictionary<string, Action<CallbackEvent>> eventCallbackDictionary;
                if (Instance.eventDictionary.TryGetValue(eventName, out eventCallbackDictionary))
                {
                    CallbackEvent eventData = triggeredEvents[i].eventData;
                    foreach (KeyValuePair<string, Action<CallbackEvent>> keyValuePair in eventCallbackDictionary)
                    {
                        Action<CallbackEvent> action = keyValuePair.Value;
                        action(eventData);
                    }
                }
            }

            triggeredEvents.Clear();
        }
    }

    public void RegisterEvent(string aEventName, string aUniqueId, Action<CallbackEvent> aCallback)
    {
        Dictionary<string, Action<CallbackEvent>> eventCallbackDictionary;
        if (!eventDictionary.TryGetValue(aEventName, out eventCallbackDictionary))
        {
            eventCallbackDictionary = new Dictionary<string, Action<CallbackEvent>>();
            eventDictionary.Add(aEventName, eventCallbackDictionary);
        }

        Action<CallbackEvent> action;
        if (eventCallbackDictionary.TryGetValue(aUniqueId, out action))
        {
            Debug.LogWarning("[EventManager] WARNING - trying to assign a callback for id: " + aUniqueId +
                             " and one already exists. Failed to add event: " + aEventName);
            return;
        }

        eventCallbackDictionary.Add(aUniqueId, aCallback);
    }

    public void UnregisterEvent(string aEventName, string aUniqueId)
    {
        Dictionary<string, Action<CallbackEvent>> eventCallbackDictionary;
        if (eventDictionary.TryGetValue(aEventName, out eventCallbackDictionary))
        {
            Action<CallbackEvent> action;
            if (eventCallbackDictionary.TryGetValue(aUniqueId, out action))
            {
                eventCallbackDictionary.Remove(aUniqueId);
                if (eventCallbackDictionary.Count == 0)
                {
                    eventDictionary.Remove(aEventName);
                }
            }
        }
    }

    public void TriggerEvent(string aEventName)
    {
        TriggerEvent(aEventName, new CallbackEvent());
    }

    public void TriggerEvent(string aEventName, CallbackEvent aEvent)
    {
        TriggeredEvent newEvent = new TriggeredEvent(aEventName, aEvent);
        triggeredEvents.Add(newEvent);
    }
}
