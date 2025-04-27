using Project.Core.Interfaces;
using UnityEngine;
using UnityEngine.AI;

namespace Project.Game.Enemies.EscapistEnemy.States
{
    public class EstadoAcercarse : IState
    {
        private EscapistEnemyFSM enemy;

        public EstadoAcercarse(EscapistEnemyFSM enemy)
        {
            this.enemy = enemy;
        }

        public void OnEnter()
        {
            Debug.Log("EstadoAcercarse: OnEnter");
            enemy.shootPositionTimer = 0f;
            enemy.shootTimer = 0f;
        }

        public void OnUpdate()
        {
            // Comportamiento de acercarse (equivalente a ShootPosition)
            enemy.agent.speed = enemy.agentMaxSpeed;
            float z = enemy.transform.position.z;
            float desiredX = (enemy.transform.position.x < enemy.player.position.x)
                ? enemy.player.position.x - enemy.shootApproachDistance
                : enemy.player.position.x + enemy.shootApproachDistance;
            Vector3 desiredPos = new Vector3(desiredX, enemy.player.position.y, z);

            float horDir = (enemy.transform.position.x < enemy.player.position.x) ? -1f : 1f;
            RaycastHit2D hit = Physics2D.Raycast(new Vector2(enemy.player.position.x, enemy.player.position.y), new Vector2(horDir, 0), enemy.shootApproachDistance);
            if (hit.collider != null)
            {
                float clearance = hit.distance;
                if (clearance < enemy.minShootingClearance)
                {
                    // Si no hay suficiente clearance, cambia a estado Huir.
                    enemy.stateMachine.ChangeState(new EstadoHuir(enemy));
                    return;
                }
                desiredX = enemy.player.position.x + horDir * clearance;
                desiredPos = new Vector3(desiredX, enemy.player.position.y, z);
            }

            NavMeshHit navHit;
            Vector3 finalPos = desiredPos;
            if (NavMesh.SamplePosition(desiredPos, out navHit, 1f, NavMesh.AllAreas))
                finalPos = navHit.position;
            enemy.agent.isStopped = false;
            enemy.agent.SetDestination(finalPos);
            enemy.currentTargetPosition = finalPos;

            float dist = Vector2.Distance(new Vector2(enemy.transform.position.x, enemy.transform.position.y),
                new Vector2(finalPos.x, finalPos.y));
            if (dist < enemy.shootAlignmentThreshold || enemy.shootPositionTimer >= enemy.maxShootPositionTime)
            {
                if (enemy.shootTimer <= 0f)
                {
                    enemy.SpawnBullet();
                    enemy.shootTimer = enemy.shootCooldown;
                    enemy.shootPositionTimer = 0f;
                }
            }
            else
            {
                enemy.shootPositionTimer += Time.deltaTime;
            }

            // Verifica si debe cambiar a huir
            if (enemy.ShouldFlee())
            {
                enemy.stateMachine.ChangeState(new EstadoHuir(enemy));
            }

            // Verifica atasco
            if (enemy.agent.velocity.magnitude < enemy.stuckVelocityThreshold)
            {
                enemy.stuckTimer += Time.deltaTime;
                if (enemy.stuckTimer >= enemy.maxStuckTime)
                {
                    enemy.stateMachine.ChangeState(new EstadoRush(enemy));
                    enemy.stuckTimer = 0f;
                }
            }
            else
            {
                enemy.stuckTimer = 0f;
            }
        }

        public void OnExit()
        {
            Debug.Log("EstadoAcercarse: OnExit");
        }
    }
}
