using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    [CreateAssetMenu(menuName = "Abyssalis/Parallax Config")]
    public class PerspectiveParallaxConfig : ScriptableObject
    {
        [Serializable]
        public struct LayerConfig
        {
            [SortingLayerDropdown]
            public int SortingLayerID;
            public float ZCoordinate;
        }
        
        public List<LayerConfig> Layers;

        public LayerConfig GetConfig(int sortingLayerID)
        {
            foreach (var layer in Layers)
            {
                if (layer.SortingLayerID == sortingLayerID)
                {
                    return layer;
                }
            }

            return default;
        }
    }
}