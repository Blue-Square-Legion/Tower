using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAlongPath : MonoBehaviour
{
    [SerializeField] Vector3[] nodePositions;
    [SerializeField] float speed;
    private int currentNode;
    public Transform NodeParent;

    void Start()
    {
        //initialize path
        NodeParent = GameObject.Find("Nodes").transform;
        currentNode = 0;
        nodePositions = new Vector3[NodeParent.childCount];

        for(int i = 0; i < nodePositions.Length; i++)
        {
            nodePositions[i] = NodeParent.GetChild(i).position;
        }
    }

    void Update()
    {
        if (currentNode > nodePositions.Length-1) return;

        if (transform.position == nodePositions[currentNode])
            currentNode++;
        //move to next node
        transform.position = Vector3.MoveTowards(transform.position, nodePositions[currentNode], speed * Time.deltaTime);
    }

    public float DistanceUntilEnd()
    {
        float distance = 0;
        for(int i = currentNode; i<nodePositions.Length-1; i++)
        {
            distance += Vector3.Distance(nodePositions[i], nodePositions[i+1]);
        }
        distance += Vector3.Distance(transform.position, nodePositions[currentNode]);
        return distance;
    }
}
