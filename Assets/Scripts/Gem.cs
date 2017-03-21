using System.Collections.Generic;
using UnityEngine;

public class Gem : MonoBehaviour
{
    [SerializeField] private Transform markersPlace;
    [SerializeField] private Transform container;
    [SerializeField] private Renderer sphereRenderer;
    [SerializeField] private Rigidbody rgbody;
    [SerializeField] private GameObject toggle;
    [SerializeField] private List<Gem> neighbours = new List<Gem>();
    private int size;
    private bool isOn = false;
    private bool isMatched = false;
    private Material material;
    private Color color;
    private Grid grid;

    public bool IsMatched { get { return isMatched; } set { isMatched = value; } }
    public int X { get { return Mathf.RoundToInt(transform.localPosition.x); } }
    public int Y { get { return Mathf.RoundToInt(transform.localPosition.y); } }
    public Color Color { get { return color; } }
    public Rigidbody RigidBody { get { return rgbody; } }
    public List<Gem> Neighbours { get { return neighbours; } }

    public void Init(Grid grid, int size, Shader shader)
    {
        this.grid = grid;

        this.size = size;
        container.transform.localScale *= size;

        material = new Material(shader);
        RenewColor();
    }

    public void RenewColor()
    {
        isMatched = false;

        color = Colors.GetRandomGemColor();
        material.color = color;
        sphereRenderer.material = material;
    }

    public void CreateMarkers(int spaceBetweenGems)
    {
        float markerShift = size / 2f + spaceBetweenGems / 4f;
        float markerSize = spaceBetweenGems / 2f + 1f;

        CreateMarker(markerShift, 0f, markerSize, 1f, markersPlace);
        CreateMarker(-markerShift, 0f, markerSize, 1f, markersPlace);
        CreateMarker(0f, markerShift, 1f, markerSize, markersPlace);
        CreateMarker(0f, -markerShift, 1f, markerSize, markersPlace);
    }

    private void CreateMarker(float shift_x, float shift_y, float size_x, float size_y, Transform markersPlace)
    {
        GameObject markerObject = new GameObject("Marker");
        markerObject.transform.SetParent(markersPlace, false);
        markerObject.transform.localPosition = new Vector3(shift_x, shift_y, 0f);
        markerObject.tag = Globals.MarkerTag;

        BoxCollider markerCollider = markerObject.AddComponent<BoxCollider>();
        markerCollider.size = new Vector3(size_x, size_y, 1f);
        markerCollider.isTrigger = true;

        Marker marker = markerObject.AddComponent<Marker>();
        marker.InitMarker(this);
    }

    public bool HasNeighbour(Gem gem)
    {
        return neighbours.Contains(gem);
    }

    public void AddNeighbour(Gem gem)
    {
        neighbours.Add(gem);
    }

    public void RemoveNeighbour(Gem gem)
    {
        neighbours.Remove(gem);
    }

    public void Toggle()
    {
        isOn = !isOn;
        toggle.SetActive(isOn);
    }

    private void OnMouseDown()
    {
        if (!grid.IsSwapping)
        {
            Toggle();
            grid.SwapGems(this);
        }
    }
}
