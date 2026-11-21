using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NavMeshAgent))]
public class ClickToMove : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private NavMeshAgent agent;
    private Renderer rend;
    private Color normalColor;
    private bool selected = false;
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rend = GetComponent<Renderer>();
        normalColor = rend.material.color;
        
        // Debug: is the pony on the NavMesh? If not, how far is the nearest point?
        if (UnityEngine.AI.NavMesh.SamplePosition(transform.position, out UnityEngine.AI.NavMeshHit navHit, 10f, UnityEngine.AI.NavMesh.AllAreas))
            Debug.Log("On NavMesh: " + agent.isOnNavMesh + ", nearest NavMesh point: " + navHit.position);
        else
            Debug.Log("No NavMesh within 10 units of the pony");


    }

    // Update is called once per frame
    void Update()
    {
        Mouse mouse = Mouse.current;
        if(mouse == null) return;

        //Left-click: select if we clicked this pony, otherwise deselect
        if(mouse.leftButton.wasPressedThisFrame)
        {
            if(Physics.Raycast(MouseRay(mouse), out RaycastHit hit))
                SetSelected(hit.collider.gameObject == gameObject);
            else
                SetSelected(false);
        }

        //Right-click: if selected, walk to the clicked point
        if(selected && mouse.rightButton.wasPressedThisFrame)
        {
            if(Physics.Raycast(MouseRay(mouse), out RaycastHit hit))
                agent.SetDestination(hit.point);
        }

        
    }

    //A line from the camera through the mouse cursor into the world
    private Ray MouseRay(Mouse mouse)
    {
        return Camera.main.ScreenPointToRay(mouse.position.ReadValue());
    }

    //Turns the pony yellow while selected
    private void SetSelected(bool value)
    {
        selected = value;
        rend.material.color = value ? Color.yellow : normalColor;
    }
}
