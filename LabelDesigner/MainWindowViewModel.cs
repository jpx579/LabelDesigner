using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using LabelDesigner.ViewModels;
using LabelDesigner.Views;

namespace LabelDesigner
{
    public partial class MainWindowViewModel: ObservableObject
    {
        [ObservableProperty]
        public UserControl currentContent;

        public MainWindowViewModel() {
            currentContent = new LabelDesignView(new LabelDesignViewModel());   
        }
    }
}
