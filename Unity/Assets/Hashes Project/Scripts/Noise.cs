/*using Unity.Mathematics;

using static Unity.Mathematics.math;

public enum NoiseType { Perlin, PerlinTurbulence, Value, ValueTurbulence, Voronoi }

public interface ILattice {
	LatticeSpan4 GetLatticeSpan4 (float4 coordinates, int frequency);

	int4 ValidateSingleStep (int4 points, int frequency);
}

public struct LatticeNormal : ILattice {

	public int4 ValidateSingleStep (int4 points, int frequency) => points;
}

public struct LatticeTiling : ILattice {

	public int4 ValidateSingleStep (int4 points, int frequency) =>
	select(points, 0, points == frequency);
}

public int4 ValidateSingleStep (int4 points, int frequency) =>
select(select(points, 0, points == frequency), frequency - 1, points == -1);

static float4 GetDistance (float4 x, float4 y) => sqrt(x * x + y * y);

public static partial class Noise {
	static ScheduleDelegate[,] noiseJobs = {
		
	{ … },
	{ … },
	{ … },
		{
			Job<Voronoi1D<LatticeNormal>>.ScheduleParallel,
			Job<Voronoi1D<LatticeTiling>>.ScheduleParallel,
			Job<Voronoi2D<LatticeNormal>>.ScheduleParallel,
			Job<Voronoi2D<LatticeTiling>>.ScheduleParallel,
			Job<Voronoi3D<LatticeNormal>>.ScheduleParallel,
			Job<Voronoi3D<LatticeTiling>>.ScheduleParallel
		}
	};

	public struct Voronoi1D<L> : INoise where L : struct, ILattice {

		public float4 GetNoise4 (float4x3 positions, SmallXXHash4 hash, int frequency) {
			LatticeSpan4 x = default(L).GetLatticeSpan4(positions.c0, frequency);

			SmallXXHash4 h = hash.Eat(x.p0);
			return abs(h.Floats01A - x.g0);
		}
	}

	public struct Voronoi2D<L> : INoise where L : struct, ILattice {

		public float4 GetNoise4 (float4x3 positions, SmallXXHash4 hash, int frequency) {
			var l = default(L);
			LatticeSpan4
			x = l.GetLatticeSpan4(positions.c0, frequency),
			z = l.GetLatticeSpan4(positions.c2, frequency);

			return 0f;
		}
	}

	public struct Voronoi3D<L> : INoise where L : struct, ILattice {

		public float4 GetNoise4 (float4x3 positions, SmallXXHash4 hash, int frequency) {
			var l = default(L);
			LatticeSpan4
			x = l.GetLatticeSpan4(positions.c0, frequency),
			y = l.GetLatticeSpan4(positions.c1, frequency),
			z = l.GetLatticeSpan4(positions.c2, frequency);

			return 0f;
		}
	}

	static float4 UpdateVoronoiMinima (float4 minima, float4 distances) {
		return select(minima, distances, distances < minima);
	}

	public float4 GetNoise4 (float4x3 positions, SmallXXHash4 hash, int frequency) {
		var l = default(L);
		LatticeSpan4 x = l.GetLatticeSpan4(positions.c0, frequency);

		float4 minima = 2f;
		for (int u = -1; u <= 1; u++) {
			SmallXXHash4 h = hash.Eat(l.ValidateSingleStep(x.p0 + u, frequency));
			minima = UpdateVoronoiMinima(minima, abs(h.Floats01A + u - x.g0));
		}
		return minima;
	}
}*/