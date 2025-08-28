using UnityEngine;

public class Tile : MonoBehaviour
{
    public bool IsVisible
    {
        get { return gameObject.activeSelf; }
        set { gameObject.SetActive(value); }
    }

    public float TargetDistance
    {
        get { return _targetDistance; }
    }

    public Vector3 TargetPosition
    {
        get { return _targetPosition; }
        set
        {
            _targetPosition = value;
            _targetDistance = Vector3.Distance(transform.position, value);
        }
    }

    public TileTypeScriptableObject Type
    {
        get { return _type; }
        set
        {
            _type = value;
            _spriteRenderer.sprite = value.sprite;
        }
    }

    private SpriteRenderer _spriteRenderer;
    private float _targetDistance;
    private Vector3 _targetPosition;
    private TileTypeScriptableObject _type;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }
}
