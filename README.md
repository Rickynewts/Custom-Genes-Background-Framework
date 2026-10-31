# Custom Genes Background Framework

给 RimWorld 1.6/1.7 的基因界面提供自定义背景图的通用框架。

- 需要：Harmony、Biotech
- 不会默认修改任何基因；只有带 `GeneBackgroundExtension` 的 GeneDef 才会被接管
- 与 Vanilla Expanded Framework（VEF）的基因背景补丁兼容，可同时使用

## 用法

在目标 `GeneDef` 里加 `modExtensions`：

```xml
<GeneDef>
  <defName>Mygene</defName><!--基因Def名-->
  ...<!--新增以下-->
  <modExtensions><!--通用扩展-->
    <li Class="GeneBackgroundFramework.GeneBackgroundExtension">
      <backgroundPath>UI/Gene/MyGeneBackground</backgroundPath><!--自定义图标-->
    </li>
  </modExtensions>
</GeneDef>
```
### 按基因类型（原版）分开指定

```xml
<li Class="GeneBackgroundFramework.GeneBackgroundExtension">
  <backgroundPath>UI/Gene/CommonBackground</backgroundPath>
  <backgroundPathXenogenes>UI/Gene/XenogeneBackground</backgroundPathXenogenes><!--异种基因-->
  <backgroundPathEndogenes>UI/Gene/EndogeneBackground</backgroundPathEndogenes><!--谱系基因-->
  <backgroundPathArchite>UI/Gene/ArchiteBackground</backgroundPathArchite><!--超凡基因-->
</li>
```

优先级：`backgroundPathArchite`（biostatArc > 0 时）> `backgroundPathXenogenes` / `backgroundPathEndogenes` > `backgroundPath`。

### 关闭某个基因

不写扩展，或写：

```xml
<li Class="GeneBackgroundFramework.GeneBackgroundExtension">
  <useCustomBackground>false</useCustomBackground>
</li>
```

## 用 Patch 给其它 mod 的基因加背景用法

```xml
<Patch>
  <Operation Class="PatchOperationAddModExtension">
    <success>Always</success>
    <xpath>Defs/GeneDef[defName="SomeGene"]</xpath>
    <value>
      <li Class="GeneBackgroundFramework.GeneBackgroundExtension">
        <backgroundPath>UI/Gene/MyGeneBackground</backgroundPath>
      </li>
    </value>
  </Operation>
</Patch>
```

## 排查

log里搜 `GeneBackgroundFramework`：

- `基因背景补丁已插入 3 处`：补丁已生效；
