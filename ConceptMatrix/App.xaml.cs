﻿using ConceptMatrix.Models;
using ConceptMatrix.Utility;
using ConceptMatrix.ViewModel;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Application = System.Windows.Application;
using Clipboard = System.Windows.Clipboard;
namespace ConceptMatrix
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
		public static readonly string ToolName = "ConceptMatrix";
		public static readonly string ToolBin = "CMTool";
		public static readonly string UpdaterName = "Concept Matrix Updater";
		public static readonly string UpdaterBin = "ConceptMatrixUpdater";
		public static readonly string GithubRepo = "imchillin/CMTool";
		public static readonly string TwitterHandle = "FFXIVCMTool";
		public static readonly string DiscordCode = "EenZwsN";

        public static Stopwatch sw;

		public CharacterDetails CharacterDetails { get => (CharacterDetails)BaseViewModel.model; set => BaseViewModel.model = value; }
        protected override void OnStartup(StartupEventArgs e)
        {
            sw = Stopwatch.StartNew();

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Dispatcher.UnhandledException += DispatcherOnUnhandledException;

            Current.DispatcherUnhandledException += CurrentOnDispatcherUnhandledException;

            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;

            GetDotNetFromRegistry();
            base.OnStartup(e);

            // This is weird to do here, but this app already sucks.
            MainViewModel.mediator = new Mediator();

            this.Exit += App_Exit;
        }

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteObject(IntPtr value);

        public static BitmapSource GetImageStream(System.Drawing.Image image)
        {
            var bitmap = new System.Drawing.Bitmap(image);
            var bmpPt = bitmap.GetHbitmap();
            var bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                bmpPt,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions()
            );

            //freeze bitmapSource and clear memory to avoid memory leaks
            bitmapSource.Freeze();
            DeleteObject(bmpPt);

            return bitmapSource;
        }

        private static void GetDotNetFromRegistry()
        {
            const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";

            using (var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkey))
            {
                if (ndpKey != null && ndpKey.GetValue("Release") != null)
                {
                    if ((int)ndpKey.GetValue("Release") <= 394801)
                    {
                        var msgResult = System.Windows.MessageBox.Show(".NET Framework Version 4.6.2 or later is not detected. Please install", ".Net Framework not installed", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
                        if (msgResult == MessageBoxResult.Yes)
                        {

                            Process.Start("https://dotnet.microsoft.com/download/dotnet-framework/net472");
                            Current.Shutdown();
                        }
                    }
                }
                else
                {
                    var msgResult = System.Windows.MessageBox.Show(".NET Framework Version 4.6.2 or later is not detected. Please install", ".Net Framework not installed", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
                    if (msgResult == MessageBoxResult.Yes)
                    {

                        Process.Start("https://dotnet.microsoft.com/download/dotnet-framework/net472");
                        Current.Shutdown();
                    }
                }
            }
        }

        public static bool IsValidGamePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            if (!Directory.Exists(path))
                return false;

            if (File.Exists(Path.Combine(path, "game", "ffxivgame.ver")))
                return File.Exists(Path.Combine(path, "game", "ffxivgame.ver"));

            return false;
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            if(MainViewModel.worker.IsBusy)
                MainViewModel.worker.CancelAsync();
            SaveSettings.Default.Save();
            if (MainViewModel.posing2View.EditModeButton.IsChecked == true)
                MainViewModel.posing2View.EditModeButton.IsChecked = false;
            if (MainViewModel.posingView.EditModeButton.IsChecked == true)
                MainViewModel.posingView.EditModeButton.IsChecked = false;

            MainViewModel.Shutdown();
        }

        private void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            var ver = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).FileVersion;

            const string lineBreak = "\n======================================================\n";

            var errorText = "Concept Matirx ran into an error.\n\n" +
                            "Please submit a bug report with the following information.\n " +
                            lineBreak +
                            e.Exception +
                            lineBreak + "\n" +
                            "Copy to clipboard?";

            if (FlexibleMessageBox.Show(errorText, "Crash Report " + ver, MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
            {
                Clipboard.SetText(e.Exception.ToString());
            }
        }

        private void CurrentOnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var ver = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).FileVersion;

            const string lineBreak = "\n======================================================\n";

            var errorText = "Concept Matirx ran into an error.\n\n" +
                            "Please submit a bug report with the following information.\n " +
                            lineBreak +
                            e.Exception +
                            lineBreak + "\n" +
                            "Copy to clipboard?";

            if (FlexibleMessageBox.Show(errorText, "Crash Report " + ver, MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
            {
                Clipboard.SetText(e.Exception.ToString());
            }
        }

        private void DispatcherOnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var ver = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).FileVersion;

            const string lineBreak = "\n======================================================\n";

            var errorText = "Concept Matirx ran into an error.\n\n" +
                            "Please submit a bug report with the following information.\n " +
                            lineBreak +
                            e.Exception +
                            lineBreak + "\n" +
                            "Copy to clipboard?";

            if (FlexibleMessageBox.Show(errorText, "Crash Report " + ver, MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
            {
                Clipboard.SetText(e.Exception.ToString());
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ver = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).FileVersion;

            const string lineBreak = "\n======================================================\n";

            var errorText = "Concept Matirx ran into an error.\n\n" +
                            "Please submit a bug report with the following information.\n " +
                            lineBreak +
                            e.ExceptionObject +
                            lineBreak + "\n" +
                            "Copy to clipboard?";

            if (FlexibleMessageBox.Show(errorText, "Crash Report " + ver, MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
            {
                Clipboard.SetText(e.ExceptionObject.ToString());
            }
        }
    }
}
