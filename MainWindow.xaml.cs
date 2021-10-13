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
 
    public class OGD592_Raw_Data
    {
        public Dictionary<string,string> data { get; set; }
    }
    public class DV2130_Command
    {
        public string cid { get; set; }

        public Dictionary<string, Dictionary<string, string>> data { get; set; }
    }

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private async void HTTPGetAndDisplayDistance()
        {
            string url = "http://10.122.136.183/iolinkmaster/port[4]/iolinkdevice/pdin/getdata";
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
            HTTPGetAndDisplayDistance();
        }
        private async void PostColor(string ColorCode)
        {
            // Documentation :
            // Format de la requête POST
            // https://www.ifm.com/mounting/7391156UK.pdf 
            // Attention : la documentation ne donne pas directement les informations à envoyer
            // Pseudoformat "Request object" qui mélange une partie de l'URL ( /device/application/tag) avec les données (data)
            // Il faut en déduire les vraies requêtes HTTP/POST.
            // 
            // Spécifications de la verrine
            // https://www.ifm.com/download/files/IFM_00043D_20200325_IODD11_en/$file/IFM_00043D_20200325_IODD11_en.pdf 
            // Il faut envoyer deux octets pour commander l'éclairage et le buzzer de la verrine
            // Par exemple "FF" ou "00" (en hexa)

            // Format imposé : "data":{"newvalue":"XXXXX"}
            // On est donc obligé d'utiliser une structure de donnée imbriquée

            // Ne pas chercher à écrire le json directement dans une variable
            // Les quotes servent en C# pour délimiter les variables texte
            // ET elles servent en JSON pour délimiter les champs

            // Ne fonctionne pas en l'état actuel. Réponse du master "401 Bad Request" sans plus de précisions.
            // Il faudrait pouvoir accéder aux logs du serveur Web du Master IOLINK pour pouvoir débugger correctement.
            // Un serveur Web sans logs lisibles est un demi-serveur Web.
            
             var values = 
                new Dictionary<string, Dictionary<string, string>>
            {
                {
                    "data",
                    new Dictionary<string, string>
                    {
                        { "newvalue", ColorCode }
                    }
                }
            };
            var values_json = JsonSerializer.Serialize(values);
            var data = new StringContent(values_json, Encoding.UTF8, "application/json");
            var url = "http://10.122.136.183/iolinkmaster/port[1]/iolinkdevice/pdout/setdata";
            //Test pour vérifier si on envoie bien une requête POST valide.
            //var url = "https://httpbin.org/post";
            using var client = new HttpClient();
            var response = await client.PostAsync(url, data);
            string result = "Résultat Requête : " + response.Content.ReadAsStringAsync().Result;
            TBPostResult.Text = response.ToString();
        }

        private void Button_Click_Rouge(object sender, RoutedEventArgs e)
        {
            PostColor("01");
        }

    private void Button_Click_Vert(object sender, RoutedEventArgs e)
        {
            PostColor("02");
        }
    }
}
