using System.Media;
using System.Reflection;

namespace Key2Mouse.Audio;

/// <summary>
/// 播放嵌入式 wav 提示音（资源路径约定：Key2Mouse.Audio.{name}.wav）
/// </summary>
public sealed class EmbeddedBeepPlayer
{
    private readonly Assembly _assembly = Assembly.GetExecutingAssembly();

    public void Play(string name)
    {
        string resourceName = $"Key2Mouse.Audio.{name}.wav";
        using var stream = _assembly.GetManifestResourceStream(resourceName);

        if (stream is null)
        {
            Console.WriteLine($"未找到嵌入式资源: {resourceName}");
            return;
        }

        using var player = new SoundPlayer(stream);
        player.Play(); // 异步
    }
}