using UnityEditor;
using UnityEngine;
using Utils;

namespace InteractiveWater
{
    public class SimplePlanarReflection : MonoBehaviour
    {
        #region Fields
        
        [SerializeField] private LayerMask _reflectionMask;
        [SerializeField] private RenderTexture _reflectionTexture;
        [Range(0.1f,1f)] 
        [SerializeField] private float _resolutionScale = 0.5f;
        [SerializeField] private Vector3 _reflectionSurfaceNormal = Vector3.up;
        [SerializeField] private bool _runInEditor;
        private Camera _reflectionCam;
        private Camera _mainCamera;
        
        #endregion

        #region Life Cycle

        private void OnEnable()
        {
            if (!_mainCamera) _mainCamera = Camera.main;
            SetupCamera();
            SetupRenderTexture();
            
            if (_runInEditor)
            {
                EditorApplication.update -= RenderToTexture;
            }
        }
        
        private void OnDisable()
        {
            if (_reflectionTexture) _reflectionTexture.Release();
            if (_runInEditor)
            {
                EditorApplication.update -= RenderToTexture;
            }
        }
        
        private void LateUpdate()
        {
            RenderToTexture();
        }

        #endregion

        #region Public Methods

        [CreateInspectorButton("Setup")]
        public void EditorSetup()
        {
            SetupCamera();
            SetupRenderTexture();

            if (_runInEditor)
            {
                EditorApplication.update += RenderToTexture;
            }
        }

        #endregion

        #region Private Methods

        private void RenderToTexture()
        {
            if (!_mainCamera || !_reflectionCam) return;
            if (_mainCamera.transform.position.y <= transform.position.y) return;
            
            var camPos = _mainCamera.transform.position;
            var dot = Vector3.Dot(_reflectionSurfaceNormal, camPos - transform.position);
            var reflectedPos = camPos - 2f * dot * _reflectionSurfaceNormal;
            var reflectedForward = Vector3.Reflect(_mainCamera.transform.forward, _reflectionSurfaceNormal);
            var reflectedUp = Vector3.Reflect(_mainCamera.transform.up, _reflectionSurfaceNormal);
            _reflectionCam.transform.position = reflectedPos;
            _reflectionCam.transform.rotation = Quaternion.LookRotation(reflectedForward, reflectedUp);

            var viewSpace = _reflectionCam.worldToCameraMatrix;
            var pointViewSpace = viewSpace.MultiplyPoint(transform.position);
            var normalViewSpace = viewSpace.MultiplyVector(_reflectionSurfaceNormal).normalized;
            var planeViewSpace = new Vector4(
                normalViewSpace.x, 
                normalViewSpace.y, 
                normalViewSpace.z, 
                -Vector3.Dot(pointViewSpace, normalViewSpace));
            var projectionMatrix = _mainCamera.CalculateObliqueMatrix(planeViewSpace);
            
            _reflectionCam.projectionMatrix = projectionMatrix;
            
            _reflectionCam.Render();
        }
        
        private void SetupCamera()
        {
            if (transform.childCount % 2 == 1)
            {
                var go = transform.GetChild(transform.childCount > 2 ? 2 : 0);
                _reflectionCam = go.GetComponent<Camera>();
            }
            else
            {
                var go = new GameObject("ReflectionCam");
                go.transform.parent = transform;
                _reflectionCam = go.AddComponent<Camera>();
                
            }
            
            _reflectionCam.enabled = false;

            if (!_mainCamera) _mainCamera = Camera.main;
            _reflectionCam.CopyFrom(_mainCamera);
            _reflectionCam.cullingMask = _reflectionMask;
            _reflectionCam.clearFlags = _mainCamera.clearFlags;
            _reflectionCam.backgroundColor = _mainCamera.backgroundColor;
        }
        
        private void SetupRenderTexture()
        {
            var w = Mathf.RoundToInt(_mainCamera.pixelWidth  * _resolutionScale);
            var h = Mathf.RoundToInt(_mainCamera.pixelHeight * _resolutionScale);
            
            if (!_reflectionTexture)
            {
                _reflectionTexture = new RenderTexture(w,h,16, RenderTextureFormat.DefaultHDR)
                {
                    name = "ReflectionRenderTexture",
                    useMipMap = false,
                    autoGenerateMips = false
                };
            }
            else
            {
                _reflectionTexture.Release();
                _reflectionTexture.width = w;
                _reflectionTexture.height = h;
            }
            
            _reflectionCam.targetTexture = _reflectionTexture;
        }

        #endregion
    }
}