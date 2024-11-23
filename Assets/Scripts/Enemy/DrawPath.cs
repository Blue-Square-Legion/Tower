using System.IO;
using UnityEngine;
using UnityEngine.AI;

public class DrawPath : MonoBehaviour
{
    private LineRenderer line;
    private Transform target; 
    private NavMeshAgent agent;

    void Start()
    {
        line = GetComponent<LineRenderer>();
        agent = GetComponent<NavMeshAgent>();
        
        line.startWidth = 0.15f;
        line.endWidth = 0.15f;
        line.positionCount = 0;
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
        }   
    }

    public void DrawAgentPath()
    {
        line.positionCount = agent.path.corners.Length;

        if (agent.path.corners.Length < 2)
            return;

        line.SetPosition(0, transform.position);

        for (var i = 1; i < agent.path.corners.Length; i++)
        {
            line.SetPosition(i, agent.path.corners[i]);
        }
    }
}
