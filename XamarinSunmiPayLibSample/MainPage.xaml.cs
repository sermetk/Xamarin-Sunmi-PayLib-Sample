using System;
using System.ComponentModel;
using Xamarin.Forms;
using XamarinSunmiPayLibSample.Helpers;

namespace XamarinSunmiPayLibSample
{
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            BindingContext = new MainPageViewModel();
            InitializeComponent();
        }
    }
}
