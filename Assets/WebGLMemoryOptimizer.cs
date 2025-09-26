using UnityEngine;

public class WebGLMemoryOptimizer : MonoBehaviour
{
    [Header("Memory Optimization Settings")]
    [SerializeField] private bool enableMemoryOptimization = true;
    [SerializeField] private int targetFrameRate = 60;
    [SerializeField] private bool enableGarbageCollection = true;
    [SerializeField] private float gcInterval = 5f; // Run GC every 5 seconds
    
    private float gcTimer = 0f;
    
    void Start()
    {
        if (enableMemoryOptimization)
        {
            OptimizeMemorySettings();
        }
    }
    
    void Update()
    {
        if (enableGarbageCollection && gcTimer >= gcInterval)
        {
            System.GC.Collect();
            gcTimer = 0f;
            Debug.Log("[WebGLMemoryOptimizer] Garbage collection performed");
        }
        else
        {
            gcTimer += Time.deltaTime;
        }
    }
    
    private void OptimizeMemorySettings()
    {
        // Set target frame rate
        Application.targetFrameRate = targetFrameRate;
        
        // Disable vsync for better performance
        QualitySettings.vSyncCount = 0;
        
        // Optimize quality settings for WebGL
        QualitySettings.SetQualityLevel(0); // Use lowest quality preset
        
        // Disable unnecessary features
        QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
        QualitySettings.antiAliasing = 0;
        
        Debug.Log("[WebGLMemoryOptimizer] Memory optimization applied");
    }
    
    // Method to manually trigger garbage collection
    public void ForceGarbageCollection()
    {
        System.GC.Collect();
        Debug.Log("[WebGLMemoryOptimizer] Manual garbage collection triggered");
    }
    
    // Method to get memory usage info
    public void LogMemoryUsage()
    {
        long totalMemory = System.GC.GetTotalMemory(false);
        Debug.Log($"[WebGLMemoryOptimizer] Total Memory Usage: {totalMemory / 1024 / 1024} MB");
    }
}
