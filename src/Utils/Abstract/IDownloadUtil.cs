using System.Threading.Tasks;

namespace Soenneker.Runners.FFmpeg.Utils.Abstract;

public interface IDownloadUtil
{
    ValueTask<string> Download();
}