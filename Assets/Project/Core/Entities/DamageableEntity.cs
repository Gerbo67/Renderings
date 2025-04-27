using System;
using Project.Core.Interfaces;
using UnityEngine;

namespace Project.Core.Entities
{
    public abstract class DamageableEntity : MonoBehaviour, IDamageable
    {
        [Header("Health Settings")]
        [SerializeField] public int maxHealth = 1000;
        public int currentHealth;
        
        public event Action<int, int> OnHealthChanged;

        protected virtual void Awake()
        {
            currentHealth = maxHealth;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public virtual void TakeDamage(int amount)
        {
            currentHealth = Mathf.Max(currentHealth - amount, 0);
            Debug.Log($"{name} recibió {amount} de daño. Vida restante: {currentHealth}");
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (currentHealth <= 0)
                Die();
        }

        /// <summary>
        /// Lógica de muerte. Debe implementarse en cada entidad.
        /// </summary>
        protected abstract void Die();
    }
}