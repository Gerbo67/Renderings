namespace Project.Core.Interfaces
{
    public interface IDamageable
    {
        /// <summary>
        /// Aplica daño a esta entidad.
        /// </summary>
        /// <param name="amount">Cantidad de vida a restar.</param>
        void TakeDamage(int amount);
    }
}