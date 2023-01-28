using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;

namespace TimeKeep.Client;

public static class AsyncServerStreamingCallExtension
{
	public static IAsyncEnumerator<T> GetAsyncEnumerator<T>(this AsyncServerStreamingCall<T> asyncServerStreamingCall) =>
		new AsyncStreamReaderEnumerator<T>(asyncServerStreamingCall.ResponseStream);

	private class AsyncStreamReaderEnumerator<T> : IAsyncEnumerator<T>
	{
		private readonly IAsyncStreamReader<T> asyncStreamReader;

		public T Current => asyncStreamReader.Current;

		public AsyncStreamReaderEnumerator(IAsyncStreamReader<T> asyncStreamReader) =>
			this.asyncStreamReader = asyncStreamReader;

		public ValueTask<bool> MoveNextAsync() =>
			new ValueTask<bool>(asyncStreamReader.MoveNext(CancellationToken.None));

		public ValueTask DisposeAsync() =>
			ValueTask.CompletedTask;
	}
}
