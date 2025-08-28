using System.Collections;
using System.Collections.Generic;
//using System.Text;
using UnityEngine;

public class Field : MonoBehaviour
{
    [System.NonSerialized] public List<TileTypeScriptableObject> tileTypes;

    [SerializeField] private int _width;
    [SerializeField] private int _height;
    [SerializeField] private GameObject _tilePrefab;
    [SerializeField] private AnimationCurve _tileFallingCurve;
    [SerializeField] private float _tileFallingDuration = 1.0f;
    [SerializeField] private AnimationCurve _tileDestroyingCurve;
    [SerializeField] private float _tileDestroyingDuration = 0.5f;
    [SerializeField] private AnimationCurve _tileSwappingCurve;
    [SerializeField] private float _tileSwappingDuration = 0.25f;

    private Tile[,] _tiles;

    public IEnumerator DestroyTiles(ICollection<Tile> tiles)
    {
        if (tiles.Count > 0)
        {
            float time = 0.0f;

            while (time < _tileDestroyingDuration)
            {
                time += Time.deltaTime;

                float currentScale = 1 - _tileDestroyingCurve.Evaluate(Mathf.Clamp01(time / _tileDestroyingDuration));

                foreach (Tile tile in tiles)
                {
                    tile.transform.localScale = currentScale * Vector3.one;
                }

                yield return null;
            }

            foreach (Tile tile in tiles)
            {
                tile.IsVisible = false;
                tile.transform.localScale = Vector3.one;
            }
        }
    }

    public IEnumerator FillGaps()
    {
        float maxDistance = 0;

        for (int x = 0; x < _width; x++)
        {
            int count = 0;

            for (int y = _height - 1; y > -1; y--)
            {
                Tile tile = _tiles[y, x];

                if (!tile.IsVisible)
                {
                    count++;

                    tile.IsVisible = true;
                    tile.Type = tileTypes[Random.Range(0, tileTypes.Count)];

                    for (int dy = y + 1; dy < _height; dy++)
                    {
                        (_tiles[dy - 1, x], _tiles[dy, x]) = (_tiles[dy, x], _tiles[dy - 1, x]);
                    }
                }
            }

            for (int y = _height - count; y < _height; y++)
            {
                Tile tile = _tiles[y, x];
                tile.transform.position = new Vector3(x, y + count, 0) + transform.position;
            }

            if (maxDistance < count) maxDistance = count;
        }

        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                Tile tile = _tiles[y, x];
                tile.TargetPosition = new Vector3(x, y, 0) + transform.position;
            }
        }

        float time = 0.0f;
        float maxDuration = (float)maxDistance / _height * _tileFallingDuration;

        while (time < maxDuration)
        {
            time += Time.deltaTime;

            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    Tile tile = _tiles[y, x];

                    float distance = tile.TargetDistance;
                    if (distance <= 0) continue;

                    Vector3 currentPosition = (1 - _tileFallingCurve.Evaluate(Mathf.Clamp01(time / (_tileFallingDuration * (distance / _height))))) * distance * Vector3.up;
                    tile.transform.position = tile.TargetPosition + currentPosition;
                }
            }

            yield return null;
        }
    }

    public ICollection<Tile> FindPossibleTileMatches()
    {
        HashSet<Tile> result = new();

        for (int y = 0; y < _height; y++)
        {
            int startX = 0;
            int count = 1;

            for (int x = 1; x < _width; x++)
            {
                Tile targetTile = _tiles[y, x];
                TileTypeScriptableObject tileType = targetTile.Type;

                if (tileType != _tiles[y, startX].Type)
                {
                    startX = x;
                    count = 1;
                }
                else
                {
                    count++;
                }

                Tile tile = targetTile;
                List<Tile> possibleMatchedTiles = new();

                if (startX - 2 > -1)
                {
                    tile = _tiles[y, startX - 2];

                    if (tile.Type == tileType) possibleMatchedTiles.Add(tile);
                }

                if (startX - 1 > -1)
                {
                    if (y - 1 > -1)
                    {
                        tile = _tiles[y - 1, startX - 1];

                        if (tile.Type == tileType) possibleMatchedTiles.Add(tile);
                    }

                    if (y + 1 < _height)
                    {
                        tile = _tiles[y + 1, startX - 1];

                        if (tile.Type == tileType) possibleMatchedTiles.Add(tile);
                    }
                }

                if (count == 2)
                {
                    int endX = startX + count - 1;

                    if (endX + 2 < _width)
                    {
                        tile = _tiles[y, endX + 2];

                        if (tile.Type == tileType) possibleMatchedTiles.Add(tile);
                    }

                    if (endX + 1 < _width)
                    {
                        if (y - 1 > -1)
                        {
                            tile = _tiles[y - 1, endX + 1];

                            if (tile.Type == tileType) possibleMatchedTiles.Add(tile);
                        }

                        if (y + 1 < _height)
                        {
                            tile = _tiles[y + 1, endX + 1];

                            if (tile.Type == tileType) possibleMatchedTiles.Add(tile);
                        }
                    }

                    if (possibleMatchedTiles.Count > 0)
                    {
                        result.UnionWith(possibleMatchedTiles);

                        int length = startX + count;

                        for (int dx = startX; dx < length; dx++)
                        {
                            result.Add(_tiles[y, dx]);
                        }
                    }
                }
                else if (count == 1 && possibleMatchedTiles.Count >= 2)
                {
                    result.Add(targetTile);
                    result.UnionWith(possibleMatchedTiles);
                }
            }
        }

        for (int x = 0; x < _width; x++)
        {
            int startY = 0;
            int count = 1;

            for (int y = 1; y < _height; y++)
            {
                Tile targetTile = _tiles[y, x];
                TileTypeScriptableObject tileType = targetTile.Type;

                if (tileType != _tiles[startY, x].Type)
                {
                    startY = y;
                    count = 1;
                }
                else
                {
                    count++;
                }

                Tile tile = targetTile;
                List<Tile> possibleMatchedTiles = new();

                if (startY - 2 > -1)
                {
                    tile = _tiles[startY - 2, x];

                    if (tile.Type == tileType) possibleMatchedTiles.Add(tile);
                }

                if (startY - 1 > -1)
                {
                    if (x - 1 > -1)
                    {
                        tile = _tiles[startY - 1, x - 1];

                        if (tile.Type == tileType) possibleMatchedTiles.Add(tile);
                    }

                    if (x + 1 < _width)
                    {
                        tile = _tiles[startY - 1, x + 1];

                        if (tile.Type == tileType) possibleMatchedTiles.Add(tile);
                    }
                }

                if (count == 2)
                {
                    int endY = startY + count - 1;

                    if (endY + 2 < _height)
                    {
                        tile = _tiles[endY + 2, x];

                        if (tile.Type == tileType) possibleMatchedTiles.Add(tile);
                    }

                    if (endY + 1 < _height)
                    {
                        if (x - 1 > -1)
                        {
                            tile = _tiles[endY + 1, x - 1];

                            if (tile.Type == tileType) possibleMatchedTiles.Add(tile);
                        }

                        if (x + 1 < _width)
                        {
                            tile = _tiles[endY + 1, x + 1];

                            if (tile.Type == tileType) possibleMatchedTiles.Add(tile);
                        }
                    }

                    if (possibleMatchedTiles.Count > 0)
                    {
                        result.UnionWith(possibleMatchedTiles);

                        int length = startY + count;

                        for (int dy = startY; dy < length; dy++)
                        {
                            result.Add(_tiles[dy, x]);
                        }
                    }
                }
                else if (count == 1 && possibleMatchedTiles.Count >= 2)
                {
                    result.Add(targetTile);
                    result.UnionWith(possibleMatchedTiles);
                }
            }
        }

        return result;
    }

    public Tile FindTile(Vector3 position)
    {
        int x = (int)position.x;
        int y = (int)position.y;

        if (x > -1 && x < _width && y > -1 && y < _height)
        {
            return _tiles[y, x];
        }

        return null;
    }

    public ICollection<Tile> FindTileMatches()
    {
        HashSet<Tile> result = new();

        for (int y = 0; y < _height; y++)
        {
            int startX = 0;
            int count = 1;

            for (int x = 1; x < _width; x++)
            {
                Tile tile = _tiles[y, x];

                if (tile.Type != _tiles[y, startX].Type)
                {
                    startX = x;
                    count = 1;
                }
                else
                {
                    count++;
                }

                if (count > 3)
                {
                    result.Add(tile);
                }
                else if (count == 3)
                {
                    int length = startX + count;

                    for (int dx = startX; dx < length; dx++)
                    {
                        result.Add(_tiles[y, dx]);
                    }
                }
            }
        }

        for (int x = 0; x < _width; x++)
        {
            int startY = 0;
            int count = 1;

            for (int y = 1; y < _height; y++)
            {
                Tile tile = _tiles[y, x];

                if (tile.Type != _tiles[startY, x].Type)
                {
                    startY = y;
                    count = 1;
                }
                else
                {
                    count++;
                }

                if (count > 3)
                {
                    result.Add(tile);
                }
                else if (count == 3)
                {
                    int length = startY + count;

                    for (int dy = startY; dy < length; dy++)
                    {
                        result.Add(_tiles[dy, x]);
                    }
                }
            }
        }

        return result;
    }

    public bool IsNeighbors(Tile tile1, Tile tile2)
    {
        return Vector3.Distance(tile1.transform.position, tile2.transform.position) == 1;
    }

    public IEnumerator ResetAllTiles()
    {
        foreach (Tile tile in _tiles)
        {
            tile.Type = tileTypes[Random.Range(0, tileTypes.Count)];
            tile.TargetPosition = tile.transform.position;
            tile.transform.position += _height * Vector3.up;
        }

        float time = 0.0f;

        while (time < _tileFallingDuration)
        {
            time += Time.deltaTime;

            Vector3 currentPosition = (1 - _tileFallingCurve.Evaluate(Mathf.Clamp01(time / _tileFallingDuration))) * _height * Vector3.up;

            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    Tile tile = _tiles[y, x];
                    tile.transform.position = tile.TargetPosition + currentPosition;
                }
            }

            yield return null;
        }
    }

    //public void ResetAllTilesScale()
    //{
    //    foreach (Tile tile in _tiles)
    //    {
    //        tile.transform.localScale = Vector3.one;
    //    }
    //}

    public IEnumerator SwapTiles(Tile tile1, Tile tile2)
    {
        Vector3 tile1Position = tile1.transform.position - transform.position;
        Vector3 tile2Position = tile2.transform.position - transform.position;

        _tiles[(int)tile1Position.y, (int)tile1Position.x] = tile2;
        _tiles[(int)tile2Position.y, (int)tile2Position.x] = tile1;

        tile1.TargetPosition = tile2.transform.position;
        tile2.TargetPosition = tile1.transform.position;

        Vector3 difference = tile1.transform.position - tile2.transform.position;

        float time = 0.0f;

        while (time < _tileSwappingDuration)
        {
            time += Time.deltaTime;

            Vector3 currentPosition = (1 - _tileSwappingCurve.Evaluate(Mathf.Clamp01(time / _tileSwappingDuration))) * difference;

            tile1.transform.position = tile1.TargetPosition + currentPosition;
            tile2.transform.position = tile2.TargetPosition - currentPosition;

            yield return null;
        }
    }

    //public override string ToString()
    //{
    //    StringBuilder textStringBuilder = new();

    //    for (int y = _height - 1; y > -1; y--)
    //    {
    //        StringBuilder lineStringBuilder = new();

    //        for (int x = 0; x < _width; x++)
    //        {
    //            if (x > 0) lineStringBuilder.Append("\t");

    //            lineStringBuilder.Append(_tiles[y, x].Type.id);
    //        }

    //        textStringBuilder.Append(lineStringBuilder);

    //        if (y > 0) textStringBuilder.Append("\n");
    //    }

    //    return textStringBuilder.ToString();
    //}

    private void Awake()
    {
        _tiles = new Tile[_height, _width];

        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                GameObject gameObject = Instantiate(_tilePrefab,
                                                    new Vector3(x, y, 0) + transform.position,
                                                    _tilePrefab.transform.rotation,
                                                    transform);

                _tiles[y, x] = gameObject.GetComponent<Tile>();
            }
        }
    }
}
