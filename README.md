🧩 一、库的整体功能概述

这是一个基于 .NET 8.0 + WPF 的标签设计与打印库，用于工业或生产系统中打印自定义标签（如零件标签、二维码标签、条码贴纸等）。 它支持用户在界面上可视化设计标签，保存模板，并在程序中动态替换内容后直接打印。

📦 二、功能总览 功能 说明 🧱 标签设计器 提供 WPF 可视化界面，支持拖拽、缩放、文字、图片、二维码等元素 💾 模板保存与加载 设计完成后可保存为 JSON 或 XML 模板文件 🧭 元素字典获取 程序可通过 LabelService.GetLabelElementDic 获取模板中所有可替换元素 🔁 内容动态替换 程序可在打印时替换文字、图片、二维码内容 🖨️ 打印功能 使用 LabelService.PrintLabel() 打印指定模板 📏 尺寸与 DPI 支持 支持 mm → 像素 自动转换，打印时精确控制尺寸 ⚙️ 三、主要类与方法说明 🏷️ 1️⃣ LabelService 类（核心服务类）

此类为主要的外部接口类。开发者只需调用此类的三个核心方法即可实现标签打印全流程。

✅ 方法一：设计标签 LabelService.DesignLabel();

打开标签设计窗口。

用户可拖拽、添加文字、图片、二维码等元素。

设计完成后自动保存为模板文件（建议路径：Labels/xxx.json）。

✅ 方法二：获取可替换字典 var elementDic = LabelService.GetLabelElementDic(string labelPath);

解析指定的模板文件（JSON），获取所有可替换字段。

返回 Dictionary<string, string>，键为元素的唯一 Key，值为默认值。

例如：

{ "CarNumber": "ABC123", "Date": "2025-10-21", "QRCode": "http://example.com/123" }

✅ 方法三：打印标签 bool success = LabelService.PrintLabel( string labelPath, Dictionary<string, string> valueDic, string printerName, out string message, double paddingMm = 0 );

参数说明： 参数 类型 说明 labelPath string 标签模板文件路径（JSON 文件） valueDic Dictionary<string, string> 替换字段键值对 printerName string 打印机名称（可为空，使用默认打印机） message out string 返回打印状态信息 paddingMm double 打印边距，单位毫米 返回值：

true 表示打印成功，false 表示失败（错误信息在 message 中）。

🧠 四、使用示例 using LabelDesigner;

class Program { static void Main() { // 1. 打开标签设计器 LabelService.DesignLabel();

    // 2. 获取模板字段
    var fields = LabelService.GetLabelElementDic("Labels/CarLabel.json");

    // 3. 设置替换值
    fields["CarNumber"] = "LNBSCA5K5SSA29216";
    fields["Date"] = DateTime.Now.ToString("yyyy-MM-dd");
    fields["QRCode"] = "https://factorysystem.com/cars/LNBSCA5K5SSA29216";

    // 4. 打印标签
    if (LabelService.PrintLabel("Labels/CarLabel.json", fields, "ZDesigner ZT411", out string msg, 2))
        Console.WriteLine("✅ 打印成功");
    else
        Console.WriteLine($"❌ 打印失败：{msg}");
}
}
