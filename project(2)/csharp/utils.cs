namespace BaconEggs;

public static class ImageLoader
{
    public static Task<string> LoadImageAsync(string src)
    {
        return Task.FromResult(src);
    }
}
