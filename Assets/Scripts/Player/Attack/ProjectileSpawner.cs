using UnityEngine;

public class ProjectileSpawner : MonoBehaviour
{
    private void OnEnable()
    {
        EventBus.Subscribe<SpawnProjectileEvent>(OnSpawnProjectile);
        EventBus.Subscribe<SpawnVFXEvent>(OnSpawnVFX);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<SpawnProjectileEvent>(OnSpawnProjectile);
        EventBus.Unsubscribe<SpawnVFXEvent>(OnSpawnVFX);
    }

    private void OnSpawnProjectile(SpawnProjectileEvent e)
    {
        if (e.ProjectilePrefab == null) return;

        GameObject obj = PoolManager.Instance.Get(e.ProjectilePrefab, e.Position, e.Rotation);

        if (obj.TryGetComponent<Projectile>(out Projectile proj))
        {
            proj.Setup(e.Direction, e.Speed, e.Damage, e.IsPiercing);
        }
    }

    private void OnSpawnVFX(SpawnVFXEvent e)
    {
        if (e.VFXPrefab == null) return;
        PoolManager.Instance.Get(e.VFXPrefab, e.Position, e.Rotation);
    }
}