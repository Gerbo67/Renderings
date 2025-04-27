using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal; // Requerido para Light2D

namespace Project.Game.Enemies.EscapistEnemy
{
    public class EnemyShieldController : MonoBehaviour
    {
        [Header("Shield Settings")]
        [Tooltip("Radio del escudo")]
        public float shieldRadius = 1f;
        
        [Tooltip("Cantidad de segmentos para dibujar el círculo")]
        public int segments = 100;
        
        [Tooltip("Color base del escudo (azul semi-transparente)")]
        public Color shieldColor = new Color(0f, 1f, 1f, 0.5f);

        private LineRenderer shieldLine;
        private CircleCollider2D shieldCollider;

        private void Awake()
        {
            // Crear y configurar el LineRenderer para dibujar el círculo del escudo
            shieldLine = gameObject.AddComponent<LineRenderer>();
            shieldLine.useWorldSpace = false;
            shieldLine.loop = true;
            shieldLine.widthMultiplier = 0.05f;
            shieldLine.positionCount = segments;
            // Usar el shader "Unlit/Color" para un color sólido
            Material mat = new Material(Shader.Find("Unlit/Color"));
            mat.color = shieldColor;
            shieldLine.material = mat;
            shieldLine.startColor = shieldColor;
            shieldLine.endColor = shieldColor;
            DrawCircle();

            // Agregar el CircleCollider2D y configurarlo como trigger para que no interfiera físicamente
            shieldCollider = gameObject.AddComponent<CircleCollider2D>();
            shieldCollider.radius = shieldRadius;
            shieldCollider.isTrigger = true;

            // Agregar Light2D para que el escudo emita luz azul (requiere URP 2D)
            Light2D light2d = gameObject.AddComponent<Light2D>();
            light2d.lightType = Light2D.LightType.Point;
            light2d.intensity = 1f;
            light2d.pointLightInnerRadius = shieldRadius;
            light2d.pointLightOuterRadius = shieldRadius * 1.5f;
            light2d.color = new Color(0f, 0f, 1f, 1f);
        }

        // Dibuja el círculo completo usando el LineRenderer
        private void DrawCircle()
        {
            float deltaTheta = (2f * Mathf.PI) / segments;
            float theta = 0f;
            for (int i = 0; i < segments; i++)
            {
                float x = shieldRadius * Mathf.Cos(theta);
                float y = shieldRadius * Mathf.Sin(theta);
                shieldLine.SetPosition(i, new Vector3(x, y, 0f));
                theta += deltaTheta;
            }
        }

        // Detecta la colisión con una bala (se asume que la bala tiene la etiqueta "Bullet")
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Bullet"))
            {
                // Determinar el punto de impacto relativo al centro del escudo
                Vector3 hitPoint = other.ClosestPoint(transform.position);
                Vector3 localHitPoint = transform.InverseTransformPoint(hitPoint);
                float hitAngle = Mathf.Atan2(localHitPoint.y, localHitPoint.x) * Mathf.Rad2Deg;

                // Iniciar la animación ripple en el ángulo del impacto
                StartCoroutine(SpawnRipple(hitAngle));

                // Destruir la bala
                Destroy(other.gameObject);
            }
        }

        // Corrutina para generar el efecto ripple (onda) en forma de arco que se expande y desvanece
        private IEnumerator SpawnRipple(float hitAngle)
        {
            // Crear un GameObject hijo para el efecto ripple
            GameObject ripple = new GameObject("ShieldRipple");
            ripple.transform.parent = transform;
            ripple.transform.localPosition = Vector3.zero;

            LineRenderer rippleLine = ripple.AddComponent<LineRenderer>();
            rippleLine.useWorldSpace = false;
            rippleLine.widthMultiplier = 0.05f;
            // Usar el shader "Unlit/Color" para el efecto ripple
            Material rippleMat = new Material(Shader.Find("Unlit/Color"));
            Color rippleColor = new Color(0.5f, 0.5f, 1f, 1f); // Azul más claro
            rippleMat.color = rippleColor;
            rippleLine.material = rippleMat;
            rippleLine.startColor = rippleColor;
            rippleLine.endColor = rippleColor;

            // Configurar el arco del ripple: un arco de 30° centrado en el ángulo de impacto
            float arcAngle = 30f;
            int arcSegments = 20;
            float startAngle = hitAngle - arcAngle / 2f;
            float endAngle = hitAngle + arcAngle / 2f;

            float currentRadius = shieldRadius;
            float targetRadius = shieldRadius * 1.5f; // El arco se expande un 50%
            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                // Interpolar el radio y la transparencia
                currentRadius = Mathf.Lerp(shieldRadius, targetRadius, t);
                float alpha = Mathf.Lerp(1f, 0f, t);
                rippleColor.a = alpha;
                rippleLine.startColor = rippleColor;
                rippleLine.endColor = rippleColor;
                rippleLine.material.color = rippleColor;

                // Actualizar los puntos del arco
                rippleLine.positionCount = arcSegments;
                float angleStep = (endAngle - startAngle) / (arcSegments - 1);
                for (int i = 0; i < arcSegments; i++)
                {
                    float angle = startAngle + i * angleStep;
                    float rad = angle * Mathf.Deg2Rad;
                    float x = currentRadius * Mathf.Cos(rad);
                    float y = currentRadius * Mathf.Sin(rad);
                    rippleLine.SetPosition(i, new Vector3(x, y, 0f));
                }

                yield return null;
            }

            Destroy(ripple);
        }
    }
}
