using UnityEngine;

public class BloodEffect : MonoBehaviour
{
    public int particleCount = 10;
    public float spread = 1f;
    public float minSpeed = 2f;
    public float maxSpeed = 5f;
    public float gravity = -9.8f;
    public float lifetime = 1f;
    public Color bloodColor = new Color(0.8f, 0f, 0f, 1f); // Red
    public float particleSize = 0.1f;
    
    private ParticleSystem ps;
    
    void Start()
    {
        CreateBloodParticles();
    }
    
    void CreateBloodParticles()
    {
        // Add particle system if it doesn't exist
        ps = GetComponent<ParticleSystem>();
        if (ps == null)
        {
            ps = gameObject.AddComponent<ParticleSystem>();
        }
        
        // Configure main module
        var main = ps.main;
        main.startLifetime = lifetime;
        main.startSpeed = new ParticleSystem.MinMaxCurve(minSpeed, maxSpeed);
        main.startSize = particleSize;
        main.startColor = bloodColor;
        main.gravityModifier = gravity / 9.8f;
        main.maxParticles = particleCount;
        main.loop = false;
        
        // Configure emission
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0.0f, particleCount)
        });
        
        // Configure shape (spread in all directions)
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = spread;
        
        // Configure renderer
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.material.color = bloodColor;
        
        // Play the effect
        ps.Play();
    }
}
