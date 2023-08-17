using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] float _distance;
    private Transform _player;

    private void Start()
    {
        _player = Camera.main.transform;

        ChangePosition();
    }

    public void ChangePosition()
    {
        transform.position = new Vector3(
            (Random.insideUnitSphere.x * _distance), 
            transform.position.y,
            (Random.insideUnitSphere.z * _distance)
            );

        transform.LookAt(_player);
    }
}
