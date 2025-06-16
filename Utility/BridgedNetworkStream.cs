using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Hi3Helper.Plugin.Core.Utility;

public class BridgedNetworkStream : Stream
{
    private readonly Stream              _stream;
    private readonly HttpRequestMessage  _request;
    private readonly HttpResponseMessage _response;

    private BridgedNetworkStream(Stream stream, HttpRequestMessage request, HttpResponseMessage response, long length)
    {
        _stream   = stream;
        _request  = request;
        _response = response;
        Length    = length;
    }

    public static Task<BridgedNetworkStream> CreateStream(HttpClient client, string url, long fromPos = 0, long? toPos = null, CancellationToken token = default)
        => CreateStream(client, new Uri(url), fromPos, toPos, token);

    public static async Task<BridgedNetworkStream> CreateStream(HttpClient client, Uri url, long fromPos = 0, long? toPos = null, CancellationToken token = default)
    {
        HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, url);

        requestMessage.Version       = client.DefaultRequestVersion;
        requestMessage.VersionPolicy = client.DefaultVersionPolicy;
        requestMessage.Headers.Range = new RangeHeaderValue(fromPos, toPos);

        HttpResponseMessage responseMessage = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, token);
        if (!responseMessage.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Cannot retrieve file from URL: {url} (Status Code: {responseMessage.StatusCode} ({(int)responseMessage.StatusCode})) (StartPos: {fromPos}, EndPos: {toPos})",
                null,
                responseMessage.StatusCode);
        }

        Stream networkStream = await responseMessage.Content.ReadAsStreamAsync(token);
        return new BridgedNetworkStream(networkStream, requestMessage, responseMessage, responseMessage.Content.Headers.ContentLength ?? 0);
    }

    public override void Flush() => _stream.Flush();

    public override int Read(byte[] buffer, int offset, int count)
    {
        int read = _stream.Read(new Span<byte>(buffer, offset, count));
        Position += read;
        return read;
    }

    public override int Read(Span<byte> buffer)
    {
        int read = _stream.Read(buffer);
        Position += read;
        return read;
    }

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        int read = await _stream.ReadAsync(new Memory<byte>(buffer), cancellationToken);
        Position += read;
        return read;
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        int read = await _stream.ReadAsync(buffer, cancellationToken);
        Position += read;
        return read;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }

    protected override void Dispose(bool disposing)
    {
        _stream.Dispose();
        _request.Dispose();
        _response.Dispose();

        base.Dispose(disposing);
    }

    public override async ValueTask DisposeAsync()
    {
        await _stream.DisposeAsync();
        _request.Dispose();
        _response.Dispose();

        GC.SuppressFinalize(this);
    }

    public override bool CanRead  => true;
    public override bool CanSeek  => false;
    public override bool CanWrite => false;
    public override long Length   { get; }
    public override long Position { get; set; }
}
