using UnityEngine;
using IslandSurvival.Core;
using IslandSurvival.Survival;

namespace IslandSurvival.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        public static FirstPersonController Instance { get; private set; }

        [Header("Movement")]
        public float walkSpeed = 4.2f;
        public float sprintSpeed = 7.2f;
        public float crouchSpeed = 2.2f;
        public float jumpHeight = 1.2f;
        public float gravity = -18f;
        public float acceleration = 12f;
        public float deceleration = 14f;

        [Header("Look")]
        public float mouseSensitivity = 1.6f;
        public float minPitch = -88f;
        public float maxPitch = 88f;
        public float standingHeight = 1.8f;
        public float crouchHeight = 1.1f;
        public Transform cameraPivot;

        CharacterController _cc;
        Vector3 _velocity;
        Vector3 _horizontal;
        float _pitch;
        bool _crouching;
        bool _grounded;
        SurvivalStats _stats;

        void Awake()
        {
            Instance = this;
            _cc = GetComponent<CharacterController>();
            _stats = GetComponent<SurvivalStats>();
            if (cameraPivot == null)
            {
                var cam = GetComponentInChildren<Camera>();
                if (cam) cameraPivot = cam.transform;
            }
        }

        void Start()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.SetState(GameState.Playing);
        }

        void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing)
                return;

            HandleLook();
            HandleMove();
        }

        void HandleLook()
        {
            float mx = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
            float my = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
            transform.Rotate(0f, mx, 0f);
            _pitch = Mathf.Clamp(_pitch - my, minPitch, maxPitch);
            if (cameraPivot) cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }

        void HandleMove()
        {
            _grounded = _cc.isGrounded;
            if (_grounded && _velocity.y < 0f) _velocity.y = -2f;

            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector3 wish = (transform.right * h + transform.forward * v).normalized;

            bool sprint = Input.GetKey(KeyCode.LeftShift) && v > 0.1f && !_crouching;
            bool wantCrouch = Input.GetKey(KeyCode.LeftControl);
            SetCrouch(wantCrouch);

            float targetSpeed = _crouching ? crouchSpeed : (sprint ? sprintSpeed : walkSpeed);
            if (sprint && _stats != null) _stats.DrainEnergy(8f * Time.deltaTime);
            if (sprint && _stats != null && _stats.Energy <= 1f) targetSpeed = walkSpeed;

            Vector3 targetVel = wish * targetSpeed;
            float rate = wish.sqrMagnitude > 0.01f ? acceleration : deceleration;
            _horizontal = Vector3.MoveTowards(_horizontal, targetVel, rate * Time.deltaTime);

            if (Input.GetButtonDown("Jump") && _grounded && !_crouching)
                _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            _velocity.y += gravity * Time.deltaTime;
            Vector3 motion = _horizontal + Vector3.up * _velocity.y;
            _cc.Move(motion * Time.deltaTime);
        }

        void SetCrouch(bool crouch)
        {
            if (_crouching == crouch) return;
            _crouching = crouch;
            float h = crouch ? crouchHeight : standingHeight;
            _cc.height = h;
            _cc.center = new Vector3(0f, h * 0.5f, 0f);
            if (cameraPivot)
            {
                var lp = cameraPivot.localPosition;
                lp.y = h - 0.15f;
                cameraPivot.localPosition = lp;
            }
        }

        public void Teleport(Vector3 pos, Quaternion rot)
        {
            _cc.enabled = false;
            transform.SetPositionAndRotation(pos, rot);
            _cc.enabled = true;
            _velocity = Vector3.zero;
            _horizontal = Vector3.zero;
        }
    }
}
