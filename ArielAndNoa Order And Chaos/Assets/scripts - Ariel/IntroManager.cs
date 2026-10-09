using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // required for scene loading

public class IntroManager : MonoBehaviour
{
    [Header("Particle Settings")]
    public GameObject particlePrefab;
    public Texture2D introImage;
    public float explosionForce = 6f;
    public float particleScaleSize = 0.12f;

    [Header("Resolution & Detail")]
    [Range(10, 100)]
    public int imageResolution = 70;

    [Header("Timing Settings")]
    public float timeToForm = 4f;     // Time taken for particles to aggregate
    public float holdTime = 1.5f;     // Brief pause before switching scenes

    [Header("Scene Management")]
    [Tooltip("Type the exact name of your gameplay scene here")]
    public string nextSceneName;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip explosionSound;

    private List<PointParticle> particles = new List<PointParticle>();

    void Start()
    {
        if (introImage == null)
        {
            Debug.LogError("Please assign the Intro Image in the Inspector!");
            return;
        }

        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogError("Please specify the Next Scene Name in the Inspector!");
            return;
        }

        StartCoroutine(IntroToNextSceneRoutine());
    }

    IEnumerator IntroToNextSceneRoutine()
    {
        // 1. Extract position and color data scaled to fit full screen
        List<Vector3> targetPositions = new List<Vector3>();
        List<Color> targetColors = new List<Color>();
        ExtractDataFromImage(introImage, targetPositions, targetColors);

        AdjustParticleCount(targetPositions.Count);

        // 2. Play explosion sound effect
        if (audioSource != null && explosionSound != null)
        {
            audioSource.PlayOneShot(explosionSound);
        }

        // 3. Trigger particle movement to assemble the image
        Vector3 particleScale = new Vector3(particleScaleSize, particleScaleSize, 1f);
        int countToUpdate = Mathf.Min(particles.Count, targetPositions.Count);

        for (int i = 0; i < countToUpdate; i++)
        {
            particles[i].InitParticle(
                targetPositions[i],
                particleScale,
                targetColors[i],
                explosionForce,
                timeToForm
            );
        }

        // 4. Wait for the image to assemble completely
        yield return new WaitForSeconds(timeToForm + holdTime);

        // 5. Load the gameplay scene
        SceneManager.LoadScene(nextSceneName);
    }

    void ExtractDataFromImage(Texture2D img, List<Vector3> outPositions, List<Color> outColors)
    {
        Camera mainCam = Camera.main;

        float screenHeight = mainCam.orthographicSize * 2f;
        float screenWidth = screenHeight * mainCam.aspect;

        int step = Mathf.Max(1, img.width / imageResolution);

        for (int x = 0; x < img.width; x += step)
        {
            for (int y = 0; y < img.height; y += step)
            {
                Color pixelColor = img.GetPixel(x, y);

                if (pixelColor.a > 0.2f)
                {
                    float worldX = ((float)x / img.width - 0.5f) * screenWidth;
                    float worldY = ((float)y / img.height - 0.5f) * screenHeight;

                    outPositions.Add(new Vector3(worldX, worldY, 0));
                    outColors.Add(pixelColor);
                }
            }
        }
    }

    void AdjustParticleCount(int requiredCount)
    {
        while (particles.Count < requiredCount)
        {
            GameObject newParticle = Instantiate(particlePrefab, Vector3.zero, Quaternion.identity);
            PointParticle particleScript = newParticle.GetComponent<PointParticle>();
            particles.Add(particleScript);
        }

        for (int i = 0; i < particles.Count; i++)
        {
            particles[i].gameObject.SetActive(i < requiredCount);
        }
    }
}