using System.Collections.Generic;
using UnityEngine;

public class PullingManager : MonoBehaviour
{
    [System.Serializable]
    public class Preload
    {
        public GameObject prefab;
        public int count = 20;
    }

    private class Bucket
    {
        public Transform root;
        public readonly Stack<GameObject> idle = new Stack<GameObject>();
        public readonly HashSet<GameObject> active = new HashSet<GameObject>();
    }

    private struct Entry
    {
        public Bucket bucket;
        public IPoolable[] hooks;
    }

    private static PullingManager instance;

    public static PullingManager Instance
    {
        get
        {
            if (instance == null) instance = FindFirstObjectByType<PullingManager>();
            if (instance == null) instance = new GameObject("PullingManager").AddComponent<PullingManager>();
            return instance;
        }
    }

    [Tooltip("씬 시작 시 미리 만들어 둘 프리팹과 개수")]
    [SerializeField] private Preload[] preloads;

    private readonly Dictionary<GameObject, Bucket> buckets = new Dictionary<GameObject, Bucket>();
    private readonly Dictionary<GameObject, Entry> entries = new Dictionary<GameObject, Entry>();
    private readonly List<GameObject> buffer = new List<GameObject>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        if (preloads == null) return;
        foreach (var p in preloads)
        {
            if (p != null) Prewarm(p.prefab, p.count);
        }
    }

    public void Prewarm(GameObject prefab, int count)
    {
        if (prefab == null) return;
        var bucket = GetBucket(prefab);
        while (bucket.idle.Count < count) bucket.idle.Push(Create(prefab, bucket));
    }

    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;
        var bucket = GetBucket(prefab);

        GameObject go = null;
        while (bucket.idle.Count > 0 && go == null) go = bucket.idle.Pop();
        if (go == null) go = Create(prefab, bucket);

        go.transform.SetPositionAndRotation(position, rotation);
        bucket.active.Add(go);
        go.SetActive(true);

        var hooks = entries[go].hooks;
        for (int i = 0; i < hooks.Length; i++) hooks[i].OnSpawned();
        return go;
    }

    public T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
    {
        if (prefab == null) return null;
        var go = Spawn(prefab.gameObject, position, rotation);
        return go != null ? go.GetComponent<T>() : null;
    }

    public void Despawn(GameObject go)
    {
        if (go == null) return;

        if (!entries.TryGetValue(go, out var entry))
        {
            Destroy(go);
            return;
        }

        if (!entry.bucket.active.Remove(go)) return;

        for (int i = 0; i < entry.hooks.Length; i++) entry.hooks[i].OnDespawned();
        go.SetActive(false);
        entry.bucket.idle.Push(go);
    }

    public static void Release(GameObject go)
    {
        if (go == null) return;
        if (instance != null) instance.Despawn(go);
        else Destroy(go);
    }

    public void DespawnAll(GameObject prefab)
    {
        if (prefab == null || !buckets.TryGetValue(prefab, out var bucket)) return;
        buffer.Clear();
        buffer.AddRange(bucket.active);
        for (int i = 0; i < buffer.Count; i++) Despawn(buffer[i]);
        buffer.Clear();
    }

    public int CountActive(GameObject prefab)
    {
        return prefab != null && buckets.TryGetValue(prefab, out var bucket) ? bucket.active.Count : 0;
    }

    private Bucket GetBucket(GameObject prefab)
    {
        if (buckets.TryGetValue(prefab, out var bucket)) return bucket;

        bucket = new Bucket { root = new GameObject(prefab.name + " Pool").transform };
        bucket.root.SetParent(transform, false);
        buckets.Add(prefab, bucket);
        return bucket;
    }

    private GameObject Create(GameObject prefab, Bucket bucket)
    {
        var go = Instantiate(prefab, bucket.root);
        go.name = prefab.name;
        go.SetActive(false);
        entries.Add(go, new Entry { bucket = bucket, hooks = go.GetComponentsInChildren<IPoolable>(true) });
        return go;
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }
}
