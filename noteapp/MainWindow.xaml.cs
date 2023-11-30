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

namespace noteapp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<Note> notes = new ObservableCollection<Note>();
        MongoClient client;
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
            a.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEF5F"));

            StackPanel stackPanel = new StackPanel()
            {
                Orientation = Orientation.Vertical
            };
            TextBox title = new TextBox()
            {
                Text = document.Title,
                FontSize = 26,
                BorderThickness = new Thickness(0),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEF5F"))
            };
            FlowDocument flowDocument = new FlowDocument();
            Paragraph paragraph = new Paragraph()
            {
                Margin= new Thickness(0),
            };
            Run run= new Run(document.Content); 
            paragraph.Inlines.Add(run);
            flowDocument.Blocks.Add(paragraph);
            
            RichTextBox textBlock = new RichTextBox()
            {
                 Document= flowDocument,
                BorderThickness = new Thickness(0),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEF5F")),
            };
            
            stackPanel.Children.Add(title);
            stackPanel.Children.Add(textBlock);
            a.Content = stackPanel;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("ok"); 
            notes.Add(new Note(){ Content="...", Title="Notatka", IDD = "id1", Color = "#FFEF5F" });
            a.Content = itemsc;
            var collection = client.GetDatabase("db1").GetCollection<Note>("notes");
            collection.InsertOne(new Note() { Content = "...", Title = "Notatka", IDD = "id1" });
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
            header.Content = "Tablica";
            a.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#315B80"));
            backBtn.Visibility = Visibility.Collapsed;
            a.Content = itemsc;
            noteBtnsPanel.Visibility = Visibility.Collapsed; 
            addBtn.Visibility = Visibility.Visible;
        }
    }
}
