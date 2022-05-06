using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System;
using System.Linq;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GridManager : MonoBehaviour
{
    #region Inspector
    // resources
    [Header("Resources"),Required]
    public GameObject superpositionPrefab;
    [Required, Expandable]
    public TileSet tileset;

    // size + seed
    [Header("Initial settings"), HorizontalLine]
    [ValidateInput("ValidateSize","size must be >0 and odd")]
    public int size;
    bool ValidateSize(int newSize) => newSize > 0 && newSize % 2 == 1;
    public int seed;

    [Header("Settings"), HorizontalLine]
    public bool throwOnInvalidStates = true;
    
    // auto update
    public bool autoUpdate = true;
    [EnableIf("autoUpdate"), MinValue(0)] 
    public float autoUpdateTime = 0.02f;
    float _autoUpdateTime;

    // auto collapse
    public bool autoCollapse = true;
    [EnableIf("autoCollapse"), MinValue(0)]
    public float autoCollapseTime = 0.2f;
    float _autoCollapseTime;
    #endregion
    public HexPosition middle;
    public Slot[,] grid;

    RandomQueue<(Slot slot, HexSide side, Slot other, RemoveSuperpositionsChange change)> updateQueue;
    Stack<IChange> changesStack = new();
    System.Random rand;

    #region Misc
    void Awake()
    {
        if (seed == 0)
            seed = UnityEngine.Random.Range(1,int.MaxValue); // this is silly
        rand = new System.Random(seed);
        updateQueue = new(rand);

        grid = new Slot[size, size];
        middle = new HexPosition(size / 2, size / 2);

        for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
            {
                var pos = new HexPosition(i, j);
                if (InBounds(pos))
                    Slot.Create(this, pos);
            }
    }

    private void FixedUpdate()
    {
        _autoUpdateTime -= Time.deltaTime;
        _autoCollapseTime -= Time.deltaTime;
        Auto();
    }

    // handles auto updating and auto collapsing 
    public void Auto()
    {
        // never auto collapse if there are updates to be processed
        if (updateQueue.Count > 0)
        {
            if (!autoUpdate) return;

            if (autoUpdateTime == 0)
                UpdateAll();
            
            else if (_autoUpdateTime <= 0)
            {
                _autoUpdateTime = autoUpdateTime;
                UpdateOnce();
                
                if (updateQueue.Count > 0)
                    return;
            }
        }

        if (autoCollapse && _autoCollapseTime <= 0)
        {
            // can cause stack overflows because Auto() gets called again when calling CollapseRandom()
            _autoCollapseTime = autoCollapseTime;
            CollapseRandom();
        }
    }

    public bool InBounds(HexPosition hexPos)
    {
        if (!(0 <= hexPos.X && hexPos.X < size && 0 <= hexPos.Y && hexPos.Y < size))
            return false;
        return middle.Distance(hexPos) <= size/2;
    }
    #endregion

    #region Updates
    public void AssertNoPendingUpdates()
    {
        if (updateQueue.Count > 0 && throwOnInvalidStates)
            throw new InvalidOperationException();
    }

    public void RegisterUpdate(Slot slot, HexSide side, Slot other, RemoveSuperpositionsChange change)
    {
        updateQueue.Push((slot, side, other, change));

        Auto();
    }

    [Button]
    void UpdateOnce()
    {
        if (updateQueue.Count == 0)
            return;

        var info = updateQueue.PopRandom();
        info.slot.UpdateFromSide(info.side, info.other, info.change);
    }
    
    [Button]
    void UpdateAll()
    {
        while (updateQueue.Count > 0)
            UpdateOnce();
    }
    #endregion

    #region Collapse
    [Button]
    void CollapseRandom()
    {
        AssertNoPendingUpdates();

        // todo: optimize
        int best = int.MaxValue;
        List<HexPosition> candidates = new();
        for (int i=0;i<size;i++)
            for (int j=0;j<size;j++)
            {
                HexPosition pos = new(i, j);
                if (!InBounds(pos))
                    continue;

                int superpositons = grid[pos.X, pos.Y].superpositions.Count;
                if (superpositons <= 1) continue;
                if (superpositons < best)
                {
                    best = superpositons;
                    candidates.Clear();
                    candidates.Add(pos);
                }
                else if (superpositons == best)
                    candidates.Add(pos);
            }

        if (candidates.Count == 0)
        {
            // no tiles left to collapse
            autoCollapse = false;
            return;
        }

        var p = candidates.Count == 0 ? candidates[0] : candidates[rand.Next(candidates.Count)];
        var slot = grid[p.X, p.Y];
        //todo: very very very very very bad
        var tile = slot.superpositions.Count == 0 ? slot.superpositions.First() : slot.superpositions.ElementAt(rand.Next(slot.superpositions.Count));

        // todo: some sort of weighted tile picking
        var change = new RemoveSuperpositionsChange();
        RegisterChange(change);

        slot.Collapse(tile ,change);
    }
    #endregion

    #region Changes
    public void RegisterChange(IChange op) => changesStack.Push(op);

    [Button("Undo")]
    public void UndoChange()
    {
        AssertNoPendingUpdates();
        if (changesStack.Count > 0)
            changesStack.Pop().Undo();
    }

    //[Button]
    public void RefreshAll()
    {
        AssertNoPendingUpdates();
        
        // todo: what was this supposed to do?
        throw new NotImplementedException();
    }
    #endregion

    #region Debug
    [HorizontalLine, Header("Debug")]
    [SerializeField, ProgressBar("Collapsed", "maxTiles", EColor.Green)]
    int collapsedTiles;
    int maxTiles = 0;

    [ReadOnly, SerializeField]
    int pendingUpdates;
    
    public bool visualizeSlotCaches = true;

    private void Start()
    {
        int n = size / 2;
        maxTiles = 1 + 3 * n * (n + 1);
    }

    private void Update()
    {
        pendingUpdates = updateQueue.Count;

        collapsedTiles = 0;
        for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
                if (grid[i, j] != null && grid[i,j].collapsed)
                    collapsedTiles++;
    }

    [Button]
    void RestartScene() =>
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single)
        .completed += (obj) =>
        Selection.activeObject = FindObjectOfType<GridManager>();

    private void OnDrawGizmos()
    {
        if (updateQueue == null) return;
        var edgeOffsets = HexPosition.GetEdgeCenterOffsets().ToList();

        foreach (var tuple in updateQueue.Values)
        {
            Gizmos.color = new Color(0,1,0,0.1f);
            Gizmos.DrawSphere(tuple.slot.transform.position, 1.2f);
            Gizmos.color = Color.green;

            Vector3 from = tuple.other.transform.position + new Vector3(0,2,0);
            Vector3 to = tuple.slot.transform.position + new Vector3(0, 2, 0);

            Gizmos.DrawLine(from, to);

            float t = Time.timeSinceLevelLoad % 1;
            Vector3 spherePos = new Vector3(
                Mathf.SmoothStep(from.x,to.x,t),
                Mathf.SmoothStep(from.y,to.y,t),
                Mathf.SmoothStep(from.z,to.z,t));
            Gizmos.DrawSphere(spherePos, 0.1f);
        }
    }
    #endregion
}