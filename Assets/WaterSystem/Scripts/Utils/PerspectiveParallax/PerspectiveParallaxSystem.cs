using UnityEngine;
using System.Collections.Generic;

namespace Utils
{
    public class PerspectiveParallaxSystem : MonoSingleton<PerspectiveParallaxSystem>
    {
        #region Fields

        [SerializeField] private PerspectiveParallaxConfig _parallaxConfig;
        public static List<PerspectiveParallaxLayer> Layers = new();
        
        #endregion

        #region Life Cycle

        protected override void OnStart()
        {
            GetLayers();
            ApplyConfigToLayers();
        }

        #endregion

        #region Public Methods

        [CreateInspectorButton("Apply Parallax Config")]
        public void EditorParallaxSetup()
        {
            GetLayers();
            ApplyConfigToLayers();
        }

        #endregion

        #region Private Methods

        private void GetLayers()
        {
            var layers = FindObjectsByType<PerspectiveParallaxLayer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var layer in layers)
            {
                Layers.Add(layer);
            }
        }

        private void ApplyConfigToLayers()
        {
            foreach (var layer in Layers)
            {
                //somehow it happens
                if (!layer)
                {
                    continue;
                }
                
                var layerConfig = _parallaxConfig.GetConfig(layer.SortingLayerID);

                //Sorting layer
                if (layer.TryGetComponent<SpriteRenderer>(out var renderer))
                {
                    renderer.sortingLayerID = layer.SortingLayerID;
                }
                
                //Position
                var oldPosition = layer.transform.position;
                layer.transform.position = new Vector3(oldPosition.x, oldPosition.y, layerConfig.ZCoordinate);
                
                //Tag
            }
        }

        #endregion
    }
}