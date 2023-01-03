using System.Collections.Generic;
using System.IO;
using System.Linq;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.Extensions;

public class OsEx
{
	public static class File
	{
		public static bool Exists(string file)
		{
			if (string.IsNullOrEmpty(file))
			{
				return false;
			}

			return System.IO.File.Exists(file);
		}
		public static void Create(string file, string content)
		{
			if (!string.IsNullOrEmpty(file))
			{
				System.IO.File.WriteAllText(file, content);
			}
		}
		public static void Create(string file, string[] contents)
		{
			if (!string.IsNullOrEmpty(file))
			{
				System.IO.File.WriteAllLines(file, contents);
			}
		}
		public static void Create(string file, byte[] contents)
		{
			if (!string.IsNullOrEmpty(file))
			{
				System.IO.File.WriteAllBytes(file, contents);
			}
		}
		public static void Delete(string file)
		{
			if (!string.IsNullOrEmpty(file))
			{
				if (System.IO.File.Exists(file))
				{
					System.IO.File.Delete(file);
				}
			}
		}

		public static string Copy(string file, string destination, bool overwrite = true)
		{
			if (!string.IsNullOrEmpty(file) && !string.IsNullOrEmpty(destination))
			{
				if (System.IO.File.Exists(file))
				{
					var destinationFolder = System.IO.Path.GetDirectoryName(destination);
					if (!string.IsNullOrEmpty(destinationFolder))
					{
						if (!System.IO.Directory.Exists(destinationFolder))
						{
							System.IO.Directory.CreateDirectory(destinationFolder);
						}
					}

					System.IO.File.Copy(file, destination, overwrite);
				}
			}

			return destination;
		}
		public static string Move(string file, string destination, bool overwrite = true)
		{
			if (!string.IsNullOrEmpty(file) && !string.IsNullOrEmpty(destination))
			{
				if (System.IO.File.Exists(file))
				{
					File.Copy(file, destination);
					File.Delete(file);
				}
			}

			return destination;
		}
		public static string Find(string filter, string folder, SearchOption option = SearchOption.AllDirectories)
		{
			foreach (var f in Folder.GetFilesWithExtension(folder, "*", option))
			{
				if (f.Contains(filter))
				{
					return f;
				}
			}

			return null;
		}

		public static string ReadText(string file)
		{
			if (!string.IsNullOrEmpty(file))
			{
				if (System.IO.File.Exists(file))
				{
					return System.IO.File.ReadAllText(file);
				}
			}

			return string.Empty;
		}
		public static string[] ReadTextLines(string file)
		{
			if (!string.IsNullOrEmpty(file))
			{
				if (System.IO.File.Exists(file))
				{
					return System.IO.File.ReadAllLines(file);
				}
			}

			return new string[0];
		}
		public static byte[] ReadBytes(string file)
		{
			if (!string.IsNullOrEmpty(file))
			{
				if (System.IO.File.Exists(file))
				{
					return System.IO.File.ReadAllBytes(file);
				}
			}

			return new byte[0];
		}
	}

	public static class Folder
	{
		public static bool Exists(string folder)
		{
			if (string.IsNullOrEmpty(folder))
			{
				return false;
			}

			return System.IO.Directory.Exists(folder);
		}
		public static void Create(string folder, bool recreate = false)
		{
			if (!string.IsNullOrEmpty(folder))
			{
				if (recreate)
				{
					if (System.IO.Directory.Exists(folder))
					{
						System.IO.Directory.Delete(folder, true);
					}
				}

				if (!System.IO.Directory.Exists(folder))
				{
					System.IO.Directory.CreateDirectory(folder);
				}
			}
		}

		public static void Delete(string folder)
		{
			if (!string.IsNullOrEmpty(folder))
			{
				if (System.IO.Directory.Exists(folder) && !string.IsNullOrEmpty(folder))
				{
					System.IO.Directory.Delete(folder, true);
				}
			}
		}
		public static void DeleteFilesWithExtension(string folder, string extension, System.IO.SearchOption option = System.IO.SearchOption.AllDirectories)
		{
			if (!string.IsNullOrEmpty(folder))
			{
				if (Exists(folder))
				{
					foreach (string file in System.IO.Directory.GetFiles(folder, string.Format("*.{0}", extension), option))
					{
						System.IO.File.Delete(file);
					}
				}
			}
		}
		public static void DeleteFilesWithExtension(string folder, string extension, string[] exceptions, System.IO.SearchOption option = System.IO.SearchOption.AllDirectories)
		{
			if (System.IO.Directory.Exists(folder) && !string.IsNullOrEmpty(folder))
			{
				foreach (string file in System.IO.Directory.GetFiles(folder, string.Format("*.{0}", extension), option))
				{
					if (exceptions.Any(x => x != Path.GetFileName(file)))
					{
						System.IO.File.Delete(file);
					}
				}
			}
		}
		public static void DeleteContents(string folder, string folderPattern = "*", string filePattern = "*", System.IO.SearchOption option = System.IO.SearchOption.AllDirectories)
		{
			if (System.IO.Directory.Exists(folder) && !string.IsNullOrEmpty(folder))
			{
				foreach (string _file in System.IO.Directory.GetFiles(folder, filePattern, option))
				{
					if (System.IO.File.Exists(_file))
					{
						System.IO.File.Delete(_file);
					}
				}

				foreach (string _folder in System.IO.Directory.GetDirectories(folder, folderPattern, option))
				{
					if (System.IO.Directory.Exists(_folder))
					{
						System.IO.Directory.Delete(_folder, true);
					}
				}
			}
		}
		public static Dictionary<string, string> Copy(string folder, string destination, bool subdirectories = true, bool overwrite = true, System.IO.SearchOption option = SearchOption.AllDirectories)
		{
			if (!string.IsNullOrEmpty(folder) && !string.IsNullOrEmpty(destination))
			{
				if (System.IO.Directory.Exists(folder))
				{
					var folderInfo = new DirectoryInfo(folder);
					var retDictionary = new Dictionary<string, string>();

					var folders = folderInfo.GetDirectories();
					Create(destination);
					retDictionary.Add(folderInfo.FullName, destination);

					var files = folderInfo.GetFiles();
					foreach (var file in files)
					{
						var tempPath = Path.Combine(destination, file.Name);
						file.CopyTo(tempPath, overwrite);
						retDictionary.Add(file.FullName, tempPath);
					}

					foreach (var subDirectory in folders)
					{
						var tempPath = Path.Combine(destination, subDirectory.Name);
						Copy(subDirectory.FullName, tempPath, subdirectories, overwrite, option);
						retDictionary.Add(subDirectory.FullName, tempPath);
					}

					return retDictionary;
				}
			}

			return null;
		}
		public static List<string> Move(string folder, string destination, bool subdirectories = true, bool overwrite = true, System.IO.SearchOption option = SearchOption.AllDirectories)
		{
			if (!string.IsNullOrEmpty(folder) && !string.IsNullOrEmpty(destination))
			{
				if (System.IO.Directory.Exists(folder))
				{
					var folderInfo = new DirectoryInfo(folder);
					var retList = new List<string>();

					var folders = folderInfo.GetDirectories();
					Create(destination);
					retList.Add(destination);

					var files = folderInfo.GetFiles();
					foreach (var file in files)
					{
						var tempPath = Path.Combine(destination, file.Name);
						file.CopyTo(tempPath, overwrite);
						retList.Add(tempPath);
						file.Delete();
					}

					foreach (var subDirectory in folders)
					{
						var tempPath = Path.Combine(destination, subDirectory.Name);
						Copy(subDirectory.FullName, tempPath, subdirectories, overwrite, option);
						retList.Add(tempPath);
						subDirectory.Delete(subdirectories);
					}

					Delete(folder);

					return retList;
				}
			}

			return null;
		}
		public static string[] GetFilesWithExtension(string folder, string extension, System.IO.SearchOption option = System.IO.SearchOption.AllDirectories)
		{
			var files = new List<string>();

			if (System.IO.Directory.Exists(folder) && !string.IsNullOrEmpty(folder))
			{
				foreach (string file in System.IO.Directory.GetFiles(folder, string.Format("*.{0}", extension), option))
				{
					files.Add(file);
				}
			}

			return files.ToArray();
		}
		public static string[] GetFilesWithExtension(string folder, string extension, string[] exceptions, System.IO.SearchOption option = System.IO.SearchOption.AllDirectories)
		{
			var files = new List<string>();

			if (System.IO.Directory.Exists(folder) && !string.IsNullOrEmpty(folder))
			{
				foreach (string file in System.IO.Directory.GetFiles(folder, string.Format("*.{0}", extension), option))
				{
					if (exceptions.Any(x => x != Path.GetFileName(file)))
					{
						files.Add(file);
					}
				}
			}

			return files.ToArray();
		}
	}

	public static class Utils
	{
		public static string Copy(string fileOrFolder, string destination, bool subdirectories = true, bool overwrite = true)
		{
			if (!string.IsNullOrEmpty(fileOrFolder) && !string.IsNullOrEmpty(destination))
			{
				if (File.Exists(fileOrFolder))
				{
					File.Copy(fileOrFolder, destination, overwrite);
				}
				else if (Folder.Exists(fileOrFolder))
				{
					Folder.Copy(fileOrFolder, destination, subdirectories, overwrite);
				}

				return destination;
			}

			return fileOrFolder;
		}

		public static string Move(string fileOrFolder, string destination, bool subdirectories = true, bool overwrite = true)
		{
			if (!string.IsNullOrEmpty(fileOrFolder) && !string.IsNullOrEmpty(destination))
			{
				if (File.Exists(fileOrFolder))
				{
					File.Move(fileOrFolder, destination, overwrite);
				}
				else if (Folder.Exists(fileOrFolder))
				{
					Folder.Move(fileOrFolder, destination, subdirectories, overwrite);
				}

				return destination;
			}

			return fileOrFolder;
		}
		public static void Delete(string fileOrFolder)
		{
			if (!string.IsNullOrEmpty(fileOrFolder))
			{
				if (System.IO.File.Exists(fileOrFolder))
				{
					File.Delete(fileOrFolder);
				}
				else if (System.IO.Directory.Exists(fileOrFolder))
				{
					Folder.Delete(fileOrFolder);
				}
			}
		}
	}
}
