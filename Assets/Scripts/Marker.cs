using UnityEngine;

public class Marker : MonoBehaviour
{
    private Gem owner;
    
    public Gem Owner { get { return owner; } }

    public void InitMarker(Gem owner)
    {
        this.owner = owner;
    }

    private Gem GetGem(Collider other)
    {
        if (string.Compare(other.gameObject.tag, Globals.MarkerTag, System.StringComparison.Ordinal) == 0)
        {
            Marker g = other.GetComponent<Marker>();
            if (g != null)
                return g.Owner;

            return null;
        }

        return null;
    }

    private void OnTriggerEnter(Collider other)
    {
        Gem g = GetGem(other);
        if (g != null)
        {
            if (!owner.HasNeighbour(g))
                owner.AddNeighbour(g);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Gem g = GetGem(other);
        if (g != null)
        {
            owner.RemoveNeighbour(g);
        }
    }
}