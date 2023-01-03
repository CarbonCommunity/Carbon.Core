using System;
using System.Collections.Generic;
using System.Linq;
using Carbon.Core;
using Facepunch;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon;

public class Entities : IDisposable
{
	public static void Init()
	{
		try
		{
			Community.Runtime.Entities?.Dispose();

			foreach (var type in _findSubClassesOf<BaseEntity>())
			{
				Mapping.Add(type, new List<BaseEntity>(Community.Runtime.Config.EntityMapBufferSize));
			}

			Carbon.Logger.Warn($"Mapping {BaseNetworkable.serverEntities.Count:n0} entities... This will take a while.");

			using (TimeMeasure.New("Entity mapping"))
			{
				foreach (var type in Mapping)
				{
					var p1 = BaseNetworkable.serverEntities.Where(x => x.GetType() == type.Key);
					var p2 = p1.Select(x => x as BaseEntity);

					type.Value.AddRange(p2);

					Pool.Free(ref p1);
					Pool.Free(ref p2);
				}
			}

			Carbon.Logger.Warn($"Done mapping.");
		}
		catch (Exception ex) { Carbon.Logger.Error($"Failed Entities.Init()", ex); }
	}

	public void Dispose()
	{
		foreach (var map in Mapping)
		{
			map.Value.Clear();
		}

		Mapping.Clear();
	}

	public static Dictionary<Type, List<BaseEntity>> Mapping { get; internal set; } = new Dictionary<Type, List<BaseEntity>>();

	internal static IEnumerable<Type> _findSubClassesOf<TBaseType>()
	{
		var baseType = typeof(TBaseType);
		var assembly = baseType.Assembly;

		return assembly.GetTypes().Where(t => t.IsSubclassOf(baseType));
	}

	public static Map<T> Get<T>(bool inherited = false) where T : BaseEntity
	{
		var map = new Map<T>
		{
			Pool = Facepunch.Pool.GetList<T>()
		};

		if (inherited)
		{
			foreach (var entry in Mapping)
			{
				if (entry.Key.IsSubclassOf(typeof(T)))
				{
					foreach (var entity in entry.Value)
					{
						map.Pool.Add(entity as T);
					}
				}
			}
		}
		else
		{
			if (Mapping.TryGetValue(typeof(T), out var mapping))
			{
				foreach (var entity in mapping)
				{
					if (entity is T result) map.Pool.Add(result);
				}
			}
		}

		return map;
	}
	public static T GetOne<T>(bool inherited = false) where T : BaseEntity
	{
		using (var map = Get<T>(inherited))
		{
			return map.Pool.FirstOrDefault();
		}
	}
	public static void AddMap(BaseEntity entity)
	{
		if (!Mapping.TryGetValue(entity.GetType(), out var map))
		{
			return;
			// EntityMapping.Add(entity.GetType(), map = new List<BaseEntity> { entity });
		}

		map.Add(entity);
	}
	public static void RemoveMap(BaseEntity entity)
	{
		if (!Mapping.TryGetValue(entity.GetType(), out var map))
		{
			return;
		}

		map.Remove(entity);
	}

	public struct Map<T> : IDisposable where T : BaseEntity
	{
		public List<T> Pool;

		public Map<T> Each(Action<T> callback, Func<T, bool> condition)
		{
			foreach (var drop in Pool)
			{
				if (!condition(drop)) continue;

				callback.Invoke(drop);
			}

			Facepunch.Pool.Free(ref callback);
			Facepunch.Pool.Free(ref condition);
			return this;
		}
		public T Pick(int index)
		{
			if (Pool.Count == 0)
			{
				Logger.Warn($"[Entities.Map.Pick] Pool is empty. Index {index} is unreachable.");
				return default;
			}

			if (Pool.Count - 1 > index)
			{
				Logger.Warn($"[Entities.Map.Pick] Index {index} is higher than the pool count {Pool.Count - 1}");
				return default;
			}

			return Pool[index];
		}

		public void Dispose()
		{
			Carbon.Logger.Warn($"Cleaned {typeof(T).Name}");
			Facepunch.Pool.FreeList(ref Pool);
		}
	}
}
