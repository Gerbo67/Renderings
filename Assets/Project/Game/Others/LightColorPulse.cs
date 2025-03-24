using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Project.Game.Others
{
    public class LightColorPulse : MonoBehaviour
    {
        [SerializeField] private Light2D light2D;
        [SerializeField] private Color colorA = Color.red;
        [SerializeField] private Color colorB = Color.blue;
        [SerializeField] private float pulseSpeed = 1f;

        void Update()
        {
            var t = Mathf.PingPong(Time.time * pulseSpeed, 1f);
            light2D.color = Color.Lerp(colorA, colorB, t);
        }
    }
}
