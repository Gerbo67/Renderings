using Project.Core.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Game.UI
{
    [RequireComponent(typeof(Image))]
    public class HealthBarUI : MonoBehaviour
    {
        [Tooltip("De vacío (índice 0) a lleno (último índice)")]
        [SerializeField] private Sprite[] healthSprites;

        private Image _image;
        private DamageableEntity _entity;

        void Awake()
        {
            _image = GetComponent<Image>();
            // Busca hacia arriba en la jerarquía la entidad con vida
            _entity = GetComponentInParent<DamageableEntity>();
        }

        void OnEnable()
        {
            if (_entity != null)
                _entity.OnHealthChanged += UpdateBar;
        }

        void OnDisable()
        {
            if (_entity != null)
                _entity.OnHealthChanged -= UpdateBar;
        }

        void Start()
        {
            if (_entity != null)
                UpdateBar(_entity.currentHealth, _entity.maxHealth);
        }

        private void UpdateBar(int current, int max)
        {
            if (healthSprites == null || healthSprites.Length == 0) return;

            // Normaliza vida entre 0 y 1
            float t = (float)current / max;
            // Índice de sprite
            int idx = Mathf.RoundToInt(t * (healthSprites.Length - 1));
            idx = Mathf.Clamp(idx, 0, healthSprites.Length - 1);

            _image.sprite = healthSprites[idx];
        }
    }
}

