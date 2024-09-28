namespace Meow.Plugin.UsagiPlugin;

/// <summary>
/// 资源管理
/// </summary>
public class UsagiResourceManager
{
    /// <summary>
    /// 肯定语气音频
    /// </summary>
    private string[] SureAudio { get; set; }
    
    /// <summary>
    /// 疑问语气音频
    /// </summary>
    private string[] QuestionableAudio { get; set; }
    
    /// <summary>
    /// 发疯音频
    /// </summary>
    private string[] CrazyAudio { get; set; }
    
    /// <summary>
    /// 表示肯定的表情包id
    /// </summary>
    private string[] SureImageUid { get; set; }
    
    /// <summary>
    /// 表示疑问的表情包id
    /// </summary>
    private string[] QuestionableImageUid { get; set; }
    
    /// <summary>
    /// 表示发疯的表情包id
    /// </summary>
    private string[] CrazyImageUid { get; set; }
}