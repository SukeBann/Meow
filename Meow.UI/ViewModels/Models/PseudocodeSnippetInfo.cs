using System.Windows.Documents;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entity;
using PropertyChanged;
using SixLabors.ImageSharp;

namespace Meow.UI.ViewModels.Models;

/// <summary>
/// 伪代码段信息 
/// </summary>
[AddINotifyPropertyChangedInterface]
public class PseudocodeSnippetInfo
{
    public PseudocodeSnippetInfo(MessageChain rawMessage)
    {
        RawMessage = rawMessage;
        switch (rawMessage.Type)
        {
            case MessageChain.MessageType.Group:
                Author = rawMessage.GroupMemberInfo?.MemberCard ?? rawMessage.GroupMemberInfo?.MemberName ?? "Unknown";
                break;
            case MessageChain.MessageType.Temp:
            case MessageChain.MessageType.Friend:
            default:
                Author = rawMessage.FriendInfo?.Nickname ?? "Unknown";
                break;
        }

        Author = Author.Replace("\n", "").Replace("\r", "");
        
        GitChangeInfo =
            $"Text:{rawMessage.Count(x => x is TextEntity)}, Image:{rawMessage.Count(x => x is ImageEntity)}, Other:{rawMessage.Count(x => x is not TextEntity && x is not ImageEntity)}";
        ChangeTime = rawMessage.Time.AddHours(8).ToString("s");
        AccessControlCharacter = GetRandomArrayElement(AccessControlCharacterArray);
        IsReadMsg = false;
        ReturnValueType = GetRandomArrayElement(TypeArray);
        MethodName = $"{GetRandomArrayElement(ActionTypeArray)}{GetRandomArrayElement(TypeArray)}";
        ParamType = GetRandomArrayElement(TypeArray);
        ParamName = GetRandomArrayElement(ParamNameArray);
    }

    /// <summary>
    /// 获取数组中的随机元素
    /// </summary>
    /// <returns></returns>
    private T GetRandomArrayElement<T>(T[] array)
    {
        var index = Random.Next(0, array.Length);
        return array[index];
    }

    #region Properties

    /// <summary>
    /// 访问控制符数组
    /// </summary>
    private string[] AccessControlCharacterArray { get; } = ["private", "public", "protected", "internal"];
    
    /// <summary>
    /// 操作类型
    /// </summary>
    private string[] ActionTypeArray { get; } = ["Get", "Set", "Transfer", "Reset", "Calculate", "Restart", "Process", "Clear", "Check"];
    
    /// <summary>
    /// 类型数组
    /// </summary>
    private string[] TypeArray { get; } = [
        "Nozzle", "SpinMotor", "ConveyorBelt", "TemperatureSensor", "PressureValve", "ControlPanel", "HydraulicPump",
        "CoolingFan", "ServoMotor", "Encoder", "RobotArm", "PlcController", "FlowMeter", "ProximitySensor",
        "LimitSwitch", "PressureGauge", "ThermalCouple", "SafetyRelay", "VacuumPump", "Actuator"];
    
    /// <summary>
    /// 参数名称
    /// </summary>
    private string[] ParamNameArray { get; } = [
        "mainCylinder", "waferCarrier", "conveyorBelt", "motorSpeed", "temperatureSensor", "pressureValve",
        "controlPanel", "hydraulicPump", "coolingFan", "servoMotor", "encoder", "robotArm", "plcController",
        "flowMeter", "proximitySensor", "limitSwitch", "pressureGauge", "thermalCouple", "safetyRelay", "vacuumPump"];
    
    private Random Random { get; } = new();

    /// <summary>
    /// 消息链
    /// </summary>
    public MessageChain RawMessage { get; set; }

    /// <summary>
    /// 作者
    /// </summary>
    public string Author { get; set; }
    
    /// <summary>
    /// 变动信息提示
    /// example: [image: 1 text: 1 other: 1]
    /// </summary>
    public string GitChangeInfo { get; set; }
    
    /// <summary>
    /// 变更时间
    /// </summary>
    public string ChangeTime { get; set; }
    
    /// <summary>
    /// 访问控制符
    /// </summary>
    public string AccessControlCharacter { get; set; }

    /// <summary>
    /// 消息是否已经被查看
    /// </summary>
    [AlsoNotifyFor(nameof(DisplayCode))]
    public bool IsReadMsg { get; set; }

    /// <summary>
    /// 返回值类型
    /// </summary>
    public string ReturnValueType { get; set; }

    /// <summary>
    /// 方法名称
    /// </summary>
    public string MethodName { get; set; }

    /// <summary>
    /// 消息未被查看时伪代码段抛出的异常类型
    /// </summary>
    private const string MsgOfUnreadExceptionName = "NotImplementedException";
    
    /// <summary>
    /// 消息被查看后伪代码段抛出的异常类型
    /// </summary>
    private const string MsgOfReadExceptionName = "StackOverflowException";

    /// <summary>
    /// 显示的代码变化
    /// </summary>
    public string DisplayCode => IsReadMsg ? MsgOfReadExceptionName : MsgOfUnreadExceptionName;
    
    /// <summary>
    /// 形参类型
    /// </summary>
    public string ParamType { get; set; }
    
    /// <summary>
    /// 形参名称
    /// </summary>
    public string ParamName { get; set; }
    
    /// <summary>
    /// 显示真实信息窗口
    /// </summary>
    public bool OpenTrueMsgPop { get; set; }

    #endregion
}