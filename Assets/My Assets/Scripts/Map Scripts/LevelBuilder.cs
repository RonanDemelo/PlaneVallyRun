using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelBuilderTut : MonoBehaviour
{
    [SerializeField] GameManage gameManage;

    //Balance Vars
    [SerializeField] int mainPathLength = 5;                 // Length of the main path
    [SerializeField] int sidePathLength = 3;                 // Length of side paths
    [SerializeField] int sidePathAmount = 4;                 // Number of side paths
    [SerializeField][Range(0f, 1f)] float corridorChance = 0.8f;          // Chance of creating a corridor instead of a room
    [SerializeField][Range(0f, 1f)] float stackingCorridorChance = 0.1f; // Increase in corridor chance when multiple corridors are stacked
    [SerializeField][Range(0f, 1f)] float lootRoomChance = 0.5f;          // Chance of creating a corridor instead of a room


    //Working Bools
    bool makingLevel;
    bool makingChain;

    //Room Types
    [SerializeField] Tile startRoomType;                      // Start room tile type
    [SerializeField] Tile endRoomType;                        // End room tile type
    [SerializeField] Tile lootRoomType;                        //A Room that contains loot
    [SerializeField] Tile[] corridorTypes;                     // Array of corridor tile types
    [SerializeField] Tile[] roomTypes;                         // Array of room tile types

    //Build data
    [SerializeField] List<Vector2> tileCoords = new List<Vector2>();   // List of occupied tile coordinates
    List<Tile> placedTiles = new List<Tile>();                  // List of placed tiles
    List<Transform> availableExits = new List<Transform>();     // List of available exit points

    private void Start()
    {
        NewLevel();
    }

    void EmptyLevel()
    {
        StopAllCoroutines();

        foreach (Transform t in transform)
        {
            Destroy(t.gameObject);
        }

        tileCoords = new List<Vector2>();
        ClearVars();
    }

    void ClearVars()
    {
        availableExits.RemoveAll(i => i == null);
        placedTiles.RemoveAll(i => i == null);
    }

    public void NewLevel()
    {
        EmptyLevel();
        makingLevel = makingChain = false;

        if (makingLevel == false)
        {
            StartCoroutine(CrouteMakeLevel());
        }

    }

    IEnumerator CrouteMakeLevel()
    {
        makingLevel = true;

        // Make first room
        Tile startTile = Instantiate(startRoomType,Vector3.zero,
            Quaternion.identity, transform);

        yield return new WaitForSeconds(1);

        AddTile(startTile);

        // Make first chain and wait
        StartCoroutine(CrouteMakeChain(mainPathLength, startTile));
        while (makingChain) { yield return null; }

        // Make Last Room by replacing the last tile in the chain with the end room
        Tile lastTileMade = placedTiles[placedTiles.Count - 1];
        Tile _endTile = Instantiate(endRoomType,
           lastTileMade.transform.position,
           lastTileMade.transform.rotation,
           transform);

        // Remove destroyed tile from list (update arrays by returning null) then add new end tile to lists
        Destroy(lastTileMade.gameObject);
        yield return null;

        tileCoords.RemoveAt(tileCoords.Count - 1);
        ClearVars();

        AddTile(_endTile);

        // Make side Paths
        int sidePaths = sidePathAmount;
        while (sidePaths > 0)
        {
            // Use the same function but start at a random tile
            StartCoroutine(CrouteMakeChain(sidePathLength,
                placedTiles[Random.Range(0, placedTiles.Count - 1)]));
            while (makingChain) { yield return null; }


            Tile _lastTileMade2 = placedTiles[placedTiles.Count - 1];
            Destroy(_lastTileMade2.gameObject);

            if (Random.Range(0f,1f) < lootRoomChance)
            {
                Tile _lootTile = Instantiate(lootRoomType,
                   _lastTileMade2.transform.position,
                   _lastTileMade2.transform.rotation,
                   transform);

                //Making the last tile in the side path a loot room
                yield return null;
                tileCoords.RemoveAt(tileCoords.Count - 1);
                ClearVars();

                AddTile(_lootTile);
            }
            else
            {
                Tile _roomTile = Instantiate(roomTypes[Random.Range(0, roomTypes.Length)],
                    _lastTileMade2.transform.position,
                    _lastTileMade2.transform.rotation,
                    transform);

                //Making the last tile in the side path a room
                yield return null;
                tileCoords.RemoveAt(tileCoords.Count - 1);
                ClearVars();

                AddTile(_roomTile);
            }

            sidePaths--;
        }

        makingLevel = false;

        gameManage.StartLevel();
    }

    IEnumerator CrouteMakeChain(int _roomCount, Tile _currentTile)
    {
        makingChain = true;

        while (_roomCount > 0)
        {
            float probMult = 0f; // steadily increase chance to make room, not a corridor

            Transform joiningPoint = null;
            bool goodEntry = false;

            // Check if there is a possible exit from the current tile
            while (goodEntry == false)
            {
                List<Transform> availableExits = GetAvailableExits(_currentTile);

                
                if (availableExits.Count > 0)
                {
                    goodEntry = true;
                    joiningPoint = availableExits[Random.Range(0, availableExits.Count)];
                }
                // If there is no available exit, go back to the previous tile
                else
                {
                    int newTileIndex = placedTiles.IndexOf(_currentTile) - 1;

                    if (newTileIndex >= 0)
                    {
                        _currentTile = placedTiles[newTileIndex];
                    }
                    else // Loop back to the last tile if at the start
                    {
                        _currentTile = placedTiles[placedTiles.Count - 1];
                    }
                }
            }

            Tile newTileType;
            bool isRoom = true;

            if (Random.Range(0f, 1f) < corridorChance - probMult)
            {
                // Make a Corridor
                newTileType = corridorTypes[Random.Range(0, corridorTypes.Length)];
                isRoom = false;
            }
            else
            {
                // Make a Room
                newTileType = roomTypes[Random.Range(0, roomTypes.Length)];
            }

            Vector3Int tileSize = _currentTile.GetTileSize();
            Vector2 tilePos = Round(joiningPoint.position + joiningPoint.forward * (tileSize.z / 2f));

            // Check if space is available
            if (IsSpaceAvailable(newTileType, tilePos))
            {
                // Build Tile
                Tile newTile = Instantiate(newTileType,
                    new Vector3(tilePos.x, 0, tilePos.y),
                    joiningPoint.rotation,
                    transform);

                _currentTile = newTile; // Set the next starting tile to this new one
                AddTile(newTile);

                //remove wall once room is added
                joiningPoint.gameObject.SetActive(false);

                // If it's a room, lower the remaining room count and reset the corridor chance
                if (!isRoom)
                {
                    probMult += stackingCorridorChance;
                    
                }

                _roomCount--;
                probMult = 0;
            }
            // Once the exit has a tile or is found to be blocked, remove the exit from available exits
            availableExits.Remove(joiningPoint);
            yield return null;
        }

        makingChain = false;
    }

    bool IsSpaceAvailable(Tile _tile, Vector2 _position)
    {
        if (tileCoords.Contains(_position))
        {
            return false;
        }
        else
        {
            return true;
        }
    }


    void AddTile(Tile _tile)
    {
        // Add exits to available exits
        foreach (Transform t in _tile.transform.Find("Tile/AnchorPoints/Exits"))
        {
            availableExits.Add(t);
        }

        Vector3Int tileSize = _tile.GetTileSize();
        Vector3 tilePos = _tile.transform.position;

        // Add occupied positions based on the size of the tile
        for (int x = 0; x < tileSize.x; x++)
        {
            for (int z = 0; z < tileSize.y; z++)
            {
                Vector2 occupiedPos = new Vector2(tilePos.x + x, tilePos.z + z);
                tileCoords.Add(occupiedPos);
            }
        }

        placedTiles.Add(_tile); // Add the tile to the list of placed tiles
    }

    Vector3 Round(Vector3 v)
    {
        return new Vector3(
            Mathf.Round(v.x),
            Mathf.Round(v.z)
            );
    }

    List<Transform> GetAvailableExits(Tile _newTile)
    {
        List<Transform> exits = new List<Transform>();

        // Iterate through all anchors in the tile of the specified type (Exit) and add them to the list
        foreach (Transform t in _newTile.transform.Find("Tile/AnchorPoints/Exits"))
        {
            if (availableExits.Contains(t))
            {
                exits.Add(t);
            }
        }

        return exits;
    }
}


