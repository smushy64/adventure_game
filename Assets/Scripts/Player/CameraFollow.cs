using UnityEngine;

public class CameraFollow : MonoBehaviour {

    [SerializeField]
    float followSpeed = 50f;
    [SerializeField]
    float rotationSpeed = 50f;

    Transform pivot;

    void Awake() {
        pivot = transform.parent;
        transform.parent = null;
    }

    void Update() {
        transform.position = Vector3.Lerp(
            transform.position,
            pivot.position,
            followSpeed * Time.deltaTime
        );
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            pivot.rotation,
            rotationSpeed * Time.deltaTime
        );
    }
}
