using System.Threading.Tasks;

namespace Soenneker.Runners.FFmpeg.Utils.Abstract;

public interface IExtractionUtil
{
    ValueTask<string> Extract7Zip(string fileNamePath, string? specificFileFilter = null);
}