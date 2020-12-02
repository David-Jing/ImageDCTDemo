using Accord.Math;
using UnityEngine;
using UnityEngine.UI;

// Behaviors of user selected DCT components on displayed image.
public class ImageRenderer : MonoBehaviour
{
    public Transform dctToggleContainer;
    public DCTToggles dctToggles;
    public Image imagePanel;
    public Texture2D sampleImage;

    public bool halt = false;           // For halting dctCompUpdateCheck() during all toggle/untoggle
    public bool usingCustom = false;    // For switching to rendering player inputted image
    public bool forceRefresh = false;   // Forces updateDCTImage() to be ran.
    public Texture2D customImageRef;    // Saves the player inputted image in memory.

    private bool[,] componentMap;       // Tracks what DCT component is enabled/disabled
    private int currentSize = 0;        // Tracks the current DCT matrix size used

    // ====================================================================================================================
    // ================================================ IMAGE BEHAVIOR ====================================================
    // ====================================================================================================================

    /*
     * Checks DCT component toggle state and DCT matrix size by updating the componentMap variable.
     * If updates are detected, UpdateDCTImage() is called. This method can be halted for 
     * operation by changing the halt variable and can be forced to call the UpdateDCTImage()
     * method regardless via the forceRefresh variable. The forceRefresh variable will auto
     * reset upon one call, halt does not.
     **/
    public void DCTCompUpdateCheck()
    {
        // Do nothing if halt is set to true
        if (!halt)
        {
            bool changeDetected = false;
            int gridLength = (int)Mathf.Sqrt(dctToggleContainer.childCount);

            // Bug fix for child destruction in DCTToggle being slow.
            gridLength = (gridLength > dctToggles.maxMatrixSize) ? dctToggles.maxMatrixSize : gridLength;

            // If DCT matrix size has been changed.
            if (currentSize != gridLength)
            {
                changeDetected = true;
                currentSize = gridLength;
            }
            // If forceRefresh is activated.
            else if (forceRefresh)
            {
                changeDetected = true;
                forceRefresh = false;
                currentSize = gridLength;
            }

            // Check each DCT component's toggle state
            int count = 0;
            for (int col = 0; col < gridLength; col++)
            {
                for (int row = 0; row < gridLength; row++)
                {
                    Toggle toggle = dctToggleContainer.GetChild(count++).GetComponent<Toggle>();

                    if (!changeDetected)
                    {
                        if (toggle.isOn != componentMap[col, row])
                        {
                            changeDetected = true;
                        }
                    }

                    componentMap[col, row] = toggle.isOn;
                }
            }

            // If changed is detected, update the image.
            if (changeDetected)
            {
                UpdateDCTImage(gridLength);
            }
        }
    }

    /**
     * Calculates DCT transform of the image, zero-out the corresponding untoggled DCT components, and 
     * revertes back to image. The resultant image is loaded onto the imagePanel. The referenced image
     * used is determined by the usingCustom variable; the default image is loaded on false and the custom
     * loaded image is loaded on true.
     **/
    private void UpdateDCTImage(int N)
    {
        // Get height and width of image.
        int height = (usingCustom) ? customImageRef.height : sampleImage.height;
        int width = (usingCustom) ? customImageRef.width : sampleImage.width;

        // Setting up empty image canvas
        Texture2D image_Texture = new Texture2D(width, height)
        {
            // Disable image compression for a pixel-perfect image.
            filterMode = FilterMode.Point
        };
        Sprite image = Sprite.Create(image_Texture, new Rect(0, 0, width, height), Vector2.zero);
        imagePanel.sprite = image;

        // Get DCT matrix and its transpose.
        double[,] dctMatrix = dctToggles.GetDCTMatrix(N);
        double[,] dctMatrix_T = Matrix.Transpose(dctMatrix);

        // Iterate on each N*N block of image
        for (int i = 0; i < width; i += N)
        {
            for (int j = 0; j < height; j += N)
            {
                double[,] pixelBlock = new double[N, N];

                // ====================================== Load Greyscale Value ========================================
                for (int x = 0; x < N; x++)
                {
                    for (int y = 0; y < N; y++)
                    {
                        if (x + i < width && N - y + j - 1 < height)
                        {
                            // Note that sprite pixels are indexed bottom-up, left-right
                            pixelBlock[x, y] = (usingCustom) ? 256 * customImageRef.GetPixel(x + i, N - y + j - 1)[0] :
                                256 * sampleImage.GetPixel(x + i, N - y + j - 1)[0];
                        }
                    }
                }

                // ======================================== Get DCT Transform =========================================

                double[,] dctTransform = Matrix.Dot(Matrix.Dot(dctMatrix, pixelBlock), dctMatrix_T);

                // =================================== Zero-out Untoggled Components ==================================

                for (int x = 0; x < N; x++)
                {
                    for (int y = 0; y < N; y++)
                    {
                        if (!componentMap[y, x])
                        {
                            dctTransform[x, y] = 0;
                        }
                    }
                }

                // ========================================== Revert to Image =========================================

                double[,] transformedPixelBlock = Matrix.Dot(Matrix.Dot(dctMatrix_T, dctTransform), dctMatrix);

                // ======================================= Load Pixels to Canvas ======================================

                for (int x = 0; x < N; x++)
                {
                    for (int y = 0; y < N; y++)
                    {
                        if (x + i < width && N - y + j - 1 < height)
                        {
                            // Note that sprite pixels are indexed bottom-up, left-right
                            float grey = (float)(transformedPixelBlock[x, y] / 256);
                            image_Texture.SetPixel(x + i, N - y + j - 1, new Color(grey, grey, grey));
                        }
                    }
                }
            }
        }

        // Commit pixel changes.
        image_Texture.Apply();
    }

    // ====================================================================================================================
    // ====================================================================================================================
    // ====================================================================================================================

    /**
     * Start is called before the first frame update.
     **/
    private void Start()
    {
        // Create component map for tracking toggles based on max matrix size.
        componentMap = new bool[dctToggles.maxMatrixSize, dctToggles.maxMatrixSize];

        // Get starting DCT size.
        currentSize = dctToggles.defaultDCTSize;
    }
}
