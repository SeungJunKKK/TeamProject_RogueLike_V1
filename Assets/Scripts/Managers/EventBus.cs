using System;
using System.Collections.Generic;

public static class EventBus
{
    private static readonly Dictionary<Type, Delegate> s_Handlers = new Dictionary<Type, Delegate>();

    // 이벤트 구독
    public static void Subscribe<T>(Action<T> handler)
    {
        Type type = typeof(T);
        if (s_Handlers.TryGetValue(type, out Delegate d))
        {
            s_Handlers[type] = (Action<T>)d + handler;
        }
        else
        {
            s_Handlers[type] = handler;
        }
    }

    // 이벤트 해제
    public static void Unsubscribe<T>(Action<T> handler)
    {
        Type type = typeof(T);
        if (!s_Handlers.TryGetValue(type, out Delegate d))
        {
            return;
        }


        Delegate result = Delegate.Remove(d, handler);
        if (result == null)
        {
            s_Handlers.Remove(type);
        }
        else
        {
            s_Handlers[type] = result;
        }
    }

    // 이벤트 발행
    public static void Publish<T>(T eventData)
    {
        Type type = typeof(T);
        if (s_Handlers.TryGetValue(type, out Delegate d))
        {
            ((Action<T>)d)(eventData);
        }
    }

    // 구독 전체 초기화
    public static void Clear()
    {
        s_Handlers.Clear();
    }
}
