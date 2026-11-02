# Custom Genes Background Framework

给 RimWorld 1.6/1.7 的基因界面提供自定义背景图的通用框架。

- 需要Harmony
- 本框架允许在Genedef XML中添加modExtensions字段来修改基因的背景。
- 与VEF的基因背景补丁兼容，可同时使用。

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
### 按基因类型分开指定

```xml
<li Class="GeneBackgroundFramework.GeneBackgroundExtension">
  <backgroundPath>UI/Gene/CommonBackground</backgroundPath>
  <backgroundPathXenogenes>UI/Gene/XenogeneBackground</backgroundPathXenogenes><!--异种基因-->
  <backgroundPathEndogenes>UI/Gene/EndogeneBackground</backgroundPathEndogenes><!--谱系基因-->
  <backgroundPathArchite>UI/Gene/ArchiteBackground</backgroundPathArchite><!--超凡基因-->
</li>
```

优先级：`backgroundPathArchite`（biostatArc > 0 时）> `backgroundPathXenogenes` / `backgroundPathEndogenes` > `backgroundPath`。

### 关闭某个基因自定义背景

不写扩展，或：

```xml
<li Class="GeneBackgroundFramework.GeneBackgroundExtension">
  <useCustomBackground>false</useCustomBackground><!--关闭自定义背景-->
</li>
```

## Patch用法

```xml
<Patch>
  <Operation Class="PatchOperationAddModExtension">
    <success>Always</success>
    <xpath>Defs/GeneDef[defName="SomeGene"]</xpath><!--补丁-->
    <value>
      <li Class="GeneBackgroundFramework.GeneBackgroundExtension">
        <backgroundPath>UI/Gene/MyGeneBackground</backgroundPath>
      </li>
    </value>
  </Operation>
</Patch>
```
