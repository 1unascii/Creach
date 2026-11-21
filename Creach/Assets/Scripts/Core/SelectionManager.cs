using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class SelectionManager : MonoBehaviour
{

    private List<Unit> selected = new List<Unit>();
    private Vector2 selectionBoxStart;
    private bool playerIsDraggingMouse = false;
    private const float mouseDragThreshold = 10f;
    private Formation currentFormation = new GridFormation();
    private Vector3 lastTarget;
    private Quaternion lastFacing = Quaternion.identity;
    private bool hasTarget = false;
    private const float formationSpacing = 1.5f;

    // Update is called once per frame
    void Update()
    {
        Mouse mouse = Mouse.current;
        if(mouse == null) return;
        // CHANGED: press starts a drag, release decides click vs. box
        if(mouse.leftButton.wasPressedThisFrame)
        {
            selectionBoxStart = mouse.position.ReadValue();
            playerIsDraggingMouse = true;
        }
        if(mouse.leftButton.wasReleasedThisFrame)
        {
            playerIsDraggingMouse = false;
            Vector2 dragEnd = mouse.position.ReadValue();
            if(Vector2.Distance(selectionBoxStart, dragEnd) < mouseDragThreshold)
                HandleLeftClick(mouse);
            else
                BoxSelect(selectionBoxStart, dragEnd);
        }
        if(mouse.rightButton.wasPressedThisFrame)
            HandleRightClick(mouse);
            HandleFormationKeys();

    }

    // 1-4 switch formation, even while moving
    private void HandleFormationKeys()
    {
        Keyboard keyboard = Keyboard.current;
        if(keyboard == null) return;

        Formation newFormation = null;
        if(keyboard.digit1Key.wasPressedThisFrame) newFormation = new GridFormation();
        if(keyboard.digit2Key.wasPressedThisFrame) newFormation = new LineFormation();
        if(keyboard.digit3Key.wasPressedThisFrame) newFormation = new ColumnFormation();
        if(keyboard.digit4Key.wasPressedThisFrame) newFormation = new WedgeFormation();

        if(newFormation == null) return;
        currentFormation = newFormation;
        if(hasTarget && selected.Count > 0)
            MoveInFormation();   // same target, same facing, new shape
    }

    private void HandleLeftClick(Mouse mouse)
    {
        if(!ShiftHeld())                        // CHANGED: uses ShiftHeld()
            ClearSelection();

        if(Physics.Raycast(MouseRay(mouse), out RaycastHit hit))
        {
            Unit unit = hit.collider.GetComponent<Unit>();
            if(unit != null)
                AddToSelection(unit);           // CHANGED: uses AddToSelection()
        }
    }

    // NEW: select every unit inside the dragged box
    private void BoxSelect(Vector2 start, Vector2 end)
    {
        if(!ShiftHeld())
            ClearSelection();

        Rect box = ScreenRect(start, end);
        foreach(Unit unit in Unit.All)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(unit.transform.position);
            // z > 0 means the unit is in front of the camera
            if(screenPos.z > 0 && box.Contains(screenPos))
                AddToSelection(unit);
        }
    }

    // NEW: shared by click and box select
    private void AddToSelection(Unit unit)
    {
        if(selected.Contains(unit)) return;
        selected.Add(unit);
        unit.SetSelected(true);
    }

    // NEW
    private bool ShiftHeld()
    {
        return Keyboard.current != null && Keyboard.current.shiftKey.isPressed;
    }

    private void HandleRightClick(Mouse mouse)
    {
        if(selected.Count == 0) return;

        if(Physics.Raycast(MouseRay(mouse), out RaycastHit hit))
        {
            // Face from the middle of the group toward the click
            Vector3 center = Vector3.zero;
            foreach(Unit unit in selected)
                center += unit.transform.position;
            center /= selected.Count;

            Vector3 direction = hit.point - center;
            direction.y = 0;
            if(direction.sqrMagnitude > 0.01f)
                lastFacing = Quaternion.LookRotation(direction);

            lastTarget = hit.point;
            hasTarget = true;
            MoveInFormation();
        }
    }

    // Sends each unit to its spot, rotated to face the direction of travel
    // Sends each unit to the nearest open spot in the formation
    private void MoveInFormation()
    {
        int count = selected.Count;

        // Work out every spot first
        List<Vector3> slots = new List<Vector3>();
        for(int i = 0; i < count; i++)
        {
            Vector3 offset = currentFormation.GetOffset(i, count, formationSpacing);
            slots.Add(lastTarget + lastFacing * offset);
        }

        // Repeatedly match the closest unit-spot pair
        List<Unit> units = new List<Unit>(selected);
        while(units.Count > 0)
        {
            float bestDistance = float.MaxValue;
            int bestUnit = 0;
            int bestSlot = 0;

            for(int u = 0; u < units.Count; u++)
            {
                for(int s = 0; s < slots.Count; s++)
                {
                    float distance = (units[u].transform.position - slots[s]).sqrMagnitude;
                    if(distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestUnit = u;
                        bestSlot = s;
                    }
                }
            }

            units[bestUnit].MoveTo(slots[bestSlot]);
            units.RemoveAt(bestUnit);
            slots.RemoveAt(bestSlot);
        }
    }


    private void ClearSelection()
    {
        foreach(Unit unit in selected)
            unit.SetSelected(false);
        selected.Clear();
    }

    private Ray MouseRay(Mouse mouse)
    {
        return Camera.main.ScreenPointToRay(mouse.position.ReadValue());
    }

     // NEW: a rectangle from any two corners, whichever way you dragged
    private Rect ScreenRect(Vector2 a, Vector2 b)
    {
        return Rect.MinMaxRect(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y),
                               Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y));
    }

    // NEW: draws the box while playerIsDraggingMouse
    void OnGUI()
    {
        if(!playerIsDraggingMouse || Mouse.current == null) return;

        Vector2 now = Mouse.current.position.ReadValue();
        if(Vector2.Distance(selectionBoxStart, now) < mouseDragThreshold) return;

        Rect box = ScreenRect(selectionBoxStart, now);
        // GUI measures y from the top; the mouse measures from the bottom
        box.y = Screen.height - box.y - box.height;
        GUI.Box(box, "");
    }

}
