namespace PlayifyUtility.Utils.Extensions;

public static partial class AsyncExtensions{
	public static async ValueTask ToNonGeneric<T>(this ValueTask<T> t)=>await t;

	public static async ValueTask<T> WithResult<T>(this ValueTask t,T result){
		await t;
		return result;
	}

	public static async Task<T> WithResult<T>(this Task t,T result){
		await t;
		return result;
	}
}