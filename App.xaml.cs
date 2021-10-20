using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace EyeMovementAnalyzer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // Register Syncfusion license
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NTEyNjcyQDMxMzkyZTMzMmUzMFpFdkR2V0pyOEp6aUpzdlZ5ZGVTc1RUK2JwdjdmUmFsMWZDeXNZSUhzZmM9");
        }
    }
}
