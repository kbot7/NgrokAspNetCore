// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copyright (c) 2019 Kevin Gysberg

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Ngrok.AspNetCore.Exceptions;
using Ngrok.AspNetCore.Internal;
using Mono.Unix;

namespace Ngrok.AspNetCore.Services
{
	public class NGrokDownloader
	{
		private readonly HttpClient _httpClient;

		public NGrokDownloader(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}

		/// <summary>
		/// Download NGrok from equinox.io CDN
		/// </summary>
		/// <exception cref="NGrokUnsupportedException">Throws if platform not supported by NGrok</exception>
		/// <exception cref="HttpRequestException">Throws if failed to download from CDN</exception>
		/// <returns></returns>
		public async Task DownloadExecutableAsync(CancellationToken cancellationToken)
		{
			var ngrokPath = $"{Path.Combine(Directory.GetCurrentDirectory(), "ngrok.exe")}";
			if (File.Exists(ngrokPath))
				return;

			var downloadUrl = GetDownloadPath();
			var fileName = $"{RuntimeExtensions.GetOsArchitectureString()}.zip";
			var filePath = $"{Path.Combine(Directory.GetCurrentDirectory(), fileName)}";

			var downloadResponse = await _httpClient.GetAsync(downloadUrl, cancellationToken);
			downloadResponse.EnsureSuccessStatusCode();

			// Download Zip
			var downloadStream = await downloadResponse.Content.ReadAsStreamAsync();
			await using (var writer = File.Create(filePath))
			{
				await downloadStream.CopyToAsync(writer, cancellationToken);
			}

			// Extract zip
			ZipFile.ExtractToDirectory(filePath, Directory.GetCurrentDirectory());

			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				GrantNGrokFileExecutablePermissions();
		}

		private static void GrantNGrokFileExecutablePermissions()
		{
			var process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					RedirectStandardOutput = true,
					UseShellExecute = false,
					CreateNoWindow = true,
					WindowStyle = ProcessWindowStyle.Hidden,
					FileName = "/bin/bash",
					Arguments = $"-c \"chmod +x {Directory.GetCurrentDirectory()}/ngrok\""
				}
			};

			process.Start();
			process.WaitForExit();
		}

		/// <summary>
		/// Get full url to download NGrok on this platform
		/// </summary>
		/// <exception cref="NGrokUnsupportedException">Throws if platform not supported by NGrok</exception>
		/// <returns></returns>
		private string GetDownloadPath()
		{
			const string cdn = "https://bin.equinox.io";
			const string cdnPath = "c/4VmDzA7iaHb/NGrok-stable";

			return $"{cdn}/{cdnPath}-{RuntimeExtensions.GetOsArchitectureString()}.zip";
		}
	}
}