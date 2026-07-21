using UnityEngine;

public struct SceneLoadStartedEvent
{
    public string SceneName;
}

public struct SceneLoadCompletedEvent
{
    public string SceneName;
}

public struct SpawnProjectileEvent
{
    public GameObject ProjectilePrefab;
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector2 Direction;
    public float Damage;
    public float Speed; 

    public bool IsPiercing;
}
public struct SpawnVFXEvent
{
    public GameObject VFXPrefab;
    public Vector3 Position;
    public Quaternion Rotation;
}