using System.Collections.ObjectModel;
using Lagrange.Core.Message;
using Meow.Core.Model.Base;
using Meow.UI.ViewModels.Models;
using PropertyChanged;

namespace Meow.UI.ViewModels;

[AddINotifyPropertyChangedInterface]
public class SimulationViewModel: HostDatabaseSupport
{
    /// <inheritdoc />
    public SimulationViewModel(Core.Meow host) : base(host)
    {
        //TODO 这里先模拟创建好
        var sessionInfo = new SessionInfo(host, MessageChain.MessageType.Group, 726070631, "模拟名称", "模拟重命名");
        ChatSession.Add(sessionInfo);
    }

    #region Propeties

    public ObservableCollection<SessionInfo> ChatSession { get; } = new();

    #endregion 
}