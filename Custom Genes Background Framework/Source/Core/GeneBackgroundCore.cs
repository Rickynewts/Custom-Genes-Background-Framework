using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace GeneBackgroundFramework
{
    /// <summary>
    /// 背景选择逻辑：只处理带 GeneBackgroundExtension 的基因
    /// 由 Harmony 的 Transpiler 在 GeneUIUtility.DrawGeneBasics 里插入调用。
    /// </summary>
    public static class GeneBackgroundCore
    {
        private static readonly Dictionary<string, CachedTexture> Textures = new Dictionary<string, CachedTexture>();
        private static readonly HashSet<string> WarnedPaths = new HashSet<string>();

        /// <summary>
        /// current 是原版 / VEF 已经选好的背景（栈上的 CachedTexture）。
        /// 返回该基因最终应该绘制的背景。
        /// </summary>
        public static CachedTexture OverrideBackground(object current, GeneDef gene, GeneType geneType)
        {
            if (gene == null)
                return current as CachedTexture;

            GeneBackgroundExtension extension = gene.GetModExtension<GeneBackgroundExtension>();
            if (extension == null || !extension.useCustomBackground)
                return current as CachedTexture;

            string path = PickPath(extension, gene, geneType);
            if (string.IsNullOrEmpty(path))
                return current as CachedTexture;

            return TextureFor(path);
        }

        private static string PickPath(GeneBackgroundExtension extension, GeneDef gene, GeneType geneType)
        {
            if (gene.biostatArc > 0)
                return FirstNotEmpty(extension.backgroundPathArchite, extension.backgroundPath);

            if (geneType == GeneType.Xenogene)
                return FirstNotEmpty(extension.backgroundPathXenogenes, extension.backgroundPath);

            return FirstNotEmpty(extension.backgroundPathEndogenes, extension.backgroundPath);
        }

        private static string FirstNotEmpty(string primary, string fallback)
        {
            return string.IsNullOrEmpty(primary) ? fallback : primary;
        }

        private static CachedTexture TextureFor(string path)
        {
            if (!Textures.TryGetValue(path, out CachedTexture texture))
            {
                texture = new CachedTexture(path);
                Textures[path] = texture;
            }

            if (WarnedPaths.Add(path))
            {
                Texture2D loaded = texture.Texture;
                if (loaded == null || loaded == BaseContent.BadTex)
                {
                    Log.Warning("[GeneBackgroundFramework] 找不到基因背景贴图：Textures/" + path + ".png");
                }
            }

            return texture;
        }
    }
}
