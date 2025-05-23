#region

using System.IO;
using System.Runtime.CompilerServices;
using Debug = UnityEngine.Debug;

#endregion

namespace Tool
{
    public static class Logger
    {
        // 基础日志
        public static void Log(object message) => Debug.Log(message);

        // 带格式的日志
        public static void Log(string format, params object[] args)
            => Debug.Log(string.Format(format, args));

        // 带调用信息的调试日志
        public static void TLog(
            object message,
            [CallerMemberName] string member = "",
            [CallerFilePath] string path = "",
            [CallerLineNumber] int line = 0)
        {
            Debug.Log($"[Trace] {Path.GetFileName(path)}:{line} ({member}) {message}");
        }

        // 错误日志（始终保留）
        public static void Error(object message) => Debug.LogError(message);
    }
}