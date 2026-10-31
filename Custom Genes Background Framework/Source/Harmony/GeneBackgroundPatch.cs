using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace GeneBackgroundFramework
{
    /// <summary>
    /// 原版把基因背景画在私有方法 GeneUIUtility.DrawGeneBasics 里，从三个静态字段读取：
    ///   GeneBackground_Archite / GeneBackground_Endogene / GeneBackground_Xenogene
    ///
    /// 这里用 Transpiler 在这些 ldsfld 之后插入一次 GeneBackgroundCore.OverrideBackground 调用：
    ///   栈上的原背景 → 如果该基因带 GeneBackgroundExtension → 换成扩展里指定的背景。
    ///
    /// 采用"插入覆盖"而不是"替换字段"：
    ///   Vanilla Expanded Framework 也会把同样的 ldsfld 替换成它的 Choose*Background。
    ///   两个 Transpiler 同时作用于这个方法，执行顺序不固定：
    ///     - VEF 先执行：把 ldsfld 换成 call Choose*，我们在 call 之后插入覆盖；
    ///     - 本框架先执行：在 ldsfld 之后插入覆盖，VEF 之后仍能找到 ldsfld 并替换。
    /// </summary>
    [HarmonyPatch]
    public static class Patch_GeneUIUtility_DrawGeneBasics
    {
        /// <summary>Transpiler 是否成功插入了背景覆盖（调试用）。</summary>
        public static bool LastTranspilerMatched;

        // DrawGeneBasics 是私有静态方法：
        // static void DrawGeneBasics(GeneDef gene, Rect geneRect, GeneType geneType,
        //                            bool doBackground, bool clickable, bool overridden)
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(
                typeof(GeneUIUtility),
                "DrawGeneBasics",
                new[] { typeof(GeneDef), typeof(Rect), typeof(GeneType), typeof(bool), typeof(bool), typeof(bool) });
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo endoField = AccessTools.Field(typeof(GeneUIUtility), "GeneBackground_Endogene");
            FieldInfo xenoField = AccessTools.Field(typeof(GeneUIUtility), "GeneBackground_Xenogene");
            FieldInfo architeField = AccessTools.Field(typeof(GeneUIUtility), "GeneBackground_Archite");

            // VEF（如果加载）的 Choose* 方法：它可能先于本框架执行，把 ldsfld 替换成 call Choose*。
            Type vefPatchType = AccessTools.TypeByName("VEF.Genes.VanillaExpandedFramework_GeneUIUtility_DrawGeneBasics_Patch");
            MethodInfo vefEndo = vefPatchType != null ? AccessTools.Method(vefPatchType, "ChooseEndogeneBackground") : null;
            MethodInfo vefXeno = vefPatchType != null ? AccessTools.Method(vefPatchType, "ChooseXenogeneBackground") : null;
            MethodInfo vefArchite = vefPatchType != null ? AccessTools.Method(vefPatchType, "ChooseArchiteBackground") : null;

            MethodInfo overrideMethod = AccessTools.Method(typeof(GeneBackgroundCore), nameof(GeneBackgroundCore.OverrideBackground));

            int inserted = 0;
            foreach (CodeInstruction code in instructions)
            {
                yield return code;

                bool vanillaField = code.opcode == OpCodes.Ldsfld
                    && ((endoField != null && code.LoadsField(endoField))
                        || (xenoField != null && code.LoadsField(xenoField))
                        || (architeField != null && code.LoadsField(architeField)));

                MethodInfo operand = code.operand as MethodInfo;
                bool vefCall = code.opcode == OpCodes.Call && operand != null
                    && ((vefEndo != null && operand.Equals(vefEndo))
                        || (vefXeno != null && operand.Equals(vefXeno))
                        || (vefArchite != null && operand.Equals(vefArchite)));

                if (vanillaField || vefCall)
                {
                    // 栈上已有"原版 / VEF 选好的背景"，压入 gene 和 geneType 后调用
                    // OverrideBackground(object, GeneDef, GeneType) 返回 CachedTexture。
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Call, overrideMethod);
                    inserted++;
                }
            }

            LastTranspilerMatched = inserted >= 1;
            if (inserted < 1)
            {
                Log.Warning("[GeneBackgroundFramework] 未能在 GeneUIUtility.DrawGeneBasics 中定位基因背景字段，框架未生效（游戏版本可能已变更）。");
            }
            else if (vefPatchType == null)
            {
                Log.Message("[GeneBackgroundFramework] 基因背景补丁已插入 " + inserted + " 处（未检测到 VEF）。");
            }
            else
            {
                Log.Message("[GeneBackgroundFramework] 基因背景补丁已插入 " + inserted + " 处（检测到 VEF，已兼容）。");
            }
        }
    }

    /// <summary>
    /// 启动时：
    ///  1. 检查 Harmony 是否已经自动应用了本补丁
    ///  2. 输出补丁状态，便于排查（日志里搜 GeneBackgroundFramework 即可）。
    /// </summary>
    [StaticConstructorOnStartup]
    public static class GeneBackgroundBootstrap
    {
        static GeneBackgroundBootstrap()
        {
            try
            {
                MethodBase target = AccessTools.Method(
                    typeof(GeneUIUtility),
                    "DrawGeneBasics",
                    new[] { typeof(GeneDef), typeof(Rect), typeof(GeneType), typeof(bool), typeof(bool), typeof(bool) });

                if (target == null)
                {
                    Log.Error("[GeneBackgroundFramework] 找不到 GeneUIUtility.DrawGeneBasics，框架无法生效。");
                    return;
                }

                if (!HasOurTranspiler(target))
                {
                    Harmony harmony = new Harmony("yushiren.customgenesbackgroundframework");
                    MethodInfo transpiler = AccessTools.Method(typeof(Patch_GeneUIUtility_DrawGeneBasics), "Transpiler");
                    harmony.Patch(target, transpiler: new HarmonyMethod(transpiler));
                    Log.Message("[GeneBackgroundFramework] Harmony 自动 patch 未生效，已手动应用 Transpiler。");
                }

                Patches patches = Harmony.GetPatchInfo(target);
                int transpilers = patches == null || patches.Transpilers == null ? 0 : patches.Transpilers.Count;
                Log.Message("[GeneBackgroundFramework] DrawGeneBasics transpilers=" + transpilers
                    + " oursMatched=" + Patch_GeneUIUtility_DrawGeneBasics.LastTranspilerMatched);
            }
            catch (Exception exception)
            {
                Log.Error("[GeneBackgroundFramework] 补丁初始化失败: " + exception);
            }
        }

        private static bool HasOurTranspiler(MethodBase target)
        {
            Patches patches = Harmony.GetPatchInfo(target);
            if (patches == null || patches.Transpilers == null)
                return false;

            foreach (Patch patch in patches.Transpilers)
            {
                if (patch.PatchMethod != null && patch.PatchMethod.DeclaringType == typeof(Patch_GeneUIUtility_DrawGeneBasics))
                    return true;
            }
            return false;
        }
    }
}
