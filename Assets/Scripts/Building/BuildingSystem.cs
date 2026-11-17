using System.Collections.Generic;
using UnityEngine;
using IslandSurvival.Core;
using IslandSurvival.Inventory;
using IslandSurvival.Visual;

namespace IslandSurvival.Building
{
    public class BuildingSystem : MonoBehaviour
    {
        public static BuildingSystem Instance { get; private set; }

        public float placeRange = 6f;
        public LayerMask groundMask = ~0;
        public Material previewValidMat;
        public Material previewInvalidMat;

        string _activePiece;
        GameObject _preview;
        bool _valid;
        float _yaw;
        readonly List<GameObject> _built = new List<GameObject>();
        public IReadOnlyList<GameObject> Built => _built;

        static readonly Dictionary<string, string> PieceToItem = new Dictionary<string, string>
        {
            {"foundation","foundation"},{"wall","wall"},{"floor","floor"},
            {"roof","roof"},{"doorway","doorway"},{"storage_box","storage_box"},{"campfire","campfire"}
        };

        void Awake() => Instance = this;

        void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing) return;
            if (string.IsNullOrEmpty(_activePiece)) return;

            UpdatePreview();
            if (Input.GetKeyDown(KeyCode.R)) _yaw += 45f;
            if (Input.GetMouseButtonDown(0) && _valid) ConfirmPlace();
            if (Input.GetKeyDown(KeyCode.Q) || Input.GetMouseButtonDown(1)) CancelBuild();
        }

        public void StartBuild(string pieceId)
        {
            CancelBuild();
            if (!PieceToItem.ContainsKey(pieceId)) return;
            var inv = InventorySystem.Instance;
            if (inv == null || !inv.HasItems(PieceToItem[pieceId], 1))
            {
                GameEvents.Notify("Missing materials");
                return;
            }
            _activePiece = pieceId;
            _preview = CreatePieceMesh(pieceId, true);
            GameEvents.Notify("Building: " + pieceId + " | LMB place, R rotate, Q cancel");
        }

        void UpdatePreview()
        {
            if (_preview == null) return;
            var cam = Camera.main;
            if (cam == null) return;
            Ray ray = new Ray(cam.transform.position, cam.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, placeRange, groundMask, QueryTriggerInteraction.Ignore))
            {
                Vector3 pos = hit.point;
                pos.y += 0.02f;
                _preview.transform.position = pos;
                _preview.transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
                _valid = Vector3.Angle(hit.normal, Vector3.up) < 35f && !Collides(_preview);
            }
            else _valid = false;

            foreach (var r in _preview.GetComponentsInChildren<Renderer>())
                r.material.color = _valid ? new Color(0.2f, 0.9f, 0.3f, 0.45f) : new Color(0.9f, 0.2f, 0.2f, 0.45f);
        }

        bool Collides(GameObject go)
        {
            var cols = go.GetComponentsInChildren<Collider>();
            foreach (var c in cols)
            {
                c.isTrigger = true;
                var hits = Physics.OverlapBox(c.bounds.center, c.bounds.extents * 0.9f, go.transform.rotation);
                foreach (var h in hits)
                {
                    if (h.transform.IsChildOf(go.transform)) continue;
                    if (h.GetComponentInParent<BuildingMarker>() != null) return true;
                }
            }
            return false;
        }

        void ConfirmPlace()
        {
            if (string.IsNullOrEmpty(_activePiece)) return;
            string item = PieceToItem[_activePiece];
            if (!InventorySystem.Instance.Consume(item, 1)) return;

            var built = CreatePieceMesh(_activePiece, false);
            built.transform.SetPositionAndRotation(_preview.transform.position, _preview.transform.rotation);
            built.AddComponent<BuildingMarker>().pieceId = _activePiece;
            _built.Add(built);
            GameEvents.Notify("Built " + _activePiece);
            // keep building mode for multi-place if more items remain
            if (!InventorySystem.Instance.HasItems(item, 1)) CancelBuild();
            else
            {
                Destroy(_preview);
                _preview = CreatePieceMesh(_activePiece, true);
            }
        }

        public void CancelBuild()
        {
            _activePiece = null;
            if (_preview) Destroy(_preview);
            _preview = null;
        }

        public GameObject CreatePieceMesh(string id, bool preview)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = (preview ? "Preview_" : "Build_") + id;
            Vector3 scale = id switch
            {
                "foundation" => new Vector3(2.5f, 0.2f, 2.5f),
                "floor" => new Vector3(2.5f, 0.12f, 2.5f),
                "wall" => new Vector3(2.5f, 2.2f, 0.18f),
                "doorway" => new Vector3(2.5f, 2.2f, 0.18f),
                "roof" => new Vector3(2.8f, 0.15f, 2.8f),
                "storage_box" => new Vector3(1.0f, 0.8f, 0.8f),
                "campfire" => new Vector3(0.9f, 0.35f, 0.9f),
                _ => Vector3.one
            };
            go.transform.localScale = scale;
            var r = go.GetComponent<Renderer>();
            VisualAssetFactory.Ensure();
            if (id == "campfire")
                r.material = VisualAssetFactory.MakeLit(new Color(0.35f, 0.2f, 0.1f), VisualAssetFactory.Wood, 0.15f);
            else
                r.material = VisualAssetFactory.MakeLit(new Color(0.48f, 0.3f, 0.14f), VisualAssetFactory.Wood, 0.28f);
            if (preview)
            {
                SetTransparent(r.material);
                foreach (var c in go.GetComponentsInChildren<Collider>()) c.isTrigger = true;
            }
            if (id == "campfire" && !preview)
            {
                var light = new GameObject("FireLight").AddComponent<Light>();
                light.type = LightType.Point;
                light.color = new Color(1f, 0.55f, 0.2f);
                light.range = 8f;
                light.intensity = 2.2f;
                light.transform.SetParent(go.transform);
                light.transform.localPosition = Vector3.up * 0.5f;
            }
            return go;
        }

        void SetTransparent(Material m)
        {
            if (m.HasProperty("_Surface")) m.SetFloat("_Surface", 1);
            m.color = new Color(m.color.r, m.color.g, m.color.b, 0.4f);
        }

        public List<BuiltSaveData> Export()
        {
            var list = new List<BuiltSaveData>();
            foreach (var b in _built)
            {
                if (!b) continue;
                var m = b.GetComponent<BuildingMarker>();
                list.Add(new BuiltSaveData
                {
                    pieceId = m ? m.pieceId : "foundation",
                    x = b.transform.position.x, y = b.transform.position.y, z = b.transform.position.z,
                    yaw = b.transform.eulerAngles.y
                });
            }
            return list;
        }

        public void Import(List<BuiltSaveData> data)
        {
            foreach (var b in _built) if (b) Destroy(b);
            _built.Clear();
            if (data == null) return;
            foreach (var d in data)
            {
                var go = CreatePieceMesh(d.pieceId, false);
                go.transform.position = new Vector3(d.x, d.y, d.z);
                go.transform.rotation = Quaternion.Euler(0f, d.yaw, 0f);
                go.AddComponent<BuildingMarker>().pieceId = d.pieceId;
                _built.Add(go);
            }
        }
    }

    public class BuildingMarker : MonoBehaviour { public string pieceId; }

    [System.Serializable]
    public class BuiltSaveData
    {
        public string pieceId;
        public float x, y, z, yaw;
    }
}
