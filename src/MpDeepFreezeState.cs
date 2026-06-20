using System.Collections.Generic;
using Mirage;
using UnityEngine;

namespace NOLoader.MultiplayerClientSideOptimization
{
    internal sealed class MpDeepFreezeState
    {
        private readonly List<BehaviourEntry> _behaviours = new List<BehaviourEntry>(32);
        private readonly List<ColliderEntry> _colliders = new List<ColliderEntry>(16);
        private readonly List<AudioEntry> _audios = new List<AudioEntry>(8);
        private readonly List<ParticleEntry> _particles = new List<ParticleEntry>(8);
        private readonly List<AnimatorEntry> _animators = new List<AnimatorEntry>(4);
        private readonly List<LightEntry> _lights = new List<LightEntry>(8);
        private readonly List<RendererEntry> _trailLineRenderers = new List<RendererEntry>(8);
        private readonly List<AnimationEntry> _animations = new List<AnimationEntry>(4);
        private readonly List<RendererEntry> _meshRenderers = new List<RendererEntry>(16);

        private bool _rbKinematic;
        private bool _rbDetectCollisions;
        private bool _rbUseGravity;
        private RigidbodyInterpolation _rbInterpolation;

        internal PersistentID UnitId { get; private set; }

        internal static bool IsFrozen(PersistentID id) =>
            MpDeepFreezeManager.IsFrozenId(id);

        internal bool Apply(Unit unit)
        {
            if (unit == null || unit.rb == null)
                return false;

            UnitId = unit.persistentID;
            Rigidbody rb = unit.rb;

            _rbKinematic = rb.isKinematic;
            _rbDetectCollisions = rb.detectCollisions;
            _rbUseGravity = rb.useGravity;
            _rbInterpolation = rb.interpolation;

            rb.isKinematic = true;
            rb.detectCollisions = false;

            CaptureAndDisableNonNetworkBehaviours(unit);
            CaptureAndDisableColliders(unit);
            CaptureAndMuteAudio(unit);
            CaptureAndStopParticles(unit);
            CaptureAndDisableAnimators(unit);

            if (MpConfig.DeepFreezeDisableLights)
                CaptureAndDisableLights(unit);

            CaptureAndDisableTrailLineRenderers(unit);
            CaptureAndDisableLegacyAnimations(unit);

            if (MpConfig.DeepFreezeDisableRenderers)
                CaptureAndDisableMeshRenderers(unit);

            return true;
        }

        internal void Restore(Unit unit, bool resync)
        {
            if (unit == null)
                return;

            if (MpConfig.DeepFreezeDisableRenderers)
                RestoreMeshRenderers();

            RestoreLegacyAnimations();
            RestoreTrailLineRenderers();

            if (MpConfig.DeepFreezeDisableLights)
                RestoreLights();

            RestoreAnimators();
            RestoreParticles();
            RestoreAudio();
            RestoreColliders();
            RestoreBehaviours();

            Rigidbody? rb = unit.rb;
            if (rb != null)
            {
                rb.isKinematic = _rbKinematic;
                rb.detectCollisions = _rbDetectCollisions;
                rb.useGravity = _rbUseGravity;
                rb.interpolation = _rbInterpolation;
            }

            if (resync)
                MpDeepFreezeResync.ForceResyncTransform(unit);
        }

        private void CaptureAndDisableNonNetworkBehaviours(Unit unit)
        {
            _behaviours.Clear();
            Behaviour[] components = unit.GetComponentsInChildren<Behaviour>(true);
            for (int i = 0; i < components.Length; i++)
            {
                Behaviour behaviour = components[i];
                if (behaviour == null || !behaviour.enabled)
                    continue;

                if (!ShouldDisableBehaviour(behaviour))
                    continue;

                _behaviours.Add(new BehaviourEntry(behaviour, true));
                behaviour.enabled = false;
            }
        }

        private static bool ShouldDisableBehaviour(Behaviour behaviour)
        {
            if (behaviour is NetworkBehaviour)
                return false;

            System.Type type = behaviour.GetType();
            string ns = type.Namespace ?? string.Empty;
            if (ns.StartsWith("NuclearOption.NetworkTransforms"))
                return false;

            return true;
        }

        private void CaptureAndDisableColliders(Unit unit)
        {
            _colliders.Clear();
            Collider[] colliders = unit.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                Collider collider = colliders[i];
                if (collider == null || !collider.enabled)
                    continue;

                _colliders.Add(new ColliderEntry(collider, true));
                collider.enabled = false;
            }
        }

        private void CaptureAndMuteAudio(Unit unit)
        {
            _audios.Clear();
            AudioSource[] sources = unit.GetComponentsInChildren<AudioSource>(true);
            for (int i = 0; i < sources.Length; i++)
            {
                AudioSource source = sources[i];
                if (source == null)
                    continue;

                _audios.Add(new AudioEntry(source, source.mute, source.volume));
                source.mute = true;
                source.volume = 0f;
            }
        }

        private void CaptureAndStopParticles(Unit unit)
        {
            _particles.Clear();
            ParticleSystem[] systems = unit.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < systems.Length; i++)
            {
                ParticleSystem ps = systems[i];
                if (ps == null)
                    continue;

                _particles.Add(new ParticleEntry(ps, ps.isPlaying));
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        private void CaptureAndDisableAnimators(Unit unit)
        {
            _animators.Clear();
            Animator[] animators = unit.GetComponentsInChildren<Animator>(true);
            for (int i = 0; i < animators.Length; i++)
            {
                Animator animator = animators[i];
                if (animator == null || !animator.enabled)
                    continue;

                _animators.Add(new AnimatorEntry(animator, true));
                animator.enabled = false;
            }
        }

        private void CaptureAndDisableLights(Unit unit)
        {
            _lights.Clear();
            Light[] lights = unit.GetComponentsInChildren<Light>(true);
            for (int i = 0; i < lights.Length; i++)
            {
                Light light = lights[i];
                if (light == null || !light.enabled)
                    continue;

                _lights.Add(new LightEntry(light, true));
                light.enabled = false;
            }
        }

        private void CaptureAndDisableTrailLineRenderers(Unit unit)
        {
            _trailLineRenderers.Clear();
            TrailRenderer[] trails = unit.GetComponentsInChildren<TrailRenderer>(true);
            for (int i = 0; i < trails.Length; i++)
            {
                TrailRenderer trail = trails[i];
                if (trail == null || !trail.enabled)
                    continue;

                _trailLineRenderers.Add(new RendererEntry(trail, true));
                trail.enabled = false;
            }

            LineRenderer[] lines = unit.GetComponentsInChildren<LineRenderer>(true);
            for (int i = 0; i < lines.Length; i++)
            {
                LineRenderer line = lines[i];
                if (line == null || !line.enabled)
                    continue;

                _trailLineRenderers.Add(new RendererEntry(line, true));
                line.enabled = false;
            }
        }

        private void CaptureAndDisableLegacyAnimations(Unit unit)
        {
            _animations.Clear();
            Animation[] animations = unit.GetComponentsInChildren<Animation>(true);
            for (int i = 0; i < animations.Length; i++)
            {
                Animation animation = animations[i];
                if (animation == null || !animation.enabled)
                    continue;

                _animations.Add(new AnimationEntry(animation, true));
                animation.enabled = false;
            }
        }

        private void CaptureAndDisableMeshRenderers(Unit unit)
        {
            _meshRenderers.Clear();
            Renderer[] renderers = unit.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || !renderer.enabled)
                    continue;

                if (renderer is TrailRenderer || renderer is LineRenderer)
                    continue;

                _meshRenderers.Add(new RendererEntry(renderer, true));
                renderer.enabled = false;
            }
        }

        private void RestoreBehaviours()
        {
            for (int i = 0; i < _behaviours.Count; i++)
            {
                BehaviourEntry entry = _behaviours[i];
                if (entry.Behaviour != null)
                    entry.Behaviour.enabled = entry.WasEnabled;
            }

            _behaviours.Clear();
        }

        private void RestoreColliders()
        {
            for (int i = 0; i < _colliders.Count; i++)
            {
                ColliderEntry entry = _colliders[i];
                if (entry.Collider != null)
                    entry.Collider.enabled = entry.WasEnabled;
            }

            _colliders.Clear();
        }

        private void RestoreAudio()
        {
            for (int i = 0; i < _audios.Count; i++)
            {
                AudioEntry entry = _audios[i];
                if (entry.Source == null)
                    continue;

                entry.Source.mute = entry.WasMuted;
                entry.Source.volume = entry.Volume;
            }

            _audios.Clear();
        }

        private void RestoreParticles()
        {
            for (int i = 0; i < _particles.Count; i++)
            {
                ParticleEntry entry = _particles[i];
                if (entry.System != null && entry.WasPlaying)
                    entry.System.Play(true);
            }

            _particles.Clear();
        }

        private void RestoreAnimators()
        {
            for (int i = 0; i < _animators.Count; i++)
            {
                AnimatorEntry entry = _animators[i];
                if (entry.Animator != null)
                    entry.Animator.enabled = entry.WasEnabled;
            }

            _animators.Clear();
        }

        private void RestoreLights()
        {
            for (int i = 0; i < _lights.Count; i++)
            {
                LightEntry entry = _lights[i];
                if (entry.Light != null)
                    entry.Light.enabled = entry.WasEnabled;
            }

            _lights.Clear();
        }

        private void RestoreTrailLineRenderers()
        {
            for (int i = 0; i < _trailLineRenderers.Count; i++)
            {
                RendererEntry entry = _trailLineRenderers[i];
                if (entry.Renderer != null)
                    entry.Renderer.enabled = entry.WasEnabled;
            }

            _trailLineRenderers.Clear();
        }

        private void RestoreLegacyAnimations()
        {
            for (int i = 0; i < _animations.Count; i++)
            {
                AnimationEntry entry = _animations[i];
                if (entry.Animation != null)
                    entry.Animation.enabled = entry.WasEnabled;
            }

            _animations.Clear();
        }

        private void RestoreMeshRenderers()
        {
            for (int i = 0; i < _meshRenderers.Count; i++)
            {
                RendererEntry entry = _meshRenderers[i];
                if (entry.Renderer != null)
                    entry.Renderer.enabled = entry.WasEnabled;
            }

            _meshRenderers.Clear();
        }

        private readonly struct BehaviourEntry
        {
            internal readonly Behaviour? Behaviour;
            internal readonly bool WasEnabled;

            internal BehaviourEntry(Behaviour behaviour, bool wasEnabled)
            {
                Behaviour = behaviour;
                WasEnabled = wasEnabled;
            }
        }

        private readonly struct ColliderEntry
        {
            internal readonly Collider? Collider;
            internal readonly bool WasEnabled;

            internal ColliderEntry(Collider collider, bool wasEnabled)
            {
                Collider = collider;
                WasEnabled = wasEnabled;
            }
        }

        private readonly struct AudioEntry
        {
            internal readonly AudioSource? Source;
            internal readonly bool WasMuted;
            internal readonly float Volume;

            internal AudioEntry(AudioSource source, bool wasMuted, float volume)
            {
                Source = source;
                WasMuted = wasMuted;
                Volume = volume;
            }
        }

        private readonly struct ParticleEntry
        {
            internal readonly ParticleSystem? System;
            internal readonly bool WasPlaying;

            internal ParticleEntry(ParticleSystem system, bool wasPlaying)
            {
                System = system;
                WasPlaying = wasPlaying;
            }
        }

        private readonly struct AnimatorEntry
        {
            internal readonly Animator? Animator;
            internal readonly bool WasEnabled;

            internal AnimatorEntry(Animator animator, bool wasEnabled)
            {
                Animator = animator;
                WasEnabled = wasEnabled;
            }
        }

        private readonly struct LightEntry
        {
            internal readonly Light? Light;
            internal readonly bool WasEnabled;

            internal LightEntry(Light light, bool wasEnabled)
            {
                Light = light;
                WasEnabled = wasEnabled;
            }
        }

        private readonly struct AnimationEntry
        {
            internal readonly Animation? Animation;
            internal readonly bool WasEnabled;

            internal AnimationEntry(Animation animation, bool wasEnabled)
            {
                Animation = animation;
                WasEnabled = wasEnabled;
            }
        }

        private readonly struct RendererEntry
        {
            internal readonly Renderer? Renderer;
            internal readonly bool WasEnabled;

            internal RendererEntry(Renderer renderer, bool wasEnabled)
            {
                Renderer = renderer;
                WasEnabled = wasEnabled;
            }
        }
    }
}
