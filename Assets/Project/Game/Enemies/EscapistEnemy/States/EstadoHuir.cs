using Project.Core.Interfaces;
using UnityEngine;
using UnityEngine.AI;

namespace Project.Game.Enemies.EscapistEnemy.States
{
    public class EstadoHuir : IState
    {
        private EscapistEnemyFSM enemy;
        private float timer;

        public EstadoHuir(EscapistEnemyFSM enemy)
        {
            this.enemy = enemy;
        }

        public void OnEnter()
        {
            Debug.Log("EstadoHuir: OnEnter");
            timer = 0f;
            enemy.rushTimer = 0f;
            enemy.CreateShield();
        }

        public void OnUpdate()
        {
            timer += Time.deltaTime;
            enemy.agent.speed = enemy.agentMaxSpeed * 3f; // Incrementa la velocidad
            Vector2 rushDir = enemy.CalculateRushDirection();
            Vector2 desiredPos = (Vector2)enemy.transform.position + rushDir * enemy.rushDistance;
            NavMeshHit hit;
            Vector3 finalPos = desiredPos;
            if (NavMesh.SamplePosition(desiredPos, out hit, 1f, NavMesh.AllAreas))
                finalPos = hit.position;
            enemy.agent.isStopped = false;
            enemy.agent.SetDestination(finalPos);
            enemy.currentTargetPosition = finalPos;

            enemy.rushTimer += Time.deltaTime;
            if (enemy.rushTimer >= enemy.rushDuration)
            {
                enemy.rushTimer = 0f;
                enemy.DestroyShield();
                enemy.stateMachine.ChangeState(new EstadoAcercarse(enemy));
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
            Debug.Log("EstadoHuir: OnExit");
            enemy.DestroyShield();
        }
    }
}
