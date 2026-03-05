using Meow.Core.Model.Base;

namespace Meow.Plugin.NeverStopTalkingPlugin.Models;

/// <summary>
/// 违禁词记录
/// </summary>
public class ForbiddenWordRecord : DatabaseRecordBase
{
    public ForbiddenWordRecord() { }

    public ForbiddenWordRecord(long recordUserId, string forbiddenWord)
    {
        RecordUserId = recordUserId;
        ForbiddenWord = forbiddenWord;
    }

   /// <summary>
   /// 记录者id
   /// </summary>
   public long RecordUserId { get; set; }

   /// <summary>
   /// 违禁词
   /// </summary>
   public string ForbiddenWord { get; set; } = string.Empty;
}