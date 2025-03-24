using UnityEngine;
using UnityEngine.AI;
using Project.Game.Bullet.Scripts; // Asegúrate de que la ruta sea correcta

namespace Project.Game.Enemies.EscapistEnemy
{
    public enum EnemyState { ShootPosition, Rush, Stuck }

    [RequireComponent(typeof(NavMeshAgent))]
    public class EscapistEnemyController : MonoBehaviour
    {
        [Header("NavMesh & Movimiento")]
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private float agentAcceleration = 30f;
        [SerializeField] private float agentMaxSpeed = 3f;

        [Header("Posición de Disparo (ShootPosition)")]
        [SerializeField] private float shootApproachDistance = 3f;    // Referencia horizontal ideal (círculo rojo)
        [SerializeField] private float shootAlignmentThreshold = 0.5f;  // Umbral para considerar que se alcanzó la posición ideal
        [SerializeField] private float minShootingClearance = 1f;       // Espacio mínimo requerido para disparar

        [Header("Rush Settings")]
        [SerializeField] private float fleeDetectionRadius = 5f;  // Si el jugador se acerca, se activa Rush
        [SerializeField] private float rushDistance = 3f;         // Distancia base para la huida en Rush
        [SerializeField] private float rushDuration = 3f;         // Tiempo máximo en estado Rush

        [Header("Disparo")]
        [SerializeField] private GameObject enemyBulletPrefab;
        [SerializeField] private Transform bulletSpawnPoint;
        [SerializeField] private float shootCooldown = 1.5f;
        [SerializeField] private float maxShootPositionTime = 2f; // Tiempo máximo en ShootPosition antes de disparar

        [Header("Detección de Atasco")]
        [SerializeField] private float stuckVelocityThreshold = 0.1f;
        [SerializeField] private float maxStuckTime = 1.5f;

        // Estado interno
        private EnemyState currentState;
        private Transform player;
        private float shootTimer = 0f;
        private float stuckTimer = 0f;
        private float rushTimer = 0f;
        private float shootPositionTimer = 0f;
        private Vector3 currentTargetPosition;

        private void Awake()
        {
            if (agent == null)
                agent = GetComponent<NavMeshAgent>();

            // Configuración para 2D
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            agent.acceleration = agentAcceleration;
            agent.speed = agentMaxSpeed;
        }

        private void Start()
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;

            currentState = EnemyState.ShootPosition;
        }

        private void Update()
        {
            if (player == null) return;
            shootTimer -= Time.deltaTime;
            float distToPlayer = Vector2.Distance(transform.position, player.position);

            // Si el jugador se acerca (dentro de fleeDetectionRadius), activamos Rush; si no, ShootPosition.
            if (distToPlayer <= fleeDetectionRadius)
            {
                currentState = EnemyState.Rush;
                rushTimer += Time.deltaTime;
            }
            else
            {
                rushTimer = 0f;
                currentState = EnemyState.ShootPosition;
            }

            // Detección de atasco (en ShootPosition o Rush)
            if ((currentState == EnemyState.ShootPosition || currentState == EnemyState.Rush) && agent.velocity.magnitude < stuckVelocityThreshold)
                stuckTimer += Time.deltaTime;
            else
                stuckTimer = 0f;
            if (stuckTimer >= maxStuckTime)
            {
                currentState = EnemyState.Stuck;
                stuckTimer = 0f;
            }
            if (currentState == EnemyState.Stuck && agent.velocity.magnitude >= stuckVelocityThreshold * 2f)
                currentState = EnemyState.ShootPosition;

            // Ejecutar comportamiento según estado
            switch (currentState)
            {
                case EnemyState.ShootPosition:
                    HandleShootPosition();
                    break;
                case EnemyState.Rush:
                    HandleRush();
                    break;
                case EnemyState.Stuck:
                    HandleStuck();
                    break;
            }

            FlipSpriteHorizontal();
        }

        // Estado ShootPosition: intenta posicionarse para disparar
        private void HandleShootPosition()
        {
            agent.speed = agentMaxSpeed; // Velocidad normal
            float z = transform.position.z;
            float desiredX = (transform.position.x < player.position.x)
                ? player.position.x - shootApproachDistance
                : player.position.x + shootApproachDistance;
            Vector3 desiredPos = new Vector3(desiredX, player.position.y, z);

            // Verificar clearance horizontal (raycast desde el jugador)
            float horDir = (transform.position.x < player.position.x) ? -1f : 1f;
            RaycastHit2D hit = Physics2D.Raycast(new Vector2(player.position.x, player.position.y), new Vector2(horDir, 0), shootApproachDistance);
            if (hit.collider != null)
            {
                float clearance = hit.distance;
                if (clearance < minShootingClearance)
                {
                    currentState = EnemyState.Rush;
                    return;
                }
                desiredX = player.position.x + horDir * clearance;
                desiredPos = new Vector3(desiredX, player.position.y, z);
            }

            NavMeshHit navHit;
            Vector3 finalPos = desiredPos;
            if (NavMesh.SamplePosition(desiredPos, out navHit, 1f, NavMesh.AllAreas))
                finalPos = navHit.position;
            agent.isStopped = false;
            agent.SetDestination(finalPos);
            currentTargetPosition = finalPos;

            float dist = Vector2.Distance(new Vector2(transform.position.x, transform.position.y),
                                            new Vector2(finalPos.x, finalPos.y));
            if (dist < shootAlignmentThreshold || shootPositionTimer >= maxShootPositionTime)
            {
                if (shootTimer <= 0f)
                {
                    SpawnBullet();
                    shootTimer = shootCooldown;
                    shootPositionTimer = 0f;
                }
            }
            else
            {
                shootPositionTimer += Time.deltaTime;
            }
        }

        // Estado Rush: el enemigo huye buscando la ruta con más clearance para luego re-posicionarse para disparar  
        private void HandleRush()
        {
            // Aumenta la velocidad (por ejemplo, 3×)
            agent.speed = agentMaxSpeed * 3f;
            Vector2 rushDir = CalculateRushDirection();
            Vector2 desiredPos = (Vector2)transform.position + rushDir * rushDistance;
            NavMeshHit hit;
            Vector3 finalPos = desiredPos;
            if (NavMesh.SamplePosition(desiredPos, out hit, 1f, NavMesh.AllAreas))
                finalPos = hit.position;
            agent.isStopped = false;
            agent.SetDestination(finalPos);
            currentTargetPosition = finalPos;
            if (rushTimer >= rushDuration)
            {
                rushTimer = 0f;
                currentState = EnemyState.ShootPosition;
            }
        }

        // Estado Stuck: si se queda atascado, elige una dirección aleatoria para salir
        private void HandleStuck()
        {
            float angle = Random.Range(0f, 360f);
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            Vector2 desiredPos = (Vector2)transform.position + dir * rushDistance;
            NavMeshHit hit;
            Vector3 finalPos = desiredPos;
            if (NavMesh.SamplePosition(desiredPos, out hit, 1f, NavMesh.AllAreas))
                finalPos = hit.position;
            agent.isStopped = false;
            agent.SetDestination(finalPos);
            currentTargetPosition = finalPos;
        }

        // Función que evalúa candidatos en Rush y devuelve la dirección con mayor clearance y distancia al jugador
        private Vector2 CalculateRushDirection()
        {
            Vector2 baseDir = ((Vector2)transform.position - (Vector2)player.position).normalized;
            float bestScore = -Mathf.Infinity;
            Vector2 bestDir = baseDir;
            int samples = 15;
            float angleRange = 90f; // Evaluar ±90° alrededor de la dirección base
            float checkDist = rushDistance * 2f;
            float clearanceWeight = 2f; // Pondera más el clearance
            float distanceWeight = 1f;  // Pondera la distancia al jugador

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

        private void SpawnBullet()
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

        private void FlipSpriteHorizontal()
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
