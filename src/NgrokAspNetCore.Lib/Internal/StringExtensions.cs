namespace NgrokAspNetCore.Internal
{
	internal static class StringExtensions
	{
		internal static bool HasValue(this string input)
		{
			return !string.IsNullOrWhiteSpace(input);
		}
	}
}
