using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Soenneker.Git.Util.Abstract;
using Soenneker.Runners.FFmpeg.Utils.Abstract;
using Soenneker.Utils.Environment;
using Soenneker.Utils.File.Abstract;

namespace Soenneker.Runners.FFmpeg.Utils;

///<inheritdoc cref="IFileOperationsUtil"/>
public class FileOperationsUtil : IFileOperationsUtil
{
    private readonly ILogger<FileOperationsUtil> _logger;
    private readonly IGitUtil _gitUtil;
    private readonly IFileUtil _fileUtil;

    public FileOperationsUtil(IFileUtil fileUtil, ILogger<FileOperationsUtil> logger, IGitUtil gitUtil)
    {
        _fileUtil = fileUtil;
        _logger = logger;
        _gitUtil = gitUtil;
    }
    
    public async ValueTask SaveToGitRepo(string filePath)
    {
        string gitDir = _gitUtil.CloneToTempDirectory("https://github.com/soenneker/soenneker.libraries.ffmpeg");

        await _gitUtil.RunCommand("lfs install", gitDir);

        await _gitUtil.RunCommand("lfs pull", gitDir);

        string targetExePath = Path.Combine(gitDir, "src", "Resources","ffmpeg.exe");

        _fileUtil.DeleteIfExists(targetExePath);

        _fileUtil.Move(filePath, targetExePath);

        _gitUtil.AddIfNotExists(gitDir, targetExePath);

        if (_gitUtil.IsRepositoryDirty(gitDir))
        {
            _logger.LogInformation("Changes have been detected in the repository, commiting and pushing...");

            string name = EnvironmentUtil.GetVariableStrict("Name");
            string email = EnvironmentUtil.GetVariableStrict("Email");
            string username = EnvironmentUtil.GetVariableStrict("Username");
            string token = EnvironmentUtil.GetVariableStrict("Token");

            await _gitUtil.RunCommand("commit -m \"Automated update\"", gitDir);

            await _gitUtil.RunCommand("lfs push origin main", gitDir);

            await _gitUtil.Push(gitDir, username, token);
        }
        else
        {
            _logger.LogInformation("There are no changes to commit");
        }
    }
}