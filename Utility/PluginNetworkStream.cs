using System;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Hi3Helper.Plugin.Core.Utility;

internal class HttpResponseInputStream : Stream
{
    private protected HttpRequestMessage NetworkRequest = null!;
    private protected HttpResponseMessage NetworkResponse = null!;
    private protected Stream NetworkStream = null!;
    private protected long NetworkLength;
    private protected long CurrentPosition;
    public HttpStatusCode StatusCode;
    public bool IsSuccessStatusCode;

    public static async Task<HttpResponseInputStream> CreateStreamAsync(HttpClient client, string url, long? startOffset, long? endOffset, CancellationToken token)
    {
        startOffset ??= 0;

        HttpResponseInputStream httpResponseInputStream = new();
        httpResponseInputStream.NetworkRequest = new HttpRequestMessage
        {
            RequestUri = new Uri(url),
            Method = HttpMethod.Get
        };

        token.ThrowIfCancellationRequested();

        httpResponseInputStream.NetworkRequest.Headers.Range = new RangeHeaderValue(startOffset, endOffset);
        httpResponseInputStream.NetworkResponse = await client
            .SendAsync(httpResponseInputStream.NetworkRequest, HttpCompletionOption.ResponseHeadersRead, token);

        httpResponseInputStream.StatusCode = httpResponseInputStream.NetworkResponse.StatusCode;
        httpResponseInputStream.IsSuccessStatusCode = httpResponseInputStream.NetworkResponse.IsSuccessStatusCode;
        if (httpResponseInputStream.IsSuccessStatusCode)
        {
            httpResponseInputStream.NetworkLength = httpResponseInputStream.NetworkResponse.Content.Headers.ContentLength ?? 0;
            httpResponseInputStream.NetworkStream = await httpResponseInputStream.NetworkResponse.Content
                .ReadAsStreamAsync(token);
            return httpResponseInputStream;
        }

        if ((int)httpResponseInputStream.StatusCode != 416)
        {
            throw new
                HttpRequestException(string.Format("HttpResponse for URL: \"{1}\" has returned unsuccessful code: {0}",
                                                   httpResponseInputStream.NetworkResponse.StatusCode, url));
        }

        await httpResponseInputStream.DisposeAsync();
        throw new HttpRequestException("Http request returned 416!");
    }

    ~HttpResponseInputStream() => Dispose();


    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        int read = await NetworkStream.ReadAsync(buffer, cancellationToken);
        CurrentPosition += read;
        return read;
    }

    public override int Read(Span<byte> buffer)
    {
        int read = NetworkStream.Read(buffer);
        CurrentPosition += read;
        return read;
    }

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    public override void Write(ReadOnlySpan<byte> buffer) => throw new NotSupportedException();

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        int read = await NetworkStream.ReadAsync(buffer.AsMemory(offset, count), cancellationToken);
        CurrentPosition += read;
        return read;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        int read = NetworkStream.Read(buffer, offset, count);
        CurrentPosition += read;
        return read;
    }

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override void Flush()
    {
        if (IsSuccessStatusCode)
        {
            NetworkStream.Flush();
        }
    }

    public override long Length => NetworkLength;

    public override long Position
    {
        get => CurrentPosition;
        set => throw new NotSupportedException();
    }

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
        {
            return;
        }

        NetworkRequest.Dispose();
        NetworkResponse.Dispose();

        if (IsSuccessStatusCode)
        {
            NetworkStream.Dispose();
        }
    }

    public override async ValueTask DisposeAsync()
    {
        NetworkRequest.Dispose();
        NetworkResponse.Dispose();
        if (IsSuccessStatusCode)
            await NetworkStream.DisposeAsync();

        await base.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
