using Accord.Math;
using UnityEngine;
using UnityEngine.UI;

// Behaviors for DCT component toggles and the toggle/untoggle all buttons.
public class DCTToggles : MonoBehaviour
{
    public GridLayoutGroup dctToggleGrid;
    public InputField sizeInput;
    public Text errorMessage;
    public Transform togglePrefab;
    public Button toggleAllButton, unToggleAllButton;
    public ImageRenderer imageRenderer;

    public int minMatrixSize = 1;
    public int maxMatrixSize = 32;
    public int toggleGridWidth = 800;
    public int defaultDCTSize = 4;

    // ====================================================================================================================
    // ====================================================== TOOLS =======================================================
    // ====================================================================================================================

    /**
     * Generates and returns a DCT matrix of size N.
     **/
    public double[,] GetDCTMatrix(int N)
    {
        double[,] dctMatrix = new double[N, N];

        for (int i = 0; i < N; i++)
        {
            for (int j = 0; j < N; j++)
            {
                if (i > 0)
                {
                    double a = Mathf.Sqrt(2f / N);
                    dctMatrix[i, j] = a * Mathf.Cos(((2 * j + 1) * i * Mathf.PI) / (2 * N));
                }
                else
                {
                    dctMatrix[i, j] = Mathf.Sqrt(1f / N);
                }
            }
        }

        return dctMatrix;
    }

    // ====================================================================================================================
    // ================================================ DCT GRID BEHAVIOR =================================================
    // ====================================================================================================================

    /**
     * Generates N*N pixeled image of the corresponding N-point DCT component based on index. 
     * The image is then loaded into the inputted toggle image component.
     **/
    private void LoadDCTComponentSprite(int row, int col, int N, Image toggleObject, double[,] dctMatrix)
    {
        // Setting up empty image canvas
        Texture2D dctComp_Texture = new Texture2D(N, N)
        {
            // Disable image compression for a pixel-perfect image.
            filterMode = FilterMode.Point
        };
        Sprite dctComp = Sprite.Create(dctComp_Texture, new Rect(0, 0, N, N), Vector2.zero);
        toggleObject.sprite = dctComp;

        // ============================== DCT Component Image Calculation =====================================

        // Get a "pure" image of the respective DCT component
        double[,] dctTransform = new double[N, N];
        dctTransform[col, row] = 256 * N;
        double[,] componentImage = Matrix.Dot(Matrix.Dot(Matrix.Transpose(dctMatrix), dctTransform), dctMatrix);

        // =============================== DCT Component Image Insertion ======================================

        // Normalize to 0-1 and copy the colors onto the sprite pixel by pixel
        double min = Matrix.Min(componentImage);
        double range = Matrix.Max(componentImage) - min;

        // Note that sprite pixels are indexed bottom-up, left-right
        for (int i = 0; i < N; i++)
        {
            for (int j = 0; j < N; j++)
            {
                // For case of DC component, black will be default if grey = NaN
                float grey = (float)((componentImage[i, N - j - 1] - min) / range);
                dctComp_Texture.SetPixel(i, j, new Color(grey, 0, 0)); // Red greyscale

            }
        }

        // Commit pixel changes.
        dctComp_Texture.Apply();
    }

    /**
     * Generates N*N new DCT component toggles with the appropriate size, and with the correct DCT images and toggle behaviors. 
     */
    private void UpdateGridCount(int N)
    {
        // ====================================== Update Grid Layout ==========================================

        // Change toggles per row to N
        dctToggleGrid.constraintCount = N;

        // Calculate size of each toggle to fit into grid
        int gridSize = toggleGridWidth / N - 3; // In-between spacing of 3.
        dctToggleGrid.cellSize = new Vector2(gridSize, gridSize);

        // ======================================== Toggle Creation ===========================================

        Transform dctToggleContainer = dctToggleGrid.GetComponent<Transform>();
        int newChildCount = N * N;

        // Enable prefab for cloning
        togglePrefab.gameObject.SetActive(true);

        // Remove previously created toggles
        foreach (Transform child in dctToggleContainer)
        {
            Destroy(child.gameObject);
        }

        // Get DCT matrix of size N for later DCT image generation.
        double[,] dctMatrix = GetDCTMatrix(N);

        // Create toggle and load the respective DCT component image
        int row = 0;
        int col = 0;
        for (int i = 0; i < newChildCount; i++)
        {
            Transform clone = Instantiate(togglePrefab, dctToggleGrid.GetComponent<Transform>());
            Image imgComp = clone.GetChild(0).GetComponent<Image>();
            clone.gameObject.name = row + " " + col;

            // Add listener to inform ImageRenderer that a DCT component is toggled/untoggled
            clone.GetComponent<Toggle>().onValueChanged.AddListener(delegate { imageRenderer.DCTCompUpdateCheck(); });

            // Add DCT component image to toggle
            LoadDCTComponentSprite(row, col, N, imgComp, dctMatrix);

            // Track 2D index
            col++;
            if (col >= N)
            {
                row++;
                col = 0;
            }
        }

        // Disable prefab after finishing
        togglePrefab.gameObject.SetActive(false);

        // Update the displayed image. Use ToggleOn to bypass child destruction delay.
        ToggleOn();
    }


    // ====================================================================================================================
    // =============================================== BUTTON BEHAVIOR ====================================================
    // ====================================================================================================================

    /**
     * Examines input after player presses "ENTER" and is focused on the inputfield. If
     * input is invalid, an error message is displayed. If the input is invalid, the error
     * message is cleared.
     **/
    private void GetSizeInput(InputField input)
    {
        if (input.text.Length > 0)
        {
            int value = int.Parse(input.text);

            if (value >= minMatrixSize && value <= maxMatrixSize)
            {
                errorMessage.text = "";
                UpdateGridCount(int.Parse(input.text));
            }
            else
            {
                errorMessage.text = "SIZE ERROR";
            }

            input.text = "";
        }
    }

    /**
     * Toggle on all created DCT component toggles. Uses halt variable to prevent
     * N*N updateDCTImage() calls in ImageRenderer during the process. After all
     * toggles are on, the displayed image is then called for update.
     **/
    private void ToggleOn()
    {
        // Use halt to prevent update after every individual change.
        imageRenderer.halt = true;
        foreach (Transform child in dctToggleGrid.GetComponent<Transform>())
        {
            child.GetComponent<Toggle>().isOn = true;
        }
        imageRenderer.halt = false;

        // Update image after toggling everything.
        imageRenderer.DCTCompUpdateCheck();
    }

    /**
     * Toggle off all created DCT component toggles. Uses halt variable to prevent
     * N*N updateDCTImage() calls in ImageRenderer during the process. After all
     * toggles are off, the displayed image is then called for update.
     **/
    private void ToggleOff()
    {
        // Use halt to prevent update after every individual change.
        imageRenderer.halt = true;
        foreach (Transform child in dctToggleGrid.GetComponent<Transform>())
        {
            child.GetComponent<Toggle>().isOn = false;
        }
        imageRenderer.halt = false;

        // Update image after untoggling everything.
        imageRenderer.DCTCompUpdateCheck();
    }

    // ====================================================================================================================
    // ====================================================================================================================
    // ====================================================================================================================

    /**
     * Start is called before the first frame update.
    **/
    private void Start()
    {
        // Clear out the error message, so nothing is displayed.
        errorMessage.text = "";

        // Listener that invokes ToggleOn()/ToggleOff() when player presses the respective button.
        toggleAllButton.onClick.AddListener(ToggleOn);
        unToggleAllButton.onClick.AddListener(ToggleOff);

        // Listener that invokes GetSizeInput() when the player finishes editing the DCT size input field.
        sizeInput.onEndEdit.AddListener(delegate { GetSizeInput(sizeInput); });

        // Default starting DCT size
        UpdateGridCount(defaultDCTSize);
    }
}
