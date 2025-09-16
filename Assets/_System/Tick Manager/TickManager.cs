using System.Collections.Generic;
using System;
using UnityEngine;

public class TickManager : MonoBehaviour
{
    #region Sub-classes

    public enum ETickPriority
    {
        High,
        Medium,
        Low,
        Disable
    }

    [Serializable]
    public struct TickSettings
    {
        [Header("Tick Frequency")]
        [Tooltip("Update every X frames (1 = every frame, 3 = every 3rd frame)")]
        [Range(1, 10)]
        public int FrameInterval;

        [Header("Performance Budget")]
        [Tooltip("Maximum time budget per update cycle (milliseconds)")]
        [Range(1f, 20f)]
        public float TimeBudgetMs;

        [Header("Object Limits")]
        [Tooltip("Maximum objects to process per frame (-1 = unlimited)")]
        [Range(-1, 500)]
        public int MaxObjectsPerFrame;

        public TickSettings(int frames, float budget, int maxObjects = -1)
        {
            FrameInterval = frames;
            TimeBudgetMs = budget;
            MaxObjectsPerFrame = maxObjects;
        }

        public bool IsValid()
        {
            return FrameInterval > 0 && TimeBudgetMs > 0;
        }
    }

    #endregion


    #region Fields

    [Header("Refs")]

    [SerializeField]
    private Transform _player = null;

    [SerializeField]
    private Camera _camera = null;

    [Header("Priority Distance Settings")]

    [SerializeField]
    private float _highPriorityDistance = 10f;

    [SerializeField]
    private float _mediumPriorityDistance = 30f;

    [Space]

    [SerializeField]
    private bool _useCullingDistance = false;

    [SerializeField]
    private float _cullingDistance = 60f;

    [SerializeField]
    [Tooltip("Sort tickables groups each X seconds")]
    private float _sortInterval = 1f;

    [Header("Tick Groups Settings")]

    [SerializeField]
    private TickSettings _highPrioritySettings = new(1, 8f, 100);

    [SerializeField]
    private TickSettings _mediumPrioritySettings = new(3, 5f, 50);

    [SerializeField]
    private TickSettings _lowPrioritySettings = new(5, 3f, 25);

    [Header("Debug")]

    [SerializeField]
    private bool _drawDebug = false;

    [SerializeField]
    private bool _showPerformanceStats = false;

    /// <summary>
    /// All registered tickables
    /// </summary>
    private HashSet<ITickable> _allTickables = new HashSet<ITickable>();

    /// <summary>
    /// Collection mapping priority levels to their respective tickables group.
    /// </summary>
    private Dictionary<ETickPriority, List<ITickable>> _priorityGroups;

    /// <summary>
    /// Collection cache that maps <see cref="ITickable"></see> instances to their current <see cref="ETickPriority"/>.
    /// </summary>
    private Dictionary<ITickable, ETickPriority> _tickablesPriorityCache = new Dictionary<ITickable, ETickPriority>();

    /// <summary>
    /// Mapping of priority levels to their corresponding tick settings.
    /// </summary>
    private Dictionary<ETickPriority, TickSettings> _settingsMap;

    /// <summary>
    /// The duration betwenn the last frame for each group.
    /// </summary>
    private Dictionary<ETickPriority, float> _lastTickTime = new Dictionary<ETickPriority, float>();


    // Frame counters 
    private int _highPriorityFrameCounter = 0;
    private int _mediumPriorityFrameCounter = 0;
    private int _lowPriorityFrameCounter = 0;

    // Groups sorting
    private float _lastSortTime = 0f;
    private float _highDistanceSqr;
    private float _mediumDistanceSqr;

    // Frame Staggering
    private Dictionary<ETickPriority, int> _lastIndexTicked = new Dictionary<ETickPriority, int>();

    // Culling
    private float _cullingDistanceSqr;

    // Performance tracking
    private PerformanceStats _performanceStats = new PerformanceStats();

    #endregion


    #region Lifecycle

    private void Awake()
    {
        InitManager();
        ValidateSettings();
    }

    private void Start()
    {
        if (_player == null)
        {
            Debug.LogError("Player Transform not assigned! UpdateManager requires a player reference.", this);
            enabled = false;
            return;
        }

        if (_drawDebug)
        {
            Debug.Log("UpdateManager initialized successfully.");
            LogCurrentSettings();
        }
    }

    private void Update()
    {
        // Sort groups each X seconds
        if (Time.time - _lastSortTime > _sortInterval)
        {
            SortUpdateGroupsByDistance();
            _lastSortTime = Time.time;
        }

        // Update groups based on their frame intervals
        TickHighPriorityGroup();
        TickMediumPriorityGroup();
        TickLowPriorityGroup();

        // Performance tracking
        if (_showPerformanceStats)
        {
            _performanceStats.TrackFrame();
        }
    }

#if UNITY_EDITOR
    private void OnGUI()
    {
        if (_showPerformanceStats)
            DrawPerformanceStats();
    }
#endif

    #endregion


    #region Public API

    public int GetTotalTickablesCount() => _allTickables.Count;

    public PerformanceStats GetPerformanceStats() => _performanceStats;

    public void Register(ITickable tickable)
    {
        if (tickable == null)
            return;

        if (_allTickables.Add(tickable))
        {
            var prio = CalculatePriority(tickable);

            if (prio != ETickPriority.Disable)
            {
                _priorityGroups[prio].Add(tickable);
                _tickablesPriorityCache[tickable] = prio;
            }

            if (_drawDebug)
            {
                Debug.Log($"Registered updatable: {tickable}. Total count: {_allTickables.Count}");
            }
        }
    }

    public void Unregister(ITickable tickable)
    {
        if (tickable == null)
            return;

        if (_allTickables.Remove(tickable))
        {
            if (_tickablesPriorityCache.TryGetValue(tickable, out var prio))
            {
                _priorityGroups[prio].Remove(tickable);
                _tickablesPriorityCache.Remove(tickable);
            }
        }

        if (_drawDebug)
        {
            Debug.Log($"Unregistered tickable: {tickable}. Total count: {_allTickables.Count}");
        }
    }

    public void GetGroupCounts(out int high, out int medium, out int low)
    {
        high = _priorityGroups[ETickPriority.High].Count;
        medium = _priorityGroups[ETickPriority.Medium].Count;
        low = _priorityGroups[ETickPriority.Low].Count;
    }

    #endregion


    #region Private API

    private void InitManager()
    {
        if (_camera == null)
            _camera = Camera.main;

        // Init culling distance
        _cullingDistanceSqr = _cullingDistance * _cullingDistance;

        // Init squared distances
        _highDistanceSqr = _highPriorityDistance * _highPriorityDistance;
        _mediumDistanceSqr = _mediumPriorityDistance * _mediumPriorityDistance;

        // Init settings mapping
        _settingsMap = new Dictionary<ETickPriority, TickSettings>
        {
            { ETickPriority.High, _highPrioritySettings },
            { ETickPriority.Medium, _mediumPrioritySettings },
            { ETickPriority.Low, _lowPrioritySettings }
        };

        // Init priority lists mapping
        _priorityGroups = new Dictionary<ETickPriority, List<ITickable>>
        {
            { ETickPriority.High, new List<ITickable>() },
            { ETickPriority.Medium, new List<ITickable>()},
            { ETickPriority.Low, new List<ITickable>() }
        };

        _lastIndexTicked = new Dictionary<ETickPriority, int>
        {
            { ETickPriority.High, 0 },
            { ETickPriority.Medium, 0 },
            { ETickPriority.Low, 0 }
        };

        _lastTickTime = new Dictionary<ETickPriority, float>
        {
            { ETickPriority.High, 0 },
            { ETickPriority.Medium, 0 },
            { ETickPriority.Low, 0 }
        };

    }

    private void ValidateSettings()
    {
        if (!_highPrioritySettings.IsValid())
        {
            Debug.LogWarning("High Priority Settings are invalid! Using default values.");
            _highPrioritySettings = new TickSettings(1, 8f, 100);
        }

        if (!_mediumPrioritySettings.IsValid())
        {
            Debug.LogWarning("Medium Priority Settings are invalid! Using default values.");
            _mediumPrioritySettings = new TickSettings(3, 5f, 50);
        }

        if (!_lowPrioritySettings.IsValid())
        {
            Debug.LogWarning("Low Priority Settings are invalid! Using default values.");
            _lowPrioritySettings = new TickSettings(5, 3f, 25);
        }
    }

    private void SortUpdateGroupsByDistance()
    {
        if (_player == null)
            return;

        float startTime = Time.realtimeSinceStartup * 1000f;
        Vector3 playerPosition = _player.position;

        // Sort all updatables
        foreach (var tickable in _allTickables)
        {
            if (tickable == null || !tickable.IsActive)
                continue;

            ReorderTickablePriority(tickable);
        }

        if (_drawDebug)
        {
            float sortTime = (Time.realtimeSinceStartup * 1000f) - startTime;
            _performanceStats.RecordSortTime(sortTime);

            // Log group counts
            var highPriorityList = _priorityGroups[ETickPriority.High];
            var mediumPriorityList = _priorityGroups[ETickPriority.Medium];
            var lowPriorityList = _priorityGroups[ETickPriority.Low];

            Debug.Log($"Groups sorted in {sortTime:F1}ms - High: {highPriorityList.Count}, " +
                     $"Medium: {mediumPriorityList.Count}, Low: {lowPriorityList.Count}");
        }
    }

    /// <summary>
    /// Calculate the priority level for a given <see cref="ITickable"/> based on its distance to the player.
    /// </summary>
    /// <param name="tickable"></param>
    /// <returns>Returns the corresponded <see cref="ETickPriority"/></returns>
    private ETickPriority CalculatePriority(ITickable tickable)
    {
        if (_player == null || tickable == null)
            return ETickPriority.Disable;

        float sqrDistance = (tickable.Position - _player.position).sqrMagnitude;

        if (_useCullingDistance && sqrDistance > _cullingDistanceSqr)
            return ETickPriority.Disable;

        if (sqrDistance < _highDistanceSqr)
            return ETickPriority.High;

        else if (sqrDistance < _mediumDistanceSqr)
            return ETickPriority.Medium;

        else
            return ETickPriority.Low;
    }

    /// <summary>
    /// Reorder the priority of a tickable and move it between groups if necessary.
    /// </summary>
    /// <param name="tickable"></param>
    private void ReorderTickablePriority(ITickable tickable)
    {
        var newPriority = CalculatePriority(tickable);

        if (_tickablesPriorityCache.TryGetValue(tickable, out var currentPriority)) // Existing tickable
        {
            if (currentPriority == newPriority)
                return;

            // Swap tickables between groups
            if (currentPriority != ETickPriority.Disable)
                _priorityGroups[currentPriority].Remove(tickable);

            if (newPriority != ETickPriority.Disable)
                _priorityGroups[newPriority].Add(tickable);

            // Update priority mapping
            _tickablesPriorityCache[tickable] = newPriority;
        }
        else // New tickable to add to the appropriate group
        {
            if (newPriority != ETickPriority.Disable)
            {
                _priorityGroups[newPriority].Add(tickable);
                _tickablesPriorityCache[tickable] = newPriority;
            }
        }

    }

    /// <summary>
    /// Tick the high priority group based on its frame interval setting.
    /// </summary>
    private void TickHighPriorityGroup()
    {
        _highPriorityFrameCounter++;
        if (_highPriorityFrameCounter >= _highPrioritySettings.FrameInterval)
        {
            _highPriorityFrameCounter = 0;
            TickGroup(ETickPriority.High);
        }
    }

    /// <summary>
    /// Tick the medium priority group based on its frame interval setting.
    /// </summary>
    private void TickMediumPriorityGroup()
    {
        _mediumPriorityFrameCounter++;
        if (_mediumPriorityFrameCounter >= _mediumPrioritySettings.FrameInterval)
        {
            _mediumPriorityFrameCounter = 0;
            TickGroup(ETickPriority.Medium);
        }
    }

    /// <summary>
    /// Tick the low priority group based on its frame interval setting.
    /// </summary>
    private void TickLowPriorityGroup()
    {
        _lowPriorityFrameCounter++;
        if (_lowPriorityFrameCounter >= _lowPrioritySettings.FrameInterval)
        {
            _lowPriorityFrameCounter = 0;
            TickGroup(ETickPriority.Low);
        }
    }

    private void TickGroup(ETickPriority priority)
    {
        var tickables = _priorityGroups[priority];
        var settings = _settingsMap[priority];

        if (tickables.Count == 0)
            return;

        // Adjust delta time based on frame interval
        float deltaTime = Time.time - _lastTickTime[priority];
        //float deltaTime = _lastTickTime[priority] * settings.FrameInterval; 

        _lastTickTime[priority] = Time.time;

        float frameStartTime = Time.realtimeSinceStartup * 1000f;
        int maxObjects = settings.MaxObjectsPerFrame == -1 ? tickables.Count : settings.MaxObjectsPerFrame;
        int processed = 0;

        // Retrieve the last object ticked.
        int startIndex = _lastIndexTicked[priority];

        // Update objects with time budget management
        for (int i = 0; i < tickables.Count && processed < maxObjects; i++)
        {
            // Ensure the index never exceed group count.
            int index = (startIndex + i) % tickables.Count;

            var tickable = tickables[index];

            if (tickable == null || !tickable.IsActive)
                continue;

            // Tick using calculated delta
            tickable.Tick(deltaTime);
            processed++;

            // Check time budget every 10 iterations for performance
            if (processed % 10 == 0)
            {
                float elapsed = (Time.realtimeSinceStartup * 1000f) - frameStartTime;
                if (elapsed > settings.TimeBudgetMs)
                {
                    if (_drawDebug)
                    {
                        Debug.Log($"{priority} group: Time budget exceeded ({elapsed:F1}ms), processed {processed}/{tickables.Count}");
                    }
                    break;
                }
            }

            if (processed > 0 && tickables.Count > 0)
            {
                _lastIndexTicked[priority] = (startIndex + processed) % tickables.Count;
            }
        }

        if (_drawDebug)
        {
            // Record performance stats
            float totalTime = (Time.realtimeSinceStartup * 1000f) - frameStartTime;
            _performanceStats.RecordGroupUpdate(priority, totalTime, processed, tickables.Count);

            if (processed > 0)
            {
                Debug.Log($"Updated {priority} group: {processed}/{tickables.Count} objects in {totalTime:F1}ms");
            }
        }
    }

    #endregion


    #region Debug

    private void OnDrawGizmosSelected()
    {
        if (_drawDebug && _player != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_player.position, _highPriorityDistance);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_player.position, _mediumPriorityDistance);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_player.position, _mediumPriorityDistance * 2f);
        }
    }

    private void LogCurrentSettings()
    {
        Debug.Log($"UpdateManager Settings:\n" +
                 $"High Priority: {_highPrioritySettings.FrameInterval} frames, {_highPrioritySettings.TimeBudgetMs}ms budget\n" +
                 $"Medium Priority: {_mediumPrioritySettings.FrameInterval} frames, {_mediumPrioritySettings.TimeBudgetMs}ms budget\n" +
                 $"Low Priority: {_lowPrioritySettings.FrameInterval} frames, {_lowPrioritySettings.TimeBudgetMs}ms budget");
    }

    private void DrawPerformanceStats()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 12;
        style.normal.textColor = Color.white;

        var lowPriorityList = _priorityGroups[ETickPriority.Low];
        var mediumPriorityList = _priorityGroups[ETickPriority.Medium];
        var highPriorityList = _priorityGroups[ETickPriority.High];

        var stats = _performanceStats;
        string statsText = $"Tick perfs:\n" +
                          $"Frame Time: {Time.unscaledDeltaTime * 1000f:F1}ms\n" +
                          $"Total Objects: {_allTickables.Count}\n" +
                          $"High: {highPriorityList.Count} | Med: {mediumPriorityList.Count} | Low: {lowPriorityList.Count}\n" +
                          $"Avg Sort Time: {stats.averageSortTime:F1}ms\n" +
                          $"High Avg: {stats.highGroupStats.averageTime:F1}ms\n" +
                          $"Med Avg: {stats.mediumGroupStats.averageTime:F1}ms\n" +
                          $"Low Avg: {stats.lowGroupStats.averageTime:F1}ms";

        GUI.Label(new Rect(10, 10, 300, 200), statsText, style);
    }

    #endregion
}

#region Performance Tracking

[Serializable]
public struct GroupStats
{
    public float averageTime;
    public float averageProcessed;
    public float averageTotal;

    private Queue<float> times;
    private Queue<int> processedCounts;
    private Queue<int> totalCounts;
    private const int MAX_SAMPLES = 60;

    public void RecordSample(float time, int processed, int total)
    {
        if (times == null)
        {
            times = new Queue<float>();
            processedCounts = new Queue<int>();
            totalCounts = new Queue<int>();
        }

        times.Enqueue(time);
        processedCounts.Enqueue(processed);
        totalCounts.Enqueue(total);

        if (times.Count > MAX_SAMPLES)
        {
            times.Dequeue();
            processedCounts.Dequeue();
            totalCounts.Dequeue();
        }

        // Calculate averages
        float timeSum = 0f;
        int processedSum = 0;
        int totalSum = 0;

        foreach (float t in times) timeSum += t;
        foreach (int p in processedCounts) processedSum += p;
        foreach (int t in totalCounts) totalSum += t;

        int count = times.Count;
        averageTime = count > 0 ? timeSum / count : 0f;
        averageProcessed = count > 0 ? (float)processedSum / count : 0f;
        averageTotal = count > 0 ? (float)totalSum / count : 0f;
    }
}

[Serializable]
public struct PerformanceStats
{
    public float averageSortTime;
    public GroupStats highGroupStats;
    public GroupStats mediumGroupStats;
    public GroupStats lowGroupStats;

    private Queue<float> sortTimes;
    private const int MAX_SAMPLES = 60;

    public void RecordSortTime(float time)
    {
        if (sortTimes == null)
            sortTimes = new Queue<float>();

        sortTimes.Enqueue(time);
        if (sortTimes.Count > MAX_SAMPLES)
            sortTimes.Dequeue();

        float sum = 0f;
        foreach (float t in sortTimes)
            sum += t;

        averageSortTime = sortTimes.Count > 0 ? sum / sortTimes.Count : 0f;
    }

    public void RecordGroupUpdate(TickManager.ETickPriority priority, float time, int processed, int total)
    {
        switch (priority)
        {
            case TickManager.ETickPriority.High:
                highGroupStats.RecordSample(time, processed, total);
                break;
            case TickManager.ETickPriority.Medium:
                mediumGroupStats.RecordSample(time, processed, total);
                break;
            case TickManager.ETickPriority.Low:
                lowGroupStats.RecordSample(time, processed, total);
                break;
        }
    }

    public void TrackFrame()
    {
    }
}

#endregion