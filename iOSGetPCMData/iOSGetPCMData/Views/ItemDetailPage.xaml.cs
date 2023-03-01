using iOSGetPCMData.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace iOSGetPCMData.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}