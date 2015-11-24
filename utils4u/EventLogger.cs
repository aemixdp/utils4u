using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

public class EventLogger : MonoBehaviour
{
    private struct EventKey
    {
        public readonly Type ComponentType;
        public readonly Type HandlerType;
        public readonly string EventName;

        public EventKey(Type componentType, Type handlerType, string eventName)
        {
            ComponentType = componentType;
            HandlerType = handlerType;
            EventName = eventName;
        }
    }

    private static Dictionary<EventKey, Delegate> _eventHandlers =
        new Dictionary<EventKey, Delegate>();

    public static void LogGameObjectEvents(GameObject gameObject)
    {
        foreach (var component in gameObject.GetComponents<Component>())
        {
            LogComponentEvents(component);
            foreach (Transform child in gameObject.transform)
            {
                LogGameObjectEvents(child.gameObject);
            }
        }
    }

    public static void LogComponentEvents(Component component)
    {
        var componentType = component.GetType();
        foreach (var eventInfo in componentType.GetEvents())
        {
            var handler = (Delegate)GetHandler(componentType, eventInfo)
                .DynamicInvoke(component.gameObject.name);
            eventInfo.AddEventHandler(component, handler);
        }
    }

    void Awake()
    {
        LogGameObjectEvents(gameObject);
    }

    static Delegate GetHandler(Type componentType, EventInfo eventInfo)
    {
        var handlerType = eventInfo.EventHandlerType;
        var eventName = eventInfo.Name;
        var key = new EventKey(componentType, handlerType, eventName);
        var handler = (Delegate)null;
        if (!_eventHandlers.TryGetValue(key, out handler))
        {
            var parameters = handlerType.GetMethod("Invoke")
                .GetParameters()
                .Select(p => Expression.Parameter(p.ParameterType, p.Name))
                .ToArray();
            var formatStringBuilder = new StringBuilder();
            formatStringBuilder.AppendFormat(
                "<b>{0}[</b><color=navy>{{0}}</color><b>]</b>.<color=purple>{1}</color>",
                componentType.Name, eventName);
            if (parameters.Length > 0)
            {
                formatStringBuilder.Append(": ");
            }
            for (int i = 0; i < parameters.Length; ++i)
            {
                formatStringBuilder.AppendFormat(
                    "[<color=blue>{0} = {{{1}}}</color>] ",
                    parameters[i].Name, i + 1);
            }
            var method = typeof(EventLogger).GetMethod("LogFormat",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(string), typeof(string), typeof(object[]) },
                null);
            var nameArg = Expression.Parameter(typeof(string), "gameObjectName");
            var lambda = Expression.Lambda(
                Expression.Lambda(handlerType,
                    Expression.Call(method,
                        Expression.Constant(formatStringBuilder.ToString()),
                        nameArg,
                        Expression.NewArrayInit(typeof(object),
                            parameters.Select(p =>
                                Expression.Convert(p, typeof(object))).ToArray())),
                    parameters),
                nameArg);
            handler = lambda.Compile();
            _eventHandlers[key] = handler;
        }
        return handler;
    }

    public static void LogFormat(string format, string arg0, object[] restArgs)
    {
        object[] args = new object[restArgs.Length + 1];
        args[0] = arg0;
        for (int i = 0; i < restArgs.Length; ++i)
        {
            args[i + 1] = Show(restArgs[i]);
        }
        Debug.Log(String.Format(format, args));
    }

    static string Show(object obj)
    {
        if (obj.GetType().IsArray)
        {
            var array = (object[])obj;
            var sb = new StringBuilder("{ ");
            foreach (var elem in array)
            {
                sb.Append(Show(elem)).Append(", ");
            }
            sb[sb.Length - 2] = ' ';
            sb[sb.Length - 1] = '}';
            return sb.ToString();
        }
        else if (obj is GameObject)
        {
            return ((GameObject)obj).name;
        }
        else
        {
            return obj.ToString();
        }
    }
}