using UnityEngine;

namespace Sperlich.Extensions.Particles {
	public static class ParticleSystemExt {

		// ── MAIN MODULE ────────────────────────────────────────────────────────

		public static ParticleSystem SetDuration(this ParticleSystem ps, float duration) {
			var main = ps.main;
			main.duration = duration;
			return ps;
		}

		public static ParticleSystem SetLooping(this ParticleSystem ps, bool loop) {
			var main = ps.main;
			main.loop = loop;
			return ps;
		}

		public static ParticleSystem SetPlayOnAwake(this ParticleSystem ps, bool playOnAwake) {
			var main = ps.main;
			main.playOnAwake = playOnAwake;
			return ps;
		}

		public static ParticleSystem SetMaxParticles(this ParticleSystem ps, int max) {
			var main = ps.main;
			main.maxParticles = max;
			return ps;
		}

		public static ParticleSystem SetSimulationSpeed(this ParticleSystem ps, float speed) {
			var main = ps.main;
			main.simulationSpeed = speed;
			return ps;
		}

		public static ParticleSystem SetSimulationSpace(this ParticleSystem ps, ParticleSystemSimulationSpace space) {
			var main = ps.main;
			main.simulationSpace = space;
			return ps;
		}

		public static ParticleSystem SetGravityModifier(this ParticleSystem ps, float gravity) {
			var main = ps.main;
			main.gravityModifier = gravity;
			return ps;
		}

		public static ParticleSystem SetStopAction(this ParticleSystem ps, ParticleSystemStopAction action) {
			var main = ps.main;
			main.stopAction = action;
			return ps;
		}

		// ── START LIFETIME ─────────────────────────────────────────────────────

		public static ParticleSystem SetStartLifetime(this ParticleSystem ps, float lifetime) {
			var main = ps.main;
			main.startLifetime = lifetime;
			return ps;
		}

		public static ParticleSystem SetStartLifetime(this ParticleSystem ps, float min, float max) {
			var main = ps.main;
			main.startLifetime = new ParticleSystem.MinMaxCurve(min, max);
			return ps;
		}

		// ── START SPEED ────────────────────────────────────────────────────────

		public static ParticleSystem SetStartSpeed(this ParticleSystem ps, float speed) {
			var main = ps.main;
			main.startSpeed = speed;
			return ps;
		}

		public static ParticleSystem SetStartSpeed(this ParticleSystem ps, float min, float max) {
			var main = ps.main;
			main.startSpeed = new ParticleSystem.MinMaxCurve(min, max);
			return ps;
		}

		// ── START SIZE ─────────────────────────────────────────────────────────

		public static ParticleSystem SetStartSize(this ParticleSystem ps, float size) {
			var main = ps.main;
			main.startSize = size;
			return ps;
		}

		public static ParticleSystem SetStartSize(this ParticleSystem ps, float min, float max) {
			var main = ps.main;
			main.startSize = new ParticleSystem.MinMaxCurve(min, max);
			return ps;
		}

		public static ParticleSystem SetStartSize3D(this ParticleSystem ps, Vector3 size) {
			var main = ps.main;
			main.startSizeXMultiplier = size.x;
			main.startSizeYMultiplier = size.y;
			main.startSizeZMultiplier = size.z;
			return ps;
		}

		// ── START COLOR ────────────────────────────────────────────────────────

		public static ParticleSystem SetStartColor(this ParticleSystem ps, Color color) {
			var main = ps.main;
			main.startColor = color;
			return ps;
		}

		public static ParticleSystem SetStartColor(this ParticleSystem ps, Color min, Color max) {
			var main = ps.main;
			main.startColor = new ParticleSystem.MinMaxGradient(min, max);
			return ps;
		}

		public static ParticleSystem SetStartColor(this ParticleSystem ps, Gradient gradient) {
			var main = ps.main;
			main.startColor = new ParticleSystem.MinMaxGradient(gradient);
			return ps;
		}

		public static ParticleSystem SetStartAlpha(this ParticleSystem ps, float alpha) {
			var main = ps.main;
			var col = main.startColor.color;
			col.a = alpha;
			main.startColor = col;
			return ps;
		}

		// ── START ROTATION ─────────────────────────────────────────────────────

		public static ParticleSystem SetStartRotation(this ParticleSystem ps, float degrees) {
			var main = ps.main;
			main.startRotation = degrees * Mathf.Deg2Rad;
			return ps;
		}

		public static ParticleSystem SetStartRotation(this ParticleSystem ps, float minDegrees, float maxDegrees) {
			var main = ps.main;
			main.startRotation = new ParticleSystem.MinMaxCurve(minDegrees * Mathf.Deg2Rad, maxDegrees * Mathf.Deg2Rad);
			return ps;
		}

		// ── EMISSION MODULE ────────────────────────────────────────────────────

		public static ParticleSystem SetEmissionEnabled(this ParticleSystem ps, bool enabled) {
			var emission = ps.emission;
			emission.enabled = enabled;
			return ps;
		}

		public static ParticleSystem SetEmissionRate(this ParticleSystem ps, float rate) {
			var emission = ps.emission;
			emission.rateOverTime = rate;
			return ps;
		}

		public static ParticleSystem SetEmissionRate(this ParticleSystem ps, float min, float max) {
			var emission = ps.emission;
			emission.rateOverTime = new ParticleSystem.MinMaxCurve(min, max);
			return ps;
		}

		public static ParticleSystem SetEmissionRateOverDistance(this ParticleSystem ps, float rate) {
			var emission = ps.emission;
			emission.rateOverDistance = rate;
			return ps;
		}

		public static ParticleSystem AddBurst(this ParticleSystem ps, float time, short count) {
			var emission = ps.emission;
			var bursts = new ParticleSystem.Burst[emission.burstCount + 1];
			emission.GetBursts(bursts);
			bursts[emission.burstCount] = new ParticleSystem.Burst(time, count);
			emission.SetBursts(bursts);
			return ps;
		}

		public static ParticleSystem AddBurst(this ParticleSystem ps, float time, short minCount, short maxCount, int cycles = 1, float repeatInterval = 0.01f) {
			var emission = ps.emission;
			var bursts = new ParticleSystem.Burst[emission.burstCount + 1];
			emission.GetBursts(bursts);
			bursts[emission.burstCount] = new ParticleSystem.Burst(time, minCount, maxCount, cycles, repeatInterval);
			emission.SetBursts(bursts);
			return ps;
		}

		public static ParticleSystem ClearBursts(this ParticleSystem ps) {
			ps.emission.SetBursts(new ParticleSystem.Burst[0]);
			return ps;
		}

		// ── SHAPE MODULE ───────────────────────────────────────────────────────

		public static ParticleSystem SetShapeEnabled(this ParticleSystem ps, bool enabled) {
			var shape = ps.shape;
			shape.enabled = enabled;
			return ps;
		}

		public static ParticleSystem SetShapeType(this ParticleSystem ps, ParticleSystemShapeType shapeType) {
			var shape = ps.shape;
			shape.shapeType = shapeType;
			return ps;
		}

		public static ParticleSystem SetShapeRadius(this ParticleSystem ps, float radius) {
			var shape = ps.shape;
			shape.radius = radius;
			return ps;
		}

		public static ParticleSystem SetShapeAngle(this ParticleSystem ps, float angle) {
			var shape = ps.shape;
			shape.angle = angle;
			return ps;
		}

		public static ParticleSystem SetShapeScale(this ParticleSystem ps, Vector3 scale) {
			var shape = ps.shape;
			shape.scale = scale;
			return ps;
		}

		public static ParticleSystem SetShapePosition(this ParticleSystem ps, Vector3 position) {
			var shape = ps.shape;
			shape.position = position;
			return ps;
		}

		public static ParticleSystem SetShapeRotation(this ParticleSystem ps, Vector3 rotation) {
			var shape = ps.shape;
			shape.rotation = rotation;
			return ps;
		}

		public static ParticleSystem SetShapeArc(this ParticleSystem ps, float arc) {
			var shape = ps.shape;
			shape.arc = arc;
			return ps;
		}

		// ── VELOCITY OVER LIFETIME ─────────────────────────────────────────────

		public static ParticleSystem SetVelocityOverLifetimeEnabled(this ParticleSystem ps, bool enabled) {
			var vol = ps.velocityOverLifetime;
			vol.enabled = enabled;
			return ps;
		}

		public static ParticleSystem SetLinearVelocity(this ParticleSystem ps, Vector3 velocity) {
			var vol = ps.velocityOverLifetime;
			vol.enabled = true;
			vol.x = velocity.x;
			vol.y = velocity.y;
			vol.z = velocity.z;
			return ps;
		}

		public static ParticleSystem SetOrbitalVelocity(this ParticleSystem ps, Vector3 velocity) {
			var vol = ps.velocityOverLifetime;
			vol.enabled = true;
			vol.orbitalX = velocity.x;
			vol.orbitalY = velocity.y;
			vol.orbitalZ = velocity.z;
			return ps;
		}

		public static ParticleSystem SetRadialVelocity(this ParticleSystem ps, float radial) {
			var vol = ps.velocityOverLifetime;
			vol.enabled = true;
			vol.radial = radial;
			return ps;
		}

		// ── COLOR OVER LIFETIME ────────────────────────────────────────────────

		public static ParticleSystem SetColorOverLifetimeEnabled(this ParticleSystem ps, bool enabled) {
			var col = ps.colorOverLifetime;
			col.enabled = enabled;
			return ps;
		}

		public static ParticleSystem SetColorOverLifetime(this ParticleSystem ps, Gradient gradient) {
			var col = ps.colorOverLifetime;
			col.enabled = true;
			col.color = new ParticleSystem.MinMaxGradient(gradient);
			return ps;
		}

		public static ParticleSystem SetColorOverLifetime(this ParticleSystem ps, Color from, Color to) {
			var col = ps.colorOverLifetime;
			col.enabled = true;
			var gradient = new Gradient();
			gradient.SetKeys(
				new[] { new GradientColorKey(from, 0f), new GradientColorKey(to, 1f) },
				new[] { new GradientAlphaKey(from.a, 0f), new GradientAlphaKey(to.a, 1f) }
			);
			col.color = new ParticleSystem.MinMaxGradient(gradient);
			return ps;
		}

		public static ParticleSystem FadeOutOverLifetime(this ParticleSystem ps) {
			var col = ps.colorOverLifetime;
			col.enabled = true;
			var gradient = new Gradient();
			gradient.SetKeys(
				new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
				new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
			);
			col.color = new ParticleSystem.MinMaxGradient(gradient);
			return ps;
		}

		// ── SIZE OVER LIFETIME ─────────────────────────────────────────────────

		public static ParticleSystem SetSizeOverLifetimeEnabled(this ParticleSystem ps, bool enabled) {
			var sol = ps.sizeOverLifetime;
			sol.enabled = enabled;
			return ps;
		}

		public static ParticleSystem SetSizeOverLifetime(this ParticleSystem ps, AnimationCurve curve) {
			var sol = ps.sizeOverLifetime;
			sol.enabled = true;
			sol.size = new ParticleSystem.MinMaxCurve(1f, curve);
			return ps;
		}

		public static ParticleSystem SetSizeOverLifetime(this ParticleSystem ps, float multiplier, AnimationCurve curve) {
			var sol = ps.sizeOverLifetime;
			sol.enabled = true;
			sol.size = new ParticleSystem.MinMaxCurve(multiplier, curve);
			return ps;
		}

		// ── ROTATION OVER LIFETIME ─────────────────────────────────────────────

		public static ParticleSystem SetRotationOverLifetimeEnabled(this ParticleSystem ps, bool enabled) {
			var rol = ps.rotationOverLifetime;
			rol.enabled = enabled;
			return ps;
		}

		public static ParticleSystem SetRotationOverLifetime(this ParticleSystem ps, float angularVelocityDegrees) {
			var rol = ps.rotationOverLifetime;
			rol.enabled = true;
			rol.z = angularVelocityDegrees * Mathf.Deg2Rad;
			return ps;
		}

		public static ParticleSystem SetRotationOverLifetime(this ParticleSystem ps, float minDegrees, float maxDegrees) {
			var rol = ps.rotationOverLifetime;
			rol.enabled = true;
			rol.z = new ParticleSystem.MinMaxCurve(minDegrees * Mathf.Deg2Rad, maxDegrees * Mathf.Deg2Rad);
			return ps;
		}

		// ── NOISE MODULE ───────────────────────────────────────────────────────

		public static ParticleSystem SetNoiseEnabled(this ParticleSystem ps, bool enabled) {
			var noise = ps.noise;
			noise.enabled = enabled;
			return ps;
		}

		public static ParticleSystem SetNoiseStrength(this ParticleSystem ps, float strength) {
			var noise = ps.noise;
			noise.enabled = true;
			noise.strength = strength;
			return ps;
		}

		public static ParticleSystem SetNoiseFrequency(this ParticleSystem ps, float frequency) {
			var noise = ps.noise;
			noise.enabled = true;
			noise.frequency = frequency;
			return ps;
		}

		public static ParticleSystem SetNoise(this ParticleSystem ps, float strength, float frequency, float scrollSpeed = 0f) {
			var noise = ps.noise;
			noise.enabled = true;
			noise.strength = strength;
			noise.frequency = frequency;
			noise.scrollSpeed = scrollSpeed;
			return ps;
		}

		public static ParticleSystem SetNoiseOctaves(this ParticleSystem ps, int octaves) {
			var noise = ps.noise;
			noise.enabled = true;
			noise.octaveCount = octaves;
			return ps;
		}

		// ── TRAILS MODULE ──────────────────────────────────────────────────────

		public static ParticleSystem SetTrailsEnabled(this ParticleSystem ps, bool enabled) {
			var trails = ps.trails;
			trails.enabled = enabled;
			return ps;
		}

		public static ParticleSystem SetTrailLifetime(this ParticleSystem ps, float lifetime) {
			var trails = ps.trails;
			trails.enabled = true;
			trails.lifetime = lifetime;
			return ps;
		}

		public static ParticleSystem SetTrailWidth(this ParticleSystem ps, float width) {
			var trails = ps.trails;
			trails.enabled = true;
			trails.widthOverTrail = width;
			return ps;
		}

		public static ParticleSystem SetTrailRatio(this ParticleSystem ps, float ratio) {
			var trails = ps.trails;
			trails.enabled = true;
			trails.ratio = Mathf.Clamp01(ratio);
			return ps;
		}

		// ── LIGHTS MODULE ──────────────────────────────────────────────────────

		public static ParticleSystem SetLightsEnabled(this ParticleSystem ps, bool enabled) {
			var lights = ps.lights;
			lights.enabled = enabled;
			return ps;
		}

		public static ParticleSystem SetLightsRatio(this ParticleSystem ps, float ratio) {
			var lights = ps.lights;
			lights.enabled = true;
			lights.ratio = Mathf.Clamp01(ratio);
			return ps;
		}

		public static ParticleSystem SetLightsIntensity(this ParticleSystem ps, float intensity) {
			var lights = ps.lights;
			lights.enabled = true;
			lights.intensityMultiplier = intensity;
			return ps;
		}

		public static ParticleSystem SetLightsRange(this ParticleSystem ps, float range) {
			var lights = ps.lights;
			lights.enabled = true;
			lights.rangeMultiplier = range;
			return ps;
		}

		// ── RENDERER ───────────────────────────────────────────────────────────

		public static ParticleSystem SetMaterial(this ParticleSystem ps, Material material) {
			var renderer = ps.GetComponent<ParticleSystemRenderer>();
			if (renderer != null) renderer.material = material;
			return ps;
		}

		public static ParticleSystem SetTrailMaterial(this ParticleSystem ps, Material material) {
			var renderer = ps.GetComponent<ParticleSystemRenderer>();
			if (renderer != null) renderer.trailMaterial = material;
			return ps;
		}

		public static ParticleSystem SetSortingOrder(this ParticleSystem ps, int order) {
			var renderer = ps.GetComponent<ParticleSystemRenderer>();
			if (renderer != null) renderer.sortingOrder = order;
			return ps;
		}

		public static ParticleSystem SetSortingLayer(this ParticleSystem ps, string layerName) {
			var renderer = ps.GetComponent<ParticleSystemRenderer>();
			if (renderer != null) renderer.sortingLayerName = layerName;
			return ps;
		}

		public static ParticleSystem SetRenderMode(this ParticleSystem ps, ParticleSystemRenderMode mode) {
			var renderer = ps.GetComponent<ParticleSystemRenderer>();
			if (renderer != null) renderer.renderMode = mode;
			return ps;
		}

		public static ParticleSystem SetRendererEnabled(this ParticleSystem ps, bool enabled) {
			var renderer = ps.GetComponent<ParticleSystemRenderer>();
			if (renderer != null) renderer.enabled = enabled;
			return ps;
		}

		// ── PLAYBACK HELPERS ───────────────────────────────────────────────────

		public static ParticleSystem Restart(this ParticleSystem ps) {
			ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			ps.Play(true);
			return ps;
		}

		public static ParticleSystem PlayChained(this ParticleSystem ps) {
			ps.Play(true);
			return ps;
		}

		public static ParticleSystem StopChained(this ParticleSystem ps, bool clear = false) {
			ps.Stop(true, clear ? ParticleSystemStopBehavior.StopEmittingAndClear : ParticleSystemStopBehavior.StopEmitting);
			return ps;
		}

		public static ParticleSystem ClearChained(this ParticleSystem ps) {
			ps.Clear(true);
			return ps;
		}

		// ── UTILITY ────────────────────────────────────────────────────────────

		public static bool IsAlive(this ParticleSystem ps) => ps.IsAlive(true);

		public static int GetParticleCount(this ParticleSystem ps) => ps.particleCount;
	}
}
