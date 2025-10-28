using UnityEngine;

public class BurnerController : MonoBehaviour
{
    [Header("Paramètres du brûleur")]
    [Range(0f, 1f)]
    public float intensity = 0f; // 0 = éteint, 1 = max
    public bool isOn = false;

    [Header("Références")]
    public Renderer burnerRenderer; // Le renderer du brûleur
    public ParticleSystem flameParticles; // Optionnel : particules de feu

    [Header("Couleurs")]
    public Color coldColor = Color.black;
    public Color hotColor = Color.red;

    private Material burnerMaterial;

    void Start()
    {
        // Récupère le material du brûleur
        if (burnerRenderer != null)
        {
            burnerMaterial = burnerRenderer.material;
        }
        
        UpdateBurnerVisuals();
    }

    void Update()
    {
        // Met à jour les visuels en temps réel
        UpdateBurnerVisuals();
    }

    // Change la couleur selon l'intensité
    void UpdateBurnerVisuals()
    {
        if (burnerMaterial != null && isOn)
        {
            // Interpole entre noir et rouge selon l'intensité
            Color targetColor = Color.Lerp(coldColor, hotColor, intensity);
            burnerMaterial.color = targetColor;
            
            // Ajoute de l'émission pour l'effet lumineux
            burnerMaterial.SetColor("_EmissionColor", targetColor * intensity * 2f);
        }
        else if (burnerMaterial != null)
        {
            // Éteint : noir
            burnerMaterial.color = coldColor;
            burnerMaterial.SetColor("_EmissionColor", Color.black);
        }

        // Gère les particules si présentes
        if (flameParticles != null)
        {
            if (isOn && intensity > 0.1f)
            {
                if (!flameParticles.isPlaying)
                    flameParticles.Play();
            }
            else
            {
                if (flameParticles.isPlaying)
                    flameParticles.Stop();
            }
        }
    }

    // Méthodes publiques pour contrôler le brûleur
    public void TurnOn()
    {
        isOn = true;
    }

    public void TurnOff()
    {
        isOn = false;
        intensity = 0f;
    }

    public void SetIntensity(float value)
    {
        intensity = Mathf.Clamp01(value); // Garde entre 0 et 1
    }

    public float GetIntensity()
    {
        return isOn ? intensity : 0f;
    }
}