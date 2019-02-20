using System.IO;
using System.Linq;

namespace NgrokAspNetCore.Internal
{
	internal static class PathExtensions
	{
		/// <summary>
		/// Get a fully qualified path for a file on the PATH variable. Include .exe if searching for an exe
		/// </summary>
		/// <param name="searchApp"></param>
		/// <returns></returns>
		public static string GetFullPathFromEnvPath(string searchApp)
		{
			var enviromentPath = System.Environment.GetEnvironmentVariable("PATH");
			if (enviromentPath == null)
			{
				return null;
			}
			var paths = enviromentPath.Split(';');
			var exePath = paths.Select(x => Path.Combine(x, searchApp))
				.FirstOrDefault(File.Exists);
			return exePath;
		}

		public static bool FileExists(this string path)
		{
			if (string.IsNullOrWhiteSpace(path)) return false;
			var relativeExists = File.Exists(path);
			if (relativeExists) return true;
			var fullExists = File.Exists(Path.Combine(Directory.GetCurrentDirectory(), path));
			return fullExists;
		}
	}
}
