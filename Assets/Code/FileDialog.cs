using SFB;
using UnityEngine;
using UnityEngine.UI;

// Behaviors for selecting custom or default images for the demo.
public class FileDialog : MonoBehaviour
{
    public Image imagePanel;
    public Text errorMessage;
    public ImageRenderer imageRenderer;
    public Button defaultButton, customButton;

    // ====================================================================================================================
    // =============================================== BUTTON BEHAVIOR ====================================================
    // ====================================================================================================================

    /**
     * Opens a file explorer and prompts player to select a PNG or JPG file; an error
     * is displayed if any other files are selected. The image is converted to grey-scale 
     * and loaded to ImageRenderer. Message is sent to ImageRenderer to render the custom image.
     **/
    private void OpenExplorer()
    {
        errorMessage.text = "";
        string[] path = StandaloneFileBrowser.OpenFilePanel("Select a PNG or JPG File...", "", "", false);

        // Check if a valid file is selected.
        if (path.Length == 0)
        {
            return;
        }
        else if (path[0].Length < 5)
        {
            return;
        }

        // Check for proper extensions
        if (path[0].Substring(path[0].Length - 4) == ".png" || path[0].Substring(path[0].Length - 4) == ".jpg")
        {
            // Retrieve image file
            WWW www = new WWW("file:///" + path[0]);
            Texture2D image_Texture = www.texture;
            int width = image_Texture.width;
            int height = image_Texture.height;

            // Convert image to greyscale
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Color rgb = image_Texture.GetPixel(i, j);
                    float grey = (float)(0.299 * rgb[0] + 0.587 * rgb[1] + 0.114 * rgb[2]);
                    image_Texture.SetPixel(i, j, new Color(grey, grey, grey));
                }
            }
            // Commit changes
            image_Texture.Apply();

            // Load the grey-scale image into the image panel
            imagePanel.sprite = Sprite.Create(image_Texture, new Rect(0, 0, width, height), Vector2.zero);

            // Sent "message" to Image_Behavior
            imageRenderer.customImageRef = image_Texture;
            imageRenderer.usingCustom = true;
            imageRenderer.forceRefresh = true;
            imageRenderer.DCTCompUpdateCheck();
        }
        else
        {
            errorMessage.text = "PNG or JPG Only!";
        }
    }

    /**
     * Messages ImageRenderer to render the default image.
     **/
    private void RestoreDefault()
    {
        imageRenderer.usingCustom = false;
        imageRenderer.forceRefresh = true;
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

        // Listener that invokes OpenExplorer()/RestoreDefault() when player presses the respective button.
        defaultButton.onClick.AddListener(RestoreDefault);
        customButton.onClick.AddListener(OpenExplorer);
    }
}