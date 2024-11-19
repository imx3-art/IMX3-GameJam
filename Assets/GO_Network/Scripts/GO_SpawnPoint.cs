using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GO_SpawnPoint : MonoBehaviour
{
    [Range(3f, 20f)]
    [SerializeField] float radiusSpawnPlayer = 5;
    public short level_ID = -1;
    private Transform _radius;
    public static GO_SpawnPoint currentSpawPoint;

    private void Awake()
    {
        currentSpawPoint = this;
    }
    public (Vector3 pos, Quaternion rot) getSpawPointPosition()
    {
        
        Vector3 position = transform.position;
        Quaternion rotation = transform.rotation;
        
        return (RandomPosition(position, radiusSpawnPlayer), rotation);
    }

    public static Vector3 RandomPosition(Vector3 _position, float _maxRadius)
    {
        Vector3 _upVector = Vector3.up;
        _maxRadius = 5 * _maxRadius / 19;
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
            _radius.localScale = new Vector3(radiusSpawnPlayer, _radius.localScale.y, radiusSpawnPlayer);
        }
    }
    #endregion
}
