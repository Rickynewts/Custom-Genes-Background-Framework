using Verse;

namespace GeneBackgroundFramework
{
    /// <summary>
    /// 用法：
    ///   <modExtensions>
    ///     <li Class="GeneBackgroundFramework.GeneBackgroundExtension">
    ///       <backgroundPath>UI/Gene/MyGeneBackground</backgroundPath>
    ///     </li>
    ///   </modExtensions>
    /// </summary>
    public class GeneBackgroundExtension : DefModExtension
    {
        /// <summary>为 false:不参与任何覆盖。</summary>
        public bool useCustomBackground = true;

        /// <summary>默认背景贴图路径。</summary>
        public string backgroundPath;

        /// <summary>xenogene背景，留空则回退 backgroundPath。</summary>
        public string backgroundPathXenogenes;

        /// <summary>endogene背景，留空则回退 backgroundPath。</summary>
        public string backgroundPathEndogenes;

        /// <summary>archite背景，留空则回退 backgroundPath。</summary>
        public string backgroundPathArchite;
    }
}
