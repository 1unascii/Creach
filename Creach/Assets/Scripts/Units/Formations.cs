using UnityEngine;

// A formation's shape, facing forward (+z). SelectionManager rotates it.
public abstract class Formation
{
    public abstract Vector3 GetOffset(int i, int count, float spacing);
}

// Square block
public class GridFormation : Formation
{
    public override Vector3 GetOffset(int i, int count, float spacing)
    {
        int columns = Mathf.CeilToInt(Mathf.Sqrt(count));
        int rows = Mathf.CeilToInt((float)count / columns);
        int col = i % columns;
        int row = i / columns;
        float x = (col - (columns - 1) / 2f) * spacing;
        float z = -(row - (rows - 1) / 2f) * spacing;
        return new Vector3(x, 0, z);
    }
}

// One row, side by side
public class LineFormation : Formation
{
    public override Vector3 GetOffset(int i, int count, float spacing)
    {
        float x = (i - (count - 1) / 2f) * spacing;
        return new Vector3(x, 0, 0);
    }
}

// Single file: good for roads and bridges
public class ColumnFormation : Formation
{
    public override Vector3 GetOffset(int i, int count, float spacing)
    {
        return new Vector3(0, 0, -i * spacing);
    }
}

// V shape, leader at the point: good for raiding
public class WedgeFormation : Formation
{
    public override Vector3 GetOffset(int i, int count, float spacing)
    {
        if(i == 0) return Vector3.zero;
        int row = (i + 1) / 2;              // 1,1,2,2,3,3...
        int side = (i % 2 == 1) ? -1 : 1;   // left, right, left, right...
        return new Vector3(side * row * spacing, 0, -row * spacing);
    }
}
