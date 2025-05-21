using Map;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomPropertyDrawer(typeof(HexCoordinates))]
    public class HexCoordinatesDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var X = property.FindPropertyRelative("m_X").intValue;
            var Z = property.FindPropertyRelative("m_Z").intValue;
            var Y = -X - Z;

            EditorGUI.LabelField(position, label.text, "(" + X + ", " + Y + ", " + Z + ")");
        }
    }
}