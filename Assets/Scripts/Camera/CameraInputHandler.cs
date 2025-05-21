using System;
using Input;
using Map;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Camera
{
    public class CameraInputHandler : MonoBehaviour
    {
        //移动速度
        public float MoveSpeed = 500;

        //摄像机高度
        public float CameraHeight = 50;

        // 缩放速度
        public float ZoomSpeed = 10f;

        // 旋转速度
        public float RotationSpeed = 50f;

        // 最小高度
        public float MinHeight = 30f;

        // 最大高度
        public float MaxHeight = 80f;

        public Action<Vector3> m_OnClickAction;

        private KInputAction.PlayerActions m_PlayerAction;
        private Transform m_CameraTransform;

        private void Awake()
        {
            m_PlayerAction = KInput.Player;
            m_CameraTransform = gameObject.transform;
            m_CameraTransform.forward = Vector3.down;
            m_CameraTransform.position = new(0, CameraHeight, 0);
        }

        private void OnEnable()
        {
            m_PlayerAction.Click.performed += OnClick;
        }

        private void OnDisable()
        {
            m_PlayerAction.Click.performed -= OnClick;
        }

        private void OnClick(InputAction.CallbackContext context)
        {
            var mainCamera = KInput.MainCamera;
            if (!mainCamera) return;

            var mousePosition = Mouse.current.position.ReadValue();

            var inputRay = mainCamera.ScreenPointToRay(new Vector3(mousePosition.x, mousePosition.y, 0f));
            RaycastHit hit;
            if (Physics.Raycast(inputRay, out hit))
            {
                m_OnClickAction?.Invoke(hit.point);
            }
        }

        private void Update()
        {
            HandleRotation();
            HandleZoom();
            HandleMovement();
        }

        private void HandleRotation()
        {
            var readValue = m_PlayerAction.Rotate.ReadValue<float>();

            if (readValue.Equals(0)) return;

            m_CameraTransform.Rotate(
                Vector3.up,
                readValue * RotationSpeed * Time.deltaTime,
                Space.World
            );
        }

        private void HandleZoom()
        {
            var scroll = m_PlayerAction.Scroll.ReadValue<Vector2>().y;
            if (scroll == 0) return;

            CameraHeight -= scroll * ZoomSpeed * Time.deltaTime;
            CameraHeight = Mathf.Clamp(CameraHeight, MinHeight, MaxHeight);

            m_CameraTransform.position = new Vector3(
                m_CameraTransform.position.x,
                CameraHeight,
                m_CameraTransform.position.z
            );
        }

        private void HandleMovement()
        {
            var readValue = m_PlayerAction.Move.ReadValue<Vector2>();
            if (readValue.magnitude.Equals(0)) return;

            var moveDirection = (Vector3.right * readValue.x + Vector3.forward * readValue.y).normalized;
            m_CameraTransform.position += moveDirection * (MoveSpeed * Time.deltaTime);
        }
    }
}