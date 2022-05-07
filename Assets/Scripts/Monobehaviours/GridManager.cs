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
    public System.Random rand;

    RandomQueue<UpdateInfo> updateQueue;
    bool pendingUpdates => updateQueue.Count > 0;

    // todo: simplify the changes system
    Stack<IChange> changesStack = new();

    bool inAuto; // prevents needless recursion

    #region Init
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
    #endregion

    #region Misc
    public bool InBounds(HexPosition hexPos)
    {
        if (!(0 <= hexPos.X && hexPos.X < size && 0 <= hexPos.Y && hexPos.Y < size))
            return false;
        return middle.Distance(hexPos) <= size / 2;
    }
    #endregion

    #region Auto
    void FixedUpdate()
    {
        _autoUpdateTime -= Time.deltaTime;
        _autoCollapseTime -= Time.deltaTime;
        Auto();
    }

    // handles auto updating and auto collapsing
    // processing is done in a loop instead of recursively to prevent stack overflows when generating large grids
    void Auto()
    {
        if (inAuto) return;
        inAuto = true;
        
        while(true)
        {
            if (pendingUpdates && autoUpdate)
            {
                // instant updating
                if (autoUpdateTime == 0)
                    UpdateAll();

                // update just once
                else if (_autoUpdateTime <= 0)
                {
                    _autoUpdateTime = autoUpdateTime;
                    UpdateOnce();
                }
            }

            // only collapse if there are no pending updates
            if (!pendingUpdates && autoCollapse)
            {
                // collapse and loop again
                if (autoCollapseTime == 0)
                {
                    CollapseRandom();
                    continue;
                }

                // collapse just once
                else if (_autoCollapseTime <= 0)
                {
                    _autoCollapseTime = autoCollapseTime;
                    CollapseRandom();
                }
            }

            // wait for next frame
            break;
        }

        inAuto = false;
    }
    #endregion

    #region Updates
    public void AssertNoPendingUpdates()
    {
        if (updateQueue.Count > 0 && throwOnInvalidStates)
            throw new InvalidOperationException();
    }

    public void RegisterUpdate(UpdateInfo info)
    {
        updateQueue.Push(info);

        Auto();
    }

    public void RegisterUpdates(IEnumerable<UpdateInfo> infos)
    {
        foreach (var info in infos)
            updateQueue.Push(info);

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

                int entropy = grid[pos.X, pos.Y].entropy;
                if (entropy <= 1) continue;
                if (entropy < best)
                {
                    best = entropy;
                    candidates.Clear();
                    candidates.Add(pos);
                }
                else if (entropy == best)
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
        var change = new RemoveSuperpositionsChange();
        RegisterChange(change);

        slot.CollapseRandom(change);
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
#if UNITY_EDITOR
    [HorizontalLine, Header("Debug")]
    [SerializeField, ProgressBar("Collapsed", "maxTiles", EColor.Green)]
    int collapsedTiles;
    int maxTiles = 0;

    [ReadOnly, SerializeField]
    int pendingUpdatesNr;
    
    public bool visualizeSlotCaches = true;

    private void Start()
    {
        int n = size / 2;
        maxTiles = 1 + 3 * n * (n + 1);
    }

    private void Update()
    {
        pendingUpdatesNr = updateQueue.Count;

        collapsedTiles = 0;
        for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
                if (grid[i, j] != null && grid[i,j].isCollapsed)
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
#endif
    #endregion
}
