using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * Copyright (c) 2021 BepInEx, released under MIT License
 * All rights reserved.
 *
 */

namespace Carbon.Utility;

internal static class Publicizer
{
	private static MemoryStream memoryStream = null;

	internal static void Read(string Source)
	{
		try
		{
			if (!File.Exists(Source))
				throw new Exception($"Assembly file '{Source}' was not found");

			memoryStream = new MemoryStream(File.ReadAllBytes(Source));
			Logger.Log($"Loaded '{Path.GetFileName(Source)}' to memory");
		}
		catch (Exception ex)
		{
			Logger.Error($"Error while reading '{Source}'", ex);
			throw (ex);
		}
	}

	internal static bool IsPublic(string Type, string Method)
	{
		try
		{
			if (memoryStream == null) throw new Exception();
			memoryStream.Position = 0;

			AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(memoryStream);
			if (assembly == null) throw new Exception();

			TypeDefinition t = assembly.MainModule.Types.First(x => x.Name == "ServerMgr");
			if (t == null) throw new Exception();

			MethodDefinition m = t.Methods.First(x => x.Name == "Shutdown");
			if (m == null) throw new Exception();

			assembly.Dispose();
			assembly = null;

			return m.IsPublic;
		}
		catch (Exception ex)
		{
			Logger.Error($"Assembly '{Type}.{Method}' was not found", ex);
			throw ex;
		}
	}

	internal static void Publicize()
	{
		AssemblyDefinition assembly = null;
		DefaultAssemblyResolver resolver = new DefaultAssemblyResolver();

		try
		{
			if (memoryStream == null) throw new Exception();
			memoryStream.Position = 0;

			resolver.AddSearchDirectory(Context.Managed);

			assembly = AssemblyDefinition.ReadAssembly(
				memoryStream, parameters: new ReaderParameters { AssemblyResolver = resolver });

			if (assembly == null) throw new Exception();
			Logger.Log($"Reading assembly from memory");
		}
		catch (Exception ex)
		{
			Logger.Error("Error while reading from memory", ex);
			throw ex;
		}

		try
		{
			AssemblyNameReference scope
				= assembly.MainModule.AssemblyReferences
					.OrderByDescending(a => a.Version)
					.FirstOrDefault(a => a.Name == "mscorlib");

			MethodReference nsAttributeCtor = new MethodReference(
				".ctor", assembly.MainModule.TypeSystem.Void,
				declaringType: new TypeReference("System", "NonSerializedAttribute", assembly.MainModule, scope))
			{
				HasThis = true
			};

			Logger.Log("Starting the publicize process");

			foreach (TypeDefinition Type in assembly.MainModule.Types)
			{
				if (Blacklist.IsBlacklisted(Type.Name))
				{
					Logger.Warn($"Excluded '{Type.Name}' due to blacklisting");
					continue;
				}

				if (Type.IsNested)
				{
					Type.IsNestedPublic = true;
				}
				else
				{
					Type.IsPublic = true;
				}

				foreach (MethodDefinition Method in Type.Methods)
				{
					if (Blacklist.IsBlacklisted($"{Type.Name}.{Method.Name}"))
					{
						Logger.Warn($"Excluded '{Type.Name}.{Method.Name}' due to blacklisting");
						continue;
					}

					Method.IsPublic = true;
				}

				foreach (FieldDefinition Field in Type.Fields)
				{
					if (Blacklist.IsBlacklisted($"{Type.Name}.{Field.Name}"))
					{
						Logger.Warn($"Excluded '{Type.Name}.{Field.Name}' due to blacklisting");
						continue;
					}

					// Prevent publicize auto-generated fields
					if (Type.Events.Any(x => x.Name == Field.Name)) continue;

					if (nsAttributeCtor != null && !Field.IsPublic
												&& !Field.CustomAttributes.Any(a => a.AttributeType.FullName == "UnityEngine.SerializeField"))
					{
						Field.IsNotSerialized = true;
						Field.CustomAttributes.Add(item: new CustomAttribute(nsAttributeCtor));
					}

					Field.IsPublic = true;
				}
			}
		}
		catch (Exception ex)
		{
			Logger.Error("Publicize process aborted", ex);
			throw ex;
		}

		try
		{
			Logger.Log($"Validating the modified assembly");
			memoryStream = new MemoryStream();
			assembly.Write(memoryStream);

			assembly.Dispose();
			assembly = null;
		}
		catch (Exception ex)
		{
			Logger.Error("Assembly failed the memory validation", ex);
			throw (ex);
		}

		try
		{
			resolver.Dispose();
			resolver = default;
		}
		catch (Exception ex)
		{
			Logger.Error("Unhandled error", ex);
			throw (ex);
		}
	}

	private static IEnumerable<object> GetAllTypeDefinitions(AssemblyDefinition assembly)
	{
		Queue<TypeDefinition> typeQueue = new Queue<TypeDefinition>(assembly.MainModule.Types);

		while (typeQueue.Count > 0)
		{
			TypeDefinition type = typeQueue.Dequeue();
			yield return type;

			foreach (TypeDefinition nestedType in type.NestedTypes)
				typeQueue.Enqueue(nestedType);
		}
	}

	internal static void Write(string Target)
	{
		if (memoryStream == null)
			throw new InvalidOperationException("No assembly was found in memory");
		memoryStream.Position = 0;

		try
		{
			FileStream outputStream = File.Open(Target, FileMode.Create);
			Logger.Log($"Writing to '{Path.GetFileName(Target)}' from memory");
			memoryStream.CopyTo(outputStream);

			outputStream.Dispose();
			outputStream = null;
		}
		catch (Exception ex)
		{
			Logger.Error($"Error while writing to '{Path.GetFileName(Target)}'", ex);
			throw (ex);
		}
	}
}
