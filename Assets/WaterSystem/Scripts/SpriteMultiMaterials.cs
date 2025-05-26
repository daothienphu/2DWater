using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InteractiveWater
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteMultiMaterial : MonoBehaviour
    {

        [SerializeField] private SpriteRenderer _mainRenderer;

        //Using 2 separate fields so I can assign default values.
        [SerializeField] private Material _spriteUnlit;
        [SerializeField] private Material _spriteLit;
        //[SerializeField] private List<Material> _materials = new();

        private void OnValidate()
        {
            _mainRenderer = GetComponent<SpriteRenderer>();
            if (!_mainRenderer)
            {
                return;
            }
            
            SetMaterials(_mainRenderer);
        }

        private void OnEnable()
        {
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
        }

        private void OnDisable()
        {
            AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
        }
        
        private void Awake ()
        {
            _mainRenderer = GetComponent<SpriteRenderer>();
            if (!_mainRenderer)
            {
                return;
            }
            
            SetMaterials(_mainRenderer);
        }
        
        public void EditorSetMaterials()
        {
            if (!_mainRenderer)
            {
                _mainRenderer = GetComponent<SpriteRenderer>();
            }
            
            if (!_mainRenderer)
            {
                return;
            }
            
            SetMaterials(_mainRenderer);
        }

        private void OnAfterAssemblyReload()
        {
            if (!_mainRenderer)
            {
                return;
            }
            
            SetMaterials(_mainRenderer);
        }

        private void SetMaterials(SpriteRenderer sRenderer)
        {
            sRenderer.SetSharedMaterials(new List<Material>{_spriteUnlit, _spriteLit});
        }
    }
}