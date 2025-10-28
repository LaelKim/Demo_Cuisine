using UnityEngine;

public class PanHeatSystem : MonoBehaviour
{
    [Header("Température")]
    [Range(0f, 300f)]
    public float currentPanTemp = 20f; // Température actuelle en °C
    public float ambientTemp = 20f; // Température ambiante
    public float maxTemp = 250f; // Température maximum
    
    [Header("Vitesses de chauffe/refroidissement")]
    public float heatingSpeed = 15f; // Vitesse de montée en température
    public float coolingSpeed = 5f; // Vitesse de refroidissement
    
    [Header("Seuils visuels")]
    public float readyTemp = 180f; // Température optimale (voile fumant)
    public float tooHotTemp = 230f; // Température de surchauffe
    
    [Header("Références")]
    public Renderer panRenderer; // Le renderer de la poêle
    public ParticleSystem smokeLight; // Fumée légère (optionnel)
    public ParticleSystem smokeHeavy; // Fumée dense (optionnel)
    
    [Header("Visuels")]
    public Color coldColor = new Color(0.5f, 0.5f, 0.5f); // Gris
    public Color hotColor = new Color(1f, 0.3f, 0f); // Orange
    
    private BurnerController currentBurner = null;
    private Material panMaterial;
    private bool isOnBurner = false;

    void Start()
    {
        // Récupère le material de la poêle
        if (panRenderer != null)
        {
            panMaterial = panRenderer.material;
        }
        
        // Arrête les particules au départ
        if (smokeLight != null) smokeLight.Stop();
        if (smokeHeavy != null) smokeHeavy.Stop();
    }

    void Update()
    {
        // Met à jour la température
        UpdateTemperature();
        
        // Met à jour les visuels
        UpdateVisuals();
    }

    void UpdateTemperature()
    {
        if (isOnBurner && currentBurner != null && currentBurner.isOn)
        {
            // Chauffe selon l'intensité du brûleur
            float targetTemp = currentBurner.GetIntensity() * maxTemp;
            currentPanTemp = Mathf.MoveTowards(currentPanTemp, targetTemp, heatingSpeed * Time.deltaTime);
        }
        else
        {
            // Refroidit vers la température ambiante
            currentPanTemp = Mathf.MoveTowards(currentPanTemp, ambientTemp, coolingSpeed * Time.deltaTime);
        }
        
        // Limite la température
        currentPanTemp = Mathf.Clamp(currentPanTemp, ambientTemp, maxTemp);
    }

    void UpdateVisuals()
    {
        // Change la couleur selon la température
        if (panMaterial != null)
        {
            float heatRatio = (currentPanTemp - ambientTemp) / (maxTemp - ambientTemp);
            Color targetColor = Color.Lerp(coldColor, hotColor, heatRatio);
            panMaterial.color = targetColor;
            
            // Émission pour l'effet de chaleur
            if (currentPanTemp > readyTemp)
            {
                panMaterial.SetColor("_EmissionColor", hotColor * heatRatio * 0.5f);
            }
            else
            {
                panMaterial.SetColor("_EmissionColor", Color.black);
            }
        }
        
        // Gère les particules de fumée
        if (currentPanTemp >= readyTemp && currentPanTemp < tooHotTemp)
        {
            // Fumée légère (prêt à cuire)
            if (smokeLight != null && !smokeLight.isPlaying)
                smokeLight.Play();
            if (smokeHeavy != null && smokeHeavy.isPlaying)
                smokeHeavy.Stop();
        }
        else if (currentPanTemp >= tooHotTemp)
        {
            // Fumée dense (trop chaud!)
            if (smokeLight != null && smokeLight.isPlaying)
                smokeLight.Stop();
            if (smokeHeavy != null && !smokeHeavy.isPlaying)
                smokeHeavy.Play();
        }
        else
        {
            // Pas de fumée
            if (smokeLight != null && smokeLight.isPlaying)
                smokeLight.Stop();
            if (smokeHeavy != null && smokeHeavy.isPlaying)
                smokeHeavy.Stop();
        }
    }

    // Détecte quand la poêle touche un brûleur
    void OnCollisionEnter(Collision collision)
    {
        BurnerController burner = collision.gameObject.GetComponent<BurnerController>();
        if (burner != null)
        {
            currentBurner = burner;
            isOnBurner = true;
            Debug.Log("Poêle posée sur le brûleur!");
        }
    }

    void OnCollisionStay(Collision collision)
    {
        // Continue à détecter le contact
        BurnerController burner = collision.gameObject.GetComponent<BurnerController>();
        if (burner != null)
        {
            currentBurner = burner;
            isOnBurner = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        BurnerController burner = collision.gameObject.GetComponent<BurnerController>();
        if (burner != null && burner == currentBurner)
        {
            isOnBurner = false;
            currentBurner = null;
            Debug.Log("Poêle retirée du brûleur!");
        }
    }

    // Méthode publique pour obtenir la température
    public float GetTemperature()
    {
        return currentPanTemp;
    }

    public string GetPanState()
    {
        if (currentPanTemp < readyTemp - 20f)
            return "Froide";
        else if (currentPanTemp >= readyTemp && currentPanTemp < tooHotTemp)
            return "Prête";
        else if (currentPanTemp >= tooHotTemp)
            return "Trop chaude!";
        else
            return "Chauffe...";
    }
}