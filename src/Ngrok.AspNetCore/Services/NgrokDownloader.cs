// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copyright (c) 2019 Kevin Gysberg

using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Ngrok.AspNetCore.Exceptions;
using Ngrok.AspNetCore.Internal;

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
		/// Check if Ngrok present in current directory or Windows PATH variable. If not, download from CDN, and throw exception if download fails
		/// </summary>
		/// <exception cref="NgrokUnsupportedException">Throws if platform not supported by Ngrok</exception>
		/// <exception cref="NgrokNotFoundException">Throws if Ngrok not found and failed to download from CDN</exception>
		/// <returns></returns>
		public async Task<string> EnsureNgrokInstalled(NgrokOptions options)
		{
			// Search options
			var fileInOptions = !string.IsNullOrWhiteSpace(options.NgrokPath) && File.Exists(options.NgrokPath);
			if (fileInOptions)
			{
				return options.NgrokPath;
			}

			// Search execution directory
			var fileExists = File.Exists(Path.Combine(Directory.GetCurrentDirectory(), RuntimeExtensions.GetNgrokExecutableString()));
			if (fileExists)
			{
				return Path.Combine(Directory.GetCurrentDirectory(), RuntimeExtensions.GetNgrokExecutableString());
			}

			// Search Windows PATH
			var envFullPath = PathExtensions.GetFullPathFromEnvPath("Ngrok.exe");
			if (!string.IsNullOrWhiteSpace(envFullPath) && File.Exists(envFullPath))
			{
				return envFullPath;
			}

			// Throw exception if not found yet and downloading is disabled
			if (!options.DownloadNgrok) throw new NgrokNotFoundException();

			await DownloadNgrokAsync();
			return RuntimeExtensions.GetNgrokExecutableString();
		}

		/// <summary>
		/// Download Ngrok from equinox.io CDN
		/// </summary>
		/// <exception cref="NgrokUnsupportedException">Throws if platform not supported by Ngrok</exception>
		/// <exception cref="HttpRequestException">Throws if failed to download from CDN</exception>
		/// <returns></returns>
		public async Task DownloadNgrokAsync()
		{
			var downloadUrl = GetDownloadPath();
			var fileName = $"{RuntimeExtensions.GetOsArchitectureString()}.zip";
			var filePath = $"{Path.Combine(Directory.GetCurrentDirectory(), fileName)}";

			var downloadResponse = await _httpClient.GetAsync(downloadUrl);
			downloadResponse.EnsureSuccessStatusCode();

			// Download Zip
			var downloadStream = await downloadResponse.Content.ReadAsStreamAsync();
			using (var writer = File.Create(filePath))
			{
				await downloadStream.CopyToAsync(writer);
			}

			// Extract zip
			ZipFile.ExtractToDirectory(filePath, Directory.GetCurrentDirectory());
		}

		/// <summary>
		/// Get full url to download Ngrok on this platform
		/// </summary>
		/// <exception cref="NgrokUnsupportedException">Throws if platform not supported by Ngrok</exception>
		/// <returns></returns>
		public string GetDownloadPath()
		{
			var architecture = RuntimeInformation.ProcessArchitecture;
			const string cdn = "https://bin.equinox.io";
			const string cdnPath = "c/4VmDzA7iaHb/Ngrok-stable";

			return $"{cdn}/{cdnPath}-{RuntimeExtensions.GetOsArchitectureString()}.zip";
		}
	}
}