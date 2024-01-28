using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Transform gameTransform;
    [SerializeField] private Transform piecePrefab;

    private List<Transform> pieces;
    private int emptyLocation;
    private int size;
    private bool shuffling = true;
    private bool canCheckCompletion = false;

    private void Start()
    {
        pieces = new List<Transform>();
        size = 3; // Adjusted for a 3x3 puzzle
        CreateGamePieces(0.01f);
        StartCoroutine(WaitShuffle(0.5f)); // Start shuffling after a brief delay
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !shuffling)
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit)
            {
                for (int i = 0; i < pieces.Count; i++)
                {
                    if (pieces[i] == hit.transform)
                    {
                        if (SwapIfValid(i, -size, size) || SwapIfValid(i, +size, size) ||
                            SwapIfValid(i, -1, 0) || SwapIfValid(i, +1, size - 1))
                        {
                            canCheckCompletion = true; // Enable completion check on first move
                            break;
                        }
                    }
                }
            }
        }

        // Check for completion if allowed
        if (canCheckCompletion && CheckCompletion())
        {
            SceneManager.LoadScene("YouWon");
        }
    }

    private IEnumerator WaitShuffle(float duration)
    {
        yield return new WaitForSeconds(duration);
        Shuffle();
        shuffling = false; // Indicate shuffling is complete
    }

    // Create the game setup with size x size pieces.
    private void CreateGamePieces(float gapThickness)
    {
        // This is the width of each tile.
        float width = 1 / (float)size;
        for (int row = 0; row < size; row++)
        {
            for (int col = 0; col < size; col++)
            {
                Transform piece = Instantiate(piecePrefab, gameTransform);
                pieces.Add(piece);
                // Pieces will be in a game board going from -1 to +1.
                piece.localPosition = new Vector3(-1 + (2 * width * col) + width,
                                                  +1 - (2 * width * row) - width,
                                                  0);
                piece.localScale = ((2 * width) - gapThickness) * Vector3.one;
                piece.name = $"{(row * size) + col}";
                // We want an empty space in the bottom right.
                if ((row == size - 1) && (col == size - 1))
                {
                    emptyLocation = (size * size) - 1;
                    piece.gameObject.SetActive(false);
                }
                else
                {
                    // We want to map the UV coordinates appropriately, they are 0->1.
                    float gap = gapThickness / 2;
                    Mesh mesh = piece.GetComponent<MeshFilter>().mesh;
                    Vector2[] uv = new Vector2[4];
                    // UV coord order: (0, 1), (1, 1), (0, 0), (1, 0)
                    uv[0] = new Vector2((width * col) + gap, 1 - ((width * (row + 1)) - gap));
                    uv[1] = new Vector2((width * (col + 1)) - gap, 1 - ((width * (row + 1)) - gap));
                    uv[2] = new Vector2((width * col) + gap, 1 - ((width * row) + gap));
                    uv[3] = new Vector2((width * (col + 1)) - gap, 1 - ((width * row) + gap));
                    // Assign our new UVs to the mesh.
                    mesh.uv = uv;
                }
            }
        }
    }

    // colCheck is used to stop horizontal moves wrapping.
    private bool SwapIfValid(int i, int offset, int colCheck)
    {
        if (((i % size) != colCheck) && ((i + offset) == emptyLocation))
        {
            // Swap them in game state.
            (pieces[i], pieces[i + offset]) = (pieces[i + offset], pieces[i]);
            // Swap their transforms.
            (pieces[i].localPosition, pieces[i + offset].localPosition) = ((pieces[i + offset].localPosition, pieces[i].localPosition));
            // Update empty location.
            emptyLocation = i;
            return true;
        }
        return false;
    }

    // We name the pieces in order so we can use this to check completion.
    private bool CheckCompletion()
    {
        for (int i = 0; i < pieces.Count; i++)
        {
            if (pieces[i].name != $"{i}")
            {
                return false;
            }
        }
        return true;
    }

    private IEnumerator EnableCompletionCheckAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        canCheckCompletion = true;
    }

    // Brute force shuffling
    private void Shuffle()
    {
        int count = 0;
        int last = -1; // Ensure the last index is initially invalid
        while (count < (size * size * size))
        {
            int rnd = Random.Range(0, size * size);
            if (rnd == last) continue; // Skip if trying to undo the last move
            last = emptyLocation;
            if (SwapIfValid(rnd, -size, size) || SwapIfValid(rnd, +size, size) ||
                SwapIfValid(rnd, -1, 0) || SwapIfValid(rnd, +1, size - 1))
            {
                count++;
            }
        }
    }
}