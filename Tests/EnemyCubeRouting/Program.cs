// Routing-only self-check. Unity physics, scene wiring and Play Mode are not simulated.
using System;
using System.Collections.Generic;
using System.Reflection;
using ShadowMaze.Area;
using ShadowMaze.InteractSystem;
using UnityEngine;

static class Program
{
    public static void Invoke(object target, string method, params object[] args) =>
        target.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, args);

    static void Check(bool condition, string name)
    {
        if (!condition) throw new Exception(name);
        Console.WriteLine("PASS: " + name);
    }

    static bool InMfa(CubeOnLight owner) => (bool)typeof(CubeOnLight)
        .GetField("inMemeoryFragmentArea", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner);

    static void Main()
    {
        var area = new GameObject().AddComponent<MemoryFragmentArea>();
        var cube = new GameObject();
        cube.AddComponent<InteractObject>();
        var collider = cube.AddComponent<Collider>();
        Invoke(area, "OnTriggerEnter", collider);
        Invoke(area, "OnTriggerExit", collider);
        Console.WriteLine("PASS: unparented interactable enters/exits MFA safely.");

        var owner = new GameObject().AddComponent<CubeOnLight>();
        var link = cube.AddComponent<EnemyCubeOwner>();
        link.Owner = owner;
        var holder = new GameObject();
        holder.AddComponent<CubeOnLight>();
        foreach (var parent in new[] { holder.transform, null })
        {
            cube.transform.parent = parent;
            Check(LightReaction.FindReceiver(cube) == owner, "held/dropped cube resolves its original owner");
            Invoke(area, "OnTriggerEnter", collider);
            Check(InMfa(owner), "MFA enter reaches owner");
            Invoke(area, "OnTriggerExit", collider);
            Check(!InMfa(owner), "MFA exit reaches owner");
        }

        var child = new GameObject();
        child.transform.parent = cube.transform;
        Check(LightReaction.FindReceiver(child) == owner, "child collider resolves cube owner");
        LightReaction.FindReceiver(cube).ReportHit(EnemyLightSource.Flashlight);
        LightReaction.FindReceiver(child).ReportHit(EnemyLightSource.Lamp);
        var hits = (EnemyLightSource)typeof(LightReaction)
            .GetField("hits", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner);
        Check(hits == (EnemyLightSource.Flashlight | EnemyLightSource.Lamp), "both light sources reach original owner");

        Invoke(area, "OnTriggerEnter", collider);
        Invoke(link, "OnDestroy");
        Check(!InMfa(owner), "destroying cube clears MFA membership");
        link.Owner = null;
        cube.transform.parent = holder.transform;
        Check(LightReaction.FindReceiver(cube) == null, "missing owner does not route to holder");
        Invoke(link, "OnDestroy");

        var legacy = new GameObject().AddComponent<ShadowMaze.EnemyCharactor.SimpleEnemyAi>();
        var oldCube = new GameObject();
        oldCube.AddComponent<InteractObject>();
        oldCube.transform.parent = legacy.transform;
        var oldCollider = oldCube.AddComponent<Collider>();
        Invoke(area, "OnTriggerEnter", oldCollider);
        Check(legacy.InMfa, "legacy enemy still receives MFA enter");
        Invoke(area, "OnTriggerExit", oldCollider);
        Check(!legacy.InMfa, "legacy enemy still receives MFA exit");
        Check(LightReaction.FindReceiver(owner.gameObject) == owner, "direct light receiver preserved");
        child.transform.parent = owner.transform;
        Check(LightReaction.FindReceiver(child) == owner, "parent light receiver preserved");
        Check(LightReaction.FindReceiver(new GameObject()) == null, "unrelated object has no receiver");
    }
}

namespace UnityEngine
{
    public class Object
    {
        public static implicit operator bool(Object value) => value != null;
    }
    public class GameObject : Object
    {
        readonly List<Component> components = new();
        public readonly Transform transform;
        public GameObject() { transform = new Transform { gameObject = this }; }
        public T AddComponent<T>() where T : Component, new()
        {
            var component = new T { gameObject = this };
            components.Add(component);
            return component;
        }
        public bool CompareTag(string tag) => tag == "Enemy";
        public bool TryGetComponent<T>(out T value) where T : Component
        {
            value = components.Find(c => c is T) as T;
            return value != null;
        }
        public T GetComponentInParent<T>() where T : Component =>
            TryGetComponent<T>(out var value) ? value : transform.parent?.gameObject.GetComponentInParent<T>();
    }
    public class Component : Object
    {
        public GameObject gameObject;
        public Transform transform => gameObject.transform;
        public bool TryGetComponent<T>(out T value) where T : Component => gameObject.TryGetComponent(out value);
        public T GetComponentInParent<T>() where T : Component => gameObject.GetComponentInParent<T>();
    }
    public class MonoBehaviour : Component { }
    public class Transform : Component { public Transform parent; }
    public class Collider : Component { }
    public class SerializeField : Attribute { }
    public static class Time { public static float deltaTime = 1; }
    public static class Debug { public static void Log(object value) { } }
}
namespace ShadowMaze.InteractSystem { public class InteractObject : MonoBehaviour { } }
namespace ShadowMaze.EnemyCharactor
{
    public enum EnemyActionState { Idle, StunnedAsCube }
    public class EnemyState { public EnemyActionState CurrEnemyActionState; }
    public class SimpleEnemyAi : MonoBehaviour
    {
        public bool InMfa;
        public void InArea(string name) => InMfa = name == "MFA";
        public void OutArea() => InMfa = false;
    }
}
public class TransitionEnemyBrain : MonoBehaviour
{
    public ShadowMaze.EnemyCharactor.EnemyState enemyState = new();
    public void TransitionToEnemyForm() { }
    public void TransitionToCubeForm() { }
}
