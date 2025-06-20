using UnityEngine;

public class Roteator : MonoBehaviour
{
    public Vector3 rotationSpeed = new Vector3(0, 100, 0); // gradi per secondo

    void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}
