﻿// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copyright (c) 2019 Kevin Gysberg

using Ngrok.AspNetCore.Exceptions;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Ngrok.AspNetCore.Services
{
	public class NgrokDownloader
	{
		private readonly HttpClient _httpClient;

		public NgrokDownloader(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}

		/// <summary>
		/// Download Ngrok from equinox.io CDN
		/// </summary>
		/// <exception cref="NgrokUnsupportedException">Throws if platform not supported by Ngrok</exception>
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
				GrantNgrokFileExecutablePermissions();
		}

		private static void GrantNgrokFileExecutablePermissions()
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
		/// Get full url to download Ngrok on this platform
		/// </summary>
		/// <exception cref="NgrokUnsupportedException">Throws if platform not supported by Ngrok</exception>
		/// <returns></returns>
		private string GetDownloadPath()
		{
			const string cdn = "https://bin.equinox.io";
			const string cdnPath = "c/4VmDzA7iaHb/Ngrok-stable";

			return $"{cdn}/{cdnPath}-{RuntimeExtensions.GetOsArchitectureString()}.zip";
		}
	}
}