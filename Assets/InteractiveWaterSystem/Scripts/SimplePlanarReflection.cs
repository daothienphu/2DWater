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
        [SerializeField] private float _reflectionResolutionScale = 0.5f;
        [SerializeField] private Vector3 _reflectionSurfaceNormal = Vector3.up;
        
        private Camera _reflectionCam;
        private Camera _mainCamera;
        
        #endregion

        #region Lifecycle

        private void OnEnable()
        {
            SetupCamera();
            SetupRenderTexture();
        }
        
        private void OnDisable()
        {
            if (_reflectionTexture) _reflectionTexture.Release();
        }
        
        private void LateUpdate()
        {
            RenderToTexture();
        }

        #endregion

        #region Public Methods

#if UNITY_EDITOR
        [CreateInspectorButton("Setup")]
        public void EditorSetup()
        {
            SetupCamera();
            SetupRenderTexture();
        }
#endif

        #endregion

        #region Private Methods

        private void RenderToTexture()
        {
            if (!_reflectionTexture) return;
            if (!_mainCamera || !_reflectionCam) return;
            
            //No need for reflections when we're underwater.
            if (_mainCamera.transform.position.y <= transform.position.y) return;
            
            //Calculating the reflection camera position and rotation.
            var camPos = _mainCamera.transform.position;
            var dot = Vector3.Dot(_reflectionSurfaceNormal, camPos - transform.position);
            var reflectedPos = camPos - 2f * dot * _reflectionSurfaceNormal;
            var reflectedForward = Vector3.Reflect(_mainCamera.transform.forward, _reflectionSurfaceNormal);
            var reflectedUp = Vector3.Reflect(_mainCamera.transform.up, _reflectionSurfaceNormal);
            _reflectionCam.transform.position = reflectedPos;
            _reflectionCam.transform.rotation = Quaternion.LookRotation(reflectedForward, reflectedUp);

            //Cut off the reflection camera projection matrix at the reflection surface.
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
            if (!_mainCamera) _mainCamera = Camera.main;
            
            if (transform.childCount % 2 == 1) //In my project, the Reflection Cam is either the first or the third child. Change this according to your project.
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
            var w = Mathf.RoundToInt(_mainCamera.pixelWidth  * _reflectionResolutionScale);
            var h = Mathf.RoundToInt(_mainCamera.pixelHeight * _reflectionResolutionScale);
            
            if (!_reflectionTexture) return;
            
            _reflectionTexture.Release();
            _reflectionTexture.width = w;
            _reflectionTexture.height = h;
            
            _reflectionCam.targetTexture = _reflectionTexture;
        }

        #endregion
    }
}