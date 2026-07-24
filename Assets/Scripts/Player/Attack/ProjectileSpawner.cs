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

    private void OnSpawnVFX(SpawnVFXEvent e)
    {
        if (string.IsNullOrEmpty(e.VFXAddress)) return;

        AddressableManager.Instance.LoadAssetAsync<GameObject>(e.VFXAddress, (prefab) =>
        {
            if (prefab != null)
            {
                GameObject vfx = PoolManager.Instance.Get(prefab, e.Position, e.Rotation);

                PooledObject pooledObj = vfx.GetComponent<PooledObject>();
                if (pooledObj != null)
                {
                    pooledObj.Init(prefab);
                }
            }
        });
    }

    private void OnSpawnProjectile(SpawnProjectileEvent e)
    {
        if (string.IsNullOrEmpty(e.ProjectileAddress)) return;

        AddressableManager.Instance.LoadAssetAsync<GameObject>(e.ProjectileAddress, (prefab) =>
        {
            if (prefab != null)
            {
                GameObject projObj = PoolManager.Instance.Get(prefab, e.Position, e.Rotation);

                PooledObject pooledObj = projObj.GetComponent<PooledObject>();
                if (pooledObj != null)
                {
                    pooledObj.Init(prefab);
                }

                Projectile proj = projObj.GetComponent<Projectile>();
                if (proj != null)
                {
                    proj.Setup(e);
                }
            }
        });
    }
}