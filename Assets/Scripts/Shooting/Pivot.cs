using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pivot : MonoBehaviour
{
    [SerializeField] private Transform _player;
    [SerializeField] private Transform _camera;
    void Update()
    {
        //Sync rotation
        Quaternion newRotation = Quaternion.Euler(_player.localRotation.eulerAngles + _camera.localRotation.eulerAngles);
        transform.rotation = newRotation;
    }
}
