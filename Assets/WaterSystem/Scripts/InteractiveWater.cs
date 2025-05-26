using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Utils;
using Random = UnityEngine.Random;

namespace InteractiveWater
{
    public class InteractiveWater : MonoBehaviour
    {
        #region Structs
        
        private struct Ripple
        {
            public Ripple(Vector2 center, float initialStrength, bool initialUp)
            {
                CenterInUVSpace = center;
                InitialStrength = initialStrength;
                InitialUp = initialUp;
            }

            public Vector2 CenterInUVSpace;
            public float InitialStrength;
            public bool InitialUp;
        }
        
        #endregion
        
        #region Fields

        private static readonly int WaterMeshDepth = Shader.PropertyToID("_WaterMeshDepth");
        private static readonly int ImpactHeight = Shader.PropertyToID("_ImpactHeight");
        private const string WaterTopMeshName = "WaterTopMesh";
        private const string WaterFrontMeshName = "WaterFrontMesh";

        [Header("Mesh Settings")] 
        [Tooltip("The x and z size of the top water mesh.")]
        [SerializeField] private Vector2 _topSurfaceSize = new (20, 6.5f);
        [Tooltip("The y size of the front water mesh, the x size is defined in Top Surface Size.")]
        [SerializeField] private float _frontSurfaceHeight = 10f;
        [Tooltip("The x and z vertex count of the top water mesh, ideally this should be a multiple of the size.")]
        [SerializeField] private Vector2Int _topSurfaceVerticesCount = new(200, 130);

        [Header("Textures")]
        [SerializeField] private CustomRenderTexture _ambientWaveTexture;
        [SerializeField] private CustomRenderTexture _rippleSimulationTexture;
        [SerializeField] private RenderTexture _reflectionTexture;
        
        [Header("Materials")]
        [SerializeField] private Material _topMeshMaterial;
        [SerializeField] private Material _frontMeshMaterial;
        [SerializeField] private Material _ambientWaveMaterial;
        [SerializeField] private Material _rippleSimulationMaterial;
        
        [Header("Other Settings")] 
        [SortingLayerDropdown] 
        [SerializeField] private int _topMeshSortingLayer;
        [SortingLayerDropdown]
        [SerializeField] private int _frontMeshSortingLayer;
        [Tooltip("How many times the texture updates in one update call.")]
        [Range(1, 8)]
        [SerializeField] private int _rippleSimulationIterationPerFrame = 5;
        [Tooltip("Allow clicking on the surface to see ripples.")]
        [SerializeField] private bool _testRipples;
        [SerializeField] private LayerMask _testWaterLayer;
        
        private Transform _topMesh;
        private Transform _frontMesh;
        private readonly Queue<Ripple> _queuedRipples = new();
        private Camera _testRippleCamera;
        
        #endregion

        #region Life Cycle

        private void OnEnable()
        {
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
        }

        private void OnDisable()
        {
            AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
        }

        private void Start()
        {
            Init();
            if (!_testRippleCamera) _testRippleCamera = Camera.main;
        }

        private void FixedUpdate()
        {
            _rippleSimulationTexture.ClearUpdateZones();
            UpdateNewRipples();
            _rippleSimulationTexture.Update(_rippleSimulationIterationPerFrame);
        }

        private void Update()
        {
            if (!_testRipples) return;
            
            var left = Input.GetMouseButtonDown(0);
            var right = Input.GetMouseButtonDown(1);
            
            if (!left && !right) return;
            
            if (!_testRippleCamera) _testRippleCamera = Camera.main;
            var ray = _testRippleCamera.ScreenPointToRay(Input.mousePosition);

            if (!Physics.Raycast(ray, out var hit, Mathf.Infinity, _testWaterLayer)) return;
            
            var point = hit.point;
            var strength = Random.Range(0.2f, 0.6f);

            CreateContactRippleAt(point, strength, left);
        }

        #endregion
        
        #region Public Methods

        [CreateInspectorButton("Generate Mesh")]
        public void EditorGeneratePreviewMesh()
        {
            Init();
        }
        
        /// <summary>
        /// Creates a ripple on the water surface.
        /// </summary>
        /// <param name="worldPosition">The center of the ripple.</param>
        /// <param name="initialStrength">How strong the initial ripple is, from 0f to 1f.</param>
        /// <param name="initialUp">Set to true if the initial ripple should point upwards, false otherwise.</param>
        public void CreateContactRippleAt(Vector3 worldPosition, float initialStrength, bool initialUp = true)
        {
            var localSpace = _topMesh.InverseTransformPoint(worldPosition);
            var uvSpace = new Vector2(localSpace.x / _topSurfaceSize.x, 1 - localSpace.z / _topSurfaceSize.y);
            
            _queuedRipples.Enqueue(new Ripple(uvSpace, initialStrength, initialUp));
        }

        #endregion

        #region Private Methods

        private void Init()
        {
            SetShaderParams();
            GenerateWaterMeshes();
            _rippleSimulationTexture.Initialize();
        }
        
        private void UpdateNewRipples()
        {
            if (_queuedRipples.Count <= 0) return;

            var ripple = _queuedRipples.Dequeue();
                
            _rippleSimulationMaterial.SetFloat(ImpactHeight, ripple.InitialStrength / 5);

            var defaultZone = new CustomRenderTextureUpdateZone
            {
                needSwap = true,
                passIndex = 0,
                rotation = 0,
                updateZoneCenter = new Vector3(0.5f, 0.5f),
                updateZoneSize = new Vector3(1f, 1f)
            };

            var impactZone = new CustomRenderTextureUpdateZone
            {
                needSwap = true,
                passIndex = ripple.InitialUp ? 1 : 2,
                rotation = 0,
                updateZoneCenter = new Vector3(ripple.CenterInUVSpace.x, ripple.CenterInUVSpace.y),
                updateZoneSize = new Vector3(0.01f, 0.01f)
            };
                
            _rippleSimulationTexture.SetUpdateZones(new [] { defaultZone , impactZone});
        }
        
        private void SetShaderParams()
        {
            _topMeshMaterial.SetFloat(WaterMeshDepth, _topSurfaceSize.y);
            _frontMeshMaterial.SetFloat(WaterMeshDepth, _topSurfaceSize.y);
        }
        
        private void GenerateWaterMeshes()
        {
            MeshFilter topMF, frontMF;
            MeshRenderer topMR, frontMR;
            SortingGroup topSG, frontSG;
            
            if (transform.childCount >= 2) //The 3rd child is the camera for planar reflection 
            {
                _topMesh = transform.GetChild(0);
                _frontMesh = transform.GetChild(1);
                
                topMF = _topMesh.GetComponent<MeshFilter>();
                topMR = _topMesh.GetComponent<MeshRenderer>();
                topSG = _topMesh.GetComponent<SortingGroup>();
                
                frontMF = _frontMesh.GetComponent<MeshFilter>();
                frontMR = _frontMesh.GetComponent<MeshRenderer>();
                frontSG = _frontMesh.GetComponent<SortingGroup>();
                
                // if (_testRipples && _topMesh.TryGetComponent(out BoxCollider2D bc2d))
                // {
                //     DestroyImmediate(bc2d);
                //     _topMesh.AddComponent<BoxCollider>();
                // }
                // else if (!_testRipples && _topMesh.TryGetComponent(out BoxCollider bc))
                // {
                //     DestroyImmediate(bc);
                //     var collider = _topMesh.AddComponent<BoxCollider2D>();
                //     collider.isTrigger = true;
                //     collider.size = new Vector2(_topSurfaceSize.x, 0.05f);
                // }
            }
            else
            {
                foreach (Transform child in transform)
                {
                    DestroyImmediate(child.gameObject);
                }

                _topMesh = new GameObject(WaterTopMeshName).transform;
                _frontMesh = new GameObject(WaterFrontMeshName).transform;
                
                _topMesh.transform.parent = transform;
                _topMesh.transform.localPosition = Vector3.zero;
                
                _frontMesh.transform.parent = transform;
                _frontMesh.transform.localPosition = Vector3.zero;

                topMF = _topMesh.gameObject.AddComponent<MeshFilter>();
                topMR = _topMesh.gameObject.AddComponent<MeshRenderer>();
                topSG = _topMesh.gameObject.AddComponent<SortingGroup>();
                
                frontMF = _frontMesh.gameObject.AddComponent<MeshFilter>();
                frontMR = _frontMesh.gameObject.AddComponent<MeshRenderer>();
                frontSG = _frontMesh.gameObject.AddComponent<SortingGroup>();

                // if (_testRippleCamera)
                // {
                //     _topMesh.AddComponent<BoxCollider>();
                // }
                // else
                // {
                //     var collider = _topMesh.AddComponent<BoxCollider2D>();
                //     collider.isTrigger = true;
                //     collider.size = new Vector2(_topSurfaceSize.x, 0.05f);
                // }
            }

            if (!transform.TryGetComponent(out BoxCollider2D collider))
            {
                collider = transform.AddComponent<BoxCollider2D>();
                collider.isTrigger = true;
                collider.size = new Vector2(_topSurfaceSize.x, 0.02f);
                collider.offset = new Vector2(_topSurfaceSize.x / 2, 0);
            }            
            
            _topMesh.gameObject.layer = Mathf.RoundToInt(Mathf.Log(_testWaterLayer.value, 2));
            topMR.sharedMaterial = _topMeshMaterial;
            topMR.sortingOrder = 0;
            topSG.sortingLayerID = _topMeshSortingLayer;
            GenerateTopMesh(topMF);
            
            frontMR.sharedMaterial = _frontMeshMaterial;
            frontMR.sortingOrder = 0;
            frontSG.sortingLayerID = _frontMeshSortingLayer;
            GenerateFrontMesh(frontMF);
        }

        private void GenerateTopMesh(MeshFilter mf)
        {
            var mesh = new Mesh { name = WaterTopMeshName };

            var vertices = new Vector3[_topSurfaceVerticesCount.x * _topSurfaceVerticesCount.y];
            var triangles = new int[(_topSurfaceVerticesCount.x - 1) * (_topSurfaceVerticesCount.y - 1) * 6];
            var uv = new Vector2[vertices.Length];

            var dx = _topSurfaceSize.x / (_topSurfaceVerticesCount.x - 1);
            var dz = _topSurfaceSize.y / (_topSurfaceVerticesCount.y - 1);

            for (var z = 0; z < _topSurfaceVerticesCount.y; z++)
            {
                for (var x = 0; x < _topSurfaceVerticesCount.x; x++)
                {
                    var i = x + z * _topSurfaceVerticesCount.x;
                    vertices[i] = new Vector3(x * dx, 0f, z * dz);
                    uv[i] = new Vector2((float)x / (_topSurfaceVerticesCount.x - 1), (float)z / (_topSurfaceVerticesCount.y - 1));
                }
            }

            var triIndex = 0;
            for (var z = 0; z < _topSurfaceVerticesCount.y - 1; z++)
            {
                for (var x = 0; x < _topSurfaceVerticesCount.x - 1; x++)
                {
                    var i = x + z * _topSurfaceVerticesCount.x;

                    triangles[triIndex++] = i;
                    triangles[triIndex++] = i + _topSurfaceVerticesCount.x;
                    triangles[triIndex++] = i + 1;

                    triangles[triIndex++] = i + 1;
                    triangles[triIndex++] = i + _topSurfaceVerticesCount.x;
                    triangles[triIndex++] = i + _topSurfaceVerticesCount.x + 1;
                }
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uv;

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            mf.sharedMesh = mesh;
        }
        
        private void GenerateFrontMesh(MeshFilter mf)
        {
            var mesh = new Mesh { name = WaterFrontMeshName };

            var vertices = new Vector3[_topSurfaceVerticesCount.x * 2];
            var triangles = new int[(_topSurfaceVerticesCount.x - 1) * 6];
            var uv = new Vector2[vertices.Length];

            var dx = _topSurfaceSize.x / (_topSurfaceVerticesCount.x - 1);

            for (var x = 0; x < _topSurfaceVerticesCount.x; x++)
            {
                vertices[x] = new Vector3(x * dx, 0f, 0f);
                uv[x] = new Vector2((float)x / (_topSurfaceVerticesCount.x - 1), 1f);
                
                var i = x + _topSurfaceVerticesCount.x;
                vertices[i] = new Vector3(x * dx, -_frontSurfaceHeight, 0f);
                uv[i] = new Vector2((float)x / (_topSurfaceVerticesCount.x - 1), 0f);
            }

            var triIndex = 0;
            for (var x = 0; x < _topSurfaceVerticesCount.x - 1; x++)
            {
                var topA = x;
                var topB = x + 1;
                var botA = x + _topSurfaceVerticesCount.x;
                var botB = x + _topSurfaceVerticesCount.x + 1;

                triangles[triIndex++] = topB;
                triangles[triIndex++] = botA;
                triangles[triIndex++] = topA;

                triangles[triIndex++] = botB;
                triangles[triIndex++] = botA;
                triangles[triIndex++] = topB;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uv;

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            mf.sharedMesh = mesh;
        }

        private void OnAfterAssemblyReload()
        {
            Init();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var isInitialUp = Random.value > 0.5f;
            if (other.TryGetComponent(out Rigidbody2D rb))
            {
                isInitialUp = rb.linearVelocity.y > 0;
            }
            CreateContactRippleAt(other.transform.position, Random.Range(0.2f, 0.6f), isInitialUp);
        }

        #endregion
    }
}