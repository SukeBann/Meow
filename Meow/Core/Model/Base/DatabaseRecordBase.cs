using LiteDB;

namespace Meow.Core.Model.Base;

/// <summary>
/// 数据库基类
/// </summary>
public abstract class DatabaseRecordBase
{
    /// <summary>
    /// 数据库id
    /// </summary>
    [BsonId]
    public int DbId { get; set; }

    /// <summary>
    /// 是否被删除
    /// </summary>
    public bool HasDelete { get; set; }


    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; }


    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdateTime { get; set; }

    /// <summary>
    /// 设置删除状态
    /// </summary>
    public virtual DatabaseRecordBase SetDeleteState(bool isDelete)
    {
        HasDelete = isDelete;
        return this; 
    }

    /// <summary>
    /// 设置创建时间
    /// </summary>
    public virtual DatabaseRecordBase SetCreateTime()
    {
        CreateTime = DateTime.Now;
        return this;
    }
    
    /// <summary>
    /// 刷新 更新时间为当前
    /// </summary>
    public virtual DatabaseRecordBase RefreshUpdateTime()
    {
        UpdateTime = DateTime.Now;
        return this;
    }
}