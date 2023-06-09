﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using FFmpeg.AutoGen;
using FrameControlEx.Core;
using FrameControlEx.Core.Actions;
using FrameControlEx.Core.FrameControl;
using FrameControlEx.Core.Shortcuts.Managing;
using FrameControlEx.Core.Shortcuts.ViewModels;
using FrameControlEx.Core.Utils;
using FrameControlEx.Shortcuts;
using FrameControlEx.Shortcuts.Converters;
using FrameControlEx.Utils;
using FrameControlEx.Views;
using FrameControlEx.Views.Main;

namespace FrameControlEx {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        private AppSplashScreen splash;

        public void RegisterActions() {
            ActionManager.SearchAndRegisterActions(ActionManager.Instance);
        }

        private async Task SetActivity(string activity) {
            this.splash.CurrentActivity = activity;
            await this.Dispatcher.InvokeAsync(() => {
            }, DispatcherPriority.ApplicationIdle);
        }

        public async Task InitApp() {
            await this.SetActivity("Loading services...");
            string[] envArgs = Environment.GetCommandLineArgs();
            if (envArgs.Length > 0 && Path.GetDirectoryName(envArgs[0]) is string dir && dir.Length > 0) {
                Directory.SetCurrentDirectory(dir);
            }

            IoC.Init();
            ShortcutManager.Instance = new WPFShortcutManager();
            ActionManager.Instance = new ActionManager();
            this.RegisterActions();

            InputStrokeViewModel.KeyToReadableString = KeyStrokeStringConverter.ToStringFunction;
            InputStrokeViewModel.MouseToReadableString = MouseStrokeStringConverter.ToStringFunction;
            IoC.Dispatcher = new DispatcherDelegate(this.Dispatcher);
            IoC.OnShortcutModified = (x) => {
                if (!string.IsNullOrWhiteSpace(x)) {
                    ShortcutManager.Instance.InvalidateShortcutCache();
                    GlobalUpdateShortcutGestureConverter.BroadcastChange();
                    // UpdatePath(this.Resources, x);
                }
            };

            IoC.BroadcastShortcutActivity = (x) => {
                foreach (object window in this.Windows) {
                    if (window is FrameControlWindow fcWin) {
                        fcWin.ActivityBarTextBlock.Text = x?.Trim() ?? "";
                    }
                }
            };

            await this.SetActivity("Loading keymap...");
            string keymapFilePath = Path.GetFullPath(@"Keymap.xml");
            if (File.Exists(keymapFilePath)) {
                using (FileStream stream = File.OpenRead(keymapFilePath)) {
                    ShortcutGroup group = WPFKeyMapSerialiser.Instance.Deserialise(stream);
                    WPFShortcutManager.WPFInstance.SetRoot(group);
                }
            }
            else {
                await IoC.MessageDialogs.ShowMessageAsync("No keymap available", "Keymap file does not exist: " + keymapFilePath + $".\nCurrent directory: {Directory.GetCurrentDirectory()}\nCommand line args:{string.Join("\n", Environment.GetCommandLineArgs())}");
            }

            await this.SetActivity("Loading FFmpeg...");
            ffmpeg.avdevice_register_all();
        }

        protected override void OnExit(ExitEventArgs e) {
            IoC.IsAppRunning = false;
            base.OnExit(e);
        }

        private async void Application_Startup(object sender, StartupEventArgs e) {
            // Dialogs may be shown, becoming the main window, possibly causing the
            // app to shutdown when the mode is OnMainWindowClose or OnLastWindowClose
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            this.MainWindow = this.splash = new AppSplashScreen();
            this.splash.Show();

            this.splash.CurrentActivity = "Initialising application...";
            try {
                await this.InitApp();
            }
            catch (Exception ex) {
                if (IoC.MessageDialogs != null) {
                    await IoC.MessageDialogs.ShowMessageExAsync("App initialisation failed", "Failed to start FrameControl", ex.GetToString());
                }
                else {
                    MessageBox.Show("Failed to start FrameControl:\n\n" + ex, "Fatal App initialisation failure");
                }

                this.Shutdown();
                return;
            }

            BitmapFactory.Instance = new WPFBitmapFactory();

            await this.SetActivity("Loading I/O registry...");
            IORegistry.RegisterStandard();

            await this.SetActivity("Loading Frame Control Window...");
            FrameControlWindow window = new FrameControlWindow();

            this.splash.Close();
            this.MainWindow = window;
            this.ShutdownMode = ShutdownMode.OnMainWindowClose;
            window.Show();

            window.FrameControl.SceneDeck.CreateScene("Scene 1", true);

            // string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "WPFStyles.xaml");
            // using (Stream stream = new BufferedStream(new FileStream(path, FileMode.Create), 16384)) {
            //     var writer = new StreamWriter(stream);
            //     writer.Write($"<!-- BEGIN STYLES -->\n");
            //     foreach ((Style style, string type) in this.GetAllStyles(this.MainWindow)) {
            //         try {
            //             string xaml = XamlWriter.Save(style);
            //             writer.Write($"<!-- STYLE FOR {type} -->\n");
            //             writer.Write(xaml);
            //         }
            //         catch (Exception ex) {
            //             writer.Write($"<!-- STYLE FOR {type} FAILED: {ex.Message} -->\n");
            //         }
            //
            //         writer.Write("\n\n\n");
            //     }
            //
            //     await writer.WriteAsync("\n\n\n\n\n\n\n\n\n\n");
            //     await writer.WriteAsync($"<!-- BEGIN TEMPLATES -->\n");
            //     foreach ((ControlTemplate template, string type) in this.GetAllTemplates(this.MainWindow)) {
            //         try {
            //             string xaml = XamlWriter.Save(template);
            //             writer.Write($"<!-- CONTROLTEMPLATE FOR {type} -->\n");
            //             writer.Write(xaml);
            //         }
            //         catch (Exception ex) {
            //             writer.Write($"<!-- CONTROLTEMPLATE FOR {type} FAILED: {ex.Message} -->\n");
            //         }
            //
            //         writer.Write("\n\n\n");
            //     }
            // }

            // ResourceDictionary dictionary = this.Resources.MergedDictionaries.First(x => x.Contains("ZZZZZ_DUMMY_KEY_FOR_IDENTIFICATION"));
            // if (dictionary == null) {
            //     throw new Exception("Could not find style dictionary");
            // }
            // this.ThemeDictionary = dictionary;
            // dictionary["_REghZy.TestBrush"] = new SolidColorBrush(Colors.Red);
            // new DemoTheme().Show();

            // this regex API is ass, surely there should be a Replace function in the match/groups?
            // string text = File.ReadAllText(@"C:\Users\kettl\Desktop\test.txt");
            // string HexToDecimal(Match match) {
            //     string hex = match.Groups[1].Value.Substring(1);
            //     if (int.TryParse(hex, NumberStyles.HexNumber, null, out int result)) {
            //         return match.Value.Replace(match.Groups[1].Value, result.ToString());
            //     }
            //     else {
            //         return match.Value;
            //     }
            // }
            // text = Regex.Replace(text, "[ARGB]=\"(#..)\"", HexToDecimal);
            // File.WriteAllText(@"C:\Users\kettl\Desktop\test.txt", text);
        }

        private IEnumerable<(Style, string)> GetAllStyles(DependencyObject root) {
            int children = VisualTreeHelper.GetChildrenCount(root);
            for (int i = 0; i < children; i++) {
                DependencyObject child = VisualTreeHelper.GetChild(root, i);
                if (child is FrameworkElement element && element.Style != null) {
                    yield return (element.Style, $"{child.GetType().Name} (Actual Style)");
                }

                if (child is Control control) {
                    foreach (Style style in control.Resources.Values.OfType<Style>()) {
                        yield return (style, $"{child.GetType().Name} (Resource styles)");
                    }
                }

                foreach ((Style, string) style in this.GetAllStyles(child)) {
                    yield return style;
                }
            }
        }

        private IEnumerable<(ControlTemplate, string)> GetAllTemplates(DependencyObject root) {
            int children = VisualTreeHelper.GetChildrenCount(root);
            for (int i = 0; i < children; i++) {
                DependencyObject child = VisualTreeHelper.GetChild(root, i);
                if (child is Control control && control.Template != null) {
                    yield return (control.Template, control.GetType().Name);
                }

                foreach ((ControlTemplate, string) item in this.GetAllTemplates(child)) {
                    yield return item;
                }
            }
        }
    }
}
