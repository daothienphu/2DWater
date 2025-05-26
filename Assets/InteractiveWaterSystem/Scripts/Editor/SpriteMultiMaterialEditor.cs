#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace InteractiveWater.WaterSystem
{
    [CustomEditor(typeof(SpriteMultiMaterial)), CanEditMultipleObjects]
    public class SpriteMultiMaterialEditor : Editor
    {
        private SpriteMultiMaterial _smm;
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (!_smm)
            {
                _smm = (SpriteMultiMaterial)target;
            }
            
            if (GUILayout.Button("Set Materials"))
            {
                _smm.EditorSetMaterials();
            }
        }
    }
}
#endif