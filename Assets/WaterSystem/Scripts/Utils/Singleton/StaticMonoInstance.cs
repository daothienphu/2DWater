using UnityEngine;

namespace Utils
{
    public abstract class StaticMonoInstance<T> : MonoBehaviour where T: MonoBehaviour{
        public static T Instance { get; private set;}
        private void Awake() => OnAwake();

        private void Start() => OnStart();

        private void Update() => OnUpdate();

        private void FixedUpdate() => OnFixedUpdate();

        private void LateUpdate() => OnLateUpdate();

        protected virtual void OnLateUpdate() { }

        protected virtual void OnFixedUpdate() { }

        protected virtual void OnUpdate() { }

        protected virtual void OnStart() { }

        protected virtual void OnAwake() => Instance = this as T;
    
        protected virtual void OnApplicationQuit(){
            Instance = null;
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
        }
    }
}