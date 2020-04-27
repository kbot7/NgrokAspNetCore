// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copyright (c) 2019 Kevin Gysberg

using Ngrok.AspNetCore.Exceptions;
using System.Runtime.InteropServices;

namespace Ngrok.AspNetCore
{
	public static class RuntimeExtensions
	{
		/// <summary>
		/// Get architecture string format used by Ngrok cdn
		/// </summary>
		/// <exception cref="NgrokUnsupportedException">Throws if platform not supported by Ngrok</exception>
		/// <returns></returns>
		public static string GetArchitectureString()
		{
			var architecture = RuntimeInformation.ProcessArchitecture;
			switch (architecture)
			{
				case Architecture.Arm:
					if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
					{
						throw new NgrokUnsupportedException();
					}
					return "arm";
				case Architecture.Arm64:
					if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
					{
						throw new NgrokUnsupportedException();
					}
					return "arm64";
				case Architecture.X64:
					return "amd64";
				case Architecture.X86:
					return "386";
				default:
					throw new NgrokUnsupportedException();
			}
		}

		/// <summary>
		/// Get OS string format used by Ngrok cdn
		/// </summary>
		/// <exception cref="NgrokUnsupportedException">Throws if platform not supported by Ngrok</exception>
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
			throw new NgrokUnsupportedException();
		}

		/// <summary>
		/// Get OS-Architecture string format used by Ngrok cdn
		/// </summary>
		/// <exception cref="NgrokUnsupportedException">Throws if platform not supported by Ngrok</exception>
		/// <returns></returns>
		public static string GetOsArchitectureString()
		{
			return $"{GetOsString()}-{GetArchitectureString()}";
		}

		/// <summary>
		/// Get Ngrok executable name
		/// </summary>
		/// <exception cref="NgrokUnsupportedException">Throws if platform not supported by Ngrok</exception>
		/// <returns></returns>
		public static string GetNgrokExecutableString()
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				return "Ngrok.exe";
			}
			return "Ngrok";
		}
	}
}