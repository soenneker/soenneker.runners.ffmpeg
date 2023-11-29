using System.Threading.Tasks;

namespace Soenneker.Runners.FFmpeg.Utils.Abstract;

public interface IFileOperationsUtil
{
    ValueTask SaveToGitRepo(string filePath);
}