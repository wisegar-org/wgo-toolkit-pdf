namespace WG.PdfTools
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            this.Title = "WGO Toolkit PDF - " + "v"+ VersionTracking.Default.CurrentVersion.ToString();
        }
    }
}
