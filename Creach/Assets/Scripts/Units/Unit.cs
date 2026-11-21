using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
public class Unit : MonoBehaviour
{
    private NavMeshAgent agent;
    private Renderer rend;
    private Color normalColor;
    public static readonly List<Unit> All = new List<Unit>();

    void OnEnable() {All.Add(this);}
    void OnDisable() {All.Remove(this);}

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        rend = GetComponent<Renderer>();
        normalColor = rend.material.color;
    }
    // Yellow while selected
    public void SetSelected(bool selected)
    {
        rend.material.color = selected ? Color.yellow : normalColor;
    }

    // Walk to a point on the NavMesh
    public void MoveTo(Vector3 point)
    {
        agent.SetDestination(point);
    }
}
