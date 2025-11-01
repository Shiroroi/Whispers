using UnityEngine;

public class EnemyWeight : MonoBehaviour
{
    [Header("Weight Settings")]
    [Tooltip("Weight affects parry outcomes. Higher = harder to parry")]
    public float weight = 50f;
    
    [Header("Weight Categories (for reference)")]
    [Tooltip("Light: 30-40 | Medium: 50-60 | Heavy: 70-100")]
    public string weightCategory = "Medium";
    
    void Start()
    {
        // Auto-categorize based on weight
        if (weight < 45f)
            weightCategory = "Light";
        else if (weight < 65f)
            weightCategory = "Medium";
        else
            weightCategory = "Heavy";
    }
}
