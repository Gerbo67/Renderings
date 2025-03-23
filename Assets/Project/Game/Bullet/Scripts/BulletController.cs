using UnityEngine;

namespace Project.Game.Bullet.Scripts
{
    public class BulletController : MonoBehaviour
    {
        [Header("Bullet Settings")] [SerializeField]
        private float speed = 5f;

        [SerializeField] private int damage = 10;
        [SerializeField] private Color bulletColor = Color.white;

        private float _direction;

        public void SetDirection(float dir)
        {
            _direction = dir;
            transform.localScale = new Vector3(Mathf.Sign(_direction), 1, 1);
        }

        private void Update()
        {
            transform.Translate(Vector2.right * (_direction * speed * Time.deltaTime));
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            SpawnImpactParticles();
            Destroy(gameObject);
        }

        private void SpawnImpactParticles()
        {
            var psGo = new GameObject("ImpactParticles")
            {
                transform =
                {
                    position = transform.position
                }
            };

            var ps = psGo.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.2f;

            var main = ps.main;
            main.duration = 0.3f;
            main.loop = false;
            main.startLifetime = 0.3f;
            main.startSpeed = 0f;
            main.startSize = 0.05f;
            main.startColor = bulletColor;

            var velModule = ps.velocityOverLifetime;
            velModule.enabled = true;
            const float particleSpeed = 0.2f;
            var xValue = (_direction > 0) ? -particleSpeed : particleSpeed;
            velModule.x = new ParticleSystem.MinMaxCurve(xValue, xValue);
            velModule.y = new ParticleSystem.MinMaxCurve(-0.05f, 0.05f);
            velModule.z = new ParticleSystem.MinMaxCurve(0f, 0f);

            var psRenderer = psGo.GetComponent<ParticleSystemRenderer>();
            psRenderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
            psRenderer.material.SetColor("_Color", bulletColor);

            ps.Emit(3);

            Destroy(psGo, main.duration + main.startLifetime.constantMax);
        }
    }
}