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
        
        public MainWindow()
        {
            InitializeComponent();

            string conString = @"mongodb+srv://matiblasz:YBG9SwsDn8KYrgHB@noteapp.8uhekot.mongodb.net/";
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
            notes.Add(new Note(){ Content="...", Title="Notatka", IDD = "id1"});
            a.Content = itemsc;
            var collection = client.GetDatabase("db1").GetCollection<Note>("notes");
            collection.InsertOne(new Note() { Content = "...", Title = "Notatka", IDD = "id1" });
        }

        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("OK", ((StackPanel)sender).Tag.ToString(), MessageBoxButton.OKCancel);
        }
    }
}
