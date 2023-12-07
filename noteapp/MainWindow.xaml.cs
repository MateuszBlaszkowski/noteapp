using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
using MongoDB.Driver;
using MongoDB.Bson;
using Amazon.SecurityToken.Model;
using Microsoft.Win32;
using System.IO;

namespace noteapp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<Note> notes = new ObservableCollection<Note>();
        MongoClient client;
        string curIDD;
        TextBox textBlock;
        TextBox title;
        // DB paswd: RqlU8WlrIX97XTQP
        public MainWindow()
        {
            InitializeComponent();

            string conString = "mongodb+srv://matiblasz:RqlU8WlrIX97XTQP@cluster0.vucqhwm.mongodb.net/?retryWrites=true&w=majority";
            var settings = MongoClientSettings.FromConnectionString(conString);
            settings.ServerApi = new ServerApi(ServerApiVersion.V1);
            client = new MongoClient(settings);

            try
            {
                var collection = client.GetDatabase("db1").GetCollection<Note>("notes");
                var filter = Builders<Note>.Filter.Empty;
                var document = collection.Find(filter).ToList();
                
                foreach(var element in document)
                {
                    notes.Add(element);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            itemsc.ItemsSource= notes;

            if(notes.Count == 0 ) 
            {
                a.Content = new TextBlock() { Text= "Kliknij przycisk na górze aby dodać notatkę.", Foreground = new SolidColorBrush(Colors.White), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment= VerticalAlignment.Center};
            }
            else
            {
                a.Content = itemsc;
            }
            
        }
        private void openNote(string tag)
        {
            var collection = client.GetDatabase("db1").GetCollection<Note>("notes");
            var filter = Builders<Note>.Filter.Eq(r => r.IDD, tag);
            var document = collection.Find(filter).First();
            header.Content = "Notatka";
            backBtn.Visibility = Visibility.Visible;
            addBtn.Visibility = Visibility.Collapsed;
            noteBtnsPanel.Visibility = Visibility.Visible;
            a.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(document.Color));

            StackPanel stackPanel = new StackPanel()
            {
                Orientation = Orientation.Vertical
            };
            title = new TextBox()
            {
                Text = document.Title,
                FontSize = 26,
                BorderThickness = new Thickness(0),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(document.Color))
            };
            
            textBlock = new TextBox()
            {
                Text = document.Content,
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                BorderThickness = new Thickness(0),
                Height = a.ActualHeight - 100,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(document.Color)),
            };
            
            stackPanel.Children.Add(title);
            stackPanel.Children.Add(textBlock);
            curIDD = document.IDD;
            a.Content = stackPanel;
        }
        private void back()
        {
            header.Content = "Tablica";
            a.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#315B80"));
            backBtn.Visibility = Visibility.Collapsed;
            a.Content = itemsc;
            var collection = client.GetDatabase("db1").GetCollection<Note>("notes");
            var filter = Builders<Note>.Filter.Eq(r => r.IDD, curIDD);
            var update = Builders<Note>.Update.Set(r => r.Content, textBlock.Text);
            collection.UpdateOne(filter, update);
            update = Builders<Note>.Update.Set(r => r.Title, title.Text);
            collection.UpdateOne(filter, update);
            var document = collection.Find(filter).First();
            notes.FirstOrDefault(r => r.IDD == curIDD).Content = textBlock.Text;
            notes.FirstOrDefault(r => r.IDD == curIDD).Title = title.Text;
            
            CollectionViewSource.GetDefaultView(notes).Refresh();

            noteBtnsPanel.Visibility = Visibility.Collapsed;
            addBtn.Visibility = Visibility.Visible;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Guid guid = Guid.NewGuid();
            string id = guid.ToString();

            notes.Add(new Note(){ Content="...", Title="Notatka", IDD = id, Color = "#FFEF5F" });
            a.Content = itemsc;
            var collection = client.GetDatabase("db1").GetCollection<Note>("notes");
            collection.InsertOne(new Note() { Content = "...", Title = "Notatka", IDD = id, Color = "#FFEF5F" });
        }

        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            
            openNote(((StackPanel)sender).Tag.ToString());
        }

        private void StackPanel_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private void MaterialIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("OK");
        }

        private void backBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            back();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            back();
            var collection = client.GetDatabase("db1").GetCollection<Note>("notes");
            var filter = Builders<Note>.Filter.Eq(r => r.IDD, curIDD);
            //var document = collection.Find(filter).First();
            collection.DeleteOne(filter);
            var removeItem = notes.Single( r => r.IDD == curIDD );
            notes.Remove(removeItem);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text file (*.txt)|*.txt";
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads\"+title.Text;
            saveFileDialog.Title = title.Text;
            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, textBlock.Text);
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
            var result = colorDialog.ShowDialog();
            if(result == System.Windows.Forms.DialogResult.OK)
            {
                var color = "#" + (colorDialog.Color.ToArgb() & 0x00FFFFFF).ToString("X6");
                var collection = client.GetDatabase("db1").GetCollection<Note>("notes");
                var filter = Builders<Note>.Filter.Eq(r => r.IDD, curIDD);
                var update = Builders<Note>.Update.Set(r => r.Color, color);
                collection.UpdateOne(filter, update);
                var document = collection.Find(filter).First();
                notes.FirstOrDefault(r => r.IDD == curIDD).Color = color;
                
                back();
            }
        }
    }
}
