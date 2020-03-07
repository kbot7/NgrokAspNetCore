// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copyright (c) 2019 Kevin Gysberg

using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using FluffySpoon.AspNet.NGrok.Exceptions;
using FluffySpoon.AspNet.NGrok.Internal;

namespace FluffySpoon.AspNet.NGrok.Services
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
		public async Task DownloadExecutableAsync()
		{
			var downloadUrl = GetDownloadPath();
			var fileName = $"{RuntimeExtensions.GetOsArchitectureString()}.zip";
			var filePath = $"{Path.Combine(Directory.GetCurrentDirectory(), fileName)}";
            if (File.Exists(filePath))
                return;

			var downloadResponse = await _httpClient.GetAsync(downloadUrl);
			downloadResponse.EnsureSuccessStatusCode();

			// Download Zip
			var downloadStream = await downloadResponse.Content.ReadAsStreamAsync();
            await using (var writer = File.Create(filePath))
			{
				await downloadStream.CopyToAsync(writer);
			}

			// Extract zip
			ZipFile.ExtractToDirectory(filePath, Directory.GetCurrentDirectory());
		}

		/// <summary>
		/// Get full url to download NGrok on this platform
		/// </summary>
		/// <exception cref="NGrokUnsupportedException">Throws if platform not supported by NGrok</exception>
		/// <returns></returns>
		public string GetDownloadPath()
		{
			var architecture = RuntimeInformation.ProcessArchitecture;
			const string cdn = "https://bin.equinox.io";
			const string cdnPath = "c/4VmDzA7iaHb/NGrok-stable";

			return $"{cdn}/{cdnPath}-{RuntimeExtensions.GetOsArchitectureString()}.zip";
		}
	}
}
