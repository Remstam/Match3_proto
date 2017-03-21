using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Grid : MonoBehaviour
{
    [SerializeField] private int amountToMatch = 3;
    [SerializeField] private int gridWidth;
    [SerializeField] private int gridHeight;
    [SerializeField] private int spaceBetweenGems;
    [SerializeField] private int gemSize;
    [SerializeField] private Gem gemPrefab;
    private bool isSwapping = false;
    private float top;
    private Gem oldGem;
    private Shader shader;
    private List<Gem> gems = new List<Gem>();

    public bool IsSwapping { get { return isSwapping; } }
    public int GridWidth { get { return gridWidth; } }
    public int GridHeight { get { return gridHeight; } }
    public List<Gem> Gems { get { return gems; } }

	private void Start ()
    {
        shader = Shader.Find("Legacy Shaders/Diffuse");
        InitializeGrid(shader, gridWidth, gridHeight, gemPrefab, gemSize, spaceBetweenGems);
	}

    private void InitializeGrid(Shader shader, int width, int height, Gem gemPrefab, int gemSize, int spaceBetweenGems)
    {
        Vector2 gemShift = new Vector2((gemSize + spaceBetweenGems) * (width - 1) / 2f, (gemSize + spaceBetweenGems) * (height - 1) / 2f);

        GameObject bottom = new GameObject("Bottom");
        bottom.transform.SetParent(transform, false);
        bottom.transform.localPosition = new Vector3(0f, -(gemShift.y + gemSize / 2f), 0f);
        top = -bottom.transform.localPosition.y;

        BoxCollider bottomCollider = bottom.AddComponent<BoxCollider>();
        bottomCollider.size = new Vector3(gemShift.x * 2f + gemSize, 0f, gemSize);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Gem gemObj = Instantiate(gemPrefab, new Vector3((gemSize + spaceBetweenGems) * x, (gemSize + spaceBetweenGems) * y, 0f), Quaternion.identity);
                gemObj.transform.SetParent(transform, false);
                gemObj.transform.localPosition -= new Vector3(gemShift.x, gemShift.y, 0f);

                gemObj.Init(this, gemSize, shader);
                gemObj.CreateMarkers(spaceBetweenGems);

                gems.Add(gemObj);
            }
        }
    }

    private void ToggleGravity(bool isOn)
    {
        isSwapping = !isOn;
        foreach (var gem in gems)
            gem.RigidBody.isKinematic = !isOn;
    }

    public void SwapGems(Gem currentGem)
    {
        if (oldGem == null)
        {
            oldGem = currentGem;
        }
        else if (oldGem == currentGem)
        {
            oldGem = null;
        }
        else
        {
            if (oldGem.HasNeighbour(currentGem))
            {
                Vector3 oldGemDest = currentGem.transform.position;
                Vector3 currentGemDest = oldGem.transform.position;

                Sequence seq = DOTween.Sequence();
                seq.Insert(0f, oldGem.transform.DOMove(oldGemDest, 1f));
                seq.Insert(0f, currentGem.transform.DOMove(currentGemDest, 1f));
                seq.OnStart(() => ToggleGravity(false));
                seq.OnComplete(
                    () =>
                    {
                        ToggleGravity(true);
                        currentGem.Toggle();
                        oldGem.Toggle();
                        if (IsMatch(oldGem, currentGem))
                            UpdateMatchedGems();
                        else
                        {
                            Sequence backSeq = DOTween.Sequence();
                            backSeq.Insert(0f, oldGem.transform.DOMove(currentGemDest, 1f));
                            backSeq.Insert(0f, currentGem.transform.DOMove(oldGemDest, 1f));
                            backSeq.OnStart(() => ToggleGravity(false));
                            backSeq.OnComplete(() => ToggleGravity(true));
                        }

                        oldGem = null;
                });
            }
            else
            {
                oldGem.Toggle();
                oldGem = currentGem;
            }
        }
    }

    private void UpdateMatchedGems()
    {
        foreach (var gem in gems)
        {
            if (gem.IsMatched)
            {
                gem.RenewColor();
                gem.transform.position += new Vector3(0f, 7f, 0f);
            }
        }
    }

    private bool IsGemOverTheTop(Gem gem, float top)
    {
        return gem.transform.localPosition.y >= top + 0.5f;
    }

    private bool IsGemOverVelocity(Gem gem)
    {
        return gem.RigidBody.velocity.y > 0.1f;
    }

    private void CheckNearby()
    {
        foreach (var gem in gems)
        {
            if (isSwapping || IsGemOverTheTop(gem, top) || IsGemOverVelocity(gem))
                return;
            List<Gem> gemList = new List<Gem>();
            ConstructMatchingArea(gem.Color, gem, gem.X, gem.Y, ref gemList);
            if (SplitToRowsAndColumns(gem, gemList))
                UpdateMatchedGems();
        }
    }

    private bool IsMatch(Gem oldGem, Gem currentGem)
    {
        bool isOldGemMatched = false;
        bool isCurrentGemMatched = false;
        List<Gem> oldGemList = new List<Gem>();
        List<Gem> currentGemList = new List<Gem>();

        ConstructMatchingArea(oldGem.Color, oldGem, oldGem.X, oldGem.Y, ref oldGemList);
        isOldGemMatched = SplitToRowsAndColumns(oldGem, oldGemList);
        ConstructMatchingArea(currentGem.Color, currentGem, currentGem.X, currentGem.Y, ref currentGemList);
        isCurrentGemMatched = SplitToRowsAndColumns(currentGem, currentGemList);

        return isOldGemMatched || isCurrentGemMatched;
    }

    private bool SplitToRowsAndColumns(Gem gem, List<Gem> listToSplit)
    {
        bool isGemMatched = false;
        List<Gem> rows = new List<Gem>();
        List<Gem> columns = new List<Gem>();

        foreach (var g in listToSplit)
        {
            if (gem.X == g.X)
                rows.Add(g);
            if (gem.Y == g.Y)
                columns.Add(g);
        }

        if (rows.Count >= amountToMatch)
        {
            isGemMatched = true;
            foreach (var g in rows)
                g.IsMatched = true;
        }

        if (columns.Count >= amountToMatch)
        {
            isGemMatched = true;
            foreach (var g in columns)
                g.IsMatched = true;
        }

        return isGemMatched;
    }

    private void ConstructMatchingArea(Color color, Gem gem, int X, int Y, ref List<Gem> matchList)
    {
        if (gem == null)
            return;

        if (gem.Color != color)
            return;

        if (matchList.Contains(gem))
            return;

        matchList.Add(gem);
        if (X == gem.X || Y == gem.Y)
        {
            foreach (var g in gem.Neighbours)
            {
                ConstructMatchingArea(color, g, X, Y, ref matchList);
            }
        }
    }

    private void Update()
    {
        CheckNearby();
    }
}
