using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Camille.Core.MiraiBase.Models.Base;
using Meow.Core.Model.Base;
using Meow.UI.ViewModels.Models;
using PropertyChanged;

namespace Meow.UI.ViewModels;

[AddINotifyPropertyChangedInterface]
public class SimulationViewModel: HostDatabaseSupport
{
    /// <inheritdoc />
    public SimulationViewModel(Core.Meow bot) : base(bot)
    {
        //TODO 这里先模拟创建好
        var sessionInfo = new SessionInfo(bot, MessageChain.MessageType.Group, 726070631, "模拟名称", "模拟重命名");
        ChatSession.Add(sessionInfo);
        SessionMapping.Add(sessionInfo.SessionUUID, sessionInfo);
    }

    #region Propeties

    public ObservableCollection<SessionInfo> ChatSession { get; } = new();
    
    /// <summary>
    /// 会话id和会话的映射关系
    /// </summary>
    private static Dictionary<string, SessionInfo> SessionMapping { get; } = new();

    #endregion

    /// <summary>
    /// 根据会话id获取会话
    /// </summary>
    /// <param name="sessionUuid">会话uid</param>
    /// <param name="sessionInfo">会话信息</param>
    /// <returns></returns>
    public static bool GetSessionById(string sessionUuid, [MaybeNullWhen(false)]out SessionInfo sessionInfo)
    {
        return SessionMapping.TryGetValue(sessionUuid, out sessionInfo);
    }
}