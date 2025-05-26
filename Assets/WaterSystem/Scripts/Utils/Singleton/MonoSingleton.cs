using UnityEngine;

namespace Utils
{
    public abstract class MonoSingleton<T> : StaticMonoInstance<T> where T: MonoBehaviour{
        protected override void OnAwake()
        {
            if (Instance != null){
                Destroy(gameObject);
                return;
            }
            base.OnAwake();
        }
    }
}