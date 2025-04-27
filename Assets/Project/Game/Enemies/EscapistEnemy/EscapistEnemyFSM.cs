using UnityEngine;
using UnityEngine.AI;
using Project.Core.StateMachine;
using Project.Game.Bullet.Scripts;

namespace Project.Game.Enemies.EscapistEnemy
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EscapistEnemyFSM : MonoBehaviour
    {
        // La máquina de estados genérica.
        public StateMachine stateMachine { get; private set; }

        [Header("NavMesh & Movimiento")]
        public NavMeshAgent agent;
        public float agentAcceleration = 30f;
        public float agentMaxSpeed = 3f;

        [Header("Posición de Disparo (Acercarse)")]
        public float shootApproachDistance = 3f;
        public float shootAlignmentThreshold = 0.5f;
        public float minShootingClearance = 1f;

        [Header("Parámetros para huir (Huir)")]
        public float fleeDetectionRadius = 5f;
        public float rushDistance = 3f;
        public float rushDuration = 3f;

        [Header("Disparo")]
        public GameObject enemyBulletPrefab;
        public Transform bulletSpawnPoint;
        public float shootCooldown = 1.5f;
        public float maxShootPositionTime = 2f;

        [Header("Detección de Atasco")]
        public float stuckVelocityThreshold = 0.1f;
        public float maxStuckTime = 1.5f;

        [Header("Shield Settings")]
        [Tooltip("Radio del escudo que se crea cuando el enemigo huye")]
        public float shieldRadius = 0.5f;
        private EnemyShieldController activeShield;

        // Variables internas para control de estados (visibles para los estados)
        [HideInInspector] public float shootTimer = 0f;
        [HideInInspector] public float stuckTimer = 0f;
        [HideInInspector] public float rushTimer = 0f;
        [HideInInspector] public float shootPositionTimer = 0f;

        [HideInInspector] public Vector3 currentTargetPosition;
        [HideInInspector] public Transform player;

        private void Awake()
        {
            if (agent == null)
                agent = GetComponent<NavMeshAgent>();

            // Configuración para 2D
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            agent.acceleration = agentAcceleration;
            agent.speed = agentMaxSpeed;

            stateMachine = new StateMachine();
        }

        private void Start()
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;

            // Estado inicial: Acercarse (equivalente a ShootPosition)
            stateMachine.ChangeState(new States.EstadoAcercarse(this));
        }

        private void Update()
        {
            if (player == null) return;

            shootTimer -= Time.deltaTime;

            // Actualiza la máquina de estados
            stateMachine.Update();

            FlipSpriteHorizontal();
        }

        // Métodos de ayuda para los estados:

        public bool ShouldFlee()
        {
            if (player == null) return false;
            float dist = Vector2.Distance(transform.position, player.position);
            return dist <= fleeDetectionRadius;
        }

        public void CreateShield()
        {
            if (activeShield == null)
            {
                GameObject shieldObj = new GameObject("EnemyShield");
                shieldObj.transform.SetParent(transform);
                shieldObj.transform.localPosition = Vector3.zero;
                activeShield = shieldObj.AddComponent<EnemyShieldController>();
                activeShield.shieldRadius = shieldRadius;
            }
        }

        public void DestroyShield()
        {
            if (activeShield != null)
            {
                Destroy(activeShield.gameObject);
                activeShield = null;
            }
        }

        public void SpawnBullet()
        {
            if (enemyBulletPrefab && bulletSpawnPoint)
            {
                GameObject bullet = Instantiate(enemyBulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
                float dir = (player.position.x < transform.position.x) ? -1f : 1f;
                BulletController bc = bullet.GetComponent<BulletController>();
                if (bc != null)
                    bc.SetDirection(dir);
            }
        }

        public Vector2 CalculateRushDirection()
        {
            Vector2 baseDir = ((Vector2)transform.position - (Vector2)player.position).normalized;
            float bestScore = -Mathf.Infinity;
            Vector2 bestDir = baseDir;
            int samples = 15;
            float angleRange = 90f;
            float checkDist = rushDistance * 2f;
            float clearanceWeight = 2f;
            float distanceWeight = 1f;

            for (int i = 0; i < samples; i++)
            {
                float angleOffset = Mathf.Lerp(-angleRange, angleRange, (float)i / (samples - 1));
                Vector2 candidateDir = Quaternion.Euler(0, 0, angleOffset) * baseDir;
                Vector2 candidatePos = (Vector2)transform.position + candidateDir * rushDistance;
                NavMeshHit navHit;
                if (!NavMesh.SamplePosition(candidatePos, out navHit, 1f, NavMesh.AllAreas))
                    continue;
                candidatePos = navHit.position;
                RaycastHit2D hit = Physics2D.Raycast(transform.position, candidateDir, checkDist);
                float clearance = (hit.collider != null) ? hit.distance : checkDist;
                float distFromPlayer = Vector2.Distance(candidatePos, player.position);
                float score = clearance * clearanceWeight + distFromPlayer * distanceWeight;
                if (score > bestScore)
                {
                    bestScore = score;
                    bestDir = candidateDir;
                }
            }
            return bestDir;
        }

        public void FlipSpriteHorizontal()
        {
            float dir = (player.position.x < transform.position.x) ? -1f : 1f;
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * dir;
            transform.localScale = scale;
        }

        private void OnDrawGizmos()
        {
            if (player != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(new Vector3(player.position.x, player.position.y, transform.position.z), shootApproachDistance);
            }
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, fleeDetectionRadius);
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(currentTargetPosition, 0.3f);
        }
    }
}
