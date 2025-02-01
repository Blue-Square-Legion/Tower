using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class DrawPath : MonoBehaviour
{
    private LineRenderer line;
    private Transform target;
    private NavMeshAgent agent;
    private Material lineMaterial;
    private Color baseColor;

    public float breathingSpeed = 4f;  // Speed of the breathing effect
    public float intensityRange = 1f;   // Intensity range for breathing

    void Start()
    {
        line = GetComponent<LineRenderer>();
        agent = GetComponent<NavMeshAgent>();

        // Ensure the LineRenderer has a material with emission
        lineMaterial = line.material;
        baseColor = lineMaterial.GetColor("_EmissionColor");  // Get the base emission color

        line.startWidth = 0.15f;
        line.endWidth = 0.15f;
        line.positionCount = 0;

        // Enable the emission in the shader if it's not already
        lineMaterial.EnableKeyword("_EMISSION");
    }

    private void Update()
    {
        if (!GameManager.Instance.showPaths)
        {
            line.enabled = false;
            return;
        }

        if (agent.path.status == NavMeshPathStatus.PathComplete)
        {
            line.enabled = true;
            DrawAgentPath();
            ApplyBreathingEffect();
        }
    }

    public void DrawAgentPath()
    {
        line.positionCount = agent.path.corners.Length;

        if (agent.path.corners.Length < 2)
            return;

        line.SetPosition(0, new Vector3(transform.position.x, transform.position.y + 0.1f, transform.position.z));

        for (var i = 1; i < agent.path.corners.Length; i++)
        {
            line.SetPosition(i, agent.path.corners[i]);
        }
    }

    private void ApplyBreathingEffect()
    {
        // Create a breathing effect using a sine wave
        float emissionStrength = Mathf.Sin(Time.time * breathingSpeed) * intensityRange;
        Color glowingColor = baseColor * (1 + emissionStrength);
        lineMaterial.SetColor("_EmissionColor", glowingColor);
    }
}