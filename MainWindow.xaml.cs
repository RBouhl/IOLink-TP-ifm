using System;
using System.Net;
using System.Net.Http;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
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






namespace IOLink_TP_ifm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    ///
    ///

    public class OGD592_Raw_Data
    {
        public Dictionary<string,string> data { get; set; }
    }
    public class DV2130_Command
    {
        //public string HexaCommand { get; set; }
        public Dictionary<string,string> data { get; set; }
    }

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void HTTPGetDistance()
        {
            string url = "http://10.122.136.208/iolinkmaster/port[4]/iolinkdevice/pdin/getdata";
            using var client = new HttpClient();
            var response = await client.GetAsync(url);
            var JSON = response.Content.ReadAsStringAsync().Result;
            OGD592_Raw_Data OGD_RawObject = JsonSerializer.Deserialize<OGD592_Raw_Data>(JSON);
            string OGD_ValueHex = OGD_RawObject.data["value"];
            // Le découpage se fait grâce aux infos données dans le PDF qui accompagne l'IODD.
            // On récupère la valeur hexa en découpant la chaîne de caractère.
            string DistanceHex = OGD_ValueHex.Substring(0, 4);
            string ReflexHex = OGD_ValueHex.Substring(8, 4);
            // Conversion et mise en forme.
            int Distance = Convert.ToInt32(DistanceHex, 16);
            int Reflex = Convert.ToInt32(ReflexHex, 16);
            string TXTDistance = "Distance : " + Distance.ToString() + " mm";
            string TXTReflex = "Réflectivité : " + Reflex.ToString() + " %";
            // Affichage (via nos TextBlock)
            TBDistance.Text = TXTDistance;
            TBReflex.Text = TXTReflex;
        }
        private void Button_Click_View(object sender, RoutedEventArgs e)
        {
            HTTPGetDistance();
            /*string URL_Raw;
            // Adresse des données, récupérée depuis le Core Visualiser
            URL_Raw = "http://10.122.136.208/iolinkmaster/port[4]/iolinkdevice/pdin/getdata";
            // Téléchargement des données (format JSON)
            var JSON = new WebClient().DownloadString(URL_Raw);
            // Désérialisation du JSON et transfert dans le modèle objet 
            OGD592_Raw_Data OGD_RawObject = JsonSerializer.Deserialize<OGD592_Raw_Data>(JSON);
            // Récupération des données depuis l'objet
            string OGD_ValueHex = OGD_RawObject.data["value"];
            // Le découpage se fait grâce aux infos données dans le PDF qui accompagne l'IODD.
            // On récupère la valeur hexa en découpant la chaîne de caractère.
            string DistanceHex = OGD_ValueHex.Substring(0, 4);
            string ReflexHex = OGD_ValueHex.Substring(8, 4);
            // Conversion et mise en forme.
            int Distance = Convert.ToInt32(DistanceHex, 16);
            int Reflex = Convert.ToInt32(ReflexHex, 16);
            string TXTDistance = "Distance : " + Distance.ToString() + " mm";
            string TXTReflex = "Réflectivité : " + Reflex.ToString() + " %";
            // Affichage (via nos TextBlock)
            TBDistance.Text = TXTDistance;
            TBReflex.Text = TXTReflex;*/
        }
        private async void PostColor(string ColorCode)
        {
            var values = new Dictionary<string, string>
            {
                { "newvalue", ColorCode }
            };
            var json_values = JsonSerializer.Serialize(values);
            var data = new StringContent(json_values, Encoding.UTF8, "application/json");
            var url = "http://10.122.136.208/iolinkmaster/port[2]/iolinkdevice/pdout/setdata";
            using var client = new HttpClient();
            var response = await client.PostAsync(url, data);
            string result = "Résultat Requête : " + response.Content.ReadAsStringAsync().Result;
            PostResult.Text = result;
        }

        private void Button_Click_Rouge(object sender, RoutedEventArgs e)
        {
            PostColor("0800");
        }

    private void Button_Click_Vert(object sender, RoutedEventArgs e)
        {
            PostColor("0800");
        }
    }
}
