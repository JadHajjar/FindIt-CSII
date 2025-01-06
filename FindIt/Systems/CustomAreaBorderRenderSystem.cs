using Colossal.Mathematics;

using Game;
using Game.Areas;
using Game.Common;
using Game.Prefabs;
using Game.Rendering;
using Game.Tools;

using System;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

using UnityEngine;

namespace FindIt.Systems
{
	public partial class CustomAreaBorderRenderSystem : GameSystemBase
	{
		private OverlayRenderSystem m_OverlayRenderSystem;

		private ToolSystem m_ToolSystem;

		private EntityQuery m_AreaBorderQuery;

		private EntityQuery m_RenderingSettingsQuery;

		protected override void OnCreate()
		{
			base.OnCreate();

			m_OverlayRenderSystem = World.GetOrCreateSystemManaged<OverlayRenderSystem>();
			m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
			m_AreaBorderQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[] { ComponentType.ReadOnly<Area>(), ComponentType.ReadOnly<Highlighted>() },
				None = new ComponentType[]
				{
					ComponentType.ReadOnly<Error>(),
					ComponentType.ReadOnly<Warning>(),
					ComponentType.ReadOnly<Temp>(),
					ComponentType.ReadOnly<Hidden>(),
					ComponentType.ReadOnly<Deleted>()
				}
			});

			m_RenderingSettingsQuery = GetEntityQuery(ComponentType.ReadOnly<RenderingSettingsData>());
			RequireForUpdate(m_AreaBorderQuery);
		}

		protected override void OnUpdate()
		{
			var renderingSettingsData = default(RenderingSettingsData);

			if (!m_RenderingSettingsQuery.IsEmptyIgnoreFilter)
			{
				var singleton = m_RenderingSettingsQuery.GetSingleton<RenderingSettingsData>();

				renderingSettingsData.m_HoveredColor = singleton.m_HoveredColor;
				renderingSettingsData.m_WarningColor = singleton.m_WarningColor;
			}
			else
			{
				renderingSettingsData.m_HoveredColor = new Color(0.5f, 0.5f, 1f, 0.5f);
				renderingSettingsData.m_WarningColor = new Color(1f, 0.65f, 0f, 0.5f);
			}

			var chunks = m_AreaBorderQuery.ToArchetypeChunkListAsync(Allocator.TempJob, out var outJobHandle);
			var jobData = default(AreaBorderRenderJob);
			jobData.m_AreaType = SystemAPI.GetComponentTypeHandle<Area>(true);
			jobData.m_Batch = SystemAPI.GetComponentTypeHandle<Batch>(true);
			jobData.m_LotType = SystemAPI.GetComponentTypeHandle<Lot>(true);
			jobData.m_MapTileType = SystemAPI.GetComponentTypeHandle<MapTile>(true);
			jobData.m_PrefabRefType = SystemAPI.GetComponentTypeHandle<PrefabRef>(true);
			jobData.m_NodeType = SystemAPI.GetBufferTypeHandle<Node>(true);
			jobData.m_PrefabGeometryData = SystemAPI.GetComponentLookup<AreaGeometryData>(true);
			jobData.m_RenderingSettingsData = renderingSettingsData;
			jobData.m_Chunks = chunks;
			jobData.m_EditorMode = m_ToolSystem.actionMode.IsEditor();
			jobData.m_OverlayBuffer = m_OverlayRenderSystem.GetBuffer(out var dependencies);

			var jobHandle = IJobExtensions.Schedule(jobData, JobHandle.CombineDependencies(base.Dependency, outJobHandle, dependencies));

			chunks.Dispose(jobHandle);

			m_OverlayRenderSystem.AddBufferWriter(jobHandle);

			Dependency = jobHandle;
		}


		private struct Border : IEquatable<Border>
		{
			public float3 m_StartPos;

			public float3 m_EndPos;

			public bool Equals(Border other)
			{
				return m_StartPos.Equals(other.m_StartPos) & m_EndPos.Equals(other.m_EndPos);
			}

			public override int GetHashCode()
			{
				return m_StartPos.GetHashCode();
			}
		}

		[BurstCompile]
		private struct AreaBorderRenderJob : IJob
		{
			[ReadOnly]
			public ComponentTypeHandle<Area> m_AreaType;

			[ReadOnly]
			public ComponentTypeHandle<Lot> m_LotType;

			[ReadOnly]
			public ComponentTypeHandle<Batch> m_Batch;

			[ReadOnly]
			public ComponentTypeHandle<MapTile> m_MapTileType;

			[ReadOnly]
			public ComponentTypeHandle<PrefabRef> m_PrefabRefType;

			[ReadOnly]
			public BufferTypeHandle<Node> m_NodeType;

			[ReadOnly]
			public ComponentLookup<AreaGeometryData> m_PrefabGeometryData;

			[ReadOnly]
			public RenderingSettingsData m_RenderingSettingsData;

			[ReadOnly]
			public NativeList<ArchetypeChunk> m_Chunks;

			[ReadOnly]
			public bool m_EditorMode;

			public OverlayRenderSystem.Buffer m_OverlayBuffer;

			public void Execute()
			{
				var num = 0;
				for (var i = 0; i < m_Chunks.Length; i++)
				{
					var archetypeChunk = m_Chunks[i];
					var nativeArray = archetypeChunk.GetNativeArray(ref m_AreaType);
					var bufferAccessor = archetypeChunk.GetBufferAccessor(ref m_NodeType);
					for (var j = 0; j < nativeArray.Length; j++)
					{
						if ((nativeArray[j].m_Flags & AreaFlags.Slave) == 0)
						{
							num += bufferAccessor[j].Length;
						}
					}
				}

				var borderMap = new NativeParallelHashSet<Border>(num, Allocator.Temp);
				var nodeMap = new NativeParallelHashSet<float3>(num, Allocator.Temp);
				for (var k = 0; k < m_Chunks.Length; k++)
				{
					AddBorders(m_Chunks[k], borderMap);
				}

				for (var l = 0; l < m_Chunks.Length; l++)
				{
					DrawBorders(m_Chunks[l], borderMap, nodeMap);
				}

				borderMap.Dispose();
				nodeMap.Dispose();
			}

			private void AddBorders(ArchetypeChunk chunk, NativeParallelHashSet<Border> borderMap)
			{
				var nativeArray = chunk.GetNativeArray(ref m_AreaType);
				var bufferAccessor = chunk.GetBufferAccessor(ref m_NodeType);

				for (var i = 0; i < nativeArray.Length; i++)
				{
					var area = nativeArray[i];
					if ((area.m_Flags & AreaFlags.Slave) != 0)
					{
						continue;
					}

					var dynamicBuffer = bufferAccessor[i];
					var @float = dynamicBuffer[0].m_Position;
					for (var j = 1; j < dynamicBuffer.Length; j++)
					{
						var position = dynamicBuffer[j].m_Position;
						if ((area.m_Flags & AreaFlags.CounterClockwise) != 0)
						{
							borderMap.Add(new Border
							{
								m_StartPos = position,
								m_EndPos = @float
							});
						}
						else
						{
							borderMap.Add(new Border
							{
								m_StartPos = @float,
								m_EndPos = position
							});
						}

						@float = position;
					}

					if ((area.m_Flags & AreaFlags.Complete) != 0)
					{
						var position2 = dynamicBuffer[0].m_Position;
						if ((area.m_Flags & AreaFlags.CounterClockwise) != 0)
						{
							borderMap.Add(new Border
							{
								m_StartPos = position2,
								m_EndPos = @float
							});
						}
						else
						{
							borderMap.Add(new Border
							{
								m_StartPos = @float,
								m_EndPos = position2
							});
						}
					}
				}
			}

			private void DrawBorders(ArchetypeChunk chunk, NativeParallelHashSet<Border> borderMap, NativeParallelHashSet<float3> nodeMap)
			{
				var nativeArray = chunk.GetNativeArray(ref m_AreaType);
				var nativeArray3 = chunk.GetNativeArray(ref m_PrefabRefType);
				var bufferAccessor = chunk.GetBufferAccessor(ref m_NodeType);
				var styleFlags = (OverlayRenderSystem.StyleFlags)0;
				var flag = chunk.Has(ref m_LotType);
				var color = chunk.Has(ref m_Batch) ? m_RenderingSettingsData.m_HoveredColor : m_RenderingSettingsData.m_WarningColor;

				for (var i = 0; i < nativeArray.Length; i++)
				{
					var flag4 = false;

					var area = nativeArray[i];
					if ((area.m_Flags & AreaFlags.Slave) != 0)
					{
						continue;
					}

					var prefab = nativeArray3[i].m_Prefab;
					var dynamicBuffer = bufferAccessor[i];
					var geometryData = m_PrefabGeometryData[prefab];
					var @float = dynamicBuffer[0].m_Position;
					if (dynamicBuffer.Length == 1)
					{
						if (flag4 && nodeMap.Add(@float))
						{
							DrawNode(color, @float, geometryData, styleFlags);
						}

						continue;
					}

					for (var j = 1; j < dynamicBuffer.Length; j++)
					{
						var position = dynamicBuffer[j].m_Position;
						Border item;
						if ((area.m_Flags & AreaFlags.CounterClockwise) != 0)
						{
							var border = default(Border);
							border.m_StartPos = @float;
							border.m_EndPos = position;
							item = border;
						}
						else
						{
							var border = default(Border);
							border.m_StartPos = position;
							border.m_EndPos = @float;
							item = border;
						}

						if (flag4 && nodeMap.Add(@float))
						{
							DrawNode(color, @float, geometryData, styleFlags);
						}

						if (!borderMap.Contains(item))
						{
							if (flag && j == 1)
							{
								DrawEdge(color * 0.5f, @float, position, geometryData, false, styleFlags);
							}
							else
							{
								DrawEdge(color, @float, position, geometryData, false, styleFlags);
							}
						}

						if (flag4 && nodeMap.Add(position))
						{
							DrawNode(color, position, geometryData, styleFlags);
						}

						@float = position;
					}

					if ((area.m_Flags & AreaFlags.Complete) != 0)
					{
						var position2 = dynamicBuffer[0].m_Position;
						Border item2;
						if ((area.m_Flags & AreaFlags.CounterClockwise) != 0)
						{
							var border = default(Border);
							border.m_StartPos = @float;
							border.m_EndPos = position2;
							item2 = border;
						}
						else
						{
							var border = default(Border);
							border.m_StartPos = position2;
							border.m_EndPos = @float;
							item2 = border;
						}

						if (flag4 && nodeMap.Add(@float))
						{
							DrawNode(color, @float, geometryData, styleFlags);
						}

						if (!borderMap.Contains(item2))
						{
							DrawEdge(color, @float, position2, geometryData, false, styleFlags);
						}

						if (flag4 && nodeMap.Add(position2))
						{
							DrawNode(color, position2, geometryData, styleFlags);
						}
					}
				}
			}

			private void DrawNode(Color color, float3 position, AreaGeometryData geometryData, OverlayRenderSystem.StyleFlags styleFlags)
			{
				m_OverlayBuffer.DrawCircle(color, color, 0f, styleFlags, new float2(0f, 1f), position, geometryData.m_SnapDistance * 0.3f);
			}

			private void DrawEdge(Color color, float3 startPos, float3 endPos, AreaGeometryData geometryData, bool dashedLines, OverlayRenderSystem.StyleFlags styleFlags)
			{
				var line = new Line3.Segment(startPos, endPos);
				if (dashedLines)
				{
					var num = math.distance(startPos.xz, endPos.xz);
					num /= math.max(1f, math.round(num / (geometryData.m_SnapDistance * 1.25f)));
					m_OverlayBuffer.DrawDashedLine(color, color, 0f, styleFlags, line, geometryData.m_SnapDistance * 0.2f, num * 0.55f, num * 0.45f);
				}
				else
				{
					m_OverlayBuffer.DrawLine(color, color, 0f, styleFlags, line, geometryData.m_SnapDistance * 0.3f, 1f);
				}
			}
		}
	}
}
