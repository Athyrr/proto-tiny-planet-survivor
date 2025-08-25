using System.Collections.Generic;
using System;
using UnityEngine;
using NUnit.Framework.Constraints;

public class TickManager : MonoBehaviour
{
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

    [Header("Player ref")]

    [SerializeField]
    private Transform _player = null;

    [Header("Priority Distance Settings")]

    [SerializeField]
    private float _highPriorityDistance = 10f;

    [SerializeField]
    private float _mediumPriorityDistance = 30f;

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

    [Header("Debug Info")]

    [SerializeField]
    private bool _drawDebug = false;

    [SerializeField]
    private bool _showPerformanceStats = false;

    // Update groups
    private List<ITickable> _highPriorityList = new List<ITickable>();
    private List<ITickable> _mediumPriorityList = new List<ITickable>();
    private List<ITickable> _lowPriorityList = new List<ITickable>();
    private List<ITickable> _allTickables = new List<ITickable>();

    // Settings mapping for groups
    private Dictionary<ETickPriority, TickSettings> _settingsMap;
    private Dictionary<ETickPriority, List<ITickable>> _priorityLists;

    // Frame counters 
    private int _highFrameCounter = 0;
    private int _mediumFrameCounter = 0;
    private int _lowFrameCounter = 0;

    // Groups sorting
    private float _lastSortTime = 0f;
    private float _highDistanceSqr;
    private float _mediumDistanceSqr;

    // Performance tracking
    private PerformanceStats _performanceStats = new PerformanceStats();

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

    private void OnGUI()
    {
        if (_showPerformanceStats)
            DrawPerformanceStats();
    }

    #endregion

    #region Initialization

    private void InitManager()
    {
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
        _priorityLists = new Dictionary<ETickPriority, List<ITickable>>
        {
            { ETickPriority.High, _highPriorityList },
            { ETickPriority.Medium, _mediumPriorityList },
            { ETickPriority.Low, _lowPriorityList }
        };

        // Clear all lists
        _allTickables.Clear();
        _highPriorityList.Clear();
        _mediumPriorityList.Clear();
        _lowPriorityList.Clear();
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

    #endregion

    #region Public API

    public void Register(ITickable tickable)
    {
        if (tickable == null)
            return;

        if (!_allTickables.Contains(tickable))
        {
            _allTickables.Add(tickable);

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

        // Remove from all lists
        _allTickables.Remove(tickable);
        _highPriorityList.Remove(tickable);
        _mediumPriorityList.Remove(tickable);
        _lowPriorityList.Remove(tickable);

        if (_drawDebug)
        {
            Debug.Log($"Unregistered tickable: {tickable}. Total count: {_allTickables.Count}");
        }
    }

    public void GetGroupCounts(out int high, out int medium, out int low)
    {
        high = _highPriorityList.Count;
        medium = _mediumPriorityList.Count;
        low = _lowPriorityList.Count;
    }

    public int GetTotalTickablesCount() => _allTickables.Count;

    public PerformanceStats GetPerformanceStats() => _performanceStats;


    #endregion

    #region Private API

    private void TickHighPriorityGroup()
    {
        _highFrameCounter++;
        if (_highFrameCounter >= _highPrioritySettings.FrameInterval)
        {
            _highFrameCounter = 0;
            TickGroup(ETickPriority.High);
        }
    }

    private void TickMediumPriorityGroup()
    {
        _mediumFrameCounter++;
        if (_mediumFrameCounter >= _mediumPrioritySettings.FrameInterval)
        {
            _mediumFrameCounter = 0;
            TickGroup(ETickPriority.Medium);
        }
    }

    private void TickLowPriorityGroup()
    {
        _lowFrameCounter++;
        if (_lowFrameCounter >= _lowPrioritySettings.FrameInterval)
        {
            _lowFrameCounter = 0;
            TickGroup(ETickPriority.Low);
        }
    }

    private void TickGroup(ETickPriority priority)
    {
        var tickables = _priorityLists[priority];
        var settings = _settingsMap[priority];


        if (tickables.Count == 0) return;

        float startTime = Time.realtimeSinceStartup * 1000f;
        float deltaTime = Time.deltaTime * settings.FrameInterval; // Adjust delta time based on frame interval
        int processed = 0;
        int maxObjects = settings.MaxObjectsPerFrame == -1 ? tickables.Count : settings.MaxObjectsPerFrame;

        // Update objects with time budget management
        for (int i = 0; i < tickables.Count && processed < maxObjects; i++)
        {
            var tickable = tickables[i];
            if (tickable == null || !tickable.IsActive)
                continue;

            tickable.Tick(deltaTime);
            //tickable.Tick(Time.deltaTime);
            processed++;

            // Check time budget every 10 iterations for performance
            if (processed > 0 && processed % 10 == 0)
            {
                float elapsed = (Time.realtimeSinceStartup * 1000f) - startTime;
                if (elapsed > settings.TimeBudgetMs)
                {
                    if (_drawDebug)
                    {
                        Debug.Log($"{priority} group: Time budget exceeded ({elapsed:F1}ms), processed {processed}/{tickables.Count}");
                    }
                    break;
                }
            }
        }

        if (_drawDebug)
        {
            // Record performance stats
            float totalTime = (Time.realtimeSinceStartup * 1000f) - startTime;
            _performanceStats.RecordGroupUpdate(priority, totalTime, processed, tickables.Count);

            if (processed > 0)
            {
                Debug.Log($"Updated {priority} group: {processed}/{tickables.Count} objects in {totalTime:F1}ms");
            }
        }
    }

    #endregion

    #region Distance Sorting

    private void SortUpdateGroupsByDistance()
    {
        if (_player == null)
            return;

        float startTime = Time.realtimeSinceStartup * 1000f;

        // Clear groups
        _highPriorityList.Clear();
        _mediumPriorityList.Clear();
        _lowPriorityList.Clear();

        Vector3 playerPosition = _player.position;

        // Sort all updatables
        for (int i = 0; i < _allTickables.Count; i++)
        {
            var updatable = _allTickables[i];
            if (updatable == null || !updatable.IsActive)
                continue;

            var monoBehaviour = updatable as MonoBehaviour;
            if (monoBehaviour == null)
                continue;

            float sqrDistance = (monoBehaviour.transform.position - playerPosition).sqrMagnitude;

            // Assign to appropriate priority group
            if (sqrDistance < _highDistanceSqr)
            {
                _highPriorityList.Add(updatable);
            }
            else if (sqrDistance < _mediumDistanceSqr)
            {
                _mediumPriorityList.Add(updatable);
            }
            else
            {
                _lowPriorityList.Add(updatable);
            }
        }


        if (_drawDebug)
        {
            float sortTime = (Time.realtimeSinceStartup * 1000f) - startTime;
            _performanceStats.RecordSortTime(sortTime);

            Debug.Log($"Groups sorted in {sortTime:F1}ms - High: {_highPriorityList.Count}, " +
                     $"Medium: {_mediumPriorityList.Count}, Low: {_lowPriorityList.Count}");
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

        var stats = _performanceStats;
        string statsText = $"UpdateManager Performance:\n" +
                          $"Frame Time: {Time.unscaledDeltaTime * 1000f:F1}ms\n" +
                          $"Total Objects: {_allTickables.Count}\n" +
                          $"High: {_highPriorityList.Count} | Med: {_mediumPriorityList.Count} | Low: {_lowPriorityList.Count}\n" +
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
        foreach (float t in sortTimes) sum += t;
        averageSortTime = sortTimes.Count > 0 ? sum / sortTimes.Count : 0f;
    }

    public void RecordGroupUpdate(ETickPriority priority, float time, int processed, int total)
    {
        switch (priority)
        {
            case ETickPriority.High:
                highGroupStats.RecordSample(time, processed, total);
                break;
            case ETickPriority.Medium:
                mediumGroupStats.RecordSample(time, processed, total);
                break;
            case ETickPriority.Low:
                lowGroupStats.RecordSample(time, processed, total);
                break;
        }
    }

    public void TrackFrame()
    {
    }
}

#endregion