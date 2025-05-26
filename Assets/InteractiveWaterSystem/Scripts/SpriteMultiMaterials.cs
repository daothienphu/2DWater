using System.Collections.Generic;
using UnityEngine;

namespace InteractiveWater
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteMultiMaterial : MonoBehaviour
    {
        #region Fields

        [SerializeField] private SpriteRenderer _mainRenderer;

        //I use 2 separate fields, so I can assign default values for this specific project.
        //If you use this for a more general project for some reason, it may be a good idea to use a list instead.
        [SerializeField] private Material _spriteUnlit;
        [SerializeField] private Material _spriteLit;
        //[SerializeField] private List<Material> _materials = new();

        #endregion

        #region Lifecyle

        private void OnValidate()
        {
            SetMaterials();
        }
        
        private void Awake ()
        {
            SetMaterials();
        }

        #endregion

        #region Public Methods

        public void EditorSetMaterials()
        {
            SetMaterials();
        }

        #endregion

        #region Private Methods

        private void SetMaterials()
        {
            if (!_mainRenderer)
            {
                _mainRenderer = GetComponent<SpriteRenderer>();
            }
            
            if (!_mainRenderer)
            {
                return;
            }
            
            _mainRenderer.SetSharedMaterials(new List<Material>{_spriteUnlit, _spriteLit});
        }

        #endregion
    }
}