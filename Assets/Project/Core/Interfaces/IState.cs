namespace Project.Core.Interfaces
{
    /// <summary>
    /// Interfaz base para los estados.
    /// </summary>
    public interface IState
    {
        void OnEnter();
        void OnUpdate();
        void OnExit();
    }
}