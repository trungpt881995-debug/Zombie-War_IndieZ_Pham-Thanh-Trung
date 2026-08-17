using System.Collections.Generic;
using UnityEngine;

namespace ZombieWar.Composition.Projectile
{
    [DisallowMultipleComponent]
    public sealed class HitscanTracerPool : MonoBehaviour
    {
        private sealed class Slot
        {
            public LineRenderer Line;
            public float ExpiresAt;
            public bool Active;
        }

        [Header("Tracer")]
        [SerializeField, Min(0.001f)]
        private float width = 0.025f;

        [SerializeField, Min(0.01f)]
        private float duration = 0.06f;

        [Header("Pool")]
        [SerializeField, Min(1)]
        private int prewarmCount = 32;

        [SerializeField, Min(1)]
        private int maxSize = 128;

        private readonly List<Slot> _slots = new List<Slot>(32);
        private Material _material;
        private bool _initialized;

        public void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            maxSize = Mathf.Max(1, maxSize);
            prewarmCount = Mathf.Clamp(prewarmCount, 1, maxSize);

            CreateMaterial();

            for (int i = 0; i < prewarmCount; i++)
            {
                CreateSlot();
            }

            _initialized = true;
        }

        public void Show(Vector3 start, Vector3 end)
        {
            if (!_initialized)
            {
                Initialize();
            }

            Slot slot = AcquireSlot();
            if (slot == null)
            {
                return;
            }

            LineRenderer line = slot.Line;
            line.positionCount = 2;
            line.SetPosition(0, start);
            line.SetPosition(1, end);
            line.startColor = Color.red;
            line.endColor = Color.red;
            line.widthMultiplier = Mathf.Max(0.001f, width);
            line.enabled = true;

            slot.ExpiresAt = Time.unscaledTime + Mathf.Max(0.01f, duration);
            slot.Active = true;
        }

        public void Clear()
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                Slot slot = _slots[i];
                slot.Active = false;

                if (slot.Line != null)
                {
                    slot.Line.enabled = false;
                }
            }
        }

        private void Update()
        {
            if (!_initialized)
            {
                return;
            }

            float now = Time.unscaledTime;

            for (int i = 0; i < _slots.Count; i++)
            {
                Slot slot = _slots[i];

                if (!slot.Active || now < slot.ExpiresAt)
                {
                    continue;
                }

                slot.Active = false;

                if (slot.Line != null)
                {
                    slot.Line.enabled = false;
                }
            }
        }

        private Slot AcquireSlot()
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                if (!_slots[i].Active)
                {
                    return _slots[i];
                }
            }

            if (_slots.Count < maxSize)
            {
                return CreateSlot();
            }

            // At saturation, reuse the tracer that will expire first.
            Slot oldest = _slots[0];

            for (int i = 1; i < _slots.Count; i++)
            {
                if (_slots[i].ExpiresAt < oldest.ExpiresAt)
                {
                    oldest = _slots[i];
                }
            }

            return oldest;
        }

        private Slot CreateSlot()
        {
            GameObject tracerObject = new GameObject("HitscanTracer");
            tracerObject.transform.SetParent(transform, false);

            LineRenderer line = tracerObject.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.loop = false;
            line.positionCount = 2;
            line.widthMultiplier = Mathf.Max(0.001f, width);
            line.startColor = Color.red;
            line.endColor = Color.red;
            line.numCapVertices = 2;
            line.numCornerVertices = 0;
            line.alignment = LineAlignment.View;
            line.textureMode = LineTextureMode.Stretch;
            line.material = _material;
            line.enabled = false;

            var slot = new Slot
            {
                Line = line,
                ExpiresAt = 0f,
                Active = false
            };

            _slots.Add(slot);
            return slot;
        }

        private void CreateMaterial()
        {
            if (_material != null)
            {
                return;
            }

            Shader shader =
                Shader.Find("Universal Render Pipeline/Unlit") ??
                Shader.Find("Sprites/Default") ??
                Shader.Find("Unlit/Color");

            if (shader == null)
            {
                Debug.LogWarning(
                    "[HitscanTracerPool] No suitable unlit shader was found. " +
                    "The LineRenderer will use Unity's fallback material.",
                    this);

                return;
            }

            _material = new Material(shader)
            {
                name = "ZombieWar_HitscanTracer_Red"
            };

            if (_material.HasProperty("_BaseColor"))
            {
                _material.SetColor("_BaseColor", Color.red);
            }

            if (_material.HasProperty("_Color"))
            {
                _material.SetColor("_Color", Color.red);
            }
        }

        private void OnDestroy()
        {
            Clear();

            if (_material != null)
            {
                Destroy(_material);
                _material = null;
            }
        }
    }
}
