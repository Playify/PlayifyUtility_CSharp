using JetBrains.Annotations;

namespace PlayifyUtility.Web.Utils;

[PublicAPI]
public enum RequestType:byte{
	Get,
	Post,
	Put,
	Head,
	Options,
}