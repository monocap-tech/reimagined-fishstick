using System.Collections.Generic;
using UnityEngine;

namespace NeonRange
{
    /// <summary>Assignment 3-style prewarmed pool. Exhaustion grows once; normal firing reuses.</summary>
    public sealed class BulletPool : MonoBehaviour
    {
        [SerializeField] private Bullet bulletPrefab;
        [SerializeField, Min(1)] private int poolSize = 12;
        private readonly List<Bullet> bullets = new List<Bullet>();
        public int InstanceCount => bullets.Count;

        private void Awake()
        {
            for (int i = 0; i < poolSize; i++) CreateBullet();
        }
        private Bullet CreateBullet()
        {
            Bullet bullet = Instantiate(bulletPrefab, transform);
            bullet.gameObject.SetActive(false);
            bullets.Add(bullet);
            return bullet;
        }
        public Bullet GetFromPool()
        {
            foreach (Bullet bullet in bullets)
                if (!bullet.gameObject.activeSelf) return bullet;
            return CreateBullet();
        }
        public void Fire(Vector2 position) => GetFromPool().Launch(position);
    }
}
