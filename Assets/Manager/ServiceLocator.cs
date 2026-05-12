using System;
using System.Collections.Generic;
using UnityEngine;

public class ServiceLocator
{
    private static ServiceLocator _instance;
    public static ServiceLocator Instance => _instance ??= new();

    private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

    private ServiceLocator() { }

    public void Register<T>(T service) where T : class
    {
        _services[typeof(T)] = service;
    }

    public T Get<T>() where T : class
    {
        if (_services.TryGetValue(typeof(T), out var obj))
            return (T)obj;
        Debug.LogError($"[ServiceLocator] {typeof(T).Name} 尚未註冊");
        return null;
    }

    public bool Has<T>() where T : class => _services.ContainsKey(typeof(T));

    public void Unregister<T>() where T : class => _services.Remove(typeof(T));

    public void Clear() => _services.Clear();
}
