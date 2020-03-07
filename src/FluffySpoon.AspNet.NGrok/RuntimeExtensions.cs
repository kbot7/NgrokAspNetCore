// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copyright (c) 2019 Kevin Gysberg

using System.Runtime.InteropServices;
using FluffySpoon.AspNet.NGrok.Exceptions;

namespace FluffySpoon.AspNet.NGrok
{
	public static class RuntimeExtensions
	{
		/// <summary>
		/// Get architecture string format used by NGrok cdn
		/// </summary>
		/// <exception cref="NGrokUnsupportedException">Throws if platform not supported by NGrok</exception>
		/// <returns></returns>
		public static string GetArchitectureString()
		{
			var architecture = RuntimeInformation.ProcessArchitecture;
			switch (architecture)
			{
				case Architecture.Arm:
					if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
					{
						throw new NGrokUnsupportedException();
					}
					return "arm";
				case Architecture.Arm64:
					if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
					{
						throw new NGrokUnsupportedException();
					}
					return "arm64";
				case Architecture.X64:
					return "amd64";
				case Architecture.X86:
					return "386";
				default:
					throw new NGrokUnsupportedException();
			}
		}

		/// <summary>
		/// Get OS string format used by NGrok cdn
		/// </summary>
		/// <exception cref="NGrokUnsupportedException">Throws if platform not supported by NGrok</exception>
		/// <returns></returns>
		public static string GetOsString()
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				return "windows";
			}
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				return "linux";
			}
			if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				return "darwin";
			}
			throw new NGrokUnsupportedException();
		}

		/// <summary>
		/// Get OS-Architecture string format used by NGrok cdn
		/// </summary>
		/// <exception cref="NGrokUnsupportedException">Throws if platform not supported by NGrok</exception>
		/// <returns></returns>
		public static string GetOsArchitectureString()
		{
			return $"{GetOsString()}-{GetArchitectureString()}";
		}

		/// <summary>
		/// Get NGrok executable name
		/// </summary>
		/// <exception cref="NGrokUnsupportedException">Throws if platform not supported by NGrok</exception>
		/// <returns></returns>
		public static string GetNGrokExecutableString()
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				return "NGrok.exe";
			}
			return "NGrok";
		}
	}
}