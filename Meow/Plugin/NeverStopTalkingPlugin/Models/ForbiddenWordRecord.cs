using Meow.Core.Model.Base;

namespace Meow.Plugin.NeverStopTalkingPlugin.Models;

/// <summary>
/// 违禁词记录
/// </summary>
public class ForbiddenWordRecord(uint recordUserId, string forbiddenWord): DatabaseRecordBase
{
   /// <summary>
   /// 记录者id
   /// </summary>
   public uint RecordUserId { get; set; } = recordUserId;

   /// <summary>
   /// 违禁词
   /// </summary>
   public string ForbiddenWord { get; set; } = forbiddenWord;
}