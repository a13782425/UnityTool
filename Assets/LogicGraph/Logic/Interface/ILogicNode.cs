
namespace Logic
{
    /// <summary>
    /// 逻辑图节点接口
    /// </summary>
    public interface ILogicNode
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        bool Init();
        /// <summary>
        /// 进入当前节点
        /// 会在OnExecute之前执行
        /// </summary>
        /// <returns></returns>
        bool OnEnter();
        /// <summary>
        /// 执行节点
        /// </summary>
        /// <returns></returns>
        bool OnExecute();
        /// <summary>
        /// 正在执行的节点会进入
        /// 节点停止
        /// </summary>
        /// <returns></returns>
        bool OnStop();
    }
}
