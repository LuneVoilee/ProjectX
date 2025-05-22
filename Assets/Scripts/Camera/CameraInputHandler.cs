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
        public float RotationSpeed = 20f;

        // 最小高度
        public float MinHeight = 30f;

        // 最大高度
        public float MaxHeight = 80f;

        // 最小俯角
        public float MinPitch = -80f;

        // 最大仰角
        public float MaxPitch = 80f;

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
            if (KInput.IsPointerOverUI()) return;

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
            HandleYawAndPitch();
            HandleZoom();
            HandleMovement();
        }

        private void HandleYawAndPitch()
        {
            var xyValue = m_PlayerAction.MouseMove.ReadValue<Vector2>();
            var xValue = xyValue.x;
            var yValue = xyValue.y;

            if (!Mathf.Approximately(xValue, 0f))
            {
                m_CameraTransform.Rotate(
                    Vector3.up,
                    xValue * RotationSpeed * Time.deltaTime,
                    Space.World
                );
            }

            if (!Mathf.Approximately(yValue, 0f))
            {
                var localAngles = m_CameraTransform.localEulerAngles;
                float currentPitch = localAngles.x > 180f ? localAngles.x - 360f : localAngles.x;
                float delta = yValue * RotationSpeed * Time.deltaTime;

                float newPitch = currentPitch - delta;
                newPitch = Mathf.Clamp(newPitch, MinPitch, MaxPitch);

                m_CameraTransform.localEulerAngles = new Vector3(
                    newPitch,
                    localAngles.y,
                    localAngles.z
                );
            }
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

            var moveDirection = (m_CameraTransform.right * readValue.x + m_CameraTransform.up * readValue.y)
                .normalized;
            moveDirection.y = 0;

            m_CameraTransform.position += moveDirection * (MoveSpeed * Time.deltaTime);
        }
    }
}