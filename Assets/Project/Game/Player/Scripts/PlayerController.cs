using Project.Game.Bullet.Scripts;
using UnityEngine;
using Unity.Cinemachine;

namespace Project.Game.Player.Scripts
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(PlayerInputHandler))]
    public class PlayerController : MonoBehaviour
    {
        private Rigidbody2D _rb;
        private Animator _animator;
        private PlayerInputHandler _input;
        private CinemachineImpulseSource _impulseSource;

        [SerializeField] private float speed = 3f;
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private Transform bulletSpawnPoint;


        private bool _isShooting;
        private bool _canShoot = true;
        private bool _shootKeyPressed;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
            _input = GetComponent<PlayerInputHandler>();
            _impulseSource = GetComponent<CinemachineImpulseSource>();
        }

        private void Update()
        {
            HandleInput();
            HandleShooting();
            UpdateAnimations();
        }

        private void FixedUpdate()
        {
            HandleMovement();
        }

        private void HandleInput()
        {
            _shootKeyPressed = Input.GetKeyDown(KeyCode.Space);
        }

        private void HandleShooting()
        {
            if (_shootKeyPressed && _canShoot)
            {
                _canShoot = false;
                _isShooting = true;
                _animator.Play("PlayerShootingAnim", -1, 0f);
            }

            _shootKeyPressed = false;
        }

        private void HandleMovement()
        {
            var move = _isShooting ? Vector2.zero : _input.MovementInput.normalized;
            _rb.linearVelocity = move * speed;

            if (move.x != 0 && !_isShooting)
                transform.localScale = new Vector3(Mathf.Sign(move.x), 1, 1);
        }

        private void UpdateAnimations()
        {
            bool isRunning = _rb.linearVelocity.sqrMagnitude > 0.01f && !_isShooting;
            _animator.SetBool("IsRun", isRunning);
            _animator.SetBool("IsShoot", _isShooting);
        }
        
        public void SpawnBullet()
        {
            // Instancia la bala en la posición/rotación del spawnPoint
            var newBullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);

            // Calcular la dirección (der = +1, izq = -1) basado en la escala del Player
            var direction = Mathf.Sign(transform.localScale.x);

            // Asignar la dirección a la bala
            BulletController bulletCtrl = newBullet.GetComponent<BulletController>();
            bulletCtrl.SetDirection(direction);
        }


        // llamado por evento al finalizar animación para habilitar otro disparo
        public void EnableShooting()
        {
            _canShoot = true;
        }

        // se llama al final de animación (evento animación)
        // para liberar estado disparando actual
        public void EndShootingAnimation()
        {
            _isShooting = false;
            _input.ResetShoot();
        }

        // llamado por la animación para la sacudida
        public void ShootCameraShake()
        {
            _impulseSource.GenerateImpulse();
        }
    }
}