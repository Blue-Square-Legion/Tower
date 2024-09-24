using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAlongPath : MonoBehaviour
{
    [SerializeField] Vector3[] nodePositions;
    [SerializeField] float speed;
    private int currentNode;
    public Transform NodeParent;

    // Start is called before the first frame update
    void Start()
    {
        currentNode = 0;
        nodePositions = new Vector3[NodeParent.childCount];

        for(int i = 0; i < nodePositions.Length; i++)
        {
            nodePositions[i] = NodeParent.GetChild(i).position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (currentNode > nodePositions.Length-1) return;

        if (transform.position == nodePositions[currentNode])
            currentNode++;
        transform.position = Vector3.MoveTowards(transform.position, nodePositions[currentNode], speed * Time.deltaTime);
    }
}
