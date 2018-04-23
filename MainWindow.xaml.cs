using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MyPasswordPrompt {
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();

            String disp = String.Join(" ", Environment.GetCommandLineArgs().Skip(1));
            isPass = disp.IndexOf("pass", StringComparison.InvariantCultureIgnoreCase) >= 0;

            lPrompt.Content = disp;

            key = "1-" + String.Join("", SHA256Managed.Create().ComputeHash(Encoding.UTF8.GetBytes(disp)).Select(b => b.ToString("x2")));
            dir = IsolatedStorageFile.GetUserStoreForAssembly();
            using (var fs = dir.OpenFile(key, System.IO.FileMode.OpenOrCreate)) {
                String text = Encoding.UTF8.GetString(Convert.FromBase64String(new StreamReader(fs, Encoding.ASCII).ReadToEnd()));
                if (isPass) {
                    tbPass.Visibility = System.Windows.Visibility.Visible;
                    tbPass.Password = text;
                    SetValue(FocusManager.FocusedElementProperty, tbPass);
                }
                else {
                    tbUser.Visibility = System.Windows.Visibility.Visible;
                    tbUser.Text = text;
                    SetValue(FocusManager.FocusedElementProperty, tbUser);
                }
            }
        }

        string body => isPass ? tbPass.Password : tbUser.Text;

        bool isPass;
        IsolatedStorageFile dir;
        String key;

        private void bOk_Click(object sender, RoutedEventArgs e) {
            Console.Write(body);
            if (object.ReferenceEquals(sender, bSave)) {
                using (var fs = dir.CreateFile(key)) {
                    new BinaryWriter(fs).Write(Encoding.ASCII.GetBytes(Convert.ToBase64String(Encoding.UTF8.GetBytes(
                        body
                        ))));
                }
            }
            Close();
        }

        private void bDelete_Click(object sender, RoutedEventArgs e) {
            if (dir.FileExists(key) && MessageBox.Show(String.Format("Delete {0}?", key), Title, MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                dir.DeleteFile(key);
                MessageBox.Show("Delete ok.");
            }
            else {
                MessageBox.Show("Not saved.");
            }
        }
    }
}
