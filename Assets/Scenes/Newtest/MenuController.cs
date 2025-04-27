using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scenes.Newtest
{
    public class MenuController : MonoBehaviour
    {
        /// <summary>
        /// Llama a esta función desde tu botón “Start” para cargar la siguiente escena.
        /// </summary>
        /// <remarks>
        /// Asegúrate de que la escena “GameScene” (o como la llames) esté añadida en 
        /// File → Build Settings → Scenes In Build.
        /// </remarks>
        public void StartGame()
        {
            SceneManager.LoadScene("GameScene");
        }

        /// <summary>
        /// Llama a esta función desde tu botón “Quit” para salir de la aplicación.
        /// </summary>
        public void QuitGame()
        {
            Debug.Log("Quitting game...");
            Application.Quit();
        }
    }
}

