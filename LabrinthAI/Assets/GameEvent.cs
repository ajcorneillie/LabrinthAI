using System;
using System.Collections.Generic;
using UnityEngine.Events;

/// <summary>
/// Represents a Unity event that carries additional data in the form of EventData.
/// </summary>
public class GameEvent : UnityEvent<Dictionary<Enum, object>>
{
    #region Fields
    // Dictionary to store event data with an Enum key.
    private Dictionary<Enum, object> data = new Dictionary<Enum, object>();
    #endregion


    #region Methods
    /// <summary>
    /// Gets the data associated with the specified key.
    /// </summary>
    /// <param name="key">The enum key representing the data name.</param>
    /// <returns>The data associated with the specified key.</returns>

    public Dictionary<Enum, object> Data { get { return data; } }
    public object GetData(Enum key)
    {
        return data.TryGetValue(key, out var value) ? value : null;
    }


    /// <summary>
    /// Adds data to the event.
    /// </summary>
    /// <param name="key">The enum key representing the data name.</param>
    /// <param name="data">The data to add.</param>
    public void AddData(Enum key, object data)
    {
        if (this.data.ContainsKey(key))
        {
            this.data[key] = data;
        }
        else
        {
            this.data.Add(key, data);
        }
    }

    /// <summary>
    /// Checks if the specified key exists in the event data.
    /// </summary>
    /// <param name="key">The key to check for in the event data.</param>
    /// <returns>True if the key exists, otherwise false.</returns>
    public bool ContainsKey(Enum key)
    {
        return data.ContainsKey(key);
    }

    public new void Invoke(Dictionary<Enum, object> data = null)
    {
        if (data == null)
        {
            data = new Dictionary<Enum, object>();
        }
        base.Invoke(data);
    }
    #endregion
}