using UnityEngine;

public class SteakCooking : MonoBehaviour
{
    [Header("Température à cœur")]
    [Range(20f, 80f)]
    public float coreTemp = 20f; // Température à cœur du steak
    public float ambientTemp = 20f;
    
    [Header("Seuils de cuisson")]
    public float rareTemp = 50f; // Saignant
    public float mediumTemp = 56f; // À point
    public float wellDoneTemp = 65f; // Bien cuit
    public float burntTemp = 75f; // Cramé
    
    [Header("Vitesses")]
    public float cookingSpeed = 2f; // Vitesse de montée en température
    public float coolingSpeed = 0.5f; // Vitesse de refroidissement
    
    [Header("Contact avec la poêle")]
    public float minContactTime = 2f; // Temps minimum de contact pour cuire
    public float minPanTemp = 150f; // Température minimum de la poêle
    
    [Header("Références")]
    public Renderer steakRenderer; // Le renderer du steak
    
    [Header("Materials de cuisson")]
    public Material rawMaterial; // Cru (rouge)
    public Material rareMaterial; // Saignant (rose)
    public Material mediumMaterial; // À point (rosé-brun)
    public Material wellDoneMaterial; // Bien cuit (brun)
    public Material burntMaterial; // Cramé (noir)
    
    private PanHeatSystem currentPan = null;
    private bool isOnPan = false;
    private float contactTime = 0f;
    private bool isCooking = false;
    private int flipCount = 0; // Nombre de retournements
    private bool firstSideDone = false;

    void Start()
    {
        // Applique le material cru au départ
        if (steakRenderer != null && rawMaterial != null)
        {
            steakRenderer.material = rawMaterial;
        }
    }

    void Update()
    {
        // Met à jour le temps de contact
        if (isOnPan && currentPan != null)
        {
            contactTime += Time.deltaTime;
            
            // Commence à cuire après le temps minimum ET si la poêle est chaude
            if (contactTime >= minContactTime && currentPan.GetTemperature() >= minPanTemp)
            {
                isCooking = true;
            }
        }
        else
        {
            contactTime = 0f;
            isCooking = false;
        }
        
        // Met à jour la température du steak
        UpdateCoreTemperature();
        
        // Met à jour l'apparence
        UpdateSteakVisual();
    }

    void UpdateCoreTemperature()
    {
        if (isCooking && currentPan != null)
        {
            // La température à cœur monte vers la température de la poêle
            float panTemp = currentPan.GetTemperature();
            float tempDifference = panTemp - coreTemp;
            
            // Monte progressivement (conductivité thermique simplifiée)
            coreTemp += tempDifference * cookingSpeed * Time.deltaTime * 0.01f;
            coreTemp = Mathf.Clamp(coreTemp, ambientTemp, panTemp);
        }
        else if (!isOnPan)
        {
            // Refroidit lentement quand pas sur la poêle
            coreTemp = Mathf.MoveTowards(coreTemp, ambientTemp, coolingSpeed * Time.deltaTime);
        }
    }

    void UpdateSteakVisual()
    {
        if (steakRenderer == null) return;
        
        // Change le material selon la température
        if (coreTemp < rareTemp)
        {
            // Cru
            if (rawMaterial != null)
                steakRenderer.material = rawMaterial;
        }
        else if (coreTemp >= rareTemp && coreTemp < mediumTemp)
        {
            // Saignant
            if (rareMaterial != null)
                steakRenderer.material = rareMaterial;
        }
        else if (coreTemp >= mediumTemp && coreTemp < wellDoneTemp)
        {
            // À point
            if (mediumMaterial != null)
                steakRenderer.material = mediumMaterial;
        }
        else if (coreTemp >= wellDoneTemp && coreTemp < burntTemp)
        {
            // Bien cuit
            if (wellDoneMaterial != null)
                steakRenderer.material = wellDoneMaterial;
        }
        else
        {
            // Cramé
            if (burntMaterial != null)
                steakRenderer.material = burntMaterial;
        }
    }

    // Détecte le contact avec la poêle
    void OnCollisionEnter(Collision collision)
    {
        PanHeatSystem pan = collision.gameObject.GetComponent<PanHeatSystem>();
        if (pan != null)
        {
            currentPan = pan;
            isOnPan = true;
            contactTime = 0f;
            Debug.Log("Steak posé sur la poêle! Température poêle: " + pan.GetTemperature() + "°C");
        }
    }

    void OnCollisionStay(Collision collision)
    {
        // Continue le contact
        PanHeatSystem pan = collision.gameObject.GetComponent<PanHeatSystem>();
        if (pan != null)
        {
            currentPan = pan;
            isOnPan = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        PanHeatSystem pan = collision.gameObject.GetComponent<PanHeatSystem>();
        if (pan != null && pan == currentPan)
        {
            isOnPan = false;
            contactTime = 0f;
            isCooking = false;
            Debug.Log("Steak retiré de la poêle!");
        }
    }

    // Méthodes publiques
    public float GetCoreTemp()
    {
        return coreTemp;
    }

    public string GetCookingState()
    {
        if (coreTemp < rareTemp)
            return "Cru";
        else if (coreTemp < mediumTemp)
            return "Saignant";
        else if (coreTemp < wellDoneTemp)
            return "À point";
        else if (coreTemp < burntTemp)
            return "Bien cuit";
        else
            return "Cramé!";
    }

    public bool IsOnPan()
    {
        return isOnPan;
    }

    public float GetContactTime()
    {
        return contactTime;
    }
}