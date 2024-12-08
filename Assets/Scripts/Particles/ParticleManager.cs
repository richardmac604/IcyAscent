using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour {
    public static ParticleManager Instance { get; private set; }

    [Header("Particle Effects")]
    [SerializeField] private ParticleSystem iceParticles;    // Particle effect for Ice
    [SerializeField] private ParticleSystem snowParticles;   // Particle effect for Snow
    [SerializeField] private ParticleSystem metalParticles;  // Particle effect for Metal
    [SerializeField] private ParticleSystem rockParticles;   // Particle effect for Rock
    [SerializeField] private ParticleSystem woodParticles;   // Particle effect for Wood

    public enum ParticleType {
        Ice,
        Snow,
        Metal,
        Rock,
        Wood,
        None,
    }

    private Dictionary<ParticleType, ParticleSystem> particleDictionary;

    private void Awake() {
        if (Instance != null && Instance != this) {
            DestroyImmediate(gameObject);
        } else {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start() {
        // Initialize the dictionary to map particle types to particle systems
        particleDictionary = new Dictionary<ParticleType, ParticleSystem> {
            { ParticleType.Ice, iceParticles },
            { ParticleType.Snow, snowParticles },
            { ParticleType.Metal, metalParticles },
            { ParticleType.Rock, rockParticles },
            { ParticleType.Wood, woodParticles },
            { ParticleType.None, null },
        };
    }

    public void PlayParticle(ParticleType particleType, Vector3 position) {
        if (particleType == ParticleType.None) return;
        if (particleDictionary.TryGetValue(particleType, out ParticleSystem particlePrefab)) {
            ParticleSystem particleInstance = Instantiate(particlePrefab, position, Quaternion.identity);

            // Destroy particles after played
            var mainModule = particleInstance.main;
            mainModule.stopAction = ParticleSystemStopAction.Destroy;

            particleInstance.Play();
        } else {
            Debug.LogError($"Particle type {particleType} not found in ParticleManager!");
        }
    }
}
