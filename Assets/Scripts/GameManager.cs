using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private DifficultyScriptableObject _difficulty;
    [SerializeField] private Field _field;
    [SerializeField] private GameObject _marker;
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private GameObject _gameOverScreen;

    private bool _canSwap = false;
    private bool _isGameOver = false;
    private int _score = 0;
    private Tile _selectedTile;

    public void BackToMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void Restart()
    {
        _isGameOver = false;
        _gameOverScreen.SetActive(false);

        StartCoroutine(DoRestart());
    }

    private IEnumerator DoRestart()
    {
        _canSwap = false;

        if (MainManager.instance != null)
        {
            _difficulty = MainManager.instance.difficulty;
        }

        _field.tileTypes = _difficulty.tileTypes;
        _score = 0;
        _scoreText.text = "0";

        yield return _field.ResetAllTiles();
        yield return RegenerateField(_field.FindTileMatches());

        _canSwap = true;
    }

    private IEnumerator RegenerateField(ICollection<Tile> matchedTiles)
    {
        //_field.ResetAllTilesScale();

        int multiplier = 1;

        while (matchedTiles.Count > 0)
        {
            yield return _field.DestroyTiles(matchedTiles);

            _score += matchedTiles.Count * _difficulty.tilePoints * multiplier++;
            _scoreText.text = _score.ToString();

            yield return _field.FillGaps();

            matchedTiles = _field.FindTileMatches();
        }

        ICollection<Tile> possibleMatchedTiles = _field.FindPossibleTileMatches();

        //foreach (Tile tile in possibleMatchedTiles)
        //{
        //    tile.gameObject.transform.localScale = 1.25f * Vector3.one;
        //}

        if (possibleMatchedTiles.Count == 0)
        {
            _isGameOver = true;
            _gameOverScreen.SetActive(true);
        }
    }

    private void Start()
    {
        StartCoroutine(DoRestart());
    }

    private IEnumerator SwapTiles(Tile tile1, Tile tile2)
    {
        _canSwap = false;

        yield return _field.SwapTiles(tile1, tile2);

        ICollection<Tile> matchedTiles = _field.FindTileMatches();

        if (matchedTiles.Contains(tile1) || matchedTiles.Contains(tile2))
        {
            yield return RegenerateField(matchedTiles);
        }
        else
        {
            yield return _field.SwapTiles(tile1, tile2);
        }

        _canSwap = true;
    }

    private void Update()
    {
        if (!_isGameOver && _canSwap && Input.GetButtonDown("Fire1"))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector3.forward);

            if (hit && hit.collider.CompareTag("Tile"))
            {
                Tile tile = _field.FindTile(hit.transform.position - _field.transform.position);

                if (tile != null)
                {
                    if (_selectedTile != null)
                    {
                        if (_field.IsNeighbors(tile, _selectedTile))
                        {
                            StartCoroutine(SwapTiles(tile, _selectedTile));
                        }

                        _selectedTile = null;
                        _marker.SetActive(false);
                    }
                    else
                    {
                        _selectedTile = tile;
                        _marker.transform.position = hit.transform.position;
                        _marker.SetActive(true);
                    }
                }
            }
        }
    }
}
