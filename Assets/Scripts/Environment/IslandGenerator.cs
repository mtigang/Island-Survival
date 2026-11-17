using UnityEngine;
using IslandSurvival.Resources;

namespace IslandSurvival.Environment
{
    /// <summary>
    /// Procedural tropical island: terrain mesh, biomes, props, water, resources.
    /// </summary>
    public class IslandGenerator : MonoBehaviour
    {
        public int resolution = 128;
        public float size = 220f;
        public float height = 28f;
        public int seed = 42;
        public Material terrainMat;
        public Material sandMat;
        public Material waterMat;
        public Material trunkMat;
        public Material leafMat;
        public Material rockMat;

        public Transform PlayerSpawn { get; private set; }

        public void Generate()
        {
            Random.InitState(seed);
            EnsureMaterials();
            BuildTerrain();
            BuildWater();
            ScatterNature();
            PlaceSpawn();
            BuildCave();
            BuildPond();
        }

        void EnsureMaterials()
        {
            Shader lit = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (terrainMat == null) { terrainMat = new Material(lit); terrainMat.color = new Color(0.22f, 0.45f, 0.16f); }
            if (sandMat == null) { sandMat = new Material(lit); sandMat.color = new Color(0.86f, 0.78f, 0.55f); }
            if (waterMat == null)
            {
                waterMat = new Material(lit);
                waterMat.color = new Color(0.1f, 0.35f, 0.55f, 0.75f);
                if (waterMat.HasProperty("_Surface")) waterMat.SetFloat("_Surface", 1);
            }
            if (trunkMat == null) { trunkMat = new Material(lit); trunkMat.color = new Color(0.35f, 0.22f, 0.1f); }
            if (leafMat == null) { leafMat = new Material(lit); leafMat.color = new Color(0.15f, 0.5f, 0.12f); }
            if (rockMat == null) { rockMat = new Material(lit); rockMat.color = new Color(0.45f, 0.45f, 0.48f); }
        }

        float HeightAt(float x, float z)
        {
            float nx = x / size;
            float nz = z / size;
            float dist = Mathf.Sqrt(nx * nx + nz * nz) * 2f; // 0 center -> ~1 edge
            float islandMask = Mathf.Clamp01(1.15f - dist);
            islandMask = islandMask * islandMask * (3f - 2f * islandMask);

            float h = 0f;
            h += Mathf.PerlinNoise(x * 0.01f + seed, z * 0.01f) * 0.55f;
            h += Mathf.PerlinNoise(x * 0.03f + seed * 1.3f, z * 0.03f) * 0.25f;
            h += Mathf.PerlinNoise(x * 0.08f + 20f, z * 0.08f) * 0.12f;
            // Hills
            h += Mathf.PerlinNoise(x * 0.005f + 90f, z * 0.005f) * 0.35f;
            return h * height * islandMask;
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
            var verts = new Vector3[(res + 1) * (res + 1)];
            var cols = new Color[verts.Length];
            var uvs = new Vector2[verts.Length];
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
                    uvs[i] = new Vector2(x / (float)res, z / (float)res);
                    // Biome tint
                    float dist = Mathf.Sqrt(wx * wx + wz * wz) / (size * 0.5f);
                    if (h < 1.2f) cols[i] = new Color(0.86f, 0.78f, 0.55f); // sand
                    else if (h < 4f) cols[i] = Color.Lerp(new Color(0.75f, 0.7f, 0.4f), new Color(0.25f, 0.5f, 0.15f), (h - 1.2f) / 2.8f);
                    else if (dist > 0.72f) cols[i] = new Color(0.5f, 0.48f, 0.45f); // rocky coast
                    else cols[i] = Color.Lerp(new Color(0.2f, 0.48f, 0.15f), new Color(0.35f, 0.4f, 0.2f), Mathf.PerlinNoise(wx * 0.05f, wz * 0.05f));
                }
            }

            int t = 0;
            for (int z = 0; z < res; z++)
            {
                for (int x = 0; x < res; x++)
                {
                    int i = z * (res + 1) + x;
                    tris[t++] = i;
                    tris[t++] = i + res + 1;
                    tris[t++] = i + 1;
                    tris[t++] = i + 1;
                    tris[t++] = i + res + 1;
                    tris[t++] = i + res + 2;
                }
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

            // Vertex color shader may not exist — blend sand/grass via main color
            mr.sharedMaterial = terrainMat;
        }

        void BuildWater()
        {
            var water = GameObject.CreatePrimitive(PrimitiveType.Plane);
            water.name = "Ocean";
            water.transform.SetParent(transform);
            water.transform.position = new Vector3(0f, 0.3f, 0f);
            water.transform.localScale = new Vector3(size * 0.15f, 1f, size * 0.15f);
            var r = water.GetComponent<Renderer>();
            r.sharedMaterial = waterMat;
            Object.Destroy(water.GetComponent<Collider>());
            water.AddComponent<WaterBob>();
        }

        void BuildPond()
        {
            // Freshwater pond in a depression
            float px = -18f, pz = 22f;
            var pond = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pond.name = "FreshwaterPond";
            pond.transform.SetParent(transform);
            float y = HeightAt(px, pz) + 0.15f;
            pond.transform.position = new Vector3(px, y, pz);
            pond.transform.localScale = new Vector3(10f, 0.08f, 10f);
            var r = pond.GetComponent<Renderer>();
            var m = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            m.color = new Color(0.15f, 0.45f, 0.7f, 0.85f);
            r.sharedMaterial = m;
            Object.Destroy(pond.GetComponent<Collider>());

            var drink = new GameObject("DrinkZone");
            drink.transform.SetParent(pond.transform);
            drink.transform.localPosition = Vector3.zero;
            var col = drink.AddComponent<SphereCollider>();
            col.radius = 0.6f;
            col.isTrigger = true;
            var g = drink.AddComponent<GatherableResource>();
            g.itemId = "freshwater";
            g.amountMin = 1; g.amountMax = 1;
            g.cooldown = 8f;
            g.promptOverride = "Drink / Collect Fresh Water";
            g.destroyOnGather = false;
        }

        void BuildCave()
        {
            float cx = 35f, cz = -28f;
            float y = HeightAt(cx, cz);
            var cave = new GameObject("CaveArea");
            cave.transform.SetParent(transform);
            cave.transform.position = new Vector3(cx, y, cz);
            for (int i = 0; i < 7; i++)
            {
                var rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                rock.transform.SetParent(cave.transform);
                rock.transform.localPosition = new Vector3(Random.Range(-4f, 4f), Random.Range(0.5f, 2.5f), Random.Range(-3f, 3f));
                rock.transform.localScale = Vector3.one * Random.Range(1.5f, 3.5f);
                rock.GetComponent<Renderer>().sharedMaterial = rockMat;
            }
        }

        void ScatterNature()
        {
            var nature = new GameObject("Nature");
            nature.transform.SetParent(transform);
            int trees = 90;
            int palms = 40;
            int rocks = 70;
            int bushes = 80;
            int branches = 50;
            int fruits = 35;

            for (int i = 0; i < trees; i++) PlaceTree(nature.transform, false);
            for (int i = 0; i < palms; i++) PlaceTree(nature.transform, true);
            for (int i = 0; i < rocks; i++) PlaceRock(nature.transform);
            for (int i = 0; i < bushes; i++) PlaceBush(nature.transform);
            for (int i = 0; i < branches; i++) PlaceGroundItem(nature.transform, "wood", 0.35f, new Color(0.4f, 0.25f, 0.1f), "Branch");
            for (int i = 0; i < fruits; i++) PlaceGroundItem(nature.transform, "fruit", 0.25f, new Color(0.9f, 0.15f, 0.15f), "Fruit");
            for (int i = 0; i < 40; i++) PlaceGroundItem(nature.transform, "stone", 0.3f, new Color(0.5f, 0.5f, 0.5f), "Stone");
            for (int i = 0; i < 40; i++) PlaceGroundItem(nature.transform, "fiber", 0.2f, new Color(0.7f, 0.65f, 0.25f), "Fiber Plant");
            for (int i = 0; i < 30; i++) PlaceGroundItem(nature.transform, "leaves", 0.25f, new Color(0.2f, 0.55f, 0.15f), "Leaves");
        }

        Vector3 RandomIslandPoint(float minH, float maxH, float minDist, float maxDist)
        {
            for (int attempt = 0; attempt < 40; attempt++)
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
            Vector3 p = RandomIslandPoint(1.8f, 18f, 12f, size * 0.42f);
            var tree = new GameObject(palm ? "Palm" : "Tree");
            tree.transform.SetParent(parent);
            tree.transform.position = p;

            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.transform.SetParent(tree.transform);
            trunk.transform.localPosition = new Vector3(0f, palm ? 2.2f : 2.5f, 0f);
            trunk.transform.localScale = palm ? new Vector3(0.25f, 2.2f, 0.25f) : new Vector3(0.35f, 2.5f, 0.35f);
            trunk.GetComponent<Renderer>().sharedMaterial = trunkMat;

            int crowns = palm ? 1 : 3;
            for (int i = 0; i < crowns; i++)
            {
                var leaf = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                leaf.transform.SetParent(tree.transform);
                leaf.transform.localPosition = new Vector3(Random.Range(-0.4f, 0.4f), (palm ? 4.4f : 4.8f) + i * 0.4f, Random.Range(-0.4f, 0.4f));
                leaf.transform.localScale = Vector3.one * (palm ? 1.8f : Random.Range(1.6f, 2.4f));
                leaf.GetComponent<Renderer>().sharedMaterial = leafMat;
                Object.Destroy(leaf.GetComponent<Collider>());
            }

            var g = tree.AddComponent<GatherableResource>();
            g.itemId = "wood";
            g.amountMin = 2; g.amountMax = 4;
            g.cooldown = 120f;
            g.promptOverride = palm ? "Chop Palm" : "Chop Tree";
        }

        void PlaceRock(Transform parent)
        {
            Vector3 p = RandomIslandPoint(0.6f, 16f, 8f, size * 0.45f);
            var rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rock.name = "Rock";
            rock.transform.SetParent(parent);
            rock.transform.position = p + Vector3.up * 0.2f;
            rock.transform.localScale = Vector3.Scale(Vector3.one * Random.Range(0.5f, 1.8f), new Vector3(1f, Random.Range(0.4f, 0.8f), 1f));
            rock.GetComponent<Renderer>().sharedMaterial = rockMat;
            var g = rock.AddComponent<GatherableResource>();
            g.itemId = "stone";
            g.amountMin = 1; g.amountMax = 3;
            g.cooldown = 100f;
            g.promptOverride = "Gather Stone";
        }

        void PlaceBush(Transform parent)
        {
            Vector3 p = RandomIslandPoint(1.5f, 14f, 10f, size * 0.4f);
            var bush = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bush.name = "Bush";
            bush.transform.SetParent(parent);
            bush.transform.position = p + Vector3.up * 0.35f;
            bush.transform.localScale = Vector3.one * Random.Range(0.6f, 1.2f);
            bush.GetComponent<Renderer>().sharedMaterial = leafMat;
            var g = bush.AddComponent<GatherableResource>();
            g.itemId = "leaves";
            g.amountMin = 1; g.amountMax = 3;
            g.cooldown = 60f;
            g.promptOverride = "Gather Leaves";
        }

        void PlaceGroundItem(Transform parent, string itemId, float scale, Color color, string label)
        {
            Vector3 p = RandomIslandPoint(0.8f, 12f, 6f, size * 0.4f);
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = label;
            go.transform.SetParent(parent);
            go.transform.position = p + Vector3.up * (scale * 0.5f);
            go.transform.localScale = Vector3.one * scale;
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.color = color;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            var g = go.AddComponent<GatherableResource>();
            g.itemId = itemId;
            g.amountMin = 1; g.amountMax = 2;
            g.cooldown = 45f;
            g.destroyOnGather = true;
            g.promptOverride = "Pick up " + label;
        }

        void PlaceSpawn()
        {
            // Beach spawn facing inland
            float x = 0f, z = -size * 0.38f;
            float y = HeightAt(x, z) + 1.2f;
            var spawn = new GameObject("PlayerSpawn");
            spawn.transform.SetParent(transform);
            spawn.transform.position = new Vector3(x, y, z);
            spawn.transform.rotation = Quaternion.LookRotation(Vector3.forward);
            PlayerSpawn = spawn.transform;
        }
    }

    public class WaterBob : MonoBehaviour
    {
        Vector3 _origin;
        void Start() => _origin = transform.position;
        void Update()
        {
            float y = _origin.y + Mathf.Sin(Time.time * 0.6f) * 0.08f;
            transform.position = new Vector3(_origin.x, y, _origin.z);
        }
    }
}
