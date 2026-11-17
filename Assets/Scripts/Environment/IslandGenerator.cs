using UnityEngine;
using IslandSurvival.Resources;
using IslandSurvival.Visual;

namespace IslandSurvival.Environment
{
    public class IslandGenerator : MonoBehaviour
    {
        public int resolution = 160;
        public float size = 240f;
        public float height = 32f;
        public int seed = 42;

        public Transform PlayerSpawn { get; private set; }

        Material _matSand, _matGrass, _matRock, _matTrunk, _matLeaf, _matPalm, _matWater, _matWood;

        public void Generate()
        {
            Random.InitState(seed);
            VisualAssetFactory.Ensure();
            BuildMaterials();
            BuildTerrain();
            BuildOcean();
            BuildShoreFoam();
            ScatterNature();
            PlaceSpawn();
            BuildCave();
            BuildPond();
            BuildLandmarks();
        }

        void BuildMaterials()
        {
            _matSand = VisualAssetFactory.MakeLit(new Color(0.9f, 0.82f, 0.6f), VisualAssetFactory.Sand, 0.35f);
            _matGrass = VisualAssetFactory.MakeLit(new Color(0.35f, 0.55f, 0.22f), VisualAssetFactory.Grass, 0.2f);
            _matRock = VisualAssetFactory.MakeLit(new Color(0.5f, 0.48f, 0.46f), VisualAssetFactory.Rock, 0.15f);
            _matTrunk = VisualAssetFactory.MakeLit(new Color(0.4f, 0.25f, 0.12f), VisualAssetFactory.Wood, 0.2f);
            _matLeaf = VisualAssetFactory.MakeFoliage(new Color(0.22f, 0.55f, 0.16f));
            _matPalm = VisualAssetFactory.MakeFoliage(new Color(0.28f, 0.58f, 0.18f));
            _matWater = VisualAssetFactory.MakeWater();
            _matWood = VisualAssetFactory.MakeLit(new Color(0.45f, 0.28f, 0.12f), VisualAssetFactory.Wood, 0.25f);
        }

        float HeightAt(float x, float z)
        {
            float nx = x / size;
            float nz = z / size;
            float dist = Mathf.Sqrt(nx * nx + nz * nz) * 2f;
            float mask = Mathf.Clamp01(1.2f - dist);
            mask = mask * mask * (3f - 2f * mask);

            float h = 0f;
            h += Mathf.PerlinNoise(x * 0.008f + seed, z * 0.008f) * 0.5f;
            h += Mathf.PerlinNoise(x * 0.02f + seed * 1.7f, z * 0.02f) * 0.28f;
            h += Mathf.PerlinNoise(x * 0.05f + 40f, z * 0.05f) * 0.14f;
            h += Mathf.PerlinNoise(x * 0.12f + 90f, z * 0.12f) * 0.06f;
            // Ridged hills
            float ridge = 1f - Mathf.Abs(Mathf.PerlinNoise(x * 0.006f + 200f, z * 0.006f) * 2f - 1f);
            h += ridge * ridge * 0.3f;
            return h * height * mask;
        }

        public float SampleHeight(float x, float z) => HeightAt(x, z);

        void BuildTerrain()
        {
            var go = new GameObject("IslandTerrain");
            go.transform.SetParent(transform);
            var mf = go.AddComponent<MeshFilter>();
            var mr = go.AddComponent<MeshRenderer>();
            var mc = go.AddComponent<MeshCollider>();

            int res = resolution;
            int vcount = (res + 1) * (res + 1);
            var verts = new Vector3[vcount];
            var norms = new Vector3[vcount];
            var uvs = new Vector2[vcount];
            var cols = new Color[vcount];
            var tris = new int[res * res * 6];
            float half = size * 0.5f;

            for (int z = 0; z <= res; z++)
            {
                for (int x = 0; x <= res; x++)
                {
                    int i = z * (res + 1) + x;
                    float wx = -half + (x / (float)res) * size;
                    float wz = -half + (z / (float)res) * size;
                    float h = HeightAt(wx, wz);
                    verts[i] = new Vector3(wx, h, wz);
                    uvs[i] = new Vector2(wx * 0.08f, wz * 0.08f);

                    float dist = Mathf.Sqrt(wx * wx + wz * wz) / (size * 0.5f);
                    // Biome vertex colors drive visual variety under single material
                    if (h < 1.0f)
                        cols[i] = new Color(0.95f, 0.88f, 0.65f);
                    else if (h < 2.5f)
                        cols[i] = Color.Lerp(new Color(0.9f, 0.8f, 0.55f), new Color(0.45f, 0.6f, 0.25f), (h - 1f) / 1.5f);
                    else if (dist > 0.75f && h > 3f)
                        cols[i] = new Color(0.55f, 0.52f, 0.48f);
                    else
                    {
                        float n = Mathf.PerlinNoise(wx * 0.04f, wz * 0.04f);
                        cols[i] = Color.Lerp(new Color(0.25f, 0.5f, 0.18f), new Color(0.35f, 0.45f, 0.2f), n);
                    }
                }
            }

            int t = 0;
            for (int z = 0; z < res; z++)
            for (int x = 0; x < res; x++)
            {
                int i = z * (res + 1) + x;
                tris[t++] = i; tris[t++] = i + res + 1; tris[t++] = i + 1;
                tris[t++] = i + 1; tris[t++] = i + res + 1; tris[t++] = i + res + 2;
            }

            var mesh = new Mesh { name = "IslandMesh", indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
            mesh.vertices = verts;
            mesh.triangles = tris;
            mesh.uv = uvs;
            mesh.colors = cols;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mf.sharedMesh = mesh;
            mc.sharedMesh = mesh;
            // Blend: grass base with vertex color tint
            mr.sharedMaterial = _matGrass;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            mr.receiveShadows = true;
        }

        void BuildOcean()
        {
            var water = GameObject.CreatePrimitive(PrimitiveType.Plane);
            water.name = "Ocean";
            water.transform.SetParent(transform);
            water.transform.position = new Vector3(0f, 0.35f, 0f);
            water.transform.localScale = new Vector3(size * 0.18f, 1f, size * 0.18f);
            water.GetComponent<Renderer>().sharedMaterial = _matWater;
            water.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Object.Destroy(water.GetComponent<Collider>());
            var bob = water.AddComponent<WaterSurface>();
            bob.amplitude = 0.12f;
            bob.speed = 0.55f;
        }

        void BuildShoreFoam()
        {
            var foam = new GameObject("ShoreFoam");
            foam.transform.SetParent(transform);
            for (int i = 0; i < 24; i++)
            {
                float ang = i / 24f * Mathf.PI * 2f;
                float dist = size * 0.36f + Random.Range(-4f, 6f);
                float x = Mathf.Cos(ang) * dist;
                float z = Mathf.Sin(ang) * dist;
                float y = 0.4f;
                var p = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                p.name = "Foam";
                p.transform.SetParent(foam.transform);
                p.transform.position = new Vector3(x, y, z);
                p.transform.localScale = new Vector3(Random.Range(2f, 5f), 0.08f, Random.Range(1.2f, 2.5f));
                var m = VisualAssetFactory.MakeLit(new Color(0.95f, 0.97f, 1f, 0.55f), null, 0.8f);
                if (m.HasProperty("_Surface")) m.SetFloat("_Surface", 1);
                m.color = new Color(0.95f, 0.97f, 1f, 0.45f);
                p.GetComponent<Renderer>().sharedMaterial = m;
                p.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                Object.Destroy(p.GetComponent<Collider>());
            }
        }

        void BuildPond()
        {
            float px = -18f, pz = 22f;
            float y = HeightAt(px, pz) + 0.12f;
            var pond = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pond.name = "FreshwaterPond";
            pond.transform.SetParent(transform);
            pond.transform.position = new Vector3(px, y, pz);
            pond.transform.localScale = new Vector3(11f, 0.06f, 11f);
            pond.GetComponent<Renderer>().sharedMaterial = _matWater;
            Object.Destroy(pond.GetComponent<Collider>());

            // Shore ring
            for (int i = 0; i < 10; i++)
            {
                float a = i / 10f * 360f;
                var rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                rock.transform.SetParent(pond.transform);
                rock.transform.localPosition = Quaternion.Euler(0, a, 0) * new Vector3(0.55f, 0.8f, 0f);
                rock.transform.localScale = Vector3.one * Random.Range(0.08f, 0.16f);
                rock.GetComponent<Renderer>().sharedMaterial = _matRock;
            }

            var drink = new GameObject("DrinkZone");
            drink.transform.SetParent(pond.transform);
            drink.transform.localPosition = Vector3.zero;
            var col = drink.AddComponent<SphereCollider>();
            col.radius = 0.55f;
            col.isTrigger = true;
            var g = drink.AddComponent<GatherableResource>();
            g.itemId = "freshwater";
            g.amountMin = 1; g.amountMax = 1;
            g.cooldown = 8f;
            g.promptOverride = "Collect Fresh Water";
        }

        void BuildCave()
        {
            float cx = 38f, cz = -30f;
            float y = HeightAt(cx, cz);
            var cave = new GameObject("CaveArea");
            cave.transform.SetParent(transform);
            cave.transform.position = new Vector3(cx, y, cz);
            for (int i = 0; i < 12; i++)
            {
                var rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                rock.transform.SetParent(cave.transform);
                rock.transform.localPosition = new Vector3(Random.Range(-5f, 5f), Random.Range(0.3f, 3.2f), Random.Range(-4f, 4f));
                rock.transform.localScale = Vector3.Scale(Vector3.one * Random.Range(1.4f, 3.8f), new Vector3(1f, Random.Range(0.6f, 1.2f), 1f));
                rock.GetComponent<Renderer>().sharedMaterial = _matRock;
            }
        }

        void BuildLandmarks()
        {
            // Hidden clearing marker — tall rock stack
            float x = 12f, z = 40f;
            float y = HeightAt(x, z);
            var lm = new GameObject("Landmark_Clearing");
            lm.transform.SetParent(transform);
            lm.transform.position = new Vector3(x, y, z);
            for (int i = 0; i < 4; i++)
            {
                var r = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                r.transform.SetParent(lm.transform);
                r.transform.localPosition = new Vector3(0, 0.6f + i * 0.9f, 0);
                r.transform.localScale = Vector3.one * (1.4f - i * 0.2f);
                r.GetComponent<Renderer>().sharedMaterial = _matRock;
            }
        }

        void ScatterNature()
        {
            var nature = new GameObject("Nature");
            nature.transform.SetParent(transform);
            for (int i = 0; i < 110; i++) PlaceTree(nature.transform, false);
            for (int i = 0; i < 50; i++) PlaceTree(nature.transform, true);
            for (int i = 0; i < 80; i++) PlaceRock(nature.transform);
            for (int i = 0; i < 100; i++) PlaceBush(nature.transform);
            for (int i = 0; i < 70; i++) PlaceGrassClump(nature.transform);
            for (int i = 0; i < 55; i++) PlaceGroundItem(nature.transform, "wood", 0.35f, new Color(0.42f, 0.26f, 0.12f), "Branch");
            for (int i = 0; i < 40; i++) PlaceGroundItem(nature.transform, "fruit", 0.28f, new Color(0.9f, 0.15f, 0.15f), "Fruit");
            for (int i = 0; i < 45; i++) PlaceGroundItem(nature.transform, "stone", 0.32f, new Color(0.5f, 0.5f, 0.5f), "Stone");
            for (int i = 0; i < 45; i++) PlaceGroundItem(nature.transform, "fiber", 0.22f, new Color(0.72f, 0.68f, 0.28f), "Fiber Plant");
            for (int i = 0; i < 35; i++) PlaceGroundItem(nature.transform, "leaves", 0.26f, new Color(0.22f, 0.55f, 0.16f), "Leaves");
            for (int i = 0; i < 30; i++) PlaceFlower(nature.transform);
        }

        Vector3 RandomIslandPoint(float minH, float maxH, float minDist, float maxDist)
        {
            for (int attempt = 0; attempt < 50; attempt++)
            {
                float ang = Random.Range(0f, Mathf.PI * 2f);
                float dist = Random.Range(minDist, maxDist);
                float x = Mathf.Cos(ang) * dist;
                float z = Mathf.Sin(ang) * dist;
                float h = HeightAt(x, z);
                if (h >= minH && h <= maxH) return new Vector3(x, h, z);
            }
            return new Vector3(0f, HeightAt(0, 0), 0f);
        }

        void PlaceTree(Transform parent, bool palm)
        {
            Vector3 p = RandomIslandPoint(1.8f, 20f, 14f, size * 0.42f);
            var tree = new GameObject(palm ? "Palm" : "Tree");
            tree.transform.SetParent(parent);
            tree.transform.position = p;
            tree.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

            float trunkH = palm ? Random.Range(3.8f, 5.2f) : Random.Range(3.5f, 5.5f);
            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.transform.SetParent(tree.transform);
            trunk.transform.localPosition = new Vector3(0f, trunkH * 0.5f, 0f);
            trunk.transform.localScale = palm
                ? new Vector3(0.28f, trunkH * 0.5f, 0.28f)
                : new Vector3(Random.Range(0.32f, 0.45f), trunkH * 0.5f, Random.Range(0.32f, 0.45f));
            trunk.GetComponent<Renderer>().sharedMaterial = _matTrunk;

            if (palm)
            {
                for (int i = 0; i < 6; i++)
                {
                    var frond = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    frond.transform.SetParent(tree.transform);
                    float a = i / 6f * 360f;
                    frond.transform.localPosition = new Vector3(Mathf.Cos(a * Mathf.Deg2Rad) * 1.1f, trunkH + 0.2f, Mathf.Sin(a * Mathf.Deg2Rad) * 1.1f);
                    frond.transform.localScale = new Vector3(1.6f, 0.25f, 0.7f);
                    frond.transform.localRotation = Quaternion.Euler(25f, a, 0);
                    frond.GetComponent<Renderer>().sharedMaterial = _matPalm;
                    Object.Destroy(frond.GetComponent<Collider>());
                }
            }
            else
            {
                int crowns = Random.Range(3, 6);
                for (int i = 0; i < crowns; i++)
                {
                    var leaf = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    leaf.transform.SetParent(tree.transform);
                    leaf.transform.localPosition = new Vector3(Random.Range(-0.7f, 0.7f), trunkH * 0.85f + i * 0.35f, Random.Range(-0.7f, 0.7f));
                    leaf.transform.localScale = Vector3.one * Random.Range(1.5f, 2.6f);
                    leaf.GetComponent<Renderer>().sharedMaterial = _matLeaf;
                    Object.Destroy(leaf.GetComponent<Collider>());
                }
            }

            var g = tree.AddComponent<GatherableResource>();
            g.itemId = "wood";
            g.amountMin = 2; g.amountMax = 4;
            g.cooldown = 120f;
            g.promptOverride = palm ? "Chop Palm" : "Chop Tree";

            // Simple wind sway
            tree.AddComponent<WindSway>().strength = palm ? 2.5f : 1.2f;
        }

        void PlaceRock(Transform parent)
        {
            Vector3 p = RandomIslandPoint(0.5f, 18f, 8f, size * 0.45f);
            var rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rock.name = "Rock";
            rock.transform.SetParent(parent);
            rock.transform.position = p + Vector3.up * 0.15f;
            rock.transform.localScale = Vector3.Scale(Vector3.one * Random.Range(0.5f, 2.0f), new Vector3(1f, Random.Range(0.35f, 0.75f), 1f));
            rock.transform.rotation = Random.rotation;
            rock.GetComponent<Renderer>().sharedMaterial = _matRock;
            var g = rock.AddComponent<GatherableResource>();
            g.itemId = "stone";
            g.amountMin = 1; g.amountMax = 3;
            g.cooldown = 100f;
            g.promptOverride = "Gather Stone";
        }

        void PlaceBush(Transform parent)
        {
            Vector3 p = RandomIslandPoint(1.4f, 15f, 10f, size * 0.4f);
            var bush = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bush.name = "Bush";
            bush.transform.SetParent(parent);
            bush.transform.position = p + Vector3.up * 0.35f;
            bush.transform.localScale = Vector3.one * Random.Range(0.55f, 1.3f);
            bush.GetComponent<Renderer>().sharedMaterial = _matLeaf;
            var g = bush.AddComponent<GatherableResource>();
            g.itemId = "leaves";
            g.amountMin = 1; g.amountMax = 3;
            g.cooldown = 60f;
            g.promptOverride = "Gather Leaves";
            bush.AddComponent<WindSway>().strength = 3f;
        }

        void PlaceGrassClump(Transform parent)
        {
            Vector3 p = RandomIslandPoint(1.5f, 12f, 12f, size * 0.38f);
            var grass = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            grass.name = "Grass";
            grass.transform.SetParent(parent);
            grass.transform.position = p + Vector3.up * 0.25f;
            grass.transform.localScale = new Vector3(0.15f, 0.35f, 0.15f);
            grass.GetComponent<Renderer>().sharedMaterial = _matLeaf;
            grass.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Object.Destroy(grass.GetComponent<Collider>());
            grass.AddComponent<WindSway>().strength = 4f;
        }

        void PlaceFlower(Transform parent)
        {
            Vector3 p = RandomIslandPoint(1.5f, 10f, 15f, size * 0.35f);
            var f = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            f.name = "Flower";
            f.transform.SetParent(parent);
            f.transform.position = p + Vector3.up * 0.15f;
            f.transform.localScale = Vector3.one * 0.18f;
            var col = Color.HSVToRGB(Random.Range(0f, 1f), 0.7f, 0.95f);
            f.GetComponent<Renderer>().sharedMaterial = VisualAssetFactory.MakeLit(col, null, 0.4f);
            Object.Destroy(f.GetComponent<Collider>());
        }

        void PlaceGroundItem(Transform parent, string itemId, float scale, Color color, string label)
        {
            Vector3 p = RandomIslandPoint(0.8f, 12f, 6f, size * 0.4f);
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = label;
            go.transform.SetParent(parent);
            go.transform.position = p + Vector3.up * (scale * 0.5f);
            go.transform.localScale = Vector3.one * scale;
            go.GetComponent<Renderer>().sharedMaterial = VisualAssetFactory.MakeLit(color, null, 0.3f);
            var g = go.AddComponent<GatherableResource>();
            g.itemId = itemId;
            g.amountMin = 1; g.amountMax = 2;
            g.cooldown = 45f;
            g.destroyOnGather = true;
            g.promptOverride = "Pick up " + label;
        }

        void PlaceSpawn()
        {
            float x = 0f, z = -size * 0.38f;
            float y = HeightAt(x, z) + 1.3f;
            var spawn = new GameObject("PlayerSpawn");
            spawn.transform.SetParent(transform);
            spawn.transform.position = new Vector3(x, y, z);
            spawn.transform.rotation = Quaternion.LookRotation(Vector3.forward);
            PlayerSpawn = spawn.transform;

            // Beach driftwood decoration near spawn
            for (int i = 0; i < 5; i++)
            {
                var d = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                d.transform.SetParent(spawn.transform);
                d.transform.localPosition = new Vector3(Random.Range(-8f, 8f), -0.9f, Random.Range(2f, 10f));
                d.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 90);
                d.transform.localScale = new Vector3(0.12f, Random.Range(0.6f, 1.4f), 0.12f);
                d.GetComponent<Renderer>().sharedMaterial = _matWood;
                Object.Destroy(d.GetComponent<Collider>());
            }
        }
    }

    public class WaterSurface : MonoBehaviour
    {
        public float amplitude = 0.1f;
        public float speed = 0.5f;
        Vector3 _origin;
        Renderer _r;
        void Start() { _origin = transform.position; _r = GetComponent<Renderer>(); }
        void Update()
        {
            float y = _origin.y + Mathf.Sin(Time.time * speed) * amplitude;
            transform.position = new Vector3(_origin.x, y, _origin.z);
            if (_r != null && _r.material != null && _r.material.mainTexture != null)
            {
                var off = _r.material.mainTextureOffset;
                off.x += Time.deltaTime * 0.02f;
                off.y += Time.deltaTime * 0.01f;
                _r.material.mainTextureOffset = off;
            }
        }
    }

    public class WindSway : MonoBehaviour
    {
        public float strength = 2f;
        Quaternion _base;
        float _phase;
        void Start() { _base = transform.localRotation; _phase = Random.Range(0f, 100f); }
        void Update()
        {
            float s = Mathf.Sin(Time.time * (0.8f + strength * 0.1f) + _phase) * strength;
            transform.localRotation = _base * Quaternion.Euler(s * 0.4f, 0f, s * 0.25f);
        }
    }
}
