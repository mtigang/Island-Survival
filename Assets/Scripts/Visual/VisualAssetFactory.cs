using UnityEngine;

namespace IslandSurvival.Visual
{
    /// <summary>
    /// Runtime procedural textures, materials, and UI sprites for premium look without external binaries.
    /// </summary>
    public static class VisualAssetFactory
    {
        static Texture2D _noise;
        static Texture2D _sand, _grass, _rock, _dirt, _wood, _leaf, _water;
        static Sprite _panel, _panelDark, _btn, _btnHover, _slot, _slotSel, _barBg, _circle;
        static Sprite _iconWood, _iconStone, _iconFruit, _iconWater, _iconLeaf, _iconFiber, _iconTool, _iconFire, _iconHome;
        static bool _init;

        public static void Ensure()
        {
            if (_init) return;
            _init = true;
            _noise = MakeNoise(256);
            _sand = MakeTerrainTex(256, new Color(0.92f, 0.84f, 0.62f), new Color(0.78f, 0.68f, 0.45f), 0.08f);
            _grass = MakeTerrainTex(256, new Color(0.28f, 0.52f, 0.18f), new Color(0.18f, 0.38f, 0.12f), 0.12f);
            _rock = MakeTerrainTex(256, new Color(0.48f, 0.47f, 0.46f), new Color(0.32f, 0.31f, 0.30f), 0.18f);
            _dirt = MakeTerrainTex(256, new Color(0.42f, 0.30f, 0.18f), new Color(0.30f, 0.20f, 0.12f), 0.1f);
            _wood = MakeTerrainTex(128, new Color(0.48f, 0.30f, 0.14f), new Color(0.32f, 0.18f, 0.08f), 0.15f);
            _leaf = MakeTerrainTex(128, new Color(0.20f, 0.55f, 0.15f), new Color(0.10f, 0.35f, 0.08f), 0.2f);
            _water = MakeWaterTex(256);

            _panel = MakeRoundedSprite(128, 128, 16, new Color(0.08f, 0.11f, 0.14f, 0.94f), new Color(0.2f, 0.55f, 0.45f, 0.9f));
            _panelDark = MakeRoundedSprite(128, 128, 16, new Color(0.04f, 0.06f, 0.08f, 0.96f), new Color(0.15f, 0.4f, 0.35f, 0.8f));
            _btn = MakeRoundedSprite(128, 48, 12, new Color(0.12f, 0.42f, 0.34f, 1f), new Color(0.35f, 0.85f, 0.7f, 1f));
            _btnHover = MakeRoundedSprite(128, 48, 12, new Color(0.18f, 0.55f, 0.44f, 1f), new Color(0.5f, 0.95f, 0.8f, 1f));
            _slot = MakeRoundedSprite(64, 64, 8, new Color(0.12f, 0.14f, 0.16f, 0.9f), new Color(0.35f, 0.4f, 0.42f, 0.7f));
            _slotSel = MakeRoundedSprite(64, 64, 8, new Color(0.15f, 0.28f, 0.24f, 0.95f), new Color(0.4f, 0.9f, 0.7f, 1f));
            _barBg = MakeRoundedSprite(128, 16, 6, new Color(0.05f, 0.05f, 0.05f, 0.7f), new Color(0.2f, 0.2f, 0.2f, 0.5f));
            _circle = MakeCircleSprite(64, new Color(1, 1, 1, 0.85f));

            _iconWood = MakeIcon(new Color(0.55f, 0.35f, 0.15f));
            _iconStone = MakeIcon(new Color(0.55f, 0.55f, 0.55f));
            _iconFruit = MakeIcon(new Color(0.9f, 0.2f, 0.2f));
            _iconWater = MakeIcon(new Color(0.25f, 0.55f, 0.95f));
            _iconLeaf = MakeIcon(new Color(0.25f, 0.6f, 0.2f));
            _iconFiber = MakeIcon(new Color(0.75f, 0.7f, 0.3f));
            _iconTool = MakeIcon(new Color(0.6f, 0.45f, 0.25f));
            _iconFire = MakeIcon(new Color(0.95f, 0.45f, 0.1f));
            _iconHome = MakeIcon(new Color(0.5f, 0.35f, 0.2f));
        }

        public static Texture2D Sand => _sand;
        public static Texture2D Grass => _grass;
        public static Texture2D Rock => _rock;
        public static Texture2D Dirt => _dirt;
        public static Texture2D Wood => _wood;
        public static Texture2D Leaf => _leaf;
        public static Texture2D Water => _water;
        public static Texture2D Noise => _noise;

        public static Sprite Panel => _panel;
        public static Sprite PanelDark => _panelDark;
        public static Sprite Button => _btn;
        public static Sprite ButtonHover => _btnHover;
        public static Sprite Slot => _slot;
        public static Sprite SlotSelected => _slotSel;
        public static Sprite BarBg => _barBg;
        public static Sprite Circle => _circle;

        public static Sprite IconFor(string itemId)
        {
            Ensure();
            return itemId switch
            {
                "wood" => _iconWood,
                "stone" => _iconStone,
                "fruit" => _iconFruit,
                "freshwater" => _iconWater,
                "leaves" => _iconLeaf,
                "fiber" => _iconFiber,
                "wooden_tool" => _iconTool,
                "campfire" => _iconFire,
                "foundation" or "wall" or "floor" or "roof" or "doorway" or "storage_box" => _iconHome,
                _ => _iconWood
            };
        }

        public static Material MakeLit(Color color, Texture2D tex = null, float smoothness = 0.25f, float metallic = 0f)
        {
            Ensure();
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var m = new Material(shader);
            m.color = color;
            if (tex != null)
            {
                m.mainTexture = tex;
                if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", tex);
            }
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smoothness);
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", metallic);
            if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", smoothness);
            return m;
        }

        public static Material MakeWater()
        {
            Ensure();
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var m = new Material(shader);
            var c = new Color(0.05f, 0.35f, 0.48f, 0.72f);
            m.color = c;
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
            m.mainTexture = _water;
            if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", _water);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.92f);
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", 0.05f);
            // Transparent attempt
            if (m.HasProperty("_Surface")) m.SetFloat("_Surface", 1f);
            if (m.HasProperty("_Blend")) m.SetFloat("_Blend", 0f);
            m.SetFloat("_Mode", 3);
            m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m.SetInt("_ZWrite", 0);
            m.DisableKeyword("_ALPHATEST_ON");
            m.EnableKeyword("_ALPHABLEND_ON");
            m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            m.renderQueue = 3000;
            return m;
        }

        public static Material MakeFoliage(Color c)
        {
            Ensure();
            var m = MakeLit(c, _leaf, 0.15f);
            return m;
        }

        static Texture2D MakeNoise(int size)
        {
            var t = new Texture2D(size, size, TextureFormat.RGBA32, true);
            t.wrapMode = TextureWrapMode.Repeat;
            t.filterMode = FilterMode.Bilinear;
            var px = new Color[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float n = Mathf.PerlinNoise(x * 0.07f, y * 0.07f);
                n = n * 0.6f + Mathf.PerlinNoise(x * 0.2f + 10, y * 0.2f) * 0.4f;
                px[y * size + x] = new Color(n, n, n, 1);
            }
            t.SetPixels(px); t.Apply(true);
            return t;
        }

        static Texture2D MakeTerrainTex(int size, Color a, Color b, float contrast)
        {
            var t = new Texture2D(size, size, TextureFormat.RGBA32, true);
            t.wrapMode = TextureWrapMode.Repeat;
            t.filterMode = FilterMode.Bilinear;
            var px = new Color[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float n = Mathf.PerlinNoise(x * 0.09f, y * 0.09f);
                n += Mathf.PerlinNoise(x * 0.25f + 3, y * 0.25f) * 0.35f;
                n = Mathf.Clamp01(n);
                n = Mathf.Lerp(0.5f - contrast, 0.5f + contrast, n);
                Color c = Color.Lerp(a, b, n);
                // micro detail
                float d = Mathf.PerlinNoise(x * 0.8f, y * 0.8f);
                c *= 0.92f + d * 0.16f;
                c.a = 1;
                px[y * size + x] = c;
            }
            t.SetPixels(px); t.Apply(true);
            return t;
        }

        static Texture2D MakeWaterTex(int size)
        {
            var t = new Texture2D(size, size, TextureFormat.RGBA32, true);
            t.wrapMode = TextureWrapMode.Repeat;
            var px = new Color[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float n = Mathf.PerlinNoise(x * 0.05f, y * 0.05f);
                float n2 = Mathf.PerlinNoise(x * 0.15f + 5, y * 0.15f);
                Color c = Color.Lerp(new Color(0.05f, 0.3f, 0.45f, 0.75f), new Color(0.15f, 0.5f, 0.55f, 0.65f), n);
                c.r += n2 * 0.05f;
                c.g += n2 * 0.08f;
                px[y * size + x] = c;
            }
            t.SetPixels(px); t.Apply(true);
            return t;
        }

        static Sprite MakeRoundedSprite(int w, int h, int radius, Color fill, Color border)
        {
            var t = new Texture2D(w, h, TextureFormat.RGBA32, false);
            t.filterMode = FilterMode.Bilinear;
            var px = new Color[w * h];
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float d = RoundedDist(x, y, w, h, radius);
                if (d > 1f) px[y * w + x] = Color.clear;
                else if (d > 0.85f) px[y * w + x] = border;
                else
                {
                    // subtle top highlight
                    float g = 1f + (1f - y / (float)h) * 0.08f;
                    px[y * w + x] = new Color(fill.r * g, fill.g * g, fill.b * g, fill.a);
                }
            }
            t.SetPixels(px); t.Apply();
            return Sprite.Create(t, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        }

        static float RoundedDist(int x, int y, int w, int h, int r)
        {
            float cx = Mathf.Clamp(x, r, w - 1 - r);
            float cy = Mathf.Clamp(y, r, h - 1 - r);
            float dx = x - cx;
            float dy = y - cy;
            return Mathf.Sqrt(dx * dx + dy * dy) / r;
        }

        static Sprite MakeCircleSprite(int size, Color c)
        {
            var t = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float mid = (size - 1) * 0.5f;
            var px = new Color[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(mid, mid)) / mid;
                if (d > 1f) px[y * size + x] = Color.clear;
                else if (d > 0.85f) px[y * size + x] = new Color(c.r, c.g, c.b, c.a * (1f - (d - 0.85f) / 0.15f));
                else px[y * size + x] = c;
            }
            t.SetPixels(px); t.Apply();
            return Sprite.Create(t, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        static Sprite MakeIcon(Color c)
        {
            int s = 48;
            var t = new Texture2D(s, s, TextureFormat.RGBA32, false);
            float mid = (s - 1) * 0.5f;
            var px = new Color[s * s];
            for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(mid, mid)) / (mid * 0.85f);
                if (d > 1f) px[y * s + x] = Color.clear;
                else
                {
                    float shade = 0.85f + (1f - d) * 0.25f;
                    float edge = d > 0.75f ? Mathf.Lerp(1f, 0.5f, (d - 0.75f) / 0.25f) : 1f;
                    px[y * s + x] = new Color(c.r * shade * edge, c.g * shade * edge, c.b * shade * edge, 1f);
                }
            }
            t.SetPixels(px); t.Apply();
            return Sprite.Create(t, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f));
        }
    }
}
