using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Runners.FFmpeg.Utils.Abstract;

public interface IFileOperationsUtil
{
    ValueTask Process(string filePath, CancellationToken cancellationToken);
}