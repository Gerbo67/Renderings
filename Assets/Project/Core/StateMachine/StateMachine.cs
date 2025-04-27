using Project.Core.Interfaces;

namespace Project.Core.StateMachine
{
    /// <summary>
    /// Clase genérica que administra la máquina de estados.
    /// </summary>
    public class StateMachine
    {
        private IState currentState;

        /// <summary>
        /// Cambia el estado actual, llamando a OnExit() del anterior y OnEnter() del nuevo.
        /// </summary>
        public void ChangeState(IState newState)
        {
            currentState?.OnExit();
            currentState = newState;
            currentState?.OnEnter();
        }

        /// <summary>
        /// Se debe llamar en el Update() del MonoBehaviour que lo utiliza.
        /// </summary>
        public void Update()
        {
            currentState?.OnUpdate();
        }
    }
}