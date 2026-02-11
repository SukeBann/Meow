using Meow.Core.Model.Base;

namespace Meow.Plugin.NeverStopTalkingPlugin.Models;

/// <summary>
/// 违禁词记录
/// </summary>
public class ForbiddenWordRecord : DatabaseRecordBase
{
    public ForbiddenWordRecord() { }

    public ForbiddenWordRecord(uint recordUserId, string forbiddenWord)
    {
        RecordUserId = recordUserId;
        ForbiddenWord = forbiddenWord;
    }

   /// <summary>
   /// 记录者id
   /// </summary>
   public uint RecordUserId { get; set; }

   /// <summary>
   /// 违禁词
   /// </summary>
   public string ForbiddenWord { get; set; } = string.Empty;
}