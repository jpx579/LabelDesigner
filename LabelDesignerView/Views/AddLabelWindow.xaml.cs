using System.IO;
using System.Windows;
using LabelDesigner.Models;
using Newtonsoft.Json;

namespace LabelDesigner.Views
{
    /// <summary>
    /// AddLabelWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AddLabelWindow : Window
    {
        public AddLabelWindow()
        {
            InitializeComponent();
        }

        private void OnConfirmClick(object sender, RoutedEventArgs e)
        {
            string labelName = NameTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(labelName))
            {
                MessageBox.Show("请输入标签名称。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // 创建 PrintLabels 文件夹路径
                string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PrintLabels");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                // 拼接完整文件路径
                string filePath = Path.Combine(folderPath, $"{labelName}.json");

                if (File.Exists(filePath))
                {
                    MessageBox.Show("该标签名称已存在，请重新输入。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 创建空 JSON 文件
                var defaultmodel = new LabelModel();
                defaultmodel.Width = 60;
                defaultmodel.Height = 60;
                string json = JsonConvert.SerializeObject(defaultmodel);
                File.WriteAllText(filePath, json);

                MessageBox.Show("标签创建成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"创建标签失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
