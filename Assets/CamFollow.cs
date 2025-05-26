using UnityEngine;

public class CamFollow : MonoBehaviour
{
    [SerializeField] private Transform _target;
    
    private void LateUpdate()
    {
        var position = transform.position;
        position.y = _target.transform.position.y + 1;

        transform.position = position;
    }
}
