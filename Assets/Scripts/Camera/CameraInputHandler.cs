#region

using System;
using Input;
using Map;
using Tool;
using UnityEngine;
using UnityEngine.InputSystem;

#endregion

namespace Camera
{
    public class CameraInputHandler : MonoBehaviour
    {
        public Action<Vector3> OnClickAction;
        public Action OnShowAction;
        private KInputAction.PlayerActions m_PlayerInput;
        private Transform m_CameraTransform;

        private void Awake()
        {
            m_PlayerInput = KInput.Player;
            m_CameraTransform = gameObject.transform;
            m_CameraTransform.forward = Vector3.down;
            m_CameraTransform.position = new(0, CameraHeight, 0);
        }

        private void OnEnable()
        {
            m_PlayerInput.Click.performed += OnClick;
            m_PlayerInput.ShowCoordinates.performed += OnShow;
        }

        private void OnDisable()
        {
            m_PlayerInput.Click.performed -= OnClick;
            m_PlayerInput.ShowCoordinates.performed -= OnShow;
        }

        private void OnShow(InputAction.CallbackContext context)
        {
            OnShowAction?.Invoke();
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
                Drawer.Instance.Ray(inputRay.origin, hit.point, Color.red, 20);
                OnClickAction?.Invoke(hit.point);
            }
        }

        private void Update()
        {
            HandleYawAndPitchByMouse();
            HandleZoom();
            HandleMovement();
        }


        #region Rotate

        // 旋转速度
        public float RotationSpeed = 5f;

        // 最小俯角
        public float MinPitch;

        // 最大仰角
        public float MaxPitch = 90f;

        public float MinYaw = -60f;

        public float MaxYaw = 60f;

        private void HandleYawAndPitchByMouse()
        {
            var xyValue = m_PlayerInput.MouseMove.ReadValue<Vector2>();
            var xValue_KeyBoard = m_PlayerInput.Rotate.ReadValue<float>();

            var xValue_Mouse = xyValue.x;
            var yValue = xyValue.y;

            var xValue = Mathf.Abs(xValue_Mouse) > Mathf.Abs(xValue_KeyBoard) ? xValue_Mouse : xValue_KeyBoard * 10;

            var localAngles = m_CameraTransform.localEulerAngles;
            var currentPitch = localAngles.x > 180f ? localAngles.x - 360f : localAngles.x;
            var currentYaw = localAngles.y > 180f ? localAngles.y - 360f : localAngles.y;

            if (Mathf.Abs(xValue) >= 0.15f)
            {
                var deltaYaw = xValue * RotationSpeed * Time.deltaTime;
                var newYaw = currentYaw + deltaYaw;
                newYaw = Mathf.Clamp(newYaw, MinYaw, MaxYaw);
                m_CameraTransform.localEulerAngles = new Vector3(
                    localAngles.x,
                    newYaw,
                    localAngles.z
                );
            }

            if (Mathf.Abs(yValue) >= 0.1f)
            {
                var deltaPitch = yValue * RotationSpeed * Time.deltaTime;
                var newPitch = currentPitch - deltaPitch;
                newPitch = Mathf.Clamp(newPitch, MinPitch, MaxPitch);

                m_CameraTransform.localEulerAngles = new Vector3(
                    newPitch,
                    m_CameraTransform.localEulerAngles.y,
                    localAngles.z
                );
            }
        }

        #endregion


        #region Zoom

        //摄像机高度
        public float CameraHeight = 50;

        // 最小高度
        public float MinHeight = 30f;

        // 最大高度
        public float MaxHeight = 80f;

        // 最大缩放速度
        public float MaxZoomSpeed = 50f;

        // 最小缩放速度
        public float MinZoomSpeed = 200f;

        //到达最大速度需要的时间
        public float MaxSpeedTime = 2f;

        private float m_ContinueTime;

        private void HandleZoom()
        {
            var scroll = m_PlayerInput.Scroll.ReadValue<Vector2>().y;
            if (scroll == 0)
            {
                m_ContinueTime = 0;
                return;
            }

            m_ContinueTime += Time.deltaTime;

            var timeFactor = Mathf.Clamp01(m_ContinueTime / MaxSpeedTime);
            var ZoomSpeed = Mathf.Lerp(MinZoomSpeed, MaxZoomSpeed, timeFactor);

            CameraHeight -= scroll * ZoomSpeed * Time.deltaTime;
            CameraHeight = Mathf.Clamp(CameraHeight, MinHeight, MaxHeight);

            m_CameraTransform.position = new Vector3(
                m_CameraTransform.position.x,
                CameraHeight,
                m_CameraTransform.position.z
            );
        }

        #endregion

        #region Move

        //移动速度
        public float MoveSpeedMinZoom = 400;
        public float MoveSpeedMaxZoom = 100;

        private void HandleMovement()
        {
            var readValue = m_PlayerInput.Move.ReadValue<Vector2>();
            if (readValue.magnitude.Equals(0)) return;

            var moveDirection = (m_CameraTransform.right * readValue.x + m_CameraTransform.up * readValue.y)
                .normalized;

            moveDirection.y = 0;

            var zoomPercent = (CameraHeight - MinHeight) / (MaxHeight - MinHeight);
            var MoveSpeed = Mathf.Lerp(MoveSpeedMaxZoom, MoveSpeedMinZoom, zoomPercent);

            m_CameraTransform.position =
                ClampPosition(m_CameraTransform.position + moveDirection * (MoveSpeed * Time.deltaTime));
        }

        private static Vector3 ClampPosition(Vector3 position)
        {
            var xMax =
                (HexMapGenerateSystem.Instance.ChunkCountX * HexUtil.ChunkSizeX - 0.5f) *
                (2f * HexUtil.InnerRadius);

            position.x = Mathf.Clamp(position.x, 0f, xMax);

            var zMax =
                (HexMapGenerateSystem.Instance.ChunkCountZ * HexUtil.ChunkSizeZ - 1f) *
                (1.5f * HexUtil.OuterRadius);

            position.z = Mathf.Clamp(position.z, 0f, zMax);

            return position;
        }

        #endregion
    }
}