using LiteDB;

namespace Meow.Plugin.NeverStopTalkingPlugin;

/// <summary>
/// 违禁词记录
/// </summary>
public class ForbiddenWordRecord(uint recordUserId, string forbiddenWord, bool hasDelete)
{
   [BsonId] public int BdId { get; set; }

   /// <summary>
   /// 记录者id
   /// </summary>
   public uint RecordUserId { get; set; } = recordUserId;

   /// <summary>
   /// 违禁词
   /// </summary>
   public string ForbiddenWord { get; set; } = forbiddenWord;

   /// <summary>
   /// 是否被删除
   /// </summary>
   public bool HasDelete { get; set; } = hasDelete;
}