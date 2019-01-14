// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copyright (c) 2019 Kevin Gysberg

using System;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;

namespace NgrokAspNetCore.Lib
{
	public class NgrokDownloader
	{
		private readonly HttpClient _httpClient;

		public NgrokDownloader(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}

		/// <summary>
		/// Check if ngrok present in current directory or Windows PATH variable. If not, download from CDN, and throw exception if download fails
		/// </summary>
		/// <exception cref="NgrokUnsupportedException">Throws if platform not supported by ngrok</exception>
		/// <exception cref="NgrokNotFoundException">Throws if ngrok not found and failed to download from CDN</exception>
		/// <returns></returns>
		public async Task<string> EnsureNgrokInstalled(NgrokOptions options)
		{
			bool fileExists = File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "ngrok.exe"));
			if (fileExists)
			{
				return Path.Combine(Directory.GetCurrentDirectory(), "ngrok.exe");
			}

			bool fileInPath = false; // TODO parse this from windows PATH variable if on windows
			if (fileInPath)
			{
				return ""; // TODO get actual path from PATH variable if on windows
			}

			bool fileInOptions = !string.IsNullOrWhiteSpace(options.NgrokPath) && File.Exists(options.NgrokPath);
			if (fileInOptions)
			{
				return options.NgrokPath;
			}

			if (!(fileExists || fileInPath || fileInOptions))
			{
				await DownloadNgrokAsync();
				return "ngrok.exe";
			}
			throw new NgrokNotFoundException();
		}

		/// <summary>
		/// Download ngrok from equinox.io CDN
		/// </summary>
		/// <exception cref="NgrokUnsupportedException">Throws if platform not supported by ngrok</exception>
		/// <exception cref="HttpRequestException">Throws if failed to download from CDN</exception>
		/// <returns></returns>
		public async Task DownloadNgrokAsync()
		{
			var downloadUrl = GetDownloadPath();
			var fileName = $"{RuntimeExtensions.GetOSArchitectureString()}.zip";
			var filePath = $"{Path.Combine(Directory.GetCurrentDirectory(), fileName)}.zip";

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
		/// Get full url to download ngrok on this platform
		/// </summary>
		/// <exception cref="NgrokUnsupportedException">Throws if platform not supported by ngrok</exception>
		/// <returns></returns>
		public string GetDownloadPath()
		{
			var architecture = RuntimeInformation.ProcessArchitecture;
			const string cdn = "https://bin.equinox.io";
			const string cdnPath = "c/4VmDzA7iaHb/ngrok-stable";

			return $"{cdn}/{cdnPath}-{RuntimeExtensions.GetOSArchitectureString()}.zip";
		}
	}
}
