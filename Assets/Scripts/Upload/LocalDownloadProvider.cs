using GLTFast;
using GLTFast.Loading;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine; // ���뵼����������� Debug ����

public class LocalDownloadProvider : IDownloadProvider
{
    string basePath;

    public LocalDownloadProvider(string basePath)
    {
        this.basePath = basePath;
    }

    public Task<IDownload> Request(Uri url)
    {
        // ƴ�ӳ���������·��
        string path = Path.Combine(basePath, Uri.UnescapeDataString(url.ToString()));
        var fileUri = new Uri("file://" + path);

        return Task.FromResult<IDownload>(new AwaitableDownload(fileUri));
    }

    public Task<ITextureDownload> RequestTexture(Uri url, bool nonReadable)
    {
        string path = Path.Combine(basePath, Uri.UnescapeDataString(url.ToString()));
        var fileUri = new Uri("file://" + path);

        return Task.FromResult<ITextureDownload>(new AwaitableTextureDownload(fileUri, nonReadable));
    }
}
