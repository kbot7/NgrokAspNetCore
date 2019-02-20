using System;

namespace NgrokAspNetCore
{
	public class NgrokHostNotFoundException : Exception
	{
		public NgrokHostNotFoundException() : base("A local address was not found or invalid. Unable to create tunnel. Try setting your local address explicitly in NgrokOptions")
		{ }
	}
}
