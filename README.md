# Design-and-Implementation-of-UAV-Group-Network-Management-System

**关键词**：无人机群；网络；数据库；WPF；GMap.NET；网络拓扑；飞行姿态

## 项目设计详情

本设计立足于解决无人机群应用时对于内部网络连接状态的监控，并可以实现实时监视无人机位置和姿态，同时还可以对单个无人机的网络信息进行设置。
本文开篇先介绍了关于无人机管理系统以及无人机网络的国内外研究现状，分析了相关发展趋势。然后简单介绍了无人机群网络管理系统开发时所涉及的相关技术和开发环境，本设计主要基于`WPF`（Windows 呈现基础），包括用于界面设计的`XAML `语言，用于实现后台逻辑的`C# `语言。数据库存储使用了`SQLite`。
文章的重点在于各个功能的设计实现上，主要包括：通过`GMap.NET` 技术加载电子地图，展现无人机群内各无人机的实时位置，进一步实现了无人机历史飞行轨迹绘制；通过在`WPF` 的`Canvas` 绘制二次贝塞尔曲线展示集群内无人机之间的网络拓扑；通过`Material Design In XAML `控件库，使得无人机网络设置信息展示更加美观，当用户更改设置后，信息将借由`Socket` 类发送给后台服务器以更新无人机设置；无人机飞行姿态的展示是通过虚拟仪表盘实现的，虚拟仪表盘展示了无人机的偏航角、俯仰角、滚动角和海拔；无人机历史飞行轨迹和姿态的展示，调用了数据库内存储的历史飞行信息，同时展示电子地图上的轨迹和实时姿态。
最后，通过对系统各模块的单独测试和系统整体调试，验证了系统设计的可行性和可靠性，满足了系统设计的需求，实现了预期效果。

## 文件结构

![structure](https://gitee.com/imbois/blogimg/raw/master/IMG/20210809022229.png)

其中`arrow`是用于绘制二次贝塞尔曲线，`Map`用于设置`GMap.NET`地图源，`HUD`绘制虚拟仪表盘。

## 依赖包

![package](https://gitee.com/imbois/blogimg/raw/master/IMG/20210809022829.png)

此处的所有依赖包均依赖`NuGet`包管理器安装。主要包括：`GMap.NET.Core` ，`GMap.NET.WinPresentation`，

`MaterialDesignColors` ，`MaterialDesignThemes`， `Newtonsoft.Json`， `System.Data.SQLite`。


> 本项目设计时参考了很多博客和GitHub开源项目，由于作者疏忽，未从项目开始时及时保存相关链接，谨在此表达对涉及原作者的歉意和感谢。