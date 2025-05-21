namespace Input
{
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class KInput
    {
        #region Camera

        private static Camera m_MainCamera;

        /// <summary>
        /// 获取当前相机
        /// </summary>
        public static Camera MainCamera
        {
            get
            {
                if (!m_MainCamera) m_MainCamera = UnityEngine.Camera.main;
                return m_MainCamera;
            }
        }

        #endregion

        #region InputAction

        public static KInputAction Action;
        public static KInputAction.PlayerActions Player => Action.Player;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void RuntimeReset()
        {
            // 访问 InputSystem 以触发InputSystem的初始化，避免KInput初始化时InputSystem未加载
            Debug.Log($"InputSystem version: {InputSystem.version}");

            Action = new KInputAction();

            //默认开启，有需要再关
            Action.Player.Enable();
        }

        #endregion
    }
}