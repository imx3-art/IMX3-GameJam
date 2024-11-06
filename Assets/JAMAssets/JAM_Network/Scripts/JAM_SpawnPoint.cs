using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JAM_SpawnPoint : MonoBehaviour
{
    [Range(5f, 20f)]
    [SerializeField] float radiusSpawn = 3;
    private Transform _radius;

    public (Vector3 pos, Quaternion rot) getSpawPointPosition()
    {
        Vector3 position = transform.position;
        Quaternion rotation = transform.rotation;

        return (RandomPosition(position, radiusSpawn), rotation);
    }
    public static Vector3 RandomPosition(Vector3 _position, float _maxRadius)
    {
        Vector3 _upVector = Vector3.up;
        Debug.Log("VALOR: " + _maxRadius);

        _maxRadius = 5 * _maxRadius / 19;
        Debug.Log("VALOR: " + _maxRadius);
        return _position + (Vector3.forward * Random.Range(-_maxRadius, _maxRadius)) + (Vector3.right * Random.Range(-_maxRadius, _maxRadius)) + (_upVector);
    }
    #region EDITOR
    void OnValidate()
    {
        if (Application.isEditor && !Application.isPlaying)
        {
            if (_radius == null)
            {
                _radius = transform.GetChild(0);
            }
            _radius.localScale = new Vector3(radiusSpawn, _radius.localScale.y, radiusSpawn);
        }
    }
    #endregion
}
