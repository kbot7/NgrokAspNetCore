using System;

namespace NgrokAspNetCore.Exceptions
{
	public class NgrokConfigNotFoundException : Exception
	{
		public NgrokConfigNotFoundException() : base("An ngrok.yml config file was expected but not found") { }
	}
}
