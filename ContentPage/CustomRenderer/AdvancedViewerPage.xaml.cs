﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CustomRenderer
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AdvancedViewerPage : ContentPage
    {
        public AdvancedViewerPage()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            InitializeComponent();
        }
    }
}