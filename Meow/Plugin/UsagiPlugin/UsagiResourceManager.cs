using Meow.Utils;

namespace Meow.Plugin.UsagiPlugin;

/// <summary>
/// 资源管理
/// </summary>
public class UsagiResourceManager
{
    public UsagiResourceManager()
    {
        SureAudio = ["ula", "yahahaula"];
        QuestionableAudio = ["ha", "ha2", "yahahaha", "hahahaduluuuuha"];
        HappyAudio = ["song", "song2"];
        CrazyAudio = ["yahaha", "iyauluuuuuuu"];
        
        // SureImageUid = ;
        // QuestionableImageUid = ;
        // CrazyImageUid = ;
    }

    #region Properties

    /// <summary>
    /// 音频基础目录
    /// </summary>
    private string AudioBaseDirectory => Path.Combine(StaticValue.AppCurrentPath, "Audios");

    /// <summary>
    /// 图像基础目录
    /// </summary>
    private string ImageBaseDirectory => Path.Combine(StaticValue.AppCurrentPath, "Images");

    /// <summary>
    /// 肯定语气音频
    /// </summary>
    private List<string> SureAudio { get; set; }

    /// <summary>
    /// 疑问语气音频
    /// </summary>
    private List<string> QuestionableAudio { get; set; }

    /// <summary>
    /// 发疯音频
    /// </summary>
    private List<string> CrazyAudio { get; set; }
    
    /// <summary>
    /// 开心音频
    /// </summary>
    private List<string> HappyAudio { get; set; }

    /// <summary>
    /// 表示肯定的表情包id
    /// </summary>
    private List<string> SureImageUid { get; set; }

    /// <summary>
    /// 表示疑问的表情包id
    /// </summary>
    private List<string> QuestionableImageUid { get; set; }

    /// <summary>
    /// 表示发疯的表情包id
    /// </summary>
    private List<string> CrazyImageUid { get; set; }

    #endregion

    #region Method

    /// <summary>
    /// 根据音频名称列表创建音频路径列表。
    /// </summary>
    /// <param name="audioNameList">音频名称列表</param>
    /// <returns>音频路径列表，每个元素对应一个音频文件的完整路径</returns>
    private List<string> CreateAudioPathByName(List<string> audioNameList)
    {
        return audioNameList.Select(x => Path.Combine(AudioBaseDirectory, $"{x}.mp3")).ToList();
    }

    #endregion
}