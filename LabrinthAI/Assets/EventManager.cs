using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

/// <summary>
/// Manages events throughout the game by linking event invokers and listeners. 
/// Provides functionality to add and link event invokers and listeners dynamically during runtime.
/// </summary>
public static class EventManager
{
    #region Fields

    // Holds all invokers for each event type, allowing multiple invokers per event.
    static Dictionary<Enum, List<GameEvent>> invokers = new Dictionary<Enum, List<GameEvent>>();
    // Holds all listeners for each event type, allowing multiple listeners per event.
    private static Dictionary<Enum, List<UnityAction<Dictionary<Enum, object>>>> listeners = new Dictionary<Enum, List<UnityAction<Dictionary<Enum, object>>>>();

    #endregion

    #region Public Methods

    /// <summary>
    /// Registers an event invoker for a specific event type.
    /// </summary>
    public static void AddInvoker(Enum eventType, GameEvent invoker)
    {
        if (!invokers.ContainsKey(eventType))
        {
            invokers[eventType] = new List<GameEvent>();
        }

        invokers[eventType].Add(invoker);
        LinkToListeners(eventType, invoker);
    }

    /// <summary>
    /// Unregisters an event invoker for a specific event type.
    /// </summary>
    public static void RemoveInvoker(Enum eventType, GameEvent invoker)
    {
        if (invokers.ContainsKey(eventType))
        {
            // Remove the invoker from the list
            invokers[eventType].Remove(invoker);
            
            // If no more invokers for this eventType, remove the key
            if (invokers[eventType].Count == 0)
            {
                invokers.Remove(eventType);
            }
        }
    }

    /// <summary>
    /// Registers an event listener for a specific event type.
    /// </summary>
    public static void AddListener(Enum eventType, UnityAction<Dictionary<Enum, object>> listener)
    {
        if (!listeners.ContainsKey(eventType))
        {
            listeners[eventType] = new List<UnityAction<Dictionary<Enum, object>>>();
        }

        listeners[eventType].Add(listener);
        LinkToInvokers(eventType, listener);
    }

    /// <summary>
    /// Unregisters an event listener for a specific event type.
    /// </summary>
    public static void RemoveListener(Enum eventType, UnityAction<Dictionary<Enum, object>> listener)
    {
        if (listeners.ContainsKey(eventType))
        {
            // Remove the listener from the list
            listeners[eventType].Remove(listener);

            // Unsubscribe the listener from all invokers
            if (invokers.ContainsKey(eventType))
            {
                foreach (GameEvent invoker in invokers[eventType])
                {
                    invoker.RemoveListener(listener);
                }
            }

            // If no more listeners for this eventType, remove the key
            if (listeners[eventType].Count == 0)
            {
                listeners.Remove(eventType);
            }
        }
    }

    /// <summary>
    /// Clears all events, invokers, and listeners.
    /// </summary>
    public static void ClearEvents()
    {
        invokers.Clear();
        listeners.Clear();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Links all existing listeners for a given event type to a new invoker.
    /// </summary>
    private static void LinkToListeners(Enum eventType, GameEvent invoker)
    {
        if (listeners.ContainsKey(eventType))
        {
            foreach (UnityAction<Dictionary<Enum, object>> listener in listeners[eventType])
            {
                invoker.AddListener(listener);
            }
        }
    }

    /// <summary>
    /// Links a new listener to all existing invokers for a given event type.
    /// </summary>
    private static void LinkToInvokers(Enum eventType, UnityAction<Dictionary<Enum, object>> listener)
    {
        if (invokers.ContainsKey(eventType))
        {
            foreach (GameEvent invoker in invokers[eventType])
            {
                invoker.AddListener(listener);
            }
        }
    }

    #endregion
}