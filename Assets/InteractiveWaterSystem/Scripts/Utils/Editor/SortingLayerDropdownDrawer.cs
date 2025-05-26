#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Utils
{
    [CustomPropertyDrawer(typeof(SortingLayerDropdownAttribute), true)]
    public class SortingLayerDropdownDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Integer)
            {
                EditorGUI.LabelField(position, label.text, "SortingLayer");
                return;
            }
            
            EditorGUI.BeginProperty(position, label, property);

            var layers = SortingLayer.layers;
            var layerNames = new string[layers.Length];
            var layerIDs = new int[layers.Length];

            var currentIndex = -1;
            for (var i = 0; i < layers.Length; i++)
            {
                layerNames[i] = layers[i].name;
                layerIDs[i] = layers[i].id;

                if (property.intValue == layerIDs[i])
                {
                    currentIndex = i;
                }
            }
            
            var selected = EditorGUI.Popup(position, label.text, currentIndex, layerNames);
            if (selected >= 0 && selected < layerIDs.Length)
            {
                property.intValue = layerIDs[selected];
            }

            EditorGUI.EndProperty();
        }
    }
}
#endif