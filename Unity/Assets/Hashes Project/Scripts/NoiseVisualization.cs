using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using System;
using static Noise;


public class NoiseVisualization : Visualization
{

	static int noiseId = Shader.PropertyToID("_Noise");

	public enum NoiseType { Perlin, PerlinTurbulence, Value, ValueTurbulence }

	[SerializeField]
	NoiseType type;

	static ScheduleDelegate[,] noiseJobs = {
		{
			Job<Lattice1D<LatticeNormal, Perlin>>.ScheduleParallel,
			Job<Lattice1D<LatticeTiling, Perlin>>.ScheduleParallel,
			Job<Lattice2D<LatticeNormal, Perlin>>.ScheduleParallel,
			Job<Lattice2D<LatticeTiling, Perlin>>.ScheduleParallel,
			Job<Lattice3D<LatticeNormal, Perlin>>.ScheduleParallel,
			Job<Lattice3D<LatticeTiling, Perlin>>.ScheduleParallel
		},
		{
			Job<Lattice1D<LatticeNormal, Turbulence<Perlin>>>.ScheduleParallel,

			Job<Lattice1D<LatticeTiling, Turbulence<Perlin>>>.ScheduleParallel,

			Job<Lattice2D<LatticeNormal, Turbulence<Perlin>>>.ScheduleParallel,

			Job<Lattice2D<LatticeTiling, Turbulence<Perlin>>>.ScheduleParallel,

			Job<Lattice3D<LatticeNormal, Turbulence<Perlin>>>.ScheduleParallel,

			Job<Lattice3D<LatticeTiling, Turbulence<Perlin>>>.ScheduleParallel
		},
		{
			Job<Lattice1D<LatticeNormal, Value>>.ScheduleParallel,

			Job<Lattice1D<LatticeTiling, Value>>.ScheduleParallel,

			Job<Lattice2D<LatticeNormal, Value>>.ScheduleParallel,

			Job<Lattice2D<LatticeTiling, Value>>.ScheduleParallel,

			Job<Lattice3D<LatticeNormal, Value>>.ScheduleParallel,

			Job<Lattice3D<LatticeTiling, Value>>.ScheduleParallel
		},
		{
			Job<Lattice1D<LatticeNormal, Turbulence<Value>>>.ScheduleParallel,

			Job<Lattice1D<LatticeTiling, Turbulence<Value>>>.ScheduleParallel,

			Job<Lattice2D<LatticeNormal, Turbulence<Value>>>.ScheduleParallel,

			Job<Lattice2D<LatticeTiling, Turbulence<Value>>>.ScheduleParallel,

			Job<Lattice3D<LatticeNormal, Turbulence<Value>>>.ScheduleParallel,

			Job<Lattice3D<LatticeTiling, Turbulence<Value>>>.ScheduleParallel
		}
	};

	[SerializeField]
	Settings noiseSettings = Settings.Default;

	[SerializeField]
	SpaceTRS domain = new SpaceTRS {
		scale = 8f
	};

	[SerializeField, Range(1, 3)]
	int dimensions = 3;

	[SerializeField]
	bool tiling;

	NativeArray<float4> noise;

	ComputeBuffer noiseBuffer;

	protected override void EnableVisualization(int dataLength, MaterialPropertyBlock propertyBlock)
	{
		noise = new NativeArray<float4>(dataLength, Allocator.Persistent);
		noiseBuffer = new ComputeBuffer(dataLength * 4, 4);

		propertyBlock.SetBuffer(noiseId, noiseBuffer);
	}

	protected override void DisableVisualization() {
		noise.Dispose();
		noiseBuffer.Release();
		noiseBuffer = null;
	}


	protected override void UpdateVisualization(NativeArray<float3x4> positions, int resolution, JobHandle handle)
	{
		noiseJobs[(int)type, 2 * dimensions - (tiling ? 1 : 2)](
			positions, noise, noiseSettings, domain, resolution, handle
		).Complete();
		noiseBuffer.SetData(noise.Reinterpret<float4>(4 * 4));

	}
}