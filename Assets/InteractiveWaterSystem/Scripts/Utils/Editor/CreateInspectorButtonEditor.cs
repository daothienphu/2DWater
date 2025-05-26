#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Utils
{
    [CustomEditor(typeof(MonoBehaviour), true)]
    public class CreateInspectorButtonEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            var type = target.GetType();
            
            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var method in methods)
            {
                var attribute = method.GetCustomAttribute<CreateInspectorButtonAttribute>();
                if (attribute == null) continue;
                
                var buttonLabel = string.IsNullOrEmpty(attribute.ButtonLabel) ? method.Name : attribute.ButtonLabel;

                if (GUILayout.Button(buttonLabel))
                {
                    method.Invoke(target, null);
                }
            }
        }
    }
}
#endif