using System.Collections;
using UnityEngine;

/// <summary>
/// AutoShooter — attaches to the player ship.
/// Automatically finds the nearest enemy (tagged "Enemy") and fires a bullet toward it.
///
/// SETUP:
///   1. Attach this component to the Player GameObject.
///   2. Create a Bullet prefab (with Rigidbody + Bullet.cs + Collider set to Trigger).
///   3. Create an empty child GameObject at the ship's nose — name it "FirePoint" — and assign to firePoint.
///   4. Assign the Bullet prefab to bulletPrefab in the Inspector.
///
/// KNOWN ISSUE: Both CameraLook and AutoShooter modify transform.forward.
///   If rotation feels jittery, comment out the Lerp in Update() temporarily.
///
/// PLANNED UPGRADES:
///   - Multiple fire points for spread / multi-shot weapons
///   - Weapon types (laser, missile, hammer chain) via a WeaponBase ScriptableObject
///   - fireRate driven by player upgrade level from a future UpgradeManager
/// </summary>
public class AutoShooter : MonoBehaviour
{
    [Header("Shooting")]
    public GameObject bulletPrefab;         // Assign a Bullet prefab in the Inspector
    public Transform firePoint;             // Empty child GameObject at the ship's nose
    public float fireRate = 1.0f;           // Shots per second (e.g. 2 = fires every 0.5s)
    public float bulletSpeed = 20f;         // How fast each bullet travels
    public float detectionRange = 50f;      // Only shoot enemies within this radius

    private float _nextFireTime = 0f;       // Internal cooldown tracker (Time.time based)

    void Update()
    {
        Transform target = FindNearestEnemy();
        if (target == null) return; // No enemies in range — idle

        // Smoothly rotate ship to face the target each frame.
        // Lerp gives a responsive but non-instant feel — pure snap feels bad in shooters.
        Vector3 dir = (target.position - transform.position).normalized;
        transform.forward = Vector3.Lerp(transform.forward, dir, Time.deltaTime * 5f);

        // Fire when cooldown has elapsed
        if (Time.time >= _nextFireTime)
        {
            Shoot(target);
            _nextFireTime = Time.time + 1f / fireRate;
        }
    }

    /// <summary>
    /// Scans all active GameObjects tagged "Enemy" and returns the closest one
    /// within detectionRange. Returns null if none are found.
    ///
    /// PERF NOTE: FindGameObjectsWithTag is O(n) over the whole scene.
    /// Fine for ~15 meteors (maxMeteors cap in MeteorSpawnerGL), but if enemy count
    /// grows significantly, replace with a cached list updated by MeteorSpawnerGL.
    /// </summary>
    Transform FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform nearest = null;
        float minDist = detectionRange;

        foreach (GameObject e in enemies)
        {
            float dist = Vector3.Distance(transform.position, e.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = e.transform;
            }
        }
        return nearest;
    }

    /// <summary>
    /// Instantiates a bullet at firePoint and sets its Rigidbody velocity toward the target.
    /// Collision and damage are handled by Bullet.cs on the prefab.
    /// TODO: add target-lead prediction once enemies move faster (currently trivial to dodge).
    /// </summary>
    void Shoot(Transform target)
    {
        if (bulletPrefab == null || firePoint == null) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 direction = (target.position - firePoint.position).normalized;
            rb.linearVelocity = direction * bulletSpeed;
        }

        // Safety cleanup — destroys the bullet after 5s if it never hits anything
        Destroy(bullet, 5f);
    }
}
