using Project.Core.Interfaces;
using UnityEngine;
using UnityEngine.AI;

namespace Project.Game.Enemies.EscapistEnemy.States
{
    public class EstadoRush : IState
    {
        private EscapistEnemyFSM enemy;
        private float timer;

        public EstadoRush(EscapistEnemyFSM enemy)
        {
            this.enemy = enemy;
        }

        public void OnEnter()
        {
            Debug.Log("EstadoRush: OnEnter");
            timer = 0f;
        }

        public void OnUpdate()
        {
            timer += Time.deltaTime;
            float angle = Random.Range(0f, 360f);
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            Vector2 desiredPos = (Vector2)enemy.transform.position + dir * enemy.rushDistance;
            NavMeshHit hit;
            Vector3 finalPos = desiredPos;
            if (NavMesh.SamplePosition(desiredPos, out hit, 1f, NavMesh.AllAreas))
                finalPos = hit.position;
            enemy.agent.isStopped = false;
            enemy.agent.SetDestination(finalPos);
            enemy.currentTargetPosition = finalPos;

            if (timer >= 2f) // Tiempo arbitrario para recuperarse
            {
                enemy.stateMachine.ChangeState(new EstadoAcercarse(enemy));
            }
        }

        public void OnExit()
        {
            Debug.Log("EstadoRush: OnExit");
        }
    }
}